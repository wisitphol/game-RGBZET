using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class TournamentBracketUI : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private TMP_Text tournamentNameText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button enterLobbyButton;
    [SerializeField] private Button sumTournament;
    [SerializeField] private Transform bracketContainer;
    [SerializeField] private GameObject matchPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference tournamentRef;
    private string tournamentId;
    private string currentMatchId;
    private string currentUsername;
    private Dictionary<string, MatchUI> matches = new Dictionary<string, MatchUI>();
    private bool isInitialized = false;
    private int retryCount = 0;
    private const int MAX_RETRIES = 3;
    private Coroutine updateUICoroutine;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(DisconnectFromPhoton());
        }
        else
        {
            StartCoroutine(InitializeWithRetry());
        }
    }

    private IEnumerator DisconnectFromPhoton()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Debug.Log("Disconnected from Photon");
        StartCoroutine(InitializeWithRetry());
    }

    private IEnumerator InitializeWithRetry()
    {
        ShowLoading("Initializing tournament...");

        while (!isInitialized && retryCount < MAX_RETRIES)
        {
            yield return StartCoroutine(TryInitialize());
            
            if (!isInitialized)
            {
                retryCount++;
                Debug.Log($"Retrying initialization attempt {retryCount}/{MAX_RETRIES}");
                yield return new WaitForSeconds(1f);
            }
        }

        if (!isInitialized)
        {
            Debug.LogError("Failed to initialize after multiple attempts");
            ShowNotification("Failed to load tournament data. Please try again.");
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("Menu");
        }

        HideLoading();
    }

    private IEnumerator TryInitialize()
    {
        // Get necessary data
        tournamentId = PlayerPrefs.GetString("TournamentId");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        if (string.IsNullOrEmpty(tournamentId) || string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogError("Missing required data (tournamentId or username)");
            yield break;
        }

        // Initialize Firebase reference
        tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        // Load initial tournament data
        var loadTask = tournamentRef.GetValueAsync();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogError($"Failed to load tournament: {loadTask.Exception}");
            yield break;
        }

        if (!loadTask.Result.Exists)
        {
            Debug.LogError("Tournament data not found");
            yield break;
        }

        // Setup successful
        isInitialized = true;
        SetupUI();
        yield return StartCoroutine(LoadTournamentBracket());

        // Check for tournament winner
        if (loadTask.Result.Child("won").Exists)
        {
            ShowSummaryButton();
        }
    }

    private void SetupUI()
    {
        // Setup tournament name
        if (tournamentNameText != null)
        {
            tournamentNameText.text = "Tournament: " + PlayerPrefs.GetString("TournamentName", "Unnamed Tournament");
        }

        // Setup buttons
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
        }

        if (enterLobbyButton != null)
        {
            enterLobbyButton.onClick.AddListener(() => SoundOnClick(OnEnterLobbyButtonClicked));
            enterLobbyButton.gameObject.SetActive(false);
        }

        if (sumTournament != null)
        {
            sumTournament.onClick.AddListener(() => SoundOnClick(OnClickSummaryButton));
            sumTournament.gameObject.SetActive(false);
        }

        // Setup Firebase listeners
        tournamentRef.Child("bracket").ValueChanged += OnBracketDataChanged;
        tournamentRef.Child("won").ValueChanged += OnWonNodeChanged;
    }

    private IEnumerator LoadTournamentBracket()
    {
        ShowLoading("Loading tournament bracket...");

        var task = tournamentRef.Child("bracket").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to load tournament data: {task.Exception}");
            yield break;
        }

        var bracketSnapshot = task.Result;
        if (!bracketSnapshot.Exists || !bracketSnapshot.HasChildren)
        {
            Debug.LogWarning("Bracket data not found or empty.");
            yield break;
        }

        CreateBracketUI(bracketSnapshot);
        CheckCurrentUserMatch(bracketSnapshot);
        
        HideLoading();
    }

    private void CreateBracketUI(DataSnapshot bracketSnapshot)
    {
        foreach (Transform child in bracketContainer)
        {
            Destroy(child.gameObject);
        }
        matches.Clear();

        foreach (var matchSnapshot in bracketSnapshot.Children)
        {
            string matchId = matchSnapshot.Key;
            Dictionary<string, object> matchData = (Dictionary<string, object>)matchSnapshot.Value;

            GameObject matchObj = Instantiate(matchPrefab, bracketContainer);
            MatchUI matchUI = matchObj.GetComponent<MatchUI>();
            if (matchUI != null)
            {
                matchUI.Initialize(matchId, matchData, currentUsername);
                matches[matchId] = matchUI;
            }
        }

        ArrangeBracketUI();
    }

    private void ArrangeBracketUI()
    {
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 4);
        foreach (var match in matches.Values)
        {
            string[] matchInfo = match.matchId.Split('_');
            if (matchInfo.Length >= 4)
            {
                if (int.TryParse(matchInfo[1], out int round) && int.TryParse(matchInfo[3], out int matchNumber))
                {
                    RectTransform rectTransform = match.GetComponent<RectTransform>();
                    ArrangeMatchPosition(rectTransform, round, matchNumber, playerCount);
                }
            }
        }
    }

    private void ArrangeMatchPosition(RectTransform rectTransform, int round, int matchNumber, int playerCount)
    {
        // Position calculations based on playerCount (4 or 8)
        if (playerCount == 4)
        {
            if (round == 0) // semi-finals
            {
                rectTransform.anchoredPosition = matchNumber == 0 ? 
                    new Vector2(0f, 115f) : new Vector2(0f, -195f);
            }
            else if (round == 1) // final
            {
                rectTransform.anchoredPosition = new Vector2(640f, -40f);
            }
        }
        else if (playerCount == 8)
        {
            if (round == 0) // quarter-finals
            {
                float yPos = 190f - (matchNumber * 150f);
                rectTransform.anchoredPosition = new Vector2(-640f, yPos);
            }
            else if (round == 1) // semi-finals
            {
                rectTransform.anchoredPosition = matchNumber == 0 ? 
                    new Vector2(0f, 115f) : new Vector2(0f, -195f);
            }
            else if (round == 2) // final
            {
                rectTransform.anchoredPosition = new Vector2(640f, -40f);
            }
        }
    }

    private void CheckCurrentUserMatch(DataSnapshot bracketSnapshot)
    {
        string latestMatchId = null;
        bool isPlayerInActiveTournament = false;

        var sortedMatches = bracketSnapshot.Children.OrderByDescending(match =>
            int.Parse(match.Key.Split('_')[1]));

        foreach (var matchSnapshot in sortedMatches)
        {
            Dictionary<string, object> matchData = matchSnapshot.Value as Dictionary<string, object>;
            if (matchData == null) continue;

            Dictionary<string, object> player1Data = matchData["player1"] as Dictionary<string, object>;
            Dictionary<string, object> player2Data = matchData["player2"] as Dictionary<string, object>;

            if (player1Data != null && player2Data != null)
            {
                string player1Username = player1Data["username"] as string;
                string player2Username = player2Data["username"] as string;

                if (player1Username == currentUsername || player2Username == currentUsername)
                {
                    if (!matchData.ContainsKey("winner") || string.IsNullOrEmpty(matchData["winner"] as string))
                    {
                        latestMatchId = matchSnapshot.Key;
                        isPlayerInActiveTournament = true;
                        break;
                    }
                }
            }
        }

        currentMatchId = latestMatchId;
        if (enterLobbyButton != null)
        {
            enterLobbyButton.gameObject.SetActive(isPlayerInActiveTournament);
        }

        ShowNotification(isPlayerInActiveTournament ? 
            "You have an active match. Click 'Enter Lobby' to continue." : 
            "Your tournament has ended.");
    }

    private void OnBracketDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (this == null) return;

        if (args.DatabaseError != null)
        {
            Debug.LogError($"Failed to read bracket data: {args.DatabaseError.Message}");
            return;
        }

        if (updateUICoroutine != null)
        {
            StopCoroutine(updateUICoroutine);
        }

        updateUICoroutine = StartCoroutine(UpdateUINextFrame(args.Snapshot));
    }

    private void OnWonNodeChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Failed to read won data: {args.DatabaseError.Message}");
            return;
        }

        if (args.Snapshot.Exists)
        {
            string winnerUsername = args.Snapshot.Value as string;
            if (currentUsername == winnerUsername && sumTournament != null)
            {
                ShowSummaryButton();
            }
            else if (sumTournament != null)
            {
                sumTournament.gameObject.SetActive(false);
            }
        }
        else if (sumTournament != null)
        {
            sumTournament.gameObject.SetActive(false);
        }
    }

    private IEnumerator UpdateUINextFrame(DataSnapshot snapshot)
    {
        yield return null;
        if (this == null) yield break;

        foreach (var childSnapshot in snapshot.Children)
        {
            string matchId = childSnapshot.Key;
            if (matches.TryGetValue(matchId, out MatchUI matchUI))
            {
                matchUI.UpdateMatchData((Dictionary<string, object>)childSnapshot.Value);
            }
        }

        CheckCurrentUserMatch(snapshot);
    }

    private void OnEnterLobbyButtonClicked()
    {
        if (!string.IsNullOrEmpty(currentMatchId))
        {
            PlayerPrefs.SetString("CurrentMatchId", currentMatchId);
            SceneManager.LoadScene("MatchLobby");
        }
        else
        {
            Debug.LogWarning("No active match found for the current user.");
            ShowNotification("No active match found.");
        }
    }

    private void ShowSummaryButton()
    {
        if (sumTournament != null)
        {
            sumTournament.gameObject.SetActive(true);
        }
    }

    private void OnClickSummaryButton()
    {
        SceneManager.LoadScene("SumTournament");
    }

    private void ShowLoading(string message = "Loading...")
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }
    }

    private void HideLoading()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationPopup.SetActive(true);
            StartCoroutine(HideNotificationAfterDelay(3f));
        }
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notificationPopup != null)
        {
            notificationPopup.SetActive(false);
        }
    }

    private void SoundOnClick(System.Action buttonAction)
    {
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TournamentBracket")
        {
            if (!isInitialized)
            {
                StartCoroutine(InitializeWithRetry());
            }
        }
    }

    private void OnDestroy()
    {
        if (tournamentRef != null)
        {
            tournamentRef.Child("bracket").ValueChanged -= OnBracketDataChanged;
            tournamentRef.Child("won").ValueChanged -= OnWonNodeChanged;
        }

        // Clean up button listeners
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }

        if (enterLobbyButton != null)
        {
            enterLobbyButton.onClick.RemoveAllListeners();
        }

        if (sumTournament != null)
        {
            sumTournament.onClick.RemoveAllListeners();
        }

        // Clean up scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Stop any running coroutines
        if (updateUICoroutine != null)
        {
            StopCoroutine(updateUICoroutine);
        }

        // Clear matches dictionary
        matches.Clear();
    }
}
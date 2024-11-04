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
        var tournamentsRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments");
        var task = tournamentsRef.GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to load tournaments: {task.Exception}");
            yield break;
        }

        DataSnapshot snapshot = task.Result;
        currentUsername = AuthManager.Instance.GetCurrentUsername();
        bool foundTournament = false;

        foreach (var tournamentSnapshot in snapshot.Children)
        {
            var bracketData = tournamentSnapshot.Child("bracket");
            foreach (var matchData in bracketData.Children)
            {
                var player1Username = matchData.Child("player1").Child("username").Value?.ToString();
                var player2Username = matchData.Child("player2").Child("username").Value?.ToString();

                if (player1Username == currentUsername || player2Username == currentUsername)
                {
                    tournamentId = tournamentSnapshot.Key;
                    PlayerPrefs.SetString("TournamentId", tournamentId);
                    PlayerPrefs.SetString("TournamentName", tournamentSnapshot.Child("name").Value?.ToString());
                    PlayerPrefs.SetInt("PlayerCount", int.Parse(tournamentSnapshot.Child("playerCount").Value.ToString()));
                    PlayerPrefs.Save();

                    foundTournament = true;
                    break;
                }
            }
            if (foundTournament) break;
        }

        if (!foundTournament)
        {
            Debug.LogError("Could not find tournament for current user");
            yield break;
        }

        tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);
        
        SetupUI();
        yield return StartCoroutine(LoadTournamentBracket());

        if (task.Result.Child("won").Exists)
        {
            ShowSummaryButton();
        }

        isInitialized = true;
    }

    private void SetupUI()
    {
        if (tournamentNameText != null)
        {
            tournamentNameText.text = "Tournament: " + PlayerPrefs.GetString("TournamentName", "Unnamed Tournament");
        }

        backButton?.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
        enterLobbyButton?.onClick.AddListener(() => SoundOnClick(OnEnterLobbyButtonClicked));
        sumTournament?.onClick.AddListener(() => SoundOnClick(OnClickSummaryButton));

        if (enterLobbyButton != null)
            enterLobbyButton.gameObject.SetActive(false);
        if (sumTournament != null)
            sumTournament.gameObject.SetActive(false);

        tournamentRef.Child("bracket").ValueChanged += OnBracketDataChanged;
        tournamentRef.Child("won").ValueChanged += OnWonNodeChanged;
    }

    private IEnumerator LoadTournamentBracket()
    {
        ShowLoading("Loading tournament bracket...");

        var tournamentTask = tournamentRef.GetValueAsync();
        yield return new WaitUntil(() => tournamentTask.IsCompleted);

        if (tournamentTask.Exception != null)
        {
            Debug.LogError($"Failed to load tournament data: {tournamentTask.Exception}");
            yield break;
        }

        var tournamentData = tournamentTask.Result;
        if (!tournamentData.Exists)
        {
            Debug.LogError("Tournament data not found");
            yield break;
        }

        var bracketSnapshot = tournamentData.Child("bracket");
        if (!bracketSnapshot.Exists)
        {
            Debug.LogError("Bracket data not found");
            yield break;
        }

        CreateBracketUI(bracketSnapshot);
        SyncMatchData(bracketSnapshot);
        CheckCurrentUserMatch(bracketSnapshot);
        
        HideLoading();
    }

    private void CreateBracketUI(DataSnapshot bracketSnapshot)
    {
        // Clear existing matches
        foreach (Transform child in bracketContainer)
        {
            Destroy(child.gameObject);
        }
        matches.Clear();

        // Create new match UIs
        foreach (var matchSnapshot in bracketSnapshot.Children)
        {
            string matchId = matchSnapshot.Key;
            Dictionary<string, object> matchData = new Dictionary<string, object>();

            // Extract match data
            foreach (var child in matchSnapshot.Children)
            {
                matchData[child.Key] = child.Value;
            }

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

    private void SyncMatchData(DataSnapshot bracketSnapshot)
    {
        Dictionary<string, Dictionary<string, object>> bracketData = new Dictionary<string, Dictionary<string, object>>();

        foreach (var matchSnapshot in bracketSnapshot.Children)
        {
            string matchId = matchSnapshot.Key;
            Dictionary<string, object> matchData = new Dictionary<string, object>();

            // Clone player1 data
            var player1Data = matchSnapshot.Child("player1");
            if (player1Data.Exists)
            {
                matchData["player1"] = new Dictionary<string, object>
                {
                    { "username", player1Data.Child("username").Value?.ToString() ?? "" },
                    { "inLobby", player1Data.Child("inLobby").Value ?? false },
                    { "isPlaying", player1Data.Child("isPlaying").Value ?? false },
                    { "hasCompleted", player1Data.Child("hasCompleted").Value ?? false }
                };
            }

            // Clone player2 data
            var player2Data = matchSnapshot.Child("player2");
            if (player2Data.Exists)
            {
                matchData["player2"] = new Dictionary<string, object>
                {
                    { "username", player2Data.Child("username").Value?.ToString() ?? "" },
                    { "inLobby", player2Data.Child("inLobby").Value ?? false },
                    { "isPlaying", player2Data.Child("isPlaying").Value ?? false },
                    { "hasCompleted", player2Data.Child("hasCompleted").Value ?? false }
                };
            }

            // Add winner and nextMatchId if they exist
            if (matchSnapshot.Child("winner").Exists)
            {
                matchData["winner"] = matchSnapshot.Child("winner").Value.ToString();
            }
            if (matchSnapshot.Child("nextMatchId").Exists)
            {
                matchData["nextMatchId"] = matchSnapshot.Child("nextMatchId").Value.ToString();
            }

            bracketData[matchId] = matchData;
        }

        // Update all matches
        foreach (var matchPair in bracketData)
        {
            if (matches.TryGetValue(matchPair.Key, out MatchUI matchUI))
            {
                matchUI.UpdateMatchData(matchPair.Value);
            }
        }
    }

    private void OnBracketDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (this == null) return;

        if (args.DatabaseError != null)
        {
            Debug.LogError($"Failed to read bracket data: {args.DatabaseError.Message}");
            return;
        }

        StartCoroutine(HandleBracketDataChange(args.Snapshot));
    }

    private IEnumerator HandleBracketDataChange(DataSnapshot bracketSnapshot)
    {
        yield return null;
        if (this == null) yield break;

        try
        {
            SyncMatchData(bracketSnapshot);
            CheckCurrentUserMatch(bracketSnapshot);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error handling bracket data change: {e}");
        }
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

    private void CheckCurrentUserMatch(DataSnapshot bracketSnapshot)
    {
        string latestMatchId = null;
        bool isPlayerInActiveTournament = false;
        bool isMatchActive = false;

        var sortedMatches = bracketSnapshot.Children.OrderByDescending(match =>
            int.Parse(match.Key.Split('_')[1]));

        foreach (var matchSnapshot in sortedMatches)
        {
            var player1Username = matchSnapshot.Child("player1/username").Value?.ToString();
            var player2Username = matchSnapshot.Child("player2/username").Value?.ToString();
            var winner = matchSnapshot.Child("winner").Value?.ToString();

            bool isUserInMatch = (player1Username == currentUsername || player2Username == currentUsername);
            bool isMatchComplete = !string.IsNullOrEmpty(winner);

            if (isUserInMatch)
            {
                isPlayerInActiveTournament = true;
                if (!isMatchComplete)
                {
                    latestMatchId = matchSnapshot.Key;
                    isMatchActive = true;
                    break;
                }
            }
        }

        currentMatchId = latestMatchId;
        
        if (enterLobbyButton != null)
        {
            enterLobbyButton.gameObject.SetActive(isMatchActive);
        }

        string message = isMatchActive ? 
            "You have an active match. Click 'Enter Lobby' to continue." :
            isPlayerInActiveTournament ? 
                "Your tournament progress is complete for now." : 
                "You are not currently in any active matches.";
                
        ShowNotification(message);
    }

    private void OnEnterLobbyButtonClicked()
    {
        if (!string.IsNullOrEmpty(currentMatchId))
        {
            PlayerPrefs.SetString("CurrentMatchId", currentMatchId);
            PlayerPrefs.Save();
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
        SceneManager.LoadScene("SumT");
    }

    public void UpdateMatchState(string matchId, Dictionary<string, object> newState)
    {
        if (string.IsNullOrEmpty(matchId) || newState == null) return;

        tournamentRef.Child("bracket").Child(matchId).UpdateChildrenAsync(newState)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to update match state: {task.Exception}");
                }
            });
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

    protected virtual void OnDestroy()
    {
        // Cleanup Firebase listeners
        if (tournamentRef != null)
        {
            tournamentRef.Child("bracket").ValueChanged -= OnBracketDataChanged;
            tournamentRef.Child("won").ValueChanged -= OnWonNodeChanged;
        }

        // Clean up button listeners
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
        if (enterLobbyButton != null)
            enterLobbyButton.onClick.RemoveAllListeners();
        if (sumTournament != null)
            sumTournament.onClick.RemoveAllListeners();

        // Clean up scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Clean up matches
        foreach (var match in matches.Values)
        {
            if (match != null)
            {
                Destroy(match.gameObject);
            }
        }
        matches.Clear();

        // Stop all coroutines
        StopAllCoroutines();
    }
}
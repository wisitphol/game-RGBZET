using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class TournamentBracketUI : MonoBehaviourPunCallbacks
{
    public GameObject matchPrefab;
    public Transform bracketContainer;
    public TMP_Text tournamentNameText;
    [SerializeField] public Button backButton;
    [SerializeField] public Button enterLobbyButton;
    [SerializeField] public Button sumTournament;
    private int playerCount;
    private DatabaseReference tournamentRef;
    private string tournamentId;
    private string currentUsername;
    private Dictionary<string, MatchUI> matches = new Dictionary<string, MatchUI>();
    private string currentUserMatchId;
    private Coroutine updateUICoroutine;
    private string winnerUsername; // ตัวแปรสำหรับเก็บชื่อผู้ชนะ
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;


    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(DisconnectFromPhoton());
        }
        else
        {
            InitializeTournamentBracket();
        }

        if (sumTournament != null)
        {

            sumTournament.onClick.AddListener(() => SoundOnClick(OnClickSummaryButton));
            sumTournament.gameObject.SetActive(false);
            Debug.Log("have sumbutton");
        }

    }

    IEnumerator DisconnectFromPhoton()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Debug.Log("Disconnected from Photon");
        InitializeTournamentBracket();
    }

    void InitializeTournamentBracket()
    {
        tournamentId = PlayerPrefs.GetString("TournamentId");
        string tournamentName = PlayerPrefs.GetString("TournamentName", "Unnamed Tournament");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        if (tournamentNameText != null)
        {
            tournamentNameText.text = "Tournament: " + tournamentName;
        }

        tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        if (backButton != null)
        {
            backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
        }
        else
        {
            Debug.LogWarning("Back button is missing or not assigned in the Inspector.");
        }

        if (enterLobbyButton != null)
        {
            enterLobbyButton.onClick.AddListener(() => SoundOnClick(OnEnterLobbyButtonClicked));
            enterLobbyButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Enter Lobby button is missing or not assigned in the Inspector.");
        }

        if (tournamentRef != null)
        {
            tournamentRef.Child("bracket").ValueChanged += OnBracketDataChanged;
        }

        tournamentRef.Child("won").ValueChanged += OnWonNodeChanged;


        Debug.Log("Checking tournament data...");
        tournamentRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var tournamentData = task.Result;
                Debug.Log("Firebase tournament data exists.");

                // เปลี่ยนเงื่อนไขตรงนี้ เพียงแค่ตรวจสอบว่ามีโหนด "won" หรือไม่
                if (tournamentData.HasChild("won"))
                {
                    ShowSummaryButton(); // แสดงปุ่มทันที
                    Debug.Log("Found 'won' node in tournament data. Showing summary button.");
                }
                else
                {
                    Debug.LogWarning("No 'won' field in tournament data.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve tournament data.");
            }
        });

        StartCoroutine(LoadTournamentBracket());

    }

    IEnumerator LoadTournamentBracket()
    {
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
    }

    void CreateBracketUI(DataSnapshot bracketSnapshot)
    {
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

    void ArrangeBracketUI()
    {
        int playerCount = GetPlayerCount(); // ตรวจสอบจำนวนผู้เล่น (4 หรือ 8 คน)
        Debug.Log("Player Count: " + playerCount);

        foreach (var match in matches.Values)
        {
            // ดึง matchId ออกมา เช่น round_0_match_0
            string matchId = match.matchId;
            Debug.Log("Match ID: " + matchId);

            // แยก matchId เป็น round และ match number
            string[] matchInfo = matchId.Split('_');

            if (matchInfo.Length >= 4)
            {
                if (int.TryParse(matchInfo[1], out int round) && int.TryParse(matchInfo[3], out int matchNumber))
                {
                    RectTransform rectTransform = match.GetComponent<RectTransform>();
                    Debug.Log($"Arranging match: Round {round}, MatchNumber {matchNumber}");

                    if (playerCount == 4)
                    {
                        if (round == 0) // semi-final
                        {
                            if (matchNumber == 0)
                            {
                                rectTransform.anchoredPosition = new Vector2(0f, 115f);
                                Debug.Log($"Semi-final match 1 placed at (0f, 115f)");
                            }
                            else if (matchNumber == 1)
                            {
                                rectTransform.anchoredPosition = new Vector2(0f, -195f);
                                Debug.Log($"Semi-final match 2 placed at (0f, -195f)");
                            }
                        }
                        else if (round == 1) // final
                        {
                            rectTransform.anchoredPosition = new Vector2(640f, -40f);
                            Debug.Log("Final match placed at (640f, -40f)");
                        }
                    }
                    else if (playerCount == 8)
                    {
                        if (round == 0) // quarterfinals
                        {
                            if (matchNumber == 0)
                            {
                                rectTransform.anchoredPosition = new Vector2(-640f, 190f);
                                Debug.Log("Quarterfinal match 1 placed at (-640f, 190f)");
                            }
                            else if (matchNumber == 1)
                            {
                                rectTransform.anchoredPosition = new Vector2(-640f, 40f);
                                Debug.Log("Quarterfinal match 2 placed at (-640f, 40f)");
                            }
                            else if (matchNumber == 2)
                            {
                                rectTransform.anchoredPosition = new Vector2(-640f, -120f);
                                Debug.Log("Quarterfinal match 3 placed at (-640f, -120f)");
                            }
                            else if (matchNumber == 3)
                            {
                                rectTransform.anchoredPosition = new Vector2(-640f, -270f);
                                Debug.Log("Quarterfinal match 4 placed at (-640f, -270f)");
                            }
                        }
                        else if (round == 1) // semi-final
                        {
                            if (matchNumber == 0)
                            {
                                rectTransform.anchoredPosition = new Vector2(0f, 115f);
                                Debug.Log($"Semi-final match 1 placed at (0f, 115f)");
                            }
                            else if (matchNumber == 1)
                            {
                                rectTransform.anchoredPosition = new Vector2(0f, -195f);
                                Debug.Log($"Semi-final match 2 placed at (0f, -195f)");
                            }
                        }
                        else if (round == 2) // final
                        {
                            rectTransform.anchoredPosition = new Vector2(640f, -40f);
                            Debug.Log("Final match placed at (640f, -40f)");
                        }
                    }
                }
            }
        }
    }


    int GetPlayerCount()
    {
        // ฟังก์ชันคืนค่าจำนวนผู้เล่นในทัวร์นาเมนต์ (4 หรือ 8 คน)
        return PlayerPrefs.GetInt("PlayerCount", playerCount); // เริ่มต้นที่ 4 แต่สามารถเปลี่ยนเป็น 8 ได้
    }


    void CheckCurrentUserMatch(DataSnapshot bracketSnapshot)
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

        currentUserMatchId = latestMatchId;

        if (enterLobbyButton != null)
        {
            enterLobbyButton.gameObject.SetActive(isPlayerInActiveTournament);
        }

        if (isPlayerInActiveTournament)
        {
            DisplayFeedback("You have an active match. Click 'Enter Lobby' to continue.");
        }
        else if (latestMatchId == null)
        {
            DisplayFeedback("Your tournament has ended.");
        }
    }

    void OnEnterLobbyButtonClicked()
    {
        if (!string.IsNullOrEmpty(currentUserMatchId))
        {
            PlayerPrefs.SetString("CurrentMatchId", currentUserMatchId);
            SceneManager.LoadScene("MatchLobby");
        }
        else
        {
            Debug.LogWarning("No active match found for the current user.");
            DisplayFeedback("No active match found.");
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
            winnerUsername = args.Snapshot.Value as string;
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

    public void DisplayFeedback(string message)
    {
        Debug.Log(message); // You can replace this with UI text update if you have a feedback text field
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
    }

    void OnDestroy()
    {
        if (tournamentRef != null)
        {
            tournamentRef.Child("bracket").ValueChanged -= OnBracketDataChanged;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }

        if (enterLobbyButton != null)
        {
            enterLobbyButton.onClick.RemoveAllListeners();
        }
    }

    void ShowSummaryButton()
    {
        Debug.Log("Trying to show summary button.");
        if (sumTournament != null)
        {
            sumTournament.gameObject.SetActive(true);
            Debug.Log("Summary button is now visible.");
        }
        else
        {
            Debug.LogError("sumTournament button is not assigned in the Inspector.");
        }
    }

    void OnClickSummaryButton()
    {
        if (sumTournament != null)
        {
            // ตรวจสอบว่าปุ่มยังอยู่ก่อนเปลี่ยนไปหน้า SumTournament
            SceneManager.LoadScene("SumTournament");
        }
        else
        {
            Debug.LogWarning("Summary button is missing or destroyed.");
        }
    }

    void SoundOnClick(System.Action buttonAction)
    {
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
            // รอให้เสียงเล่นเสร็จก่อนที่จะทำการเปลี่ยน scene
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            // ถ้าไม่มีเสียงให้เล่น ให้ทำงานทันที
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        // รอความยาวของเสียงก่อนที่จะทำงาน
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }
}
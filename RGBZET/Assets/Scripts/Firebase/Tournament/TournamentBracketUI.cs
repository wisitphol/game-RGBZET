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
    public Button backButton;
    public Button enterLobbyButton;

    private DatabaseReference tournamentRef;
    private string tournamentId;
    private string currentUsername;
    private Dictionary<string, MatchUI> matches = new Dictionary<string, MatchUI>();
    private string currentUserMatchId;
    private Coroutine updateUICoroutine;

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
            backButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        }

        if (enterLobbyButton != null)
        {
            enterLobbyButton.onClick.AddListener(OnEnterLobbyButtonClicked);
            enterLobbyButton.gameObject.SetActive(false);
        }

        if (tournamentRef != null)
        {
            tournamentRef.Child("bracket").ValueChanged += OnBracketDataChanged;
        }

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
        float roundWidth = 400f; // ความกว้างของแต่ละรอบ
        float matchHeight = 200f; // ความสูงของแต่ละแมทช์
        float verticalSpacing = 100f; // ระยะห่างระหว่างแต่ละแมทช์ในรอบเดียวกัน

        Dictionary<int, int> roundMatchCounts = new Dictionary<int, int>(); // เก็บจำนวนแมทช์ต่อรอบเพื่อนับลำดับ

        foreach (var match in matches.Values)
        {
            string[] matchInfo = match.matchId.Split('_');
            if (matchInfo.Length >= 4)
            {
                if (int.TryParse(matchInfo[1], out int round) && int.TryParse(matchInfo[3], out int matchNumber))
                {
                    if (!roundMatchCounts.ContainsKey(round))
                    {
                        roundMatchCounts[round] = 0;
                    }

                    int currentMatchIndex = roundMatchCounts[round];
                    float yPosition = -currentMatchIndex * (matchHeight + verticalSpacing); // คำนวณตำแหน่ง Y ของแต่ละแมทช์ในรอบ
                    float xPosition = round * roundWidth; // คำนวณตำแหน่ง X ของแต่ละรอบ

                    RectTransform rectTransform = match.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
                    }

                    roundMatchCounts[round]++;
                }
            }
        }
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
}
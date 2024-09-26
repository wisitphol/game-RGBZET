using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class TournamentBracketUI : MonoBehaviour
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

    void Start()
    {
        // Disconnect from Photon if connected
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        tournamentId = PlayerPrefs.GetString("TournamentId");
        string tournamentName = PlayerPrefs.GetString("TournamentName", "Unnamed Tournament");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        tournamentNameText.text = "Tournament: " + tournamentName;
        tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        backButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        enterLobbyButton.onClick.AddListener(OnEnterLobbyButtonClicked);
        enterLobbyButton.gameObject.SetActive(true);  // Always show the button

        // Move the ValueChanged registration here
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
        float xOffset = 500f;
        float yOffset = 500f;
        foreach (var match in matches.Values)
        {
            string[] matchInfo = match.matchId.Split('_');
            int round = int.Parse(matchInfo[1]);
            int matchNumber = int.Parse(matchInfo[3]);
            
            RectTransform rectTransform = match.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(round * xOffset, -matchNumber * yOffset);
        }
    }

    void CheckCurrentUserMatch(DataSnapshot bracketSnapshot)
    {
        foreach (var matchSnapshot in bracketSnapshot.Children)
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
                    currentUserMatchId = matchSnapshot.Key;
                    return;
                }
            }
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
        }
    }

    void OnDisable()
    {
        if (tournamentRef != null)
        {
            tournamentRef.Child("bracket").ValueChanged -= OnBracketDataChanged;
        }
    }

    private void OnBracketDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Failed to read bracket data: {args.DatabaseError.Message}");
            return;
        }

        foreach (var childSnapshot in args.Snapshot.Children)
        {
            string matchId = childSnapshot.Key;
            if (matches.TryGetValue(matchId, out MatchUI matchUI))
            {
                matchUI.UpdateMatchData((Dictionary<string, object>)childSnapshot.Value);
            }
        }

        CheckCurrentUserMatch(args.Snapshot);
    }
}

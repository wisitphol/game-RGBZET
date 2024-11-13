using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using System.Linq;

public class EndGameT : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    [SerializeField] private Button backToMenu;
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip endgameSound;
    [SerializeField] private AudioClip buttonSound;

    private GameObject[] playerObjects;
    private PlayerResultT[] playerResults;
    private DatabaseReference databaseReference;
    private string tournamentId;
    private string currentMatchId;
    private Player winningPlayer;
    private bool processingResults = false;

    void Start()
    {
        InitializeUI();
        SetupDatabase();
        InitializePlayers();
        PlayEndGameSound();
        StartCoroutine(ProcessMatchResults());
    }

    private void InitializeUI()
    {
        backToMenu.interactable = false;
        nextRoundButton.gameObject.SetActive(false);
        StartCoroutine(EnableBackButtonAfterDelay(3f));
        
        backToMenu.onClick.AddListener(() => SoundOnClick(OnBackToMenuButtonClicked));
        nextRoundButton.onClick.AddListener(() => SoundOnClick(OnNextRoundButtonClicked));

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
    }

    private void SetupDatabase()
    {
        tournamentId = PlayerPrefs.GetString("TournamentId");
        currentMatchId = PlayerPrefs.GetString("CurrentMatchId");
        databaseReference = FirebaseDatabase.DefaultInstance
            .GetReference("tournaments")
            .Child(tournamentId);
    }

    private void InitializePlayers()
    {
        playerObjects = new GameObject[] { player1, player2 };
        playerResults = new PlayerResultT[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] != null)
            {
                playerResults[i] = playerObjects[i].GetComponent<PlayerResultT>();
                if (playerResults[i] == null)
                {
                    Debug.LogWarning($"PlayerResultT component missing on player {i + 1}");
                }
            }
        }
    }

    private void PlayEndGameSound()
    {
        if (audioSource != null && endgameSound != null)
        {
            audioSource.PlayOneShot(endgameSound);
        }
    }

    private IEnumerator ProcessMatchResults()
    {
        ShowLoading("Processing match results...");
        processingResults = true;

        yield return StartCoroutine(FetchAndDisplayResults());
        
        if (PhotonNetwork.IsMasterClient)
        {
            yield return StartCoroutine(UpdateGameResults());
        }

        processingResults = false;
        HideLoading();
    }

    private IEnumerator FetchAndDisplayResults()
    {
        var matchTask = databaseReference
            .Child("bracket")
            .Child(currentMatchId)
            .GetValueAsync();

        yield return new WaitUntil(() => matchTask.IsCompleted);

        if (matchTask.Exception != null)
        {
            Debug.LogError($"Failed to fetch match data: {matchTask.Exception}");
            yield break;
        }

        var matchData = matchTask.Result;
        if (!matchData.Exists)
        {
            Debug.LogError("Match data not found");
            yield break;
        }

        FetchPlayerDataFromPhoton();
    }

    private void FetchPlayerDataFromPhoton()
    {
        foreach (var playerObject in playerObjects)
        {
            if (playerObject != null)
            {
                playerObject.SetActive(false);
            }
        }

        Player[] players = PhotonNetwork.PlayerList;
        int index = 0;
        int highestScore = int.MinValue;
        List<int> highestScoreIndices = new List<int>();

        foreach (Player player in players)
        {
            if (index >= playerResults.Length) break;

            string playerName = player.CustomProperties.ContainsKey("username") ? 
                player.CustomProperties["username"].ToString() : player.NickName;
            string playerScoreStr = player.CustomProperties.ContainsKey("score") ? 
                player.CustomProperties["score"].ToString() : "0";

            int playerScore = ParseScore(playerScoreStr);
            
            UpdatePlayerDisplay(index, player, playerName, playerScore);

            if (playerScore > highestScore)
            {
                highestScore = playerScore;
                highestScoreIndices.Clear();
                highestScoreIndices.Add(index);
                winningPlayer = player;
            }
            else if (playerScore == highestScore)
            {
                highestScoreIndices.Add(index);
            }

            index++;
        }

        HandleWinner(highestScoreIndices);
    }

    private int ParseScore(string scoreStr)
    {
        string cleanScoreStr = System.Text.RegularExpressions.Regex.Replace(scoreStr, @"\D", "");
        if (!int.TryParse(cleanScoreStr, out int score))
        {
            Debug.LogError($"Failed to parse score '{scoreStr}'. Defaulting to 0.");
            return 0;
        }
        return score;
    }

    private void UpdatePlayerDisplay(int index, Player player, string playerName, int playerScore)
    {
        if (playerResults[index] != null)
        {
            playerResults[index].UpdatePlayerResult(
                playerName,
                "Score: " + playerScore.ToString(),
                ""
            );

            if (player.CustomProperties.ContainsKey("iconId"))
            {
                int iconId = (int)player.CustomProperties["iconId"];
                playerResults[index].UpdatePlayerIcon(iconId);
            }

            playerObjects[index].SetActive(true);
        }
    }

    private void HandleWinner(List<int> highestScoreIndices)
    {
        if (highestScoreIndices.Count == 1)
        {
            int winnerIndex = highestScoreIndices[0];
            playerResults[winnerIndex].UpdatePlayerResult(
                playerResults[winnerIndex].NameText.text,
                playerResults[winnerIndex].ScoreText.text,
                "WIN"
            );

            if (PhotonNetwork.LocalPlayer == winningPlayer)
            {
                CheckNextMatch();
            }
        }
    }

    private bool IsValidNextMatch(string currentMatchId, string nextMatchId)
    {
        string[] currentParts = currentMatchId.Split('_');
        string[] nextParts = nextMatchId.Split('_');
        
        if (currentParts.Length >= 4 && nextParts.Length >= 4)
        {
            int currentRound = int.Parse(currentParts[1]);
            int currentMatch = int.Parse(currentParts[3]);
            int nextRound = int.Parse(nextParts[1]);
            int nextMatch = int.Parse(nextParts[3]);
            
            return nextRound == currentRound + 1 && nextMatch == currentMatch / 2;
        }
        
        return false;
    }

    private IEnumerator UpdateGameResults()
    {
        if (!PhotonNetwork.IsMasterClient || winningPlayer == null)
        {
            yield break;
        }

        string winnerUsername = winningPlayer.CustomProperties["username"].ToString();
        Debug.Log($"Starting match update process for winner: {winnerUsername}");

        yield return StartCoroutine(UpdateMatchAndAdvanceWinner(winnerUsername));
    }

    private IEnumerator UpdateMatchAndAdvanceWinner(string winnerUsername)
    {
        DatabaseReference matchRef = databaseReference
            .Child("bracket")
            .Child(currentMatchId);

        // Update current match first
        var updateCurrentMatchTask = matchRef.UpdateChildrenAsync(new Dictionary<string, object>
        {
            { "winner", winnerUsername },
            { "player1/hasCompleted", true },
            { "player2/hasCompleted", true }
        });

        yield return new WaitUntil(() => updateCurrentMatchTask.IsCompleted);

        if (updateCurrentMatchTask.Exception != null)
        {
            Debug.LogError($"Failed to update match result: {updateCurrentMatchTask.Exception}");
            yield break;
        }

        // Get nextMatchId
        var nextMatchTask = matchRef.Child("nextMatchId").GetValueAsync();
        yield return new WaitUntil(() => nextMatchTask.IsCompleted);

        if (nextMatchTask.Exception != null)
        {
            Debug.LogError($"Failed to get next match ID: {nextMatchTask.Exception}");
            yield break;
        }

        string nextMatchId = nextMatchTask.Result.Value?.ToString();

        if (!string.IsNullOrEmpty(nextMatchId) && nextMatchId != "victory")
        {
            if (!IsValidNextMatch(currentMatchId, nextMatchId))
            {
                Debug.LogError($"Invalid next match: {nextMatchId} for current match: {currentMatchId}");
                yield break;
            }

            DatabaseReference nextMatchRef = databaseReference
                .Child("bracket")
                .Child(nextMatchId);

            var nextMatchDataTask = nextMatchRef.GetValueAsync();
            yield return new WaitUntil(() => nextMatchDataTask.IsCompleted);

            if (nextMatchDataTask.Exception != null)
            {
                Debug.LogError($"Failed to get next match data: {nextMatchTask.Exception}");
                yield break;
            }

            var nextMatchData = nextMatchDataTask.Result;
            string player1Username = nextMatchData.Child("player1").Child("username").Value?.ToString();
            bool shouldBePlayer1 = string.IsNullOrEmpty(player1Username);

            var playerUpdate = new Dictionary<string, object>
            {
                { shouldBePlayer1 ? "player1" : "player2", new Dictionary<string, object>
                    {
                        { "username", winnerUsername },
                        { "inLobby", false },
                        { "isPlaying", false },
                        { "hasCompleted", false }
                    }
                }
            };

            var updateNextMatchTask = nextMatchRef.UpdateChildrenAsync(playerUpdate);
            yield return new WaitUntil(() => updateNextMatchTask.IsCompleted);

            if (updateNextMatchTask.Exception != null)
            {
                Debug.LogError($"Failed to update next match: {updateNextMatchTask.Exception}");
            }
        }
        else if (nextMatchId == "victory")
        {
            var updateTournamentTask = databaseReference.Child("won").SetValueAsync(winnerUsername);
            yield return new WaitUntil(() => updateTournamentTask.IsCompleted);

            if (updateTournamentTask.Exception != null)
            {
                Debug.LogError($"Failed to update tournament winner: {updateTournamentTask.Exception}");
            }

            yield return StartCoroutine(UpdatePlayerTournamentStats(winnerUsername));
        }
    }

    private void CheckNextMatch()
    {
        StartCoroutine(CheckNextMatchCoroutine());
    }

    private IEnumerator CheckNextMatchCoroutine()
    {
        var nextMatchTask = databaseReference
            .Child("bracket")
            .Child(currentMatchId)
            .Child("nextMatchId")
            .GetValueAsync();

        yield return new WaitUntil(() => nextMatchTask.IsCompleted);

        if (nextMatchTask.Exception != null)
        {
            Debug.LogError($"Failed to check next match: {nextMatchTask.Exception}");
            yield break;
        }

        string nextMatchId = nextMatchTask.Result.Value?.ToString();
        if (!string.IsNullOrEmpty(nextMatchId) && nextMatchId != "victory")
        {
            nextRoundButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator UpdatePlayerTournamentStats(string winnerUsername)
    {
        var userQuery = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .OrderByChild("username")
            .EqualTo(winnerUsername)
            .LimitToFirst(1)
            .GetValueAsync();

        yield return new WaitUntil(() => userQuery.IsCompleted);

        if (userQuery.Exception != null)
        {
            Debug.LogError($"Failed to find winner user data: {userQuery.Exception}");
            yield break;
        }

        var snapshot = userQuery.Result;
        if (snapshot.ChildrenCount > 0)
        {
            var userId = snapshot.Children.First().Key;
            var statsRef = FirebaseDatabase.DefaultInstance
                .GetReference("users")
                .Child(userId)
                .Child("gameswintournament");

            var currentStatsTask = statsRef.GetValueAsync();
            yield return new WaitUntil(() => currentStatsTask.IsCompleted);

            if (currentStatsTask.Exception == null && currentStatsTask.Result.Exists)
            {
                int currentWins = int.Parse(currentStatsTask.Result.Value.ToString());
                yield return statsRef.SetValueAsync(currentWins + 1);
            }
        }
    }

    private void OnNextRoundButtonClicked()
    {
        if (processingResults)
        {
            return;
        }
        SceneManager.LoadScene("TournamentBracket");
    }

    private void OnBackToMenuButtonClicked()
    {
        if (processingResults)
        {
            return;
        }
        StartCoroutine(LeaveGameAndDisconnect());
    }

    private IEnumerator LeaveGameAndDisconnect()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        }

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        }

        SceneManager.LoadScene("TournamentBracket");
    }

    private void ShowLoading(string message)
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

    private IEnumerator EnableBackButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (backToMenu != null)
        {
            backToMenu.interactable = true;
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

    protected virtual void OnDestroy()
    {
        if (backToMenu != null)
        {
            backToMenu.onClick.RemoveAllListeners();
        }
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.RemoveAllListeners();
        }

        // Clean up player results
        if (playerResults != null)
        {
            for (int i = 0; i < playerResults.Length; i++)
            {
                if (playerResults[i] != null)
                {
                    playerResults[i] = null;
                }
            }
        }

        // Clear references
        databaseReference = null;
        winningPlayer = null;
        
        // Stop all coroutines
        StopAllCoroutines();
    }

    // Photon Callbacks
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
        SceneManager.LoadScene("Menu");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room successfully");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left the room");
    }
}
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

public class MutiManageT : MonoBehaviourPunCallbacks
{
    [Header("Player UI")]
    public GameObject player1;
    public GameObject player2;

    [Header("Game Controls")]
    public Button zetButton;
    public float cooldownTime = 7f;
    public static bool isZETActive = false;
    public static Player playerWhoActivatedZET = null;

    [Header("UI Elements")]
    public TMP_Text timerText;
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private TMP_Text notificationText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference databaseRef;
    private BoardCheckT boardCheck;
    private string tournamentId;
    private string currentMatchId;
    private float timer;
    private bool isGameActive = false;
    private Player winningPlayer;
    private bool statusUpdateVerified = false;

    void Start()
    {
        tournamentId = PlayerPrefs.GetString("TournamentId");
        currentMatchId = PlayerPrefs.GetString("CurrentMatchId");
        
        UpdatePlayerList();
        ResetPlayerData();
        
        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);
        boardCheck = FindObjectOfType<BoardCheckT>();

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGameTimer", RpcTarget.All);
        }
        
        StartCoroutine(EnsureStatusUpdate());
    }

    private IEnumerator EnsureStatusUpdate()
    {
        UpdatePlayerMatchStatus("playing");
        yield return new WaitForSeconds(1f);
        
        int retryCount = 0;
        while (!statusUpdateVerified && retryCount < 3)
        {
            VerifyStatusUpdate();
            yield return new WaitForSeconds(1f);
            retryCount++;
        }
    }

    private void VerifyStatusUpdate()
    {
        DatabaseReference matchRef = FirebaseDatabase.DefaultInstance
            .GetReference("tournaments")
            .Child(tournamentId)
            .Child("bracket")
            .Child(currentMatchId);

        matchRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                var matchData = task.Result.Value as Dictionary<string, object>;
                var player1Data = matchData?["player1"] as Dictionary<string, object>;
                var player2Data = matchData?["player2"] as Dictionary<string, object>;

                bool player1Playing = player1Data != null && (bool)player1Data["isPlaying"];
                bool player2Playing = player2Data != null && (bool)player2Data["isPlaying"];

                statusUpdateVerified = player1Playing && player2Playing;

                if (!statusUpdateVerified)
                {
                    Debug.LogWarning("Not all players are marked as playing. Retrying update...");
                    UpdatePlayerMatchStatus("playing");
                }
            }
        });
    }

    private void UpdatePlayerMatchStatus(string status)
    {
        DatabaseReference matchRef = FirebaseDatabase.DefaultInstance
            .GetReference("tournaments")
            .Child(tournamentId)
            .Child("bracket")
            .Child(currentMatchId);

        matchRef.RunTransaction(mutableData =>
        {
            Dictionary<string, object> match = mutableData.Value as Dictionary<string, object>;
            if (match != null)
            {
                var player1Data = match["player1"] as Dictionary<string, object>;
                var player2Data = match["player2"] as Dictionary<string, object>;

                UpdatePlayerStatus(player1Data, status);
                UpdatePlayerStatus(player2Data, status);
                
                mutableData.Value = match;
            }
            return TransactionResult.Success(mutableData);
        });
    }

    private void UpdatePlayerStatus(Dictionary<string, object> playerData, string status)
    {
        if (playerData == null) return;

        switch (status)
        {
            case "lobby":
                playerData["inLobby"] = true;
                playerData["isPlaying"] = false;
                playerData["hasCompleted"] = false;
                break;
            case "playing":
                playerData["inLobby"] = true;
                playerData["isPlaying"] = true;
                playerData["hasCompleted"] = false;
                break;
            case "completed":
                playerData["inLobby"] = false;
                playerData["isPlaying"] = false;
                playerData["hasCompleted"] = true;
                break;
            case "left":
                playerData["inLobby"] = false;
                playerData["isPlaying"] = false;
                playerData["hasCompleted"] = false;
                break;
        }
    }

    void Update()
    {
        if (isGameActive && PhotonNetwork.IsMasterClient)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                photonView.RPC("UpdateGameTimer", RpcTarget.All, timer);

                if (!CheckIfPlayersHaveSameScore())
                {
                    EndGame();
                }
                else
                {
                    StartCoroutine(WaitForWinner());
                }
            }
            else
            {
                photonView.RPC("UpdateGameTimer", RpcTarget.All, timer);
            }
        }
    }

    void UpdatePlayerList()
    {
        GameObject[] playerObjects = { player1, player2 };
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (i < players.Length && playerObjects[i] != null)
            {
                playerObjects[i].SetActive(true);
                PlayerControlT playerCon = playerObjects[i].GetComponent<PlayerControlT>();
                if (playerCon != null)
                {
                    playerCon.SetActorNumber(players[i].ActorNumber);
                    string username = players[i].CustomProperties.ContainsKey("username") ? 
                        players[i].CustomProperties["username"].ToString() : 
                        players[i].NickName;
                    string score = players[i].CustomProperties.ContainsKey("score") ? 
                        players[i].CustomProperties["score"].ToString() : "0";
                    bool zetActive = false;

                    playerCon.UpdatePlayerInfo(username, score, zetActive);

                    if (players[i].CustomProperties.ContainsKey("iconId"))
                    {
                        int iconId = (int)players[i].CustomProperties["iconId"];
                        playerCon.UpdatePlayerIcon(iconId);
                    }
                }
            }
            else
            {
                if (playerObjects[i] != null)
                {
                    playerObjects[i].SetActive(false);
                }
            }
        }
    }

    public void OnZetButtonPressed()
    {
        audioSource.PlayOneShot(buttonSound);

        if (photonView != null && !isZETActive)
        {
            photonView.RPC("RPC_ActivateZET", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    public void RPC_ActivateZET(int playerActorNumber)
    {
        playerWhoActivatedZET = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
        StartCoroutine(ActivateZetWithCooldown(playerActorNumber));
    }

    private IEnumerator ActivateZetWithCooldown(int playerActorNumber)
    {
        isZETActive = true;
        zetButton.interactable = false;

        GameObject[] playerObjects = { player1, player2 };
        PlayerControlT activatedPlayerCon = null;
        int playerCount = Mathf.Min(playerObjects.Length, PhotonNetwork.PlayerList.Length);

        for (int i = 0; i < playerCount; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            PlayerControlT playerCon = playerObjects[i].GetComponent<PlayerControlT>();

            if (player.ActorNumber == playerActorNumber && playerCon != null)
            {
                playerCon.ActivateZetText();
                activatedPlayerCon = playerCon;
            }
        }

        yield return new WaitForSeconds(cooldownTime);

        if (activatedPlayerCon != null)
        {
            activatedPlayerCon.DeactivateZetText();
        }

        isZETActive = false;
        zetButton.interactable = true;
    }

    [PunRPC]
    public void UpdatePlayerScore(int actorNumber, int newScore)
    {
        string scoreWithPrefix = "score : " + newScore.ToString();
        PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "score", scoreWithPrefix } });

        GameObject[] players = { player1, player2 };
        foreach (GameObject player in players)
        {
            PlayerControlT playerComponent = player.GetComponent<PlayerControlT>();
            if (playerComponent != null && playerComponent.ActorNumber == actorNumber)
            {
                playerComponent.UpdateScore(newScore);
                break;
            }
        }
    }

    public void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isGameActive = false;

            Player winner = null;
            int highestScore = int.MinValue;

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                string scoreStr = player.CustomProperties["score"].ToString().Replace("score : ", "");
                if (int.TryParse(scoreStr, out int score) && score > highestScore)
                {
                    highestScore = score;
                    winner = player;
                }
            }

            winningPlayer = winner;
            UpdateMatchResult(winner);
            UpdatePlayerMatchStatus("completed");
            photonView.RPC("RPC_LoadEndGameScene", RpcTarget.All);
        }
    }

    private void UpdateMatchResult(Player winner)
    {
        if (winner == null) return;

        string winnerUsername = winner.CustomProperties["username"].ToString();
        DatabaseReference matchRef = FirebaseDatabase.DefaultInstance
            .GetReference("tournaments")
            .Child(tournamentId)
            .Child("bracket")
            .Child(currentMatchId);

        StartCoroutine(UpdateMatchAndAdvanceWinner(matchRef, winnerUsername));
    }

    private IEnumerator UpdateMatchAndAdvanceWinner(DatabaseReference matchRef, string winnerUsername)
    {
        // 1. Update current match
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

        // 2. Get nextMatchId
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
            DatabaseReference nextMatchRef = FirebaseDatabase.DefaultInstance
                .GetReference("tournaments")
                .Child(tournamentId)
                .Child("bracket")
                .Child(nextMatchId);

            var nextMatchDataTask = nextMatchRef.GetValueAsync();
            yield return new WaitUntil(() => nextMatchDataTask.IsCompleted);

            if (nextMatchDataTask.Exception != null)
            {
                Debug.LogError($"Failed to get next match data: {nextMatchDataTask.Exception}");
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
            var tournamentRef = FirebaseDatabase.DefaultInstance
                .GetReference("tournaments")
                .Child(tournamentId);

            var updateTournamentTask = tournamentRef.Child("won").SetValueAsync(winnerUsername);
            yield return new WaitUntil(() => updateTournamentTask.IsCompleted);

            if (updateTournamentTask.Exception != null)
            {
                Debug.LogError($"Failed to update tournament winner: {updateTournamentTask.Exception}");
            }

            StartCoroutine(UpdatePlayerTournamentStats(winnerUsername));
        }
    }

    private IEnumerator UpdatePlayerTournamentStats(string winnerUsername)
    {
        var userRef = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .OrderByChild("username")
            .EqualTo(winnerUsername)
            .LimitToFirst(1)
            .GetValueAsync();

        yield return new WaitUntil(() => userRef.IsCompleted);

        if (userRef.Exception != null)
        {
            Debug.LogError($"Failed to find winner user data: {userRef.Exception}");
            yield break;
        }

        var snapshot = userRef.Result;
        if (snapshot.ChildrenCount > 0)
        {
            var userId = snapshot.Children.First().Key;
            var winnerRef = FirebaseDatabase.DefaultInstance
                .GetReference("users")
                .Child(userId)
                .Child("gameswintournament");

            var currentWinsTask = winnerRef.GetValueAsync();
            yield return new WaitUntil(() => currentWinsTask.IsCompleted);

            if (currentWinsTask.Exception == null && currentWinsTask.Result.Exists)
            {
                int currentWins = int.Parse(currentWinsTask.Result.Value.ToString());
                winnerRef.SetValueAsync(currentWins + 1);
            }
        }
    }

    private bool CheckIfPlayersHaveSameScore()
    {
        Player[] players = PhotonNetwork.PlayerList;
        List<int> scores = new List<int>();

        foreach (Player player in players)
        {
            string scoreStr = player.CustomProperties.ContainsKey("score") ? 
                player.CustomProperties["score"].ToString() : "0";
            string cleanScoreStr = System.Text.RegularExpressions.Regex.Replace(scoreStr, @"\D", "");
            
            if (!int.TryParse(cleanScoreStr, out int score))
            {
                score = 0;
            }
            scores.Add(score);
        }

        for (int i = 0; i < scores.Count; i++)
        {
            for (int j = i + 1; j < scores.Count; j++)
            {
                if (scores[i] == scores[j])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerator WaitForWinner()
    {
        while (true)
        {
            if (CheckForWinner())
            {
                EndGame();
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private bool CheckForWinner()
    {
        Player[] players = PhotonNetwork.PlayerList;
        int highestScore = int.MinValue;
        int highestCount = 0;

        foreach (Player player in players)
        {
            string scoreStr = player.CustomProperties.ContainsKey("score") ? 
                player.CustomProperties["score"].ToString() : "0";
            string cleanScoreStr = System.Text.RegularExpressions.Regex.Replace(scoreStr, @"\D", "");
            
            if (!int.TryParse(cleanScoreStr, out int score))
            {
                score = 0;
            }

            if (score > highestScore)
            {
                highestScore = score;
                highestCount = 1;
            }
            else if (score == highestScore)
            {
                highestCount++;
            }
        }

        return highestCount == 1;
    }

    [PunRPC]
    private void StartGameTimer()
    {
        isGameActive = true;
        timer = 100f;
    }

    [PunRPC]
    private void UpdateGameTimer(float currentTime)
    {
        timer = currentTime;
        if (timerText != null)
        {
            if (timer <= 0)
            {
                if (CheckIfPlayersHaveSameScore())
                {
                    timerText.text = "Sudden Death!";
                }
            }
            else
            {
                int minutes = Mathf.FloorToInt(timer / 60);
                int seconds = Mathf.FloorToInt(timer % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    [PunRPC]
    private void RPC_LoadEndGameScene()
    {
        SceneManager.LoadScene("ResultT");
    }

    private void ResetPlayerData()
    {
        isZETActive = false;
        playerWhoActivatedZET = null;
        zetButton.interactable = true;

        GameObject[] playerObjects = { player1, player2 };
        foreach (var playerObject in playerObjects)
        {
            if (playerObject != null)
            {
                PlayerControlT playerCon = playerObject.GetComponent<PlayerControlT>();
                if (playerCon != null)
                {
                    playerCon.ResetScore();
                    playerCon.ResetZetStatus();

                    int actorNumber = playerCon.ActorNumber;
                    ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
                    {
                        { "score", "score : 0" }
                    };

                    var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);
                    if (player != null)
                    {
                        player.SetCustomProperties(newProperties);
                    }
                }
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        Debug.Log($"{newPlayer.NickName} joined the game");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the game");
        UpdatePlayerMatchStatus("left");

        if (isGameActive)
        {
            EndGameDueToDisconnection();
        }
    }

    private void EndGameDueToDisconnection()
    {
        Player remainingPlayer = PhotonNetwork.PlayerList.FirstOrDefault();
        if (remainingPlayer != null)
        {
            winningPlayer = remainingPlayer;

            int winningScore = 0;
            if (winningPlayer.CustomProperties.ContainsKey("score"))
            {
                string scoreStr = winningPlayer.CustomProperties["score"].ToString().Replace("score : ", "");
                int.TryParse(scoreStr, out winningScore);
            }

            UpdatePlayerScore(winningPlayer.ActorNumber, winningScore);
            UpdateMatchResult(winningPlayer);
            UpdatePlayerMatchStatus("completed");
            photonView.RPC("RPC_LoadEndGameScene", RpcTarget.All);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("MasterClient has left the game");

        if (isGameActive)
        {
            EndGameDueToDisconnection();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from server: {cause}");
        UpdatePlayerMatchStatus("left");
        SceneManager.LoadScene("Menu");
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

    void OnDestroy()
    {
        StopAllCoroutines();
        if (zetButton != null)
        {
            zetButton.onClick.RemoveAllListeners();
        }

        isZETActive = false;
        playerWhoActivatedZET = null;
    }
}
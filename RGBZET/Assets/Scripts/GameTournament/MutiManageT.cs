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
    
    private float gameTime = 100f;
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
        
        // Start status update process for both players
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

                if (player1Data != null)
                {
                    UpdatePlayerStatus(player1Data, status);
                }
                if (player2Data != null)
                {
                    UpdatePlayerStatus(player2Data, status);
                }
                
                mutableData.Value = match;
            }
            return TransactionResult.Success(mutableData);
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to update match status: {task.Exception}");
            }
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
            gameTime -= Time.deltaTime;

            if (gameTime <= 0)
            {
                gameTime = 0;
                photonView.RPC("UpdateGameTimer", RpcTarget.All, gameTime);

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
                photonView.RPC("UpdateGameTimer", RpcTarget.All, gameTime);
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
                if (player.CustomProperties.TryGetValue("score", out object scoreObj) && scoreObj is string scoreStr)
                {
                    if (int.TryParse(scoreStr.Replace("score : ", ""), out int score) && score > highestScore)
                    {
                        highestScore = score;
                        winner = player;
                    }
                }
            }

            winningPlayer = winner;
            UpdateMatchResult(winner);
            UpdatePlayerMatchStatus("completed");
            photonView.RPC("RPC_LoadEndGameScene", RpcTarget.All, winner.ActorNumber);
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

        Dictionary<string, object> updateData = new Dictionary<string, object>
        {
            { "winner", winnerUsername },
            { "winnerScore", winner.CustomProperties["score"] ?? "score : 0" }
        };

        matchRef.UpdateChildrenAsync(updateData);
    }

    [PunRPC]
    private void RPC_LoadEndGameScene(int winnerActorNumber)
    {
        PlayerPrefs.SetInt("WinnerActorNumber", winnerActorNumber);
        SceneManager.LoadScene("ResultTournament");
    }

    bool CheckIfPlayersHaveSameScore()
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

    IEnumerator WaitForWinner()
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

    bool CheckForWinner()
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
    void StartGameTimer()
    {
        isGameActive = true;
        gameTime = 100f;
    }

    [PunRPC]
    void UpdateGameTimer(float currentTime)
    {
        gameTime = currentTime;

        if (timerText != null)
        {
            if (gameTime <= 0)
            {
                if (CheckIfPlayersHaveSameScore())
                {
                    timerText.text = "Sudden Death!";
                }
            }
            else
            {
                int minutes = Mathf.FloorToInt(gameTime / 60);
                int seconds = Mathf.FloorToInt(gameTime % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    void ResetPlayerData()
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

                    PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber)
                        ?.SetCustomProperties(newProperties);
                }
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        Debug.Log($"{newPlayer.NickName} player In");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the game.");

        UpdatePlayerMatchStatus("left");

        if (isGameActive)
        {
            EndGameDueToDisconnection();
        }
    }

    public void EndGameDueToDisconnection()
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
            photonView.RPC("RPC_LoadEndGameScene", RpcTarget.All, winningPlayer.ActorNumber);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("MasterClient has left the room.");

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

    public override void OnLeftRoom()
    {
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

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdatePlayerList();
    }

    [PunRPC]
    private void RPC_UpdateGameState(bool isActive, float remainingTime)
    {
        isGameActive = isActive;
        gameTime = remainingTime;
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

    private void OnDestroy()
    {
        // Cleanup event listeners
        if (zetButton != null)
        {
            zetButton.onClick.RemoveAllListeners();
        }

        // Reset static variables
        isZETActive = false;
        playerWhoActivatedZET = null;

        // Stop all coroutines
        StopAllCoroutines();
    }
}
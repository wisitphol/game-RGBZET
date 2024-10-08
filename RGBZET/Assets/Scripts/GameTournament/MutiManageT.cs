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
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public Button zetButton;
    public float cooldownTime = 7f;
    public static bool isZETActive = false;
    public static Player playerWhoActivatedZET = null;
    private DatabaseReference databaseRef;
    private float timer;
    private string roomId;
    private BoardCheckT boardCheck;
    public TMP_Text timerText;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    private string tournamentId;
    private string currentMatchId;

    private float gameTime = 180f; // 3 minutes in seconds
    private bool isGameActive = false;
    private bool isSuddenDeath = false;
    private List<Player> suddenDeathPlayers = new List<Player>();
    private Player winningPlayer;

    void Start()
    {
        UpdatePlayerList();
        ResetPlayerData();
        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);
        boardCheck = FindObjectOfType<BoardCheckT>();

        tournamentId = PlayerPrefs.GetString("TournamentId");
        currentMatchId = PlayerPrefs.GetString("CurrentMatchId");

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGameTimer", RpcTarget.All);
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
        GameObject[] playerObjects = { player1, player2, player3, player4 };
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

    void Update()
    {
        if (isGameActive && PhotonNetwork.IsMasterClient)
        {
            if (!isSuddenDeath)
            {
                gameTime -= Time.deltaTime;
                if (gameTime <= 0)
                {
                    gameTime = 0;
                    CheckForTie();
                }
                photonView.RPC("UpdateGameTimer", RpcTarget.All, gameTime);
            }
        }
    }

    void UpdatePlayerList()
    {
        GameObject[] playerObjects = { player1, player2, player3, player4 };
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
                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;
                    string score = players[i].CustomProperties.ContainsKey("score") ? players[i].CustomProperties["score"].ToString() : "0";
                    bool zetActive = false;

                    playerCon.UpdatePlayerInfo(username, score, zetActive);
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        Debug.Log($"{newPlayer.NickName} player In");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the game.");

        // ถ้าผู้เล่นออกจากห้องระหว่างการแข่งขัน ให้ผู้เล่นที่เหลือชนะทันที
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

            // ดึงคะแนนของผู้เล่นที่เหลือจาก CustomProperties
            int winningScore = 0;
            if (winningPlayer.CustomProperties.ContainsKey("score"))
            {
                string scoreStr = winningPlayer.CustomProperties["score"].ToString().Replace("score : ", "");
                int.TryParse(scoreStr, out winningScore);
            }

            // อัปเดตคะแนนผู้ชนะด้วยคะแนนจริง
            UpdatePlayerScore(winningPlayer.ActorNumber, winningScore);

            // อัปเดตสถานะของแมตช์ไปยัง Firebase ว่าผู้เล่นที่เหลือเป็นผู้ชนะ
            UpdateMatchResult(winningPlayer);

            // ย้ายไปยังฉากสรุปผลคะแนน
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
        else
        {
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
    }

    [PunRPC]
    public void UpdatePlayerScore(int actorNumber, int newScore)
    {
        Debug.Log($"Updating score for actorNumber: {actorNumber} with newScore: {newScore}");

        string scoreWithPrefix = "score : " + newScore.ToString();

        PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "score", scoreWithPrefix } });

        GameObject[] players = { player1, player2, player3, player4 };

        foreach (GameObject player in players)
        {
            PlayerControlT playerComponent = player.GetComponent<PlayerControlT>();
            if (playerComponent != null && playerComponent.ActorNumber == actorNumber)
            {
                playerComponent.UpdateScore(newScore);
                Debug.Log($"Score updated for {playerComponent.NameText.text} to {newScore}");
                break;
            }
        }
    }

    private void UpdateMatchResult(Player winner)
    {
        DatabaseReference matchRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments")
            .Child(tournamentId).Child("bracket").Child(currentMatchId);

        Dictionary<string, object> updateData = new Dictionary<string, object>
        {
            { "winner", winner.NickName },
            { "winnerScore", winner.CustomProperties["score"] ?? "score : 1" }
        };

        matchRef.UpdateChildrenAsync(updateData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Match result updated successfully in Firebase.");
            }
            else
            {
                Debug.LogError("Failed to update match result: " + task.Exception);
            }
        });
    }

    [PunRPC]
    private void RPC_LoadEndGameScene(int winnerActorNumber)
    {
        PlayerPrefs.SetInt("WinnerActorNumber", winnerActorNumber);
        SceneManager.LoadScene("ResultTournament");
    }

    private IEnumerator DeleteRoomAndGoToMenu()
    {
        Debug.Log("Started DeleteRoomAndGoToMenu coroutine.");

        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
    }

    void ResetPlayerData()
    {
        isZETActive = false;
        playerWhoActivatedZET = null;
        zetButton.interactable = true;
        Debug.Log("ResetPlayerData called.");

        GameObject[] playerObjects = { player1, player2, player3, player4 };
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

                    PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber)?.SetCustomProperties(newProperties);
                }
            }
        }
    }

    [PunRPC]
    void StartGameTimer()
    {
        isGameActive = true;
        gameTime = 180f; // Reset to 3 minutes
    }

    [PunRPC]
    void UpdateGameTimer(float currentTime)
    {
        gameTime = currentTime;
        if (timerText != null)
        {
            if (!isSuddenDeath)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60);
                int seconds = Mathf.FloorToInt(gameTime % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            else
            {
                timerText.text = "Sudden Death!";
            }
        }
    }

    private void CheckForTie()
    {
        Player[] topPlayers = GetTopPlayers();
        if (topPlayers.Length > 1)
        {
            StartSuddenDeath(topPlayers);
        }
        else
        {
            EndGame();
        }
    }

    private Player[] GetTopPlayers()
    {
        int highestScore = int.MinValue;
        List<Player> topPlayers = new List<Player>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("score", out object scoreObj) && scoreObj is string scoreStr)
            {
                if (int.TryParse(scoreStr.Replace("score : ", ""), out int score))
                {
                    if (score > highestScore)
                    {
                        highestScore = score;
                        topPlayers.Clear();
                        topPlayers.Add(player);
                    }
                    else if (score == highestScore)
                    {
                        topPlayers.Add(player);
                    }
                }
            }
        }

        return topPlayers.ToArray();
    }

    private void StartSuddenDeath(Player[] tiePlayers)
    {
        isSuddenDeath = true;
        suddenDeathPlayers = new List<Player>(tiePlayers);
        photonView.RPC("RPC_StartSuddenDeath", RpcTarget.All, tiePlayers.Select(p => p.ActorNumber).ToArray());
    }

    [PunRPC]
    private void RPC_StartSuddenDeath(int[] suddenDeathPlayerActorNumbers)
    {
        isSuddenDeath = true;
        suddenDeathPlayers = suddenDeathPlayerActorNumbers.Select(actorNumber => PhotonNetwork.CurrentRoom.GetPlayer(actorNumber)).ToList();
        
        // Reset scores for sudden death players
        foreach (Player player in suddenDeathPlayers)
        {
            UpdatePlayerScore(player.ActorNumber, 0);
        }

        // Disable ZET button for non-sudden death players
        zetButton.interactable = suddenDeathPlayers.Contains(PhotonNetwork.LocalPlayer);

        // Show sudden death message
        if (timerText != null)
        {
            timerText.text = "Sudden Death!";
        }
    }

    private Player GetWinningPlayer()
    {
        if (isSuddenDeath)
        {
            foreach (Player player in suddenDeathPlayers)
            {
                if (player.CustomProperties.TryGetValue("score", out object scoreObj) && scoreObj is string scoreStr)
                {
                    if (int.Parse(scoreStr.Replace("score : ", "")) > 0)
                    {
                        return player;
                    }
                }
            }
            return null; // No winner yet in sudden death
        }
        else
        {
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

            return winner;
        }
    }

    public void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isGameActive = false;
            Player winner = GetWinningPlayer();
            UpdateMatchResult(winner);
            photonView.RPC("RPC_LoadEndGameScene", RpcTarget.All, winner.ActorNumber);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class TournamentLobbyUI : MonoBehaviourPunCallbacks
{
    public TMP_Text tournamentNameText;
    public TMP_Text tournamentIdText;
    public TMP_Text playerCountText;
    public TMP_Text playerListText;
    public Button startTournamentButton;
    public Button leaveTournamentButton;
    public Button readyButton;
    public Button copyButton;

    private DatabaseReference tournamentRef;
    private string tournamentId;
    private int playerCount;
    private string tournamentName;
    private bool isReady = false;
    private string currentUsername;

    void Start()
    {
        tournamentId = PlayerPrefs.GetString("TournamentId");
        playerCount = PlayerPrefs.GetInt("PlayerCount");
        tournamentName = PlayerPrefs.GetString("TournamentName", "Unnamed Tournament");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        Debug.Log($"TournamentLobbyUI Start - TournamentId: {tournamentId}, PlayerCount: {playerCount}, TournamentName: {tournamentName}");

        if (string.IsNullOrEmpty(tournamentId) || playerCount <= 0)
        {
            Debug.LogError("TournamentId or PlayerCount is missing.");
            return;
        }

        tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);
        tournamentNameText.text = "Tournament: " + tournamentName;
        tournamentIdText.text = "ID: " + tournamentId;

        startTournamentButton.onClick.AddListener(StartTournament);
        leaveTournamentButton.onClick.AddListener(LeaveTournament);
        readyButton.onClick.AddListener(ToggleReady);
        
        SetPlayerProperties();

        if (PhotonNetwork.IsConnected)
        {
            OnConnectedToMaster();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        PhotonNetwork.AutomaticallySyncScene = true;

         copyButton.onClick.AddListener(() =>
        {
            CopyRoomIdToClipboard();
        });
    }

    void SetPlayerProperties()
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", currentUsername },
            { "IsReady", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        JoinOrCreateRoom();
    }

    void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)playerCount,
            PublishUserId = true,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom(tournamentId, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        UpdateUI();
        AddPlayerToBracket();
    }

    void UpdateUI()
    {
        playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / {playerCount}";
        startTournamentButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        startTournamentButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount == playerCount && AreAllPlayersReady();
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        playerListText.text = "Players:\n";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string username = player.CustomProperties.ContainsKey("username") 
                ? player.CustomProperties["username"].ToString() 
                : player.NickName;
            string readyStatus = player.CustomProperties.TryGetValue("IsReady", out object isReady) && (bool)isReady 
                ? " (Ready)" 
                : " (Not Ready)";
            playerListText.text += username + readyStatus + "\n";
        }
        Debug.Log("Player list updated: " + playerListText.text);
    }

    void ToggleReady()
    {
        isReady = !isReady;
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable { { "IsReady", isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Cancel Ready" : "Ready";
    }

    bool AreAllPlayersReady()
    {
        return PhotonNetwork.PlayerList.All(p => p.CustomProperties.TryGetValue("IsReady", out object isReady) && (bool)isReady);
    }

    void StartTournament()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == playerCount && AreAllPlayersReady())
        {
            StartCoroutine(SetupTournamentAndLoadBracket());
        }
    }

    IEnumerator SetupTournamentAndLoadBracket()
    {
        yield return StartCoroutine(UpdateTournamentStatus("in_progress"));
        yield return StartCoroutine(ShufflePlayers());

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("TournamentBracket");
    }

    IEnumerator UpdateTournamentStatus(string status)
    {
        var task = tournamentRef.Child("status").SetValueAsync(status);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to update tournament status: {task.Exception}");
        }
    }

    IEnumerator ShufflePlayers()
    {
        List<Player> players = PhotonNetwork.PlayerList.ToList();
        System.Random rng = new System.Random();
        players = players.OrderBy(x => rng.Next()).ToList();

        var task = tournamentRef.Child("bracket").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get bracket data: {task.Exception}");
            yield break;
        }

        var bracketSnapshot = task.Result;
        Dictionary<string, object> bracket = (Dictionary<string, object>)bracketSnapshot.Value;

        // Create a new dictionary to store the updated bracket
        var updatedBracket = new Dictionary<string, object>(bracket);

        int playerIndex = 0;
        foreach (var match in bracket.ToList())  // Use ToList() to avoid modifying collection during iteration
        {
            if (match.Key.StartsWith("round_0"))
            {
                Dictionary<string, object> matchData = (Dictionary<string, object>)match.Value;
                if (playerIndex < players.Count)
                {
                    ((Dictionary<string, object>)matchData["player1"])["username"] = players[playerIndex].CustomProperties["username"];
                    playerIndex++;
                }
                if (playerIndex < players.Count)
                {
                    ((Dictionary<string, object>)matchData["player2"])["username"] = players[playerIndex].CustomProperties["username"];
                    playerIndex++;
                }
                updatedBracket[match.Key] = matchData; // Update the new dictionary instead of modifying the original one
            }
        }

        // Now update the bracket in the database
        var updateTask = tournamentRef.Child("bracket").SetValueAsync(updatedBracket);
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception != null)
        {
            Debug.LogError($"Failed to update bracket with shuffled players: {updateTask.Exception}");
        }
    }

    void LeaveTournament()
    {
        StartCoroutine(LeaveTournamentCoroutine());
    }

    IEnumerator LeaveTournamentCoroutine()
    {
        // Disable UI elements to prevent further interactions
        startTournamentButton.interactable = false;
        leaveTournamentButton.interactable = false;
        readyButton.interactable = false;

        if (PhotonNetwork.IsMasterClient)
        {
            var task = tournamentRef.RemoveValueAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogError($"Failed to delete tournament: {task.Exception}");
            }
        }

        // First, leave the Photon room
        PhotonNetwork.LeaveRoom(false);

        // Wait until we've fully left the room
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        // Then disconnect from Photon
        PhotonNetwork.Disconnect();

        // Wait until we're fully disconnected
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        // Finally, load the MainGame scene
        SceneManager.LoadScene("Menu");
    }

    void AddPlayerToBracket()
    {
        tournamentRef.Child("bracket").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                var bracket = (Dictionary<string, object>)task.Result.Value;
                foreach (var match in bracket)
                {
                    if (match.Key.StartsWith("round_0"))
                    {
                        var matchData = (Dictionary<string, object>)match.Value;
                        var player1 = (Dictionary<string, object>)matchData["player1"];
                        var player2 = (Dictionary<string, object>)matchData["player2"];

                        if (string.IsNullOrEmpty((string)player1["username"]))
                        {
                            player1["username"] = currentUsername;
                            tournamentRef.Child("bracket").Child(match.Key).SetValueAsync(matchData);
                            return;
                        }
                        else if (string.IsNullOrEmpty((string)player2["username"]))
                        {
                            player2["username"] = currentUsername;
                            tournamentRef.Child("bracket").Child(match.Key).SetValueAsync(matchData);
                            return;
                        }
                    }
                }
            }
        });
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the room successfully");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player entered room: {newPlayer.NickName}");
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();

        if (PhotonNetwork.IsMasterClient && otherPlayer.IsMasterClient)
        {
            startTournamentButton.gameObject.SetActive(true);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startTournamentButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdateUI();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Room creation failed: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Room join failed: {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
        // If we're not already in the process of leaving, load the MainGame scene
        if (!SceneManager.GetActiveScene().name.Equals("Menu"))
        {
            SceneManager.LoadScene("Menu");
        }
    }

    void OnDestroy()
    {
        if (startTournamentButton != null)
            startTournamentButton.onClick.RemoveAllListeners();
        if (leaveTournamentButton != null)
            leaveTournamentButton.onClick.RemoveAllListeners();
        if (readyButton != null)
            readyButton.onClick.RemoveAllListeners();
    }

     void CopyRoomIdToClipboard()
    {
        GUIUtility.systemCopyBuffer = tournamentId;  // ก๊อปปี้ roomId ไปที่คลิปบอร์ด
        //DisplayFeedback("Room ID copied.");
        Debug.Log("Room ID copied: " + tournamentId);
    }
}
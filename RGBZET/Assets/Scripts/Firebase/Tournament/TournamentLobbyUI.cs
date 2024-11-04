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
    [Header("Player UI")]
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public GameObject player5;
    public GameObject player6;
    public GameObject player7;
    public GameObject player8;

    [Header("UI Elements")]
    public TMP_Text tournamentNameText;
    public TMP_Text tournamentIdText;
    public TMP_Text playerCountText;
    public Button startTournamentButton;
    public Button leaveTournamentButton;
    public Button readyButton;
    public Button copyButton;
    public GameObject notificationPopup;
    public TMP_Text notificationText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference tournamentRef;
    private string tournamentId;
    private int playerCount;
    private string tournamentName;
    private bool isReady = false;
    private string currentUsername;
    private bool isCleaningUp = false;
    private bool isInitialized = false;
    private int connectionRetryCount = 0;
    private const int MAX_CONNECTION_RETRIES = 3;

    void Start()
    {
        InitializeTournamentData();
        SetupUIElements();
        
        if (!PhotonNetwork.IsConnected)
        {
            ShowNotification("Connecting to server...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.InLobby)
        {
            JoinOrCreateRoom();
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        UpdateUI();
    }

    private void InitializeTournamentData()
    {
        tournamentId = PlayerPrefs.GetString("TournamentId");
        playerCount = PlayerPrefs.GetInt("PlayerCount");
        tournamentName = PlayerPrefs.GetString("TournamentName", "Unnamed Tournament");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        Debug.Log($"TournamentLobbyUI Start - TournamentId: {tournamentId}, PlayerCount: {playerCount}, TournamentName: {tournamentName}");

        if (string.IsNullOrEmpty(tournamentId) || playerCount <= 0)
        {
            Debug.LogError("TournamentId or PlayerCount is missing.");
            ShowNotification("Invalid tournament data");
            StartCoroutine(ReturnToMenuWithDelay(2f));
            return;
        }

        tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);
    }

    private void SetupUIElements()
    {
        tournamentNameText.text = "Room: " + tournamentName;
        tournamentIdText.text = "Room ID: " + tournamentId;

        startTournamentButton.onClick.AddListener(() => SoundOnClick(StartTournament));
        leaveTournamentButton.onClick.AddListener(() => SoundOnClick(LeaveTournament));
        readyButton.onClick.AddListener(() => SoundOnClick(ToggleReady));
        copyButton.onClick.AddListener(() => SoundOnClick(CopyRoomIdToClipboard));

        if (notificationPopup != null)
        {
            notificationPopup.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        UpdatePlayerCount();
        UpdatePlayerList();
        UpdateStartButtonVisibility();
        UpdateReadyButtonVisibility();
    }

    private void UpdatePlayerCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / {playerCount}";
        }
    }

    private void UpdatePlayerList()
    {
        GameObject[] playerObjects = { player1, player2, player3, player4, player5, player6, player7, player8 };
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (i < players.Length && playerObjects[i] != null)
            {
                playerObjects[i].SetActive(true);
                PlayerLobby2 playerLobby = playerObjects[i].GetComponent<PlayerLobby2>();

                if (playerLobby != null)
                {
                    UpdateSinglePlayer(playerLobby, players[i]);
                }
            }
            else if (playerObjects[i] != null)
            {
                playerObjects[i].SetActive(false);
            }
        }
    }

    private void UpdateSinglePlayer(PlayerLobby2 playerLobby, Player player)
    {
        playerLobby.SetActorNumber(player.ActorNumber);
        string username = player.CustomProperties.ContainsKey("username") ? 
            player.CustomProperties["username"].ToString() : 
            player.NickName;
        bool isReady = player.CustomProperties.ContainsKey("IsReady") && 
            (bool)player.CustomProperties["IsReady"];
        string readyStatus = isReady ? "Ready" : "Not Ready";
        
        playerLobby.UpdatePlayerInfo(username, readyStatus);

        if (player.CustomProperties.ContainsKey("iconId"))
        {
            int iconId = (int)player.CustomProperties["iconId"];
            playerLobby.UpdatePlayerIcon(iconId);
        }
    }

    private void UpdateStartButtonVisibility()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            startTournamentButton.gameObject.SetActive(false);
            return;
        }

        bool allPlayersReady = PhotonNetwork.PlayerList.All(p => 
            p.CustomProperties.ContainsKey("IsReady") && (bool)p.CustomProperties["IsReady"]);
        bool allPlayersJoined = PhotonNetwork.CurrentRoom.PlayerCount == playerCount;

        startTournamentButton.gameObject.SetActive(true);
        startTournamentButton.interactable = allPlayersReady && allPlayersJoined;

        if (startTournamentButton.interactable)
        {
            ShowNotification("Ready to start tournament");
        }
        else if (!allPlayersJoined)
        {
            ShowNotification("Waiting for more players");
        }
        else if (!allPlayersReady)
        {
            ShowNotification("Waiting for all players to be ready");
        }
    }

    private void UpdateReadyButtonVisibility()
    {
        readyButton.gameObject.SetActive(true);
        UpdateReadyButtonText();
    }

    private void UpdateReadyButtonText()
    {
        bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsReady") && 
            (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Not Ready" : "Ready";
    }

    private void StartTournament()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            ShowNotification("Only the host can start the tournament");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount != playerCount)
        {
            ShowNotification("Waiting for more players");
            return;
        }

        if (!AreAllPlayersReady())
        {
            ShowNotification("Not all players are ready");
            return;
        }

        ShowNotification("Starting tournament...");
        StartCoroutine(SetupTournamentAndLoadBracket());
    }

    private bool AreAllPlayersReady()
    {
        return PhotonNetwork.PlayerList.All(p => 
            p.CustomProperties.ContainsKey("IsReady") && (bool)p.CustomProperties["IsReady"]);
    }

    private IEnumerator SetupTournamentAndLoadBracket()
    {
        yield return StartCoroutine(UpdateTournamentStatus("in_progress"));
        yield return StartCoroutine(ShufflePlayers());

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("TournamentBracket");
    }

    private void ToggleReady()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            ShowNotification("Cannot set ready status - not connected");
            return;
        }

        isReady = !isReady;
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable 
        { 
            { "IsReady", isReady } 
        };

        try
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting ready status: {e.Message}");
            ShowNotification("Failed to update ready status");
        }
    }

    private IEnumerator UpdateTournamentStatus(string status)
    {
        var task = tournamentRef.Child("status").SetValueAsync(status);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to update tournament status: {task.Exception}");
            ShowNotification("Failed to update tournament status");
        }
    }

    private IEnumerator ShufflePlayers()
    {
        List<Player> players = PhotonNetwork.PlayerList.ToList();
        System.Random rng = new System.Random();
        players = players.OrderBy(x => rng.Next()).ToList();

        var task = tournamentRef.Child("bracket").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get bracket data: {task.Exception}");
            ShowNotification("Failed to prepare tournament bracket");
            yield break;
        }

        var bracketSnapshot = task.Result;
        Dictionary<string, object> bracket = (Dictionary<string, object>)bracketSnapshot.Value;
        var updatedBracket = new Dictionary<string, object>(bracket);

        int playerIndex = 0;
        foreach (var match in bracket.ToList())
        {
            if (match.Key.StartsWith("round_0"))
            {
                Dictionary<string, object> matchData = (Dictionary<string, object>)match.Value;
                if (playerIndex < players.Count)
                {
                    ((Dictionary<string, object>)matchData["player1"])["username"] = 
                        players[playerIndex].CustomProperties["username"];
                    playerIndex++;
                }
                if (playerIndex < players.Count)
                {
                    ((Dictionary<string, object>)matchData["player2"])["username"] = 
                        players[playerIndex].CustomProperties["username"];
                    playerIndex++;
                }
                updatedBracket[match.Key] = matchData;
            }
        }

        var updateTask = tournamentRef.Child("bracket").SetValueAsync(updatedBracket);
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception != null)
        {
            Debug.LogError($"Failed to update bracket with shuffled players: {updateTask.Exception}");
            ShowNotification("Failed to update tournament bracket");
        }
    }

    private void CopyRoomIdToClipboard()
    {
        GUIUtility.systemCopyBuffer = tournamentId;
        ShowNotification("Room ID copied to clipboard");
    }

    private void LeaveTournament()
    {
        StartCoroutine(LeaveTournamentCoroutine());
    }

    private IEnumerator LeaveTournamentCoroutine()
    {
        DisableUIElements();
        ShowNotification("Leaving tournament...");

        if (PhotonNetwork.IsMasterClient)
        {
            var task = tournamentRef.RemoveValueAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to delete tournament: {task.Exception}");
            }
        }

        yield return StartCoroutine(DisconnectAndReturnToMenu());
    }

    private void DisableUIElements()
    {
        if (startTournamentButton != null) startTournamentButton.interactable = false;
        if (leaveTournamentButton != null) leaveTournamentButton.interactable = false;
        if (readyButton != null) readyButton.interactable = false;
        if (copyButton != null) copyButton.interactable = false;
    }

    private IEnumerator DisconnectAndReturnToMenu()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            yield return new WaitUntil(() => !PhotonNetwork.InRoom);

            PhotonNetwork.Disconnect();
            yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        }

        SceneManager.LoadScene("Menu");
    }

    private IEnumerator ReturnToMenuWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator HostLeftCleanup()
    {
        isCleaningUp = true;
        ShowNotification("Host has left the tournament");

        if (PhotonNetwork.IsMasterClient)
        {
            var task = tournamentRef.RemoveValueAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to remove tournament data: {task.Exception}");
            }
        }

        photonView.RPC("RPC_ReturnToMenu", RpcTarget.All);
    }

    private IEnumerator InitializePlayerProperties()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

        bool initSuccess = false;
        while (!initSuccess && connectionRetryCount < MAX_CONNECTION_RETRIES)
        {
            initSuccess = TryInitializeProperties();
            
            if (!initSuccess)
            {
                connectionRetryCount++;
                if (connectionRetryCount < MAX_CONNECTION_RETRIES)
                {
                    Debug.Log($"Retrying initialization... Attempt {connectionRetryCount}/{MAX_CONNECTION_RETRIES}");
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        if (!initSuccess)
        {
            Debug.LogError("Failed to initialize player properties after maximum retries");
            ShowNotification("Failed to connect. Returning to menu...");
            yield return StartCoroutine(ReturnToMenuWithDelay(2f));
        }
    }

    private bool TryInitializeProperties()
    {
        try
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
            {
                { "username", currentUsername },
                { "IsReady", false }
            };
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            isInitialized = true;
            Debug.Log("Player properties initialized successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize player properties: {e.Message}");
            ShowNotification("Failed to initialize player data");
            return false;
        }
    }

    private IEnumerator RetryInitialization()
    {
        connectionRetryCount++;
        if (connectionRetryCount < MAX_CONNECTION_RETRIES)
        {
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(InitializePlayerProperties());
        }
        else
        {
            Debug.LogError("Failed to initialize player properties after maximum retries");
            ShowNotification("Failed to connect. Returning to menu...");
            yield return StartCoroutine(ReturnToMenuWithDelay(2f));
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationText != null && notificationPopup != null)
        {
            notificationText.text = message;
            notificationPopup.SetActive(true);
            StartCoroutine(HideNotificationAfterDelay(3f));
        }
        Debug.Log($"Notification: {message}");
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

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        ShowNotification("Connected to server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        ShowNotification("Joining tournament room...");
        JoinOrCreateRoom();
    }

    private void JoinOrCreateRoom()
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
        Debug.Log("Joined Tournament Room: " + PhotonNetwork.CurrentRoom.Name);
        ShowNotification("Joined tournament room");
        
        if (!isInitialized)
        {
            StartCoroutine(InitializePlayerProperties());
        }
        
        UpdateUI();
        AddPlayerToBracket();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player joined: {newPlayer.NickName}");
        ShowNotification($"{newPlayer.NickName} joined the tournament");
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isCleaningUp) return;

        Debug.Log($"Player left: {otherPlayer.NickName}");

        if (otherPlayer.IsMasterClient)
        {
            Debug.Log("Host has left. Starting cleanup...");
            StartCoroutine(HostLeftCleanup());
        }
        else
        {
            ShowNotification($"{otherPlayer.NickName} left the tournament");
            UpdateUI();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdateUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master Client switched to: {newMasterClient.NickName}");
        
        if (isCleaningUp && PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(HostLeftCleanup());
        }
        UpdateUI();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected: {cause}");
        isInitialized = false;
        
        if (!isCleaningUp)
        {
            ShowNotification($"Disconnected: {cause}");
            SceneManager.LoadScene("Menu");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
        ShowNotification("Failed to join tournament room");
        StartCoroutine(RetryJoinRoom());
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}");
        ShowNotification("Failed to create tournament room");
        StartCoroutine(RetryJoinRoom());
    }

    private void AddPlayerToBracket()
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

    private IEnumerator RetryJoinRoom()
    {
        connectionRetryCount++;
        if (connectionRetryCount < MAX_CONNECTION_RETRIES)
        {
            ShowNotification($"Retrying... Attempt {connectionRetryCount}/{MAX_CONNECTION_RETRIES}");
            yield return new WaitForSeconds(1f);
            
            if (PhotonNetwork.IsConnectedAndReady)
            {
                JoinOrCreateRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            ShowNotification("Failed to join after multiple attempts");
            yield return StartCoroutine(ReturnToMenuWithDelay(2f));
        }
    }

    [PunRPC]
    private void RPC_ReturnToMenu()
    {
        StartCoroutine(DisconnectAndReturnToMenu());
    }

    protected virtual void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && !isInitialized && PhotonNetwork.IsConnectedAndReady)
        {
            StartCoroutine(InitializePlayerProperties());
        }
    }

    void OnDestroy()
    {
        // Cleanup button listeners
        if (startTournamentButton != null)
            startTournamentButton.onClick.RemoveAllListeners();
        if (leaveTournamentButton != null)
            leaveTournamentButton.onClick.RemoveAllListeners();
        if (readyButton != null)
            readyButton.onClick.RemoveAllListeners();
        if (copyButton != null)
            copyButton.onClick.RemoveAllListeners();

        // Stop all coroutines
        StopAllCoroutines();

        // Clear references
        tournamentRef = null;
        isInitialized = false;
        isCleaningUp = false;
    }
}
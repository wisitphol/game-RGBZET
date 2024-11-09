using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TournamentLobbyUI : MonoBehaviourPunCallbacks
{
    [Header("Player Objects")]
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
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    private DatabaseReference tournamentRef;
    private string tournamentId;
    private int playerCount;
    private string tournamentName;
    private string currentUsername;
    private bool isReady = false;
    private bool isLeavingRoom = false;
    private bool isInitialized = false;

    void Start()
    {
        InitializeTournament();
    }

    private void InitializeTournament()
    {
        // Load tournament data from PlayerPrefs
        tournamentId = PlayerPrefs.GetString("TournamentId");
        playerCount = PlayerPrefs.GetInt("PlayerCount");
        tournamentName = PlayerPrefs.GetString("TournamentName", "Unnamed Tournament");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        Debug.Log($"TournamentLobbyUI Start - TournamentId: {tournamentId}, PlayerCount: {playerCount}, TournamentName: {tournamentName}");

      /*  if (string.IsNullOrEmpty(tournamentId) || playerCount <= 0)
        {
            //Debug.LogError("TournamentId or PlayerCount is missing.");
            return;
        }*/

        // Initialize Firebase reference
        tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        // Set up UI
        SetupUI();
        SetupButtons();

        // Set up player properties and connect to Photon
        //SetPlayerProperties();
        ConnectToPhoton();

        isInitialized = true;
    }

    private void SetupUI()
    {
        tournamentNameText.text = "Room: " + tournamentName;
        
        // แสดง tournamentId และ copyButton เฉพาะ host
        bool isHost = PhotonNetwork.IsMasterClient;
        
        if (tournamentIdText != null)
        {
            tournamentIdText.gameObject.SetActive(isHost);
            if (isHost)
            {
                tournamentIdText.text = "Room ID: " + tournamentId;
            }
        }

        // ซ่อน/แสดง copyButton
        if (copyButton != null)
        {
            copyButton.gameObject.SetActive(isHost);
        }

        if (notificationPopup != null)
        {
            notificationPopup.SetActive(false);
        }
    }

    private void SetupButtons()
    {
        startTournamentButton.onClick.AddListener(() => SoundOnClick(StartTournament));
        leaveTournamentButton.onClick.AddListener(() => SoundOnClick(LeaveTournament));
        readyButton.onClick.AddListener(() => SoundOnClick(ToggleReady));
        copyButton.onClick.AddListener(() => SoundOnClick(CopyRoomIdToClipboard));
    }

    private void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            OnConnectedToMaster();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void SetPlayerProperties()
    {
        // ตรวจสอบว่าเชื่อมต่อแล้วและพร้อมใช้งาน
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Cannot set player properties - not connected or not in room");
            return;
        }

        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", currentUsername },
            { "IsReady", false }
        };

        try 
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to set player properties: {e.Message}");
        }
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
        
        // อัพเดท UI ตามสถานะ host
        bool isHost = PhotonNetwork.IsMasterClient;
        if (tournamentIdText != null)
        {
            tournamentIdText.gameObject.SetActive(isHost);
        }
        if (copyButton != null)
        {
            copyButton.gameObject.SetActive(isHost);
        }

        SetPlayerProperties();
        UpdateUI();
        AddPlayerToBracket();
        ShowNotification("Joined tournament room");
    }

    void UpdateUI()
    {
        if (!isInitialized) return;

        playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / {playerCount}";
        startTournamentButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        startTournamentButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount == playerCount && AreAllPlayersReady();
        UpdatePlayerList();
    }

    void UpdatePlayerList()
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
                    playerLobby.SetActorNumber(players[i].ActorNumber);
                    string username = players[i].CustomProperties.ContainsKey("username") ? 
                        players[i].CustomProperties["username"].ToString() : 
                        players[i].NickName;
                    bool isReady = players[i].CustomProperties.ContainsKey("IsReady") && 
                        (bool)players[i].CustomProperties["IsReady"];
                    string readyStatus = isReady ? "Ready" : "Not Ready";

                    playerLobby.UpdatePlayerInfo(username, readyStatus);

                    if (players[i].CustomProperties.ContainsKey("iconId"))
                    {
                        int iconId = (int)players[i].CustomProperties["iconId"];
                        playerLobby.UpdatePlayerIcon(iconId);
                    }
                }
            }
            else if (playerObjects[i] != null)
            {
                playerObjects[i].SetActive(false);
            }
        }
    }

    void ToggleReady()
    {
        isReady = !isReady;
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable { { "IsReady", isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Not Ready" : "Ready";
    }

    bool AreAllPlayersReady()
    {
        return PhotonNetwork.PlayerList.All(p => 
            p.CustomProperties.TryGetValue("IsReady", out object isReady) && (bool)isReady);
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
            ShowNotification("Failed to update tournament status");
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
            ShowNotification("Failed to setup tournament bracket");
            yield break;
        }

        var bracketSnapshot = task.Result;
        Dictionary<string, object> bracket = (Dictionary<string, object>)bracketSnapshot.Value;
        var updatedBracket = new Dictionary<string, object>(bracket);

        int playerIndex = 0;
        // จัดการเฉพาะ round 0 และจัดเรียงตาม matchNumber
        var firstRoundMatches = bracket.Where(m => m.Key.StartsWith("round_0_"))
                                    .OrderBy(m => int.Parse(m.Key.Split('_')[3]));

        foreach (var match in firstRoundMatches)
        {
            Dictionary<string, object> matchData = (Dictionary<string, object>)match.Value;
            
            // กำหนดผู้เล่นคนที่ 1
            if (playerIndex < players.Count)
            {
                ((Dictionary<string, object>)matchData["player1"])["username"] = 
                    players[playerIndex].CustomProperties["username"];
                playerIndex++;
            }

            // กำหนดผู้เล่นคนที่ 2
            if (playerIndex < players.Count)
            {
                ((Dictionary<string, object>)matchData["player2"])["username"] = 
                    players[playerIndex].CustomProperties["username"];
                playerIndex++;
            }

            updatedBracket[match.Key] = matchData;
        }

        var updateTask = tournamentRef.Child("bracket").SetValueAsync(updatedBracket);
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception != null)
        {
            Debug.LogError($"Failed to update bracket with shuffled players: {updateTask.Exception}");
            ShowNotification("Failed to update tournament bracket");
        }
    }

    void LeaveTournament()
    {
        if (!isLeavingRoom)
        {
            Debug.Log("Initiating voluntary leave tournament");
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_ForceLeaveTournament", RpcTarget.All);
            }
            else
            {
                StartCoroutine(ForceLeaveTournamentCoroutine());
            }
        }
    }

    [PunRPC]
    private void RPC_ForceLeaveTournament()
    {
        if (!isLeavingRoom)
        {
            Debug.Log("Executing forced leave tournament");
            StartCoroutine(ForceLeaveTournamentCoroutine());
        }
    }

    private IEnumerator ForceLeaveTournamentCoroutine()
    {
        isLeavingRoom = true;

        // Disable all interactive UI elements
        DisableAllButtons();

        // If this is the master client, delete the tournament
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master client is deleting tournament data");
            ShowNotification("Deleting tournament...");
            var deleteTask = tournamentRef.RemoveValueAsync();
            yield return new WaitUntil(() => deleteTask.IsCompleted);

            if (deleteTask.Exception != null)
            {
                Debug.LogError($"Failed to delete tournament: {deleteTask.Exception}");
            }
        }

        ShowNotification("Leaving tournament...");

        // Leave the current room if we're in one
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Leaving Photon room");
            PhotonNetwork.LeaveRoom();
            yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        }

        // Disconnect from Photon
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Disconnecting from Photon");
            PhotonNetwork.Disconnect();
            yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        }

        // Load the menu scene
        Debug.Log("Loading Menu scene");
        SceneManager.LoadScene("Menu");
    }

    private void DisableAllButtons()
    {
        if (startTournamentButton != null) startTournamentButton.interactable = false;
        if (leaveTournamentButton != null) leaveTournamentButton.interactable = false;
        if (readyButton != null) readyButton.interactable = false;
        if (copyButton != null) copyButton.interactable = false;
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
        ShowNotification($"{newPlayer.NickName} joined the tournament");
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player left room: {otherPlayer.NickName}");
            
        if (otherPlayer.IsMasterClient)
        {
            Debug.Log("Master client left, forcing everyone to leave");
            ShowNotification("Host left the tournament");
            photonView.RPC("RPC_ForceLeaveTournament", RpcTarget.All);
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
        if (!isLeavingRoom)
        {
            isLeavingRoom = true;
            ShowNotification("Disconnected from server");
            SceneManager.LoadScene("Menu");
        }
    }

    void CopyRoomIdToClipboard()
    {
        GUIUtility.systemCopyBuffer = tournamentId;
        ShowNotification("Tournament ID copied to clipboard");
        Debug.Log("Room ID copied: " + tournamentId);
    }

    void ShowNotification(string message)
    {
        if (notificationText != null && notificationPopup != null)
        {
            notificationText.text = message;
            notificationPopup.SetActive(true);
            StartCoroutine(HideNotificationAfterDelay(3f));
            Debug.Log($"Notification: {message}");
        }
    }

    IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notificationPopup != null)
        {
            notificationPopup.SetActive(false);
        }
    }

    void SoundOnClick(System.Action buttonAction)
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
        if (!isLeavingRoom)
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
        }

        // Stop all coroutines
        StopAllCoroutines();
    }
}
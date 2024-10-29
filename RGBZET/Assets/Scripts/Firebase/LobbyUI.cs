using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public TMP_Text roomCodeText;
    public TMP_Text playerCountText;

    public Button leaveRoomButton;
    public Button startGameButton;
    public Button readyButton;
    public Button copyButton;
    public GameObject notificationPopup;
    public TMP_Text notificationText;

    private DatabaseReference databaseRef;
    private string roomId;
    private string hostUserId;
    private FirebaseAuth auth;
    private int maxPlayers;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        roomId = PlayerPrefs.GetString("RoomId");
        hostUserId = PlayerPrefs.GetString("HostUserId");
        roomCodeText.text = "Room ID: " + roomId;
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(roomId);

        startGameButton.onClick.AddListener(() => SoundOnClick(StartGame));
        readyButton.onClick.AddListener(() => SoundOnClick(ToggleReady));
        leaveRoomButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            if (auth.CurrentUser.UserId == hostUserId)
            {
                StartCoroutine(DestroyRoomAndReturnToMainGame());
            }
            else
            {
                PhotonNetwork.LeaveRoom();
            }
        }));

        copyButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            CopyRoomIdToClipboard();
        }));

        notificationPopup.SetActive(false);

        UpdateUI();
    }

    void UpdateUI()
    {
        UpdatePlayerCount();
        UpdatePlayerList();
        UpdateStartButtonVisibility();
        UpdateReadyButtonVisibility();
    }

    void UpdatePlayerCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / {maxPlayers}";
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
                PlayerLobby2 playerLobby = playerObjects[i].GetComponent<PlayerLobby2>();

                if (playerLobby != null)
                {
                    playerLobby.SetActorNumber(players[i].ActorNumber);
                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;
                    bool isReady = players[i].CustomProperties.ContainsKey("IsReady") && (bool)players[i].CustomProperties["IsReady"];
                    string readyStatus = isReady ? "Ready" : "Not Ready";
                    playerLobby.UpdatePlayerInfo(username, readyStatus);

                    if (players[i].CustomProperties.ContainsKey("iconId"))
                    {
                        int iconId = (int)players[i].CustomProperties["iconId"];
                        playerLobby.UpdatePlayerIcon(iconId);
                    }

                    Debug.Log($"Updating Player {i + 1}: Name={username}, Ready={readyStatus}");
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

    void UpdateStartButtonVisibility()
    {
        bool isHost = auth.CurrentUser.UserId == hostUserId;
        bool allPlayersReady = PhotonNetwork.PlayerList.All(p => p.CustomProperties.ContainsKey("IsReady") && (bool)p.CustomProperties["IsReady"]);
        bool allPlayersJoined = PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers;

        if (maxPlayers == 1)
        {
            startGameButton.gameObject.SetActive(isHost);
        }
        else
        {
            startGameButton.gameObject.SetActive(isHost && allPlayersReady && allPlayersJoined);
        }

        if (isHost)
        {
            string statusMessage = maxPlayers == 1 ? "Ready to start" :
                                   !allPlayersJoined ? "Waiting for players" :
                                   !allPlayersReady ? "Waiting for ready" :
                                   "Ready to start";
            ShowNotification(statusMessage);
        }

        Debug.Log($"Start button visibility updated: {startGameButton.gameObject.activeSelf}");
    }

    void UpdateReadyButtonVisibility()
    {
        readyButton.gameObject.SetActive(maxPlayers > 1);
        if (maxPlayers > 1)
        {
            UpdateReadyButtonText();
        }
    }

    void UpdateReadyButtonText()
    {
        bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsReady") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Not Ready" : "Ready";
        Debug.Log($"Ready button text updated: {readyButton.GetComponentInChildren<TMP_Text>().text}");
    }

    void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;

                PhotonNetwork.IsMessageQueueRunning = false;
                photonView.RPC("RPC_StartGame", RpcTarget.AllBuffered);
            }
            else
            {
                ShowNotification("Not all players joined");
            }
        }
        else
        {
            ShowNotification("Only host can start");
        }
    }

    public IEnumerator LoadingScreen()
    {
        SceneManager.LoadScene("Loading");
        yield return new WaitForSeconds(2f);
    }

    [PunRPC]
    void RPC_StartGame()
    {
        Debug.Log("Starting game...");
        PhotonNetwork.LoadLevel("Card sample 2");
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    void ToggleReady()
    {
        bool currentReadyState = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsReady") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable { { "IsReady", !currentReadyState } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        Debug.Log($"Ready state toggled. New state: {!currentReadyState}");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log($"Player properties updated for {targetPlayer.NickName}");
        if (changedProps.ContainsKey("IsReady"))
        {
            Debug.Log($"IsReady state changed to {changedProps["IsReady"]}");
        }
        UpdateUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateUI();
        ShowNotification($"{newPlayer.NickName} joined");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
        ShowNotification($"{otherPlayer.NickName} left");

        if (otherPlayer.UserId == hostUserId)
        {
            ShowNotification("Host left. Returning to menu");
            StartCoroutine(DestroyRoomAndReturnToMainGame());
        }
    }

    public override void OnJoinedRoom()
    {
        string username = AuthManager.Instance.GetCurrentUsername();
        bool isHost = PhotonNetwork.IsMasterClient;

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", username },
            { "isHost", isHost },
            { "IsReady", maxPlayers == 1 }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Debug.Log($"Joined room. Username: {username}, IsHost: {isHost}, MaxPlayers: {maxPlayers}");
        UpdateUI();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }

    void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationPopup.SetActive(true);
        StartCoroutine(HideNotificationAfterDelay(3f));
        Debug.Log($"Notification: {message}");
    }

    IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPopup.SetActive(false);
    }

    private IEnumerator DestroyRoomAndReturnToMainGame()
    {
        if (auth.CurrentUser.UserId == hostUserId)
        {
            yield return new WaitForSeconds(1f);

            var task = databaseRef.RemoveValueAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to remove room data from Firebase.");
            }
            else
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.LeaveRoom();
            }

            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene("Menu");
        }
        else
        {
            Debug.Log("Player is not the host, so the room will not be destroyed.");
        }
    }

    void CopyRoomIdToClipboard()
    {
        GUIUtility.systemCopyBuffer = roomId;
        ShowNotification("Room ID copied");
        Debug.Log("Room ID copied: " + roomId);
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
}
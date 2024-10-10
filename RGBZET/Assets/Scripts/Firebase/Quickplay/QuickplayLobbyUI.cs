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

public class QuickplayLobbyUI : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public TMP_Text roomCodeText;
    public TMP_Text playerCountText;
    public TMP_Text feedbackText;
    public Button leaveRoomButton;
    public Button startGameButton;

    private DatabaseReference databaseRef;
    private string roomId;
    private FirebaseAuth auth;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        roomId = PlayerPrefs.GetString("RoomId");
        roomCodeText.text = "Room Code: " + roomId;
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("quickplay").Child(roomId);

        startGameButton.onClick.AddListener(() => SoundOnClick(StartGame));
        leaveRoomButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            PhotonNetwork.LeaveRoom();
        }));

        UpdateUI();
    }

    void UpdateUI()
    {
        UpdatePlayerCount();
        UpdatePlayerList();
        UpdateStartButtonVisibility();
    }

    void UpdatePlayerCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / 4";
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

                PlayerLobbyQ playerLobby = playerObjects[i].GetComponent<PlayerLobbyQ>();

                if (playerLobby != null)
                {
                    playerLobby.SetActorNumber(players[i].ActorNumber);

                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;

                    playerLobby.UpdatePlayerInfo(username, players[i].IsMasterClient ? "Host" : "Waiting");

                    if (players[i].CustomProperties.ContainsKey("iconId"))
                    {
                        int iconId = (int)players[i].CustomProperties["iconId"];
                        playerLobby.UpdatePlayerIcon(iconId);
                    }

                    Debug.Log($"Updating Player {i + 1}: Name={username}, IsMasterClient={players[i].IsMasterClient}");
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
        bool allPlayersJoined = PhotonNetwork.CurrentRoom.PlayerCount == 4;
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && allPlayersJoined);

        string statusMessage = !allPlayersJoined ? "Waiting for more players to join..." :
                               PhotonNetwork.IsMasterClient ? "Ready to start the game!" :
                               "Waiting for host to start the game...";
        DisplayFeedback(statusMessage);
    }

    void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;

                PhotonNetwork.IsMessageQueueRunning = false;
                photonView.RPC("RPC_StartGame", RpcTarget.AllBuffered);
            }
            else
            {
                DisplayFeedback("Not all players have joined yet.");
            }
        }
        else
        {
            DisplayFeedback("Only the host can start the game.");
        }
    }

    [PunRPC]
    void RPC_StartGame()
    {
        Debug.Log("Starting game...");
        PhotonNetwork.LoadLevel("Card sample Q");
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateUI();
        DisplayFeedback($"{newPlayer.NickName} has joined the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
        DisplayFeedback($"{otherPlayer.NickName} has left the room.");
        
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                DeleteRoomFromFirebase();
            }
            else
            {
                // Update room info in Firebase
                UpdateRoomInfoInFirebase();
            }
        }
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.CountOfPlayersInRooms == 0)
        {
            DeleteRoomFromFirebase();
        }
        SceneManager.LoadScene("Menu");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
        if (PhotonNetwork.CountOfPlayersInRooms == 0)
        {
            DeleteRoomFromFirebase();
        }
        SceneManager.LoadScene("Menu");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master Client switched to: {newMasterClient.NickName}");
        UpdateUI();
        if (newMasterClient.IsLocal)
        {
            DisplayFeedback("You are now the host!");
            UpdateRoomInfoInFirebase();
        }
    }

    void DeleteRoomFromFirebase()
    {
        databaseRef.RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to delete room {roomId} from Firebase: {task.Exception}");
            }
            else
            {
                Debug.Log($"Room {roomId} successfully deleted from Firebase");
            }
        });
    }

    void UpdateRoomInfoInFirebase()
    {
        Dictionary<string, object> roomUpdate = new Dictionary<string, object>
        {
            { "playerCount", PhotonNetwork.CurrentRoom.PlayerCount },
            { "hostUserId", PhotonNetwork.MasterClient.UserId }
        };

        databaseRef.UpdateChildrenAsync(roomUpdate).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to update room info in Firebase: {task.Exception}");
            }
            else
            {
                Debug.Log("Room info updated in Firebase");
            }
        });
    }

    public override void OnJoinedRoom()
    {
        string username = AuthManager.Instance.GetCurrentUsername();
        bool isHost = PhotonNetwork.IsMasterClient;

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", username },
            { "isHost", isHost }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Debug.Log($"Joined room. Username: {username}, IsHost: {isHost}");
        UpdateUI();
        
        if (isHost)
        {
            UpdateRoomInfoInFirebase();
        }
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        Debug.Log($"Feedback: {message}");
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
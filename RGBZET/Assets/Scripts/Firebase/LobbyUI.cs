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
    public TMP_Text playerListText;
    public TMP_Text feedbackText;
    public Button leaveRoomButton;
    public Button startGameButton;
    public Button readyButton;
    public Button copyButton;

    private DatabaseReference databaseRef;
    private string roomId;
    private string hostUserId;
    private FirebaseAuth auth;
    private int maxPlayers;

    void Start()
    {

        auth = FirebaseAuth.DefaultInstance;
        roomId = PlayerPrefs.GetString("RoomId");
        hostUserId = PlayerPrefs.GetString("HostUserId");
        roomCodeText.text = "Room Code: " + roomId;
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(roomId);

        startGameButton.onClick.AddListener(StartGame);
        readyButton.onClick.AddListener(ToggleReady);
        leaveRoomButton.onClick.AddListener(() =>
        {
            if (auth.CurrentUser.UserId == hostUserId)
            {
                StartCoroutine(DestroyRoomAndReturnToMainGame());
            }
            else
            {
                PhotonNetwork.LeaveRoom();
            }
        });

        // เพิ่มการฟังการคลิกปุ่ม copyButton
        copyButton.onClick.AddListener(() =>
        {
            CopyRoomIdToClipboard();
        });

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

    /* void UpdatePlayerList()
     {
         playerListText.text = "Player List:\n";
         foreach (Player player in PhotonNetwork.PlayerList)
         {
             string username = player.CustomProperties.ContainsKey("username") ? player.CustomProperties["username"].ToString() : player.NickName;
             bool isReady = player.CustomProperties.ContainsKey("IsReady") && (bool)player.CustomProperties["IsReady"];
             string readyStatus = isReady ? " (Ready)" : " (Not Ready)";

             playerListText.text += username;
             if (player.UserId == hostUserId)
             {
                 playerListText.text += " (Host)";
             }
             playerListText.text += maxPlayers > 1 ? readyStatus : "";
             playerListText.text += "\n";
         }
         Debug.Log("Player list updated: " + playerListText.text);
     }*/

    void UpdatePlayerList()
    {
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (i < players.Length && playerObjects[i] != null)
            {
                playerObjects[i].SetActive(true);

                // เข้าถึง Playerlobby2 component ของ playerObject
                PlayerLobby2 playerLobby = playerObjects[i].GetComponent<PlayerLobby2>();
                if (playerLobby != null)
                {
                    playerLobby.SetActorNumber(players[i].ActorNumber);

                    // ดึงชื่อผู้เล่นจาก CustomProperties หรือ NickName
                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;

                    // ตรวจสอบสถานะ ready
                    bool isReady = players[i].CustomProperties.ContainsKey("IsReady") && (bool)players[i].CustomProperties["IsReady"];
                    string readyStatus = isReady ? "Ready" : "Not Ready";

                    // อัปเดตข้อมูลใน Playerlobby2
                    playerLobby.UpdatePlayerInfo(username, readyStatus);

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
            string statusMessage = maxPlayers == 1 ? "Ready to start the game!" :
                                   !allPlayersJoined ? "Waiting for more players to join..." :
                                   !allPlayersReady ? "Waiting for all players to be ready..." :
                                   "Ready to start the game!";
            DisplayFeedback(statusMessage);
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
            // ตรวจสอบว่าจำนวนผู้เล่นครบหรือไม่
            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
            {
                //StartCoroutine(LoadingScreen());
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

    public IEnumerator LoadingScreen()
    {
        SceneManager.LoadScene("Loading");

        yield return new WaitForSeconds(2f); // แสดงหน้า Loading เป็นเวลา 2 วินาที

    }

    [PunRPC]
    void RPC_StartGame()
    {
        Debug.Log("Starting game...");
        PhotonNetwork.LoadLevel("Card sample 2");
    }


    void LeaveRoom()
    {
        if (auth.CurrentUser.UserId == hostUserId)
        {
            StartCoroutine(DestroyRoomAndReturnToMainGame());
        }
        else
        {
            PhotonNetwork.LeaveRoom();
        }
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
        DisplayFeedback($"{newPlayer.NickName} has joined the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
        DisplayFeedback($"{otherPlayer.NickName} has left the room.");

        if (otherPlayer.UserId == hostUserId)
        {
            DisplayFeedback("The host has left the room. Returning to main menu...");
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
            { "IsReady", maxPlayers == 1 } // Set initial ready state to true for single player, false for multiplayer
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Debug.Log($"Joined room. Username: {username}, IsHost: {isHost}, MaxPlayers: {maxPlayers}");
        UpdateUI();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        Debug.Log($"Feedback: {message}");
    }

    private IEnumerator DestroyRoomAndReturnToMainGame()
    {
        // ตรวจสอบว่าผู้เล่นเป็น host หรือไม่
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
            SceneManager.LoadScene("Menu"); //scene ก่อนหน้า
        }
        else
        {
            // ถ้าผู้เล่นไม่ใช่ host ไม่ต้องทำอะไร
            Debug.Log("Player is not the host, so the room will not be destroyed.");
        }
    }

    void CopyRoomIdToClipboard()
    {
        GUIUtility.systemCopyBuffer = roomId;  // ก๊อปปี้ roomId ไปที่คลิปบอร์ด
        DisplayFeedback("Room ID copied.");
        Debug.Log("Room ID copied: " + roomId);
    }

}
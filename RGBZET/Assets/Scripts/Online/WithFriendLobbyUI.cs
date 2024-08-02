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

public class WithFriendLobbyUI : MonoBehaviourPunCallbacks
{
    public TMP_Text roomCodeText;
    public TMP_Text playerCountText;
    public TMP_Text playerListText;
    public TMP_Text feedbackText;
    public Button leaveRoomButton;
    public Button startGameButton;

    private DatabaseReference databaseRef;
    private string roomId;
    private string hostUserId;
    private FirebaseAuth auth;
    private int maxPlayers;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        roomId = PlayerPrefs.GetString("RoomId");
        string inputRoomName = PlayerPrefs.GetString("InputRoomName");
        hostUserId = PlayerPrefs.GetString("HostUserId");
        roomCodeText.text = "Room Code: " + roomId;
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(roomId);
        UpdatePlayerCount();
        UpdatePlayerList();

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

        // เพิ่ม Listener สำหรับปุ่ม Start
        startGameButton.onClick.AddListener(StartGame);

        // ซ่อนปุ่ม Start เมื่อเริ่มต้น
        startGameButton.gameObject.SetActive(false);
    }

    void UpdatePlayerCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            playerCountText.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + maxPlayers;
            Debug.Log($"Player count updated: {PhotonNetwork.CurrentRoom.PlayerCount}/{maxPlayers}");

            // ตรวจสอบว่าจำนวนผู้เล่นครบหรือไม่
            CheckStartButtonVisibility();
        }
    }

    void UpdatePlayerList()
    {
        playerListText.text = "Player List:\n";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string username = player.CustomProperties.ContainsKey("username") ? player.CustomProperties["username"].ToString() : player.NickName;
            playerListText.text += username;
            if (player.CustomProperties.ContainsKey("isHost") && (bool)player.CustomProperties["isHost"])
            {
                playerListText.text += " (Host)";
            }
            playerListText.text += "\n";
        }
        Debug.Log(playerListText.text);
    }

    void CheckStartButtonVisibility()
    {
        bool isHost = auth.CurrentUser.UserId == hostUserId;
        bool isRoomFull = PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers;

        startGameButton.gameObject.SetActive(isHost && isRoomFull);
    }

    void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ตรวจสอบว่าจำนวนผู้เล่นครบหรือไม่
            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
            {
                // เริ่มเกมและเปลี่ยนไปยังหน้าเล่นเกม
                photonView.RPC("RPC_StartGame", RpcTarget.All);// ชื่อของ scene หน้าเล่นเกม
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
        // เปลี่ยนฉากสำหรับผู้เล่นทุกคน
        PhotonNetwork.LoadLevel("Card sample Firebase"); // ชื่อของ scene หน้าเล่นเกม
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerCount();
        UpdatePlayerList();
        DisplayFeedback(newPlayer.NickName + " has joined the room.");
        Debug.Log($"{newPlayer.NickName} has joined the room.");

        if (newPlayer.CustomProperties.ContainsKey("isHost") && (bool)newPlayer.CustomProperties["isHost"])
        {
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "isHost", false } };
            newPlayer.SetCustomProperties(playerProperties);
            Debug.Log($"Ensured {newPlayer.NickName} is not set as host");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerCount();
        UpdatePlayerList();
        DisplayFeedback(otherPlayer.NickName + " has left the room.");
        Debug.Log($"{otherPlayer.NickName} has left the room.");

        if (otherPlayer.CustomProperties.ContainsKey("isHost") && (bool)otherPlayer.CustomProperties["isHost"])
        {
            DisplayFeedback("The host has left the room. Connection lost.");
            Debug.Log("The host has left the room. Connection lost.");
            StartCoroutine(DestroyRoomAndReturnToMainGame());
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string username = PlayerPrefs.GetString("username");
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "username", username }, { "isHost", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            Debug.Log($"Host: {username}");
        }
        else
        {
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "isHost", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }

        // Force an immediate properties update
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());

        UpdatePlayerCount();
        UpdatePlayerList();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("menu"); // scene ก่อนหน้า
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        Debug.Log($"DisplayFeedback: {message}");
        StartCoroutine(ClearFeedbackAfterDelay(3f));
    }

    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
    }

    private IEnumerator DestroyRoomAndReturnToMainGame()
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
        SceneManager.LoadScene("menu"); //scene ก่อนหน้า
    }

    private void OnDestroy()
    {
        if (auth.CurrentUser != null && auth.CurrentUser.UserId == hostUserId)
        {
            var task = databaseRef.RemoveValueAsync();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    Debug.LogError("Failed to remove room data from Firebase.");
                }
            });
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CreateWithFriendUI : MonoBehaviourPunCallbacks
{
    public TMP_Dropdown playerCountDropdown;
    public Button createRoomButton;
    public Button backButton;
    public TMP_Text feedbackText;

    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private int playerCount;
    private string userId;
    private string roomId;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        createRoomButton.onClick.AddListener(() =>
        {
            playerCount = GetPlayerCountFromDropdown();
            userId = auth.CurrentUser.UserId;
            Debug.Log($"Create Room Button Clicked: playerCount={playerCount}, userId={userId}");
            if (PhotonNetwork.IsConnectedAndReady)
            {
                CreateRoom();
            }
            else
            {
                DisplayFeedback("Connecting to Master Server...");
                PhotonNetwork.ConnectUsingSettings();
            }
        });

        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("menu");//Scene ก่อนหน้า
        });
    }

    public override void OnConnectedToMaster()
    {
        DisplayFeedback("Connected to Master Server.");
        Debug.Log("OnConnectedToMaster called");
        CreateRoom();
    }

    void CreateRoom()
    {
        roomId = GenerateRoomId();

        Dictionary<string, object> withfriendsData = new Dictionary<string, object>
        {
            { "roomId", roomId },
            { "playerCount", playerCount },
            { "hostUserId", userId }
        };

        databaseRef.Child("withfriends").Child(roomId).SetValueAsync(withfriendsData).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                DisplayFeedback("Failed to create room.");
                Debug.LogError($"Failed to create room: {task.Exception}");
            }
            else
            {
                DisplayFeedback("Room created successfully.");
                Debug.Log($"Room created successfully: roomId={roomId}, playerCount={playerCount}, hostUserId={userId}");
                PhotonNetwork.CreateRoom(roomId, new Photon.Realtime.RoomOptions { MaxPlayers = (byte)playerCount });
                PlayerPrefs.SetString("RoomId", roomId);
                PlayerPrefs.SetString("HostUserId", userId);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private int GetPlayerCountFromDropdown()
    {
        switch (playerCountDropdown.value)
        {
            case 0: return 1;
            case 1: return 2;
            case 2: return 3;
            case 3: return 4;
            default: return 2;
        }
    }

    private string GenerateRoomId(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public override void OnCreatedRoom()
    {
        DisplayFeedback("Room created successfully.");
        Debug.Log("OnCreatedRoom called");
        SetPlayerUsername(() => {
            SceneManager.LoadScene("Lobby");//scene หน้า lobby
        });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        DisplayFeedback($"Failed to create room: {message}");
        Debug.LogError($"Failed to create room: {message}");
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        Debug.Log($"DisplayFeedback: {message}");
    }

    private void SetPlayerUsername(System.Action onComplete = null)
    {
        string userId = auth.CurrentUser.UserId;
        databaseRef.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string username = snapshot.Child("username").Value.ToString();
                    Debug.Log($"Setting username: {username} for userId: {userId}");
                    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "username", username }, { "isHost", true } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
                    Debug.Log($"Custom Properties Set: {PhotonNetwork.LocalPlayer.CustomProperties["username"]}, {PhotonNetwork.LocalPlayer.CustomProperties["isHost"]}");
                    
                    // Force an immediate properties update
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());
                    
                    onComplete?.Invoke();
                }
                else
                {
                    Debug.LogError("Failed to find user data in Firebase.");
                    onComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogError("Failed to get user data from Firebase.");
                onComplete?.Invoke();
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom called");
    }
}
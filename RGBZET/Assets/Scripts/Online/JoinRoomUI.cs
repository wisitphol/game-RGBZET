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

public class JoinRoomUI : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomCodeInputField;
    public Button joinRoomButton;
    public Button backButton;
    public TMP_Text feedbackText;

    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private string roomId;
    private string userId;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        userId = auth.CurrentUser.UserId;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        joinRoomButton.onClick.AddListener(() =>
        {
            roomId = roomCodeInputField.text;
            if (PhotonNetwork.IsConnectedAndReady)
            {
                JoinRoom(roomId);
            }
            else
            {
                DisplayFeedback("Connecting to Master Server...");
                PhotonNetwork.ConnectUsingSettings();
            }
        });

        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Menu");//ไป scene ก่อนหน้านี้
        });

          // เชื่อมต่อกับ Master Server หากยังไม่ได้เชื่อมต่อ
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        DisplayFeedback("Connected to Master Server.");
        JoinRoom(roomId);
    }

    void JoinRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            DisplayFeedback("Please enter a valid room code.");
            return;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRoom(roomId);
        }
        else
        {
            DisplayFeedback("Connecting to Master Server...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnJoinedRoom()
    {
        DisplayFeedback("Joined room successfully.");
        SetPlayerUsername(() =>
        {
            PlayerPrefs.SetString("RoomId", roomId);
            SceneManager.LoadScene("Lobby");//เพิ่ม scene lobby
        });
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        DisplayFeedback($"Failed to join room: {message}");
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
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
                    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "username", username } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
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
}
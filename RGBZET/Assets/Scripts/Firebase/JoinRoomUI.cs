using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

public class JoinRoomUI : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomCodeInputField;
    public Button joinRoomButton;
    public Button cancelButton;
    public Button backButton;
    public TMP_Text feedbackText;

    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private string roomId;
    private string userId;
    private string roomType;

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
                CheckRoomTypeAndJoin(roomId);
            }
            else
            {
                DisplayFeedback("Connecting to Master Server...");
                PhotonNetwork.ConnectUsingSettings();
            }
        });

        cancelButton.onClick.AddListener(() =>
        {
            roomCodeInputField.text = "";  // ลบข้อมูลใน input field
            DisplayFeedback("Room code cleared.");  // แสดงข้อความ feedback ว่าข้อมูลถูกลบแล้ว
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
        if (!string.IsNullOrEmpty(roomId))
        {
            CheckRoomTypeAndJoin(roomId);
        }
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
            if (roomType == "tournament")
            {
                PhotonNetwork.LoadLevel("LobbyTournament");
            }
            else if (roomType == "withfriend")
            {
                PhotonNetwork.LoadLevel("Lobby");
            }
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

    void CheckRoomTypeAndJoin(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            DisplayFeedback("Please enter a valid room code.");
            return;
        }

        StartCoroutine(CheckRoomTypeAndJoinCoroutine(roomId));
    }

    IEnumerator CheckRoomTypeAndJoinCoroutine(string roomId)
    {
        DisplayFeedback("Checking room type...");

        // Check if the room is a tournament
        var tournamentTask = FirebaseDatabase.DefaultInstance.GetReference("tournaments").GetValueAsync();
        yield return new WaitUntil(() => tournamentTask.IsCompleted);

        if (tournamentTask.Exception != null)
        {
            Debug.LogError($"Failed to get tournament data: {tournamentTask.Exception}");
        }
        else if (tournamentTask.Result != null && tournamentTask.Result.Exists)
        {
            foreach (var tournamentChild in tournamentTask.Result.Children)
            {
                if (tournamentChild.Child("tournamentId").Value.ToString() == roomId)
                {
                    roomType = "tournament";
                    string tournamentName = tournamentChild.Child("name").Value.ToString();
                    PlayerPrefs.SetString("TournamentName", tournamentName);
                    PlayerPrefs.Save();
                    JoinRoom(roomId);
                    yield break;
                }
            }
        }

        // Check if the room is a withfriend room
        var withfriendTask = databaseRef.Child(roomId).GetValueAsync();
        yield return new WaitUntil(() => withfriendTask.IsCompleted);

        if (withfriendTask.Exception != null)
        {
            Debug.LogError($"Failed to get withfriend room data: {withfriendTask.Exception}");
        }
        else if (withfriendTask.Result != null && withfriendTask.Result.Exists)
        {
            roomType = "withfriend";
            JoinRoom(roomId);
            yield break;
        }

        DisplayFeedback("Room not found. Please check the room code.");
    }

  
}
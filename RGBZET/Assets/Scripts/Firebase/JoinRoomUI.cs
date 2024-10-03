using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
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
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        userId = auth.CurrentUser.UserId;
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends");

        joinRoomButton.onClick.AddListener(() => SoundOnClick(() =>
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
        }));

        cancelButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            roomCodeInputField.text = "";  // ลบข้อมูลใน input field
            DisplayFeedback("Room code cleared.");  // แสดงข้อความ feedback ว่าข้อมูลถูกลบแล้ว
        }));

        backButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            SceneManager.LoadScene("Menu");//ไป scene ก่อนหน้านี้
        }));

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
                PhotonNetwork.LoadLevel("TournamentLobby");
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
        databaseRef.Root.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string username = snapshot.Child("username").Value.ToString();
                    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable 
                    { 
                        { "username", username },
                        { "IsReady", false }
                    };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
                    PhotonNetwork.NickName = username;
                    
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisplayFeedback($"Disconnected from Photon: {cause}");
    }

    void SoundOnClick(System.Action buttonAction)
    {
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
            // รอให้เสียงเล่นเสร็จก่อนที่จะทำการเปลี่ยน scene
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            // ถ้าไม่มีเสียงให้เล่น ให้ทำงานทันที
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        // รอความยาวของเสียงก่อนที่จะทำงาน
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }



}
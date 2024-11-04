using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
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
    public GameObject notificationPopup;
    public TMP_Text notificationText;

    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private string roomId;
    private string userId;
    private string roomType;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private bool isPhotonConnected = false;
    private bool isFirebaseInitialized = false;

    void Start()
    {
        joinRoomButton.interactable = false;
        cancelButton.interactable = false;

        StartCoroutine(InitializeServices());

        joinRoomButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            roomId = roomCodeInputField.text;
            CheckRoomTypeAndJoin(roomId);
        }));

        cancelButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            roomCodeInputField.text = "";
            ShowNotification("Room code cleared");
        }));

        backButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            SceneManager.LoadScene("Menu");
        }));

        notificationPopup.SetActive(false);
    }

    IEnumerator InitializeServices()
    {
        ShowNotification("Initializing...");

        yield return StartCoroutine(InitializeFirebase());

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            isPhotonConnected = true;
        }

        yield return new WaitUntil(() => isFirebaseInitialized && isPhotonConnected);

        ShowNotification("Ready to join");
        joinRoomButton.interactable = true;
        cancelButton.interactable = true;
    }

    IEnumerator InitializeFirebase()
    {
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            userId = auth.CurrentUser.UserId;
            databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends");
            isFirebaseInitialized = true;
        }
        else
        {
            Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
        }
    }

    public override void OnConnectedToMaster()
    {
        isPhotonConnected = true;
        ShowNotification("Connected to server");
    }

    void CheckRoomTypeAndJoin(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            ShowNotification("Enter valid room code");
            return;
        }

        StartCoroutine(CheckRoomTypeAndJoinCoroutine(roomId));
    }

    IEnumerator CheckRoomTypeAndJoinCoroutine(string roomId)
    {
        ShowNotification("Checking room...");

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

        ShowNotification("Room not found");
    }

    void JoinRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            ShowNotification("Enter valid room code");
            return;
        }

        PhotonNetwork.JoinRoom(roomId);
    }

    public override void OnJoinedRoom()
    {
        ShowNotification("Joined room");
        PhotonNetwork.IsMessageQueueRunning = false; // หยุดคิวข้อความขณะโหลดซีน

        SetPlayerUsername(() =>
        {
            if (roomType == "tournament")
            {
                PhotonNetwork.LoadLevel("TournamentLobby");
            }
            else if (roomType == "withfriend")
            {
                PhotonNetwork.LoadLevel("Lobby");
            }

            StartCoroutine(ResumeMessageQueueAfterSceneLoad()); // เรียกใช้ Coroutine เพื่อเปิดคิวข้อความใหม่หลังจากโหลดเสร็จ
        });
    }

    private IEnumerator ResumeMessageQueueAfterSceneLoad()
    {
        // รอให้ซีนโหลดเสร็จ
        yield return new WaitForSeconds(1f);
        PhotonNetwork.IsMessageQueueRunning = true; // เปิดใช้งานคิวข้อความใหม่
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ShowNotification("Failed to join room");
    }

    void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationPopup.SetActive(true);
        StartCoroutine(HideNotificationAfterDelay(3f));
    }

    IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPopup.SetActive(false);
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
        isPhotonConnected = false;
        ShowNotification("Disconnected");
        joinRoomButton.interactable = false;
        StartCoroutine(InitializeServices());
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
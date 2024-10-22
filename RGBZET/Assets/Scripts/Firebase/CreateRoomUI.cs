using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

public class CreateRoomUI : MonoBehaviourPunCallbacks
{
    [Header("Player Count Buttons")]
    public Button[] playerCountButtons;

    [Header("Time Buttons")]
    public Button[] timeButtons;

    [Header("Other UI Elements")]
    public Button createRoomButton;
    public Button backButton;
    public GameObject notificationPopup;
    public TMP_Text notificationText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private int playerCount = 1;
    private int selectedTime = 10;
    private string userId;
    private string roomId;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        SetupPlayerCountButtons();
        SetupTimeButtons();

        createRoomButton.onClick.AddListener(() => SoundOnClick(() =>
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log($"Create Room Button Clicked: playerCount={playerCount}, userId={userId}");

            if (PhotonNetwork.IsConnectedAndReady)
            {
                CreateRoom();
            }
            else
            {
                ShowNotification("Connecting...");
                PhotonNetwork.ConnectUsingSettings();
            }
        }));

        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));

        notificationPopup.SetActive(false);
    }

    void SetupPlayerCountButtons()
    {
        for (int i = 0; i < playerCountButtons.Length; i++)
        {
            int count = i + 1;
            playerCountButtons[i].onClick.AddListener(() => SetPlayerCount(count));
        }
        UpdatePlayerCountButtonVisuals();
    }

    void SetupTimeButtons()
    {
        for (int i = 0; i < timeButtons.Length; i++)
        {
            int time = (i == timeButtons.Length - 1) ? -1 : (i + 1) * 10;
            timeButtons[i].onClick.AddListener(() => SetTime(time));
        }
        UpdateTimeButtonVisuals();
    }

    void SetPlayerCount(int count)
    {
        playerCount = count;
        UpdatePlayerCountButtonVisuals();
    }

    void SetTime(int time)
    {
        selectedTime = time;
        UpdateTimeButtonVisuals();
    }

    void UpdatePlayerCountButtonVisuals()
    {
        for (int i = 0; i < playerCountButtons.Length; i++)
        {
            bool isSelected = (i + 1) == playerCount;
            RectTransform buttonRect = playerCountButtons[i].GetComponent<RectTransform>();
            
            // ปรับขนาดปุ่ม
            if (isSelected)
            {
                buttonRect.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else
            {
                buttonRect.localScale = Vector3.one;
            }
        }
    }

    void UpdateTimeButtonVisuals()
    {
        for (int i = 0; i < timeButtons.Length; i++)
        {
            int time = (i == timeButtons.Length - 1) ? -1 : (i + 1) * 10;
            bool isSelected = time == selectedTime;
            RectTransform buttonRect = timeButtons[i].GetComponent<RectTransform>();
            
            // ปรับขนาดปุ่ม
            if (isSelected)
            {
                buttonRect.localScale = new Vector3(1.2f, 1.2f, 1.2f); // ขนาดใหญ่ขึ้น 20%
            }
            else
            {
                buttonRect.localScale = Vector3.one; // ขนาดปกติ
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        ShowNotification("Connected");
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
            { "hostUserId", userId },
            { "gameTime", selectedTime }
        };

        databaseRef.Child("withfriends").Child(roomId).SetValueAsync(withfriendsData).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                ShowNotification("Room creation failed");
                Debug.LogError($"Failed to create room: {task.Exception}");
            }
            else
            {
                ShowNotification("Room created");
                Debug.Log($"Room created successfully: roomId={roomId}, playerCount={playerCount}, hostUserId={userId}");
                ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "gameTime", selectedTime }
                };

                RoomOptions roomOptions = new RoomOptions
                {
                    MaxPlayers = (byte)playerCount,
                    CustomRoomProperties = roomProperties,
                    CustomRoomPropertiesForLobby = new string[] { "gameTime" }
                };

                PhotonNetwork.CreateRoom(roomId, roomOptions);
                PlayerPrefs.SetString("RoomId", roomId);
                PlayerPrefs.SetString("HostUserId", userId);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
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
        ShowNotification("Room created");
        Debug.Log("OnCreatedRoom called");
        SetPlayerUsername(() =>
        {
            SceneManager.LoadScene("Lobby");
        });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ShowNotification("Room creation failed");
        Debug.LogError($"Failed to create room: {message}");
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
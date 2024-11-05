using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using System.Linq;

public class PlayUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button quickplayButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private string userId;
    private string roomId;
    private string hostUserId;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        userId = auth.CurrentUser.UserId;

        SetupButtons();
        notificationPopup.SetActive(false);
    }

    void SetupButtons()
    {
        createRoomButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Createroom")));
        quickplayButton.onClick.AddListener(() => SoundOnClick(OnQuickplayButtonClicked));
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
    }

    void OnQuickplayButtonClicked()
    {
        ShowNotification("Connecting to Quickplay...");
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            SetPlayerNameAndJoinQuickplay();
        }
    }

    private void SetPlayerNameAndJoinQuickplay()
    {
        string playerName = AuthManager.Instance.GetCurrentUsername();
        PhotonNetwork.NickName = playerName;

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", playerName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        ShowNotification("Joining Quickplay...");
        PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable { { "GameType", "Quickplay" } }, 4);
    }

    public override void OnConnectedToMaster()
    {
        ShowNotification("Connected. Setting up player...");
        SetPlayerNameAndJoinQuickplay();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        ShowNotification("Creating new Quickplay room...");
        CreateQuickplayRoom();
    }

    void CreateQuickplayRoom()
    {
        string roomId = GenerateRoomId();
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "GameType", "Quickplay" },
            { "roomId", roomId },
            { "hostUserId", userId }
        },
            CustomRoomPropertiesForLobby = new string[] { "GameType" }
        };

        Dictionary<string, object> roomData = new Dictionary<string, object>
        {
            { "roomId", roomId },
            { "playerCount", 1 },
            { "hostUserId", userId },
            { "gameType", "Quickplay" }
        };

        databaseRef.Child("quickplay").Child(roomId).SetValueAsync(roomData).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                ShowNotification("Failed to create Quickplay room");
            }
            else
            {
                PhotonNetwork.CreateRoom(roomId, roomOptions);
            }
        });
    }

    public override void OnJoinedRoom()
    {
        // ตรวจสอบค่า roomId และ hostUserId ใน CustomProperties ของห้องก่อน
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("roomId", out object roomIdValue))
        {
            roomId = roomIdValue.ToString();
        }
        else
        {
            Debug.LogError("RoomId is missing in CustomProperties");
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("hostUserId", out object hostUserIdValue))
        {
            hostUserId = hostUserIdValue.ToString();
        }
        else
        {
            Debug.LogError("HostUserId is missing in CustomProperties");
        }

        // ถ้ามี roomId เรียก UpdatePlayerCountInFirebase
        if (!string.IsNullOrEmpty(roomId))
        {
            UpdatePlayerCountInFirebase(roomId);
        }
        else
        {
            Debug.LogError("roomId is null or empty. Cannot update player count in Firebase.");
        }

        ShowNotification("Joined room. Loading lobby...");
        SceneManager.LoadScene("QuickplayLobby");
    }

    private void UpdatePlayerCountInFirebase(string roomId)
    {
        DatabaseReference roomRef = databaseRef.Child("quickplay").Child(roomId);
        roomRef.RunTransaction(mutableData =>
        {
            Dictionary<string, object> roomData = mutableData.Value as Dictionary<string, object>;
            if (roomData != null)
            {
                if (roomData.ContainsKey("playerCount"))
                {
                    roomData["playerCount"] = PhotonNetwork.CurrentRoom.PlayerCount;
                }
                mutableData.Value = roomData;
            }
            return TransactionResult.Success(mutableData);
        });
    }

    private string GenerateRoomId(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        return new string(System.Linq.Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
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

    void OnDestroy()
    {
        createRoomButton.onClick.RemoveAllListeners();
        quickplayButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.CountOfPlayersInRooms == 0 && PhotonNetwork.CurrentRoom != null)
        {
            // ตรวจสอบว่ามีค่า roomId ใน CustomProperties ของห้อง
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("roomId", out object roomId))
            {
                // ลบข้อมูลห้องใน Firebase ตาม roomId
                databaseRef.Child("quickplay").Child(roomId.ToString()).RemoveValueAsync();
            }
        }
    }
}
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

public class ModeCreateroomUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button quickplayButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private string userId;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        userId = auth.CurrentUser.UserId;

        SetupButtons();
    }

    void SetupButtons()
    {
        createRoomButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("CreateFriend")));
        quickplayButton.onClick.AddListener(() => SoundOnClick(OnQuickplayButtonClicked));
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
    }

    void OnQuickplayButtonClicked()
    {
        DisplayFeedback("Connecting to Quickplay...");
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

        DisplayFeedback("Joining Quickplay...");
        PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable { { "GameType", "Quickplay" } }, 4);
    }

    public override void OnConnectedToMaster()
    {
        DisplayFeedback("Connected to Master. Setting up player...");
        SetPlayerNameAndJoinQuickplay();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        DisplayFeedback("Creating new Quickplay room...");
        CreateQuickplayRoom();
    }

    void CreateQuickplayRoom()
    {
        string roomId = GenerateRoomId();
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "GameType", "Quickplay" } },
            CustomRoomPropertiesForLobby = new string[] { "GameType" }
        };

        // Create room in Firebase
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
                DisplayFeedback("Failed to create Quickplay room in Firebase.");
            }
            else
            {
                PhotonNetwork.CreateRoom(roomId, roomOptions);
            }
        });
    }

    public override void OnJoinedRoom()
    {
        string roomId = PhotonNetwork.CurrentRoom.Name;
        PlayerPrefs.SetString("RoomId", roomId);
        PlayerPrefs.Save();

        // Update player count in Firebase
        UpdatePlayerCountInFirebase(roomId);

        DisplayFeedback("Joined Quickplay room. Loading lobby...");
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

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        Debug.Log($"Feedback: {message}");
    }

    void OnDestroy()
    {
        createRoomButton.onClick.RemoveAllListeners();
        quickplayButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }

    public override void OnLeftRoom()
    {
        // Remove room from Firebase when the last player leaves
        if (PhotonNetwork.CountOfPlayersInRooms == 0)
        {
            string roomId = PlayerPrefs.GetString("RoomId");
            databaseRef.Child("quickplay").Child(roomId).RemoveValueAsync();
        }
    }
}
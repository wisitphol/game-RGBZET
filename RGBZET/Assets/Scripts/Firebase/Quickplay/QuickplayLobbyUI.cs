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
using System;

public class QuickplayLobbyUI : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public TMP_Text roomCodeText;
    public TMP_Text playerCountText;
    public TMP_Text feedbackText;
    public Button leaveRoomButton;
    public TMP_Text notificationText;
    
    [SerializeField] private GameObject timerUI;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private TextMeshProUGUI timerText;
    
    [SerializeField] private GameObject countdownUI;
    [SerializeField] private Image countdownFillImage;
    [SerializeField] private TextMeshProUGUI countdownText;

    private DatabaseReference databaseRef;
    private string roomId;
    private FirebaseAuth auth;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    private const float LOBBY_TIME = 180f; // 3 minutes
    private float remainingLobbyTime;
    private bool isLobbyTimerRunning = true;
    private bool isInRoom = false;
    private float notificationDuration = 5f;
    private Coroutine currentNotificationCoroutine;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        roomId = PlayerPrefs.GetString("RoomId");
        roomCodeText.text = "Room Code: " + roomId;
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("quickplay").Child(roomId);

        leaveRoomButton.onClick.AddListener(() => SoundOnClick(() => LeaveRoom()));

        countdownUI.SetActive(false);

        if (PhotonNetwork.InRoom)
        {
            isInRoom = true;
            if (PhotonNetwork.IsMasterClient)
            {
                InitializeLobbyTimer();
            }
            UpdateUI();
        }
    }

    void Update()
    {
        if (!isInRoom) return;

        if (isLobbyTimerRunning)
        {
            UpdateLobbyTimer();
        }
    }

    void UpdateUI()
    {
        UpdatePlayerCount();
        UpdatePlayerList();
    }

    void UpdatePlayerCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / 4";

            if (PhotonNetwork.CurrentRoom.PlayerCount == 4 && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("StartGameCountdown", RpcTarget.All);
            }
        }
    }

    void UpdatePlayerList()
    {
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (i < players.Length && playerObjects[i] != null)
            {
                playerObjects[i].SetActive(true);
                PlayerLobbyQ playerLobby = playerObjects[i].GetComponent<PlayerLobbyQ>();
                if (playerLobby != null)
                {
                    playerLobby.SetActorNumber(players[i].ActorNumber);
                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;
                    playerLobby.UpdatePlayerInfo(username, "Waiting");
                    if (players[i].CustomProperties.ContainsKey("iconId"))
                    {
                        int iconId = (int)players[i].CustomProperties["iconId"];
                        playerLobby.UpdatePlayerIcon(iconId);
                    }
                }
            }
            else if (playerObjects[i] != null)
            {
                playerObjects[i].SetActive(false);
            }
        }
    }

    [PunRPC]
    void StartGameCountdown()
    {
        isLobbyTimerRunning = false;
        timerUI.SetActive(false);
        countdownUI.SetActive(true);
        StartCoroutine(GameStartCountdown());
    }

    private IEnumerator GameStartCountdown()
    {
        float countdownTime = 5f;
        while (countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;
            int secondsLeft = Mathf.CeilToInt(countdownTime);
            countdownText.text = secondsLeft.ToString();
            countdownFillImage.fillAmount = countdownTime / 5f;
            yield return null;
        }
        StartGame();
    }

    void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        SceneManager.LoadScene("Card sample Q");
    }

    void InitializeLobbyTimer()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("LobbyStartTime"))
        {
            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "LobbyStartTime", PhotonNetwork.Time }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        remainingLobbyTime = LOBBY_TIME;
        UpdateLobbyTimerUI();
    }

    void UpdateLobbyTimer()
    {
        if (!isInRoom || !PhotonNetwork.InRoom) return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LobbyStartTime", out object startTimeObj))
        {
            double startTime = (double)startTimeObj;
            double elapsedTime = PhotonNetwork.Time - startTime;
            remainingLobbyTime = Mathf.Max(0, LOBBY_TIME - (float)elapsedTime);

            UpdateLobbyTimerUI();

            if (remainingLobbyTime <= 0)
            {
                isLobbyTimerRunning = false;
                ReturnToMenu();
            }
        }
    }

    void UpdateLobbyTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingLobbyTime / 60);
            int seconds = Mathf.FloorToInt(remainingLobbyTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (timerFillImage != null)
        {
            float fillAmount = remainingLobbyTime / LOBBY_TIME;
            timerFillImage.fillAmount = fillAmount;
        }
    }

    void ReturnToMenu()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            DeleteRoomFromFirebase();
        }
        LeaveRoom();
    }

    void LeaveRoom()
    {
        isInRoom = false;
        isLobbyTimerRunning = false;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        isInRoom = true;
        string username = AuthManager.Instance.GetCurrentUsername();
        bool isHost = PhotonNetwork.IsMasterClient;

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", username },
            { "isHost", isHost }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        UpdateUI();
        
        if (isHost)
        {
            UpdateRoomInfoInFirebase();
            InitializeLobbyTimer();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateUI();
        ShowNotification($"{newPlayer.NickName} has joined the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
        ShowNotification($"{otherPlayer.NickName} has left the room.");

        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                DeleteRoomFromFirebase();
            }
            else
            {
                UpdateRoomInfoInFirebase();
            }
        }
    }

    private void ShowNotification(string message)
    {
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }
        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
    }

    private IEnumerator ShowNotificationCoroutine(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(notificationDuration);
        
        notificationText.gameObject.SetActive(false);
        currentNotificationCoroutine = null;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }

    void DeleteRoomFromFirebase()
    {
        databaseRef.RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to delete room {roomId} from Firebase: {task.Exception}");
            }
            else
            {
                Debug.Log($"Room {roomId} successfully deleted from Firebase");
            }
        });
    }

    void UpdateRoomInfoInFirebase()
    {
        Dictionary<string, object> roomUpdate = new Dictionary<string, object>
        {
            { "playerCount", PhotonNetwork.CurrentRoom.PlayerCount },
            { "hostUserId", PhotonNetwork.MasterClient.UserId }
        };

        databaseRef.UpdateChildrenAsync(roomUpdate).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to update room info in Firebase: {task.Exception}");
            }
            else
            {
                Debug.Log("Room info updated in Firebase");
            }
        });
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        Debug.Log($"Feedback: {message}");
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
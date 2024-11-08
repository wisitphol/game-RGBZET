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
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField roomCodeInputField;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private TMP_Text notificationText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    // Database references
    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private string roomId;
    private string userId;
    private string roomType;

    // State management
    private bool isInitialized = false;
    private bool isProcessing = false;
    private int retryAttempts = 0;
    private const int MAX_RETRIES = 3;
    private const float RETRY_DELAY = 1f;
    private const float CONNECTION_TIMEOUT = 10f;

    void Start()
    {
        DisableInteractiveElements();
        StartCoroutine(InitializeServicesWithRetry());
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(() => SoundOnClick(OnJoinRoomButtonClicked));
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(() => SoundOnClick(OnCancelButtonClicked));
        
        if (backButton != null)
            backButton.onClick.AddListener(() => SoundOnClick(OnBackButtonClicked));
        
        if (roomCodeInputField != null)
            roomCodeInputField.onValueChanged.AddListener(OnRoomCodeInput);
    }

    private IEnumerator InitializeServicesWithRetry()
    {
        ShowNotification("Initializing services...");
        
        while (!isInitialized && retryAttempts < MAX_RETRIES)
        {
            yield return StartCoroutine(InitializeServices());
            
            if (!isInitialized)
            {
                retryAttempts++;
                ShowNotification($"Initialization attempt {retryAttempts}/{MAX_RETRIES}...");
                yield return new WaitForSeconds(RETRY_DELAY);
            }
        }

        if (!isInitialized)
        {
            ShowNotification("Failed to initialize. Please restart the app.");
            yield break;
        }

        EnableInteractiveElements();
        ShowNotification("Ready to join room");
    }

    private IEnumerator InitializeServices()
    {
        // Initialize Firebase
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Result != DependencyStatus.Available)
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyTask.Result}");
            yield break;
        }

        // Initialize Auth
        auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user logged in");
            SceneManager.LoadScene("Login");
            yield break;
        }

        userId = auth.CurrentUser.UserId;
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends");

        // Initialize Photon connection if needed
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            float timeoutTimer = 0;
            
            while (!PhotonNetwork.IsConnected && timeoutTimer < CONNECTION_TIMEOUT)
            {
                timeoutTimer += Time.deltaTime;
                yield return null;
            }

            if (!PhotonNetwork.IsConnected)
            {
                Debug.LogError("Failed to connect to Photon servers");
                ShowNotification("Connection failed. Please check your internet connection.");
                yield break;
            }
        }

        isInitialized = true;
    }

    private void OnRoomCodeInput(string code)
    {
        if (joinRoomButton != null)
        {
            joinRoomButton.interactable = !string.IsNullOrEmpty(code.Trim());
        }
    }

    private void OnJoinRoomButtonClicked()
    {
        if (!isInitialized || isProcessing)
        {
            ShowNotification("Please wait...");
            return;
        }

        roomId = roomCodeInputField.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(roomId))
        {
            ShowNotification("Please enter a room code");
            return;
        }

        isProcessing = true;
        DisableInteractiveElements();
        StartCoroutine(JoinRoomWithRetry());
    }

    private IEnumerator JoinRoomWithRetry()
    {
        retryAttempts = 0;
        bool success = false;

        while (!success && retryAttempts < MAX_RETRIES)
        {
            ShowNotification($"Checking room... Attempt {retryAttempts + 1}/{MAX_RETRIES}");
            
            bool checkResult = false;
            yield return StartCoroutine(CheckAndJoinRoom((result) => checkResult = result));
            success = checkResult;

            if (!success)
            {
                retryAttempts++;
                if (retryAttempts < MAX_RETRIES)
                {
                    yield return new WaitForSeconds(RETRY_DELAY);
                }
            }
        }

        if (!success)
        {
            ShowNotification("Room not found or cannot be joined");
            EnableInteractiveElements();
        }

        isProcessing = false;
    }

    private IEnumerator CheckAndJoinRoom(System.Action<bool> callback)
    {
        bool success = false;

        // Check Tournament rooms first
        var tournamentRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(roomId);
        var tournamentTask = tournamentRef.GetValueAsync();
        
        yield return new WaitUntil(() => tournamentTask.IsCompleted);

        if (!tournamentTask.IsFaulted && tournamentTask.Result.Exists)
        {
            roomType = "tournament";
            string tournamentName = tournamentTask.Result.Child("name").Value?.ToString();
            if (!string.IsNullOrEmpty(tournamentName))
            {
                PlayerPrefs.SetString("TournamentName", tournamentName);
                PlayerPrefs.Save();
                success = true;
                StartCoroutine(JoinPhotonRoom());
            }
        }

        if (!success)
        {
            // Then check WithFriends rooms
            var withFriendsTask = databaseRef.Child(roomId).GetValueAsync();
            yield return new WaitUntil(() => withFriendsTask.IsCompleted);

            if (!withFriendsTask.IsFaulted && withFriendsTask.Result.Exists)
            {
                roomType = "withfriend";
                success = true;
                StartCoroutine(JoinPhotonRoom());
            }
        }

        if (!success)
        {
            ShowNotification("Room not found");
        }

        callback(success);
    }

    private IEnumerator JoinPhotonRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            ShowNotification("Connection lost. Reconnecting...");
            PhotonNetwork.ConnectUsingSettings();
            yield return new WaitUntil(() => PhotonNetwork.IsConnected);
        }

        ShowNotification("Joining room...");
        PhotonNetwork.JoinRoom(roomId);

        float timeoutTimer = 0;
        while (!PhotonNetwork.InRoom && timeoutTimer < CONNECTION_TIMEOUT)
        {
            timeoutTimer += Time.deltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            ShowNotification("Failed to join room");
            EnableInteractiveElements();
        }
    }

    private IEnumerator SetPlayerUsername(System.Action onComplete = null)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("User not logged in");
            yield break;
        }

        var userRef = databaseRef.Root.Child("users").Child(auth.CurrentUser.UserId);
        var task = userRef.GetValueAsync();
        
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get user data: {task.Exception}");
            yield break;
        }

        if (task.Result.Exists)
        {
            string username = task.Result.Child("username").Value.ToString();
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
            {
                { "username", username },
                { "IsReady", false }
            };
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            PhotonNetwork.NickName = username;
        }

        onComplete?.Invoke();
    }

    public override void OnJoinedRoom()
    {
        ShowNotification("Joined room successfully");
        PhotonNetwork.IsMessageQueueRunning = false;

        StartCoroutine(SetPlayerUsernameAndLoadScene());
    }

    private IEnumerator SetPlayerUsernameAndLoadScene()
    {
        yield return StartCoroutine(SetPlayerUsername());

        string targetScene = roomType == "tournament" ? "TournamentLobby" : "Lobby";
        PhotonNetwork.LoadLevel(targetScene);

        yield return new WaitForSeconds(1f);
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ShowNotification($"Failed to join room: {message}");
        EnableInteractiveElements();
        isProcessing = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ShowNotification($"Disconnected: {cause}");
        StartCoroutine(HandleDisconnection());
    }

    private IEnumerator HandleDisconnection()
    {
        if (!isProcessing)
        {
            yield return StartCoroutine(InitializeServicesWithRetry());
        }
    }

    private void OnCancelButtonClicked()
    {
        roomCodeInputField.text = "";
        ShowNotification("Room code cleared");
    }

    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    private void EnableInteractiveElements()
    {
        if (joinRoomButton != null) joinRoomButton.interactable = true;
        if (cancelButton != null) cancelButton.interactable = true;
        if (backButton != null) backButton.interactable = true;
        if (roomCodeInputField != null) roomCodeInputField.interactable = true;
    }

    private void DisableInteractiveElements()
    {
        if (joinRoomButton != null) joinRoomButton.interactable = false;
        if (cancelButton != null) cancelButton.interactable = false;
        if (backButton != null) backButton.interactable = false;
        if (roomCodeInputField != null) roomCodeInputField.interactable = false;
    }

    private void ShowNotification(string message)
    {
        if (notificationText != null && notificationPopup != null)
        {
            notificationText.text = message;
            notificationPopup.SetActive(true);
            StartCoroutine(HideNotificationAfterDelay(3f));
            Debug.Log($"Notification: {message}");
        }
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notificationPopup != null)
        {
            notificationPopup.SetActive(false);
        }
    }

    private void SoundOnClick(System.Action buttonAction)
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
        yield return new WaitForSeconds(buttonSound != null ? buttonSound.length : 0f);
        buttonAction.Invoke();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (joinRoomButton != null)
            joinRoomButton.onClick.RemoveAllListeners();
        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
        if (roomCodeInputField != null)
            roomCodeInputField.onValueChanged.RemoveAllListeners();
    }
}
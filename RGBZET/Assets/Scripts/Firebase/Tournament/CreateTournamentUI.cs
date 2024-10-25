using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class CreateTournamentUI : MonoBehaviourPunCallbacks
{
    [Header("Player Count Selection")]
    public Button[] playerCountButtons;

    [Header("UI Elements")]
    public TMP_InputField tournamentNameInput;
    public Button createTournamentButton;
    public Button backButton;
    public GameObject notificationPopup;
    public TMP_Text notificationText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference databaseRef;
    private string tournamentId;
    private int playerCount = 4; // Default player count
    private string creatorUsername;

    void Start()
    {
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        creatorUsername = AuthManager.Instance.GetCurrentUsername();

        SetupButtons();
        SetupTournamentNameInput();
        
        notificationPopup.SetActive(false);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            ShowNotification("Connecting...");
        }
        else
        {
            OnConnectedToMaster();
        }
    }

    void SetupButtons()
    {
        // Setup player count buttons
        for (int i = 0; i < playerCountButtons.Length; i++)
        {
            int count = (i + 1) * 4; // 4 or 8 players
            playerCountButtons[i].onClick.AddListener(() => SoundOnClick(() => SetPlayerCount(count)));
        }

        createTournamentButton.onClick.AddListener(() => SoundOnClick(ValidateAndCreateTournament));
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Tournament")));

        UpdatePlayerCountButtonVisuals();
    }

    void SetupTournamentNameInput()
    {
        tournamentNameInput.characterLimit = 10;
        tournamentNameInput.onValueChanged.AddListener(OnTournamentNameChanged);
    }

    void OnTournamentNameChanged(string value)
    {
        if (value.Length > 10)
        {
            tournamentNameInput.text = value.Substring(0, 10);
        }
    }

    void SetPlayerCount(int count)
    {
        playerCount = count;
        UpdatePlayerCountButtonVisuals();
    }

    void UpdatePlayerCountButtonVisuals()
    {
        for (int i = 0; i < playerCountButtons.Length; i++)
        {
            int count = (i + 1) * 4;
            bool isSelected = count == playerCount;
            RectTransform buttonRect = playerCountButtons[i].GetComponent<RectTransform>();
            
            if (isSelected)
            {
                buttonRect.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }
            else
            {
                buttonRect.localScale = Vector3.one;
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        ShowNotification("Connected");
        createTournamentButton.interactable = true;
    }

    void ValidateAndCreateTournament()
    {
        string tournamentName = tournamentNameInput.text.Trim();

        if (string.IsNullOrEmpty(tournamentName))
        {
            ShowNotification("Enter tournament name");
            return;
        }

        if (tournamentName.Length > 10)
        {
            ShowNotification("Tournament name cannot exceed 10 characters");
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            ShowNotification("Not connected");
            return;
        }

        CreateTournament(tournamentName);
    }

    void CreateTournament(string tournamentName)
    {
        tournamentId = GenerateTournamentId();
        ShowNotification("Creating...");

        Dictionary<string, object> tournamentData = new Dictionary<string, object>
        {
            { "name", tournamentName },
            { "playerCount", playerCount },
            { "status", "waiting" },
            { "createdBy", creatorUsername },
            { "tournamentId", tournamentId }
        };

        databaseRef.Child("tournaments").Child(tournamentId).SetValueAsync(tournamentData).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                ShowNotification("Creation failed");
                Debug.LogError($"Failed to create tournament: {task.Exception}");
            }
            else
            {
                CreateTournamentBracket();
            }
        });
    }

    void CreateTournamentBracket()
    {
        int rounds = Mathf.CeilToInt(Mathf.Log(playerCount, 2));
        Dictionary<string, object> bracket = new Dictionary<string, object>();

        for (int round = 0; round < rounds; round++)
        {
            int matchesInRound = playerCount / (int)Mathf.Pow(2, round + 1);
            for (int match = 0; match < matchesInRound; match++)
            {
                string matchId = $"round_{round}_match_{match}";
                string nextMatchId = round < rounds - 1 ? $"round_{round + 1}_match_{match / 2}" : "final";

                if (matchesInRound == 1)
                {
                    nextMatchId = "final";
                }

                bracket[matchId] = new Dictionary<string, object>
                {
                    { "player1", new Dictionary<string, object> { { "username", "" }, { "inLobby", false } } },
                    { "player2", new Dictionary<string, object> { { "username", "" }, { "inLobby", false } } },
                    { "winner", "" },
                    { "nextMatchId", nextMatchId }
                };
            }
        }

        databaseRef.Child("tournaments").Child(tournamentId).Child("bracket").SetValueAsync(bracket).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                ShowNotification("Bracket creation failed");
                Debug.LogError($"Failed to create tournament bracket: {task.Exception}");
            }
            else
            {
                CreatePhotonRoom();
            }
        });
    }

    void CreatePhotonRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)playerCount,
            PublishUserId = true,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(tournamentId, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        SaveTournamentInfoToPlayerPrefs();
        ShowNotification("Tournament created");
        SceneManager.LoadScene("TournamentLobby");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ShowNotification("Room creation failed");
    }

    void SaveTournamentInfoToPlayerPrefs()
    {
        PlayerPrefs.SetString("TournamentId", tournamentId);
        PlayerPrefs.SetInt("PlayerCount", playerCount);
        PlayerPrefs.SetString("TournamentName", tournamentNameInput.text);
        PlayerPrefs.Save();
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

    string GenerateTournamentId()
    {
        return System.Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
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

    void OnDestroy()
    {
        foreach (var button in playerCountButtons)
        {
            button.onClick.RemoveAllListeners();
        }
        createTournamentButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        tournamentNameInput.onValueChanged.RemoveAllListeners();
    }

    void SetPlayerProperties()
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", creatorUsername },
            { "IsReady", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    public override void OnJoinedRoom()
    {
        SetPlayerProperties();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ShowNotification("Disconnected");
        createTournamentButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }
}
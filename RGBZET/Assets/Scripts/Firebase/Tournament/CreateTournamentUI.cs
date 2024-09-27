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
    public TMP_Dropdown playerCountDropdown;
    public TMP_InputField tournamentNameInput;
    public Button createTournamentButton;
    public Button backButton;
    public TMP_Text feedbackText;

    private DatabaseReference databaseRef;
    private string tournamentId;
    private int playerCount;
    private string creatorUsername;

    void Start()
    {
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        creatorUsername = AuthManager.Instance.GetCurrentUsername();

        playerCountDropdown.ClearOptions();
        playerCountDropdown.AddOptions(new List<string> { "4", "8" });

        createTournamentButton.onClick.AddListener(CreateTournament);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            OnConnectedToMaster();
        }
    }

    public override void OnConnectedToMaster()
    {
        DisplayFeedback("Connected to Photon Master Server");
        createTournamentButton.interactable = true;
    }
    

    void CreateTournament()
    {   
        if (!PhotonNetwork.IsConnected)
        {
            DisplayFeedback("Not connected to Photon. Please wait...");
            return;
        }
        playerCount = int.Parse(playerCountDropdown.options[playerCountDropdown.value].text);
        string tournamentName = tournamentNameInput.text;

        if (string.IsNullOrEmpty(tournamentName))
        {
            DisplayFeedback("Please enter a tournament name.");
            return;
        }

        tournamentId = GenerateTournamentId();

        Dictionary<string, object> tournamentData = new Dictionary<string, object>
        {
            { "name", tournamentName },
            { "playerCount", playerCount },
            { "status", "waiting" },
            { "createdBy", creatorUsername },
            { "tournamentId", tournamentId }
        };

        DisplayFeedback("Creating tournament...");
        databaseRef.Child("tournaments").Child(tournamentId).SetValueAsync(tournamentData).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DisplayFeedback("Failed to create tournament.");
                Debug.LogError($"Failed to create tournament: {task.Exception}");
            }
            else
            {
                Debug.Log($"Tournament created successfully. ID: {tournamentId}");
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
                bracket[matchId] = new Dictionary<string, object>
                {
                    { "player1", new Dictionary<string, object> { { "username", "" }, { "inLobby", false } } },
                    { "player2", new Dictionary<string, object> { { "username", "" }, { "inLobby", false } } },
                    { "winner", "" },
                    { "nextMatchId", round < rounds - 1 ? $"round_{round + 1}_match_{match / 2}" : "final" }
                };
            }
        }

        databaseRef.Child("tournaments").Child(tournamentId).Child("bracket").SetValueAsync(bracket).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DisplayFeedback("Failed to create tournament bracket.");
                Debug.LogError($"Failed to create tournament bracket: {task.Exception}");
            }
            else
            {
                Debug.Log("Tournament bracket created successfully");
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
        Debug.Log("Photon Room created successfully");
        SaveTournamentInfoToPlayerPrefs();
        SceneManager.LoadScene("TournamentLobby");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        DisplayFeedback($"Room creation failed: {message}");
    }

    void SaveTournamentInfoToPlayerPrefs()
    {
        PlayerPrefs.SetString("TournamentId", tournamentId);
        PlayerPrefs.SetInt("PlayerCount", playerCount);
        PlayerPrefs.SetString("TournamentName", tournamentNameInput.text);
        PlayerPrefs.Save();
    }

    void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        Debug.Log(message);
    }

    string GenerateTournamentId()
    {
        return System.Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
    }

    void OnDestroy()
    {
        createTournamentButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }

    // Add this method to set player properties in Photon
    void SetPlayerProperties()
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", creatorUsername },
            { "IsReady", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    // Override OnJoinedRoom to set player properties when joining the room
    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        SetPlayerProperties();
    }
    
     public override void OnDisconnected(DisconnectCause cause)
    {
        DisplayFeedback($"Disconnected from Photon: {cause}. Attempting to reconnect...");
        createTournamentButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }
}

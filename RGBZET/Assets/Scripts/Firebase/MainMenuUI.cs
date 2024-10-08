using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button playButton;
    //[SerializeField] private Button createWithFriendButton;
    [SerializeField] private Button TournamentButton;
    //[SerializeField] private Button quickplayButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button guideButton;
    //[SerializeField] private Button returnToTournamentButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    private DatabaseReference databaseReference;
    private string currentUsername;
    private string currentTournamentId;
    private string currentMatchId;

    void Start()
    {
        if (AuthManager.Instance.IsUserLoggedIn())
        {
            string userId = AuthManager.Instance.GetCurrentUserId();
            databaseReference = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);
            LoadUserData();
            //CheckForOngoingTournament();
        }
        else
        {
            SceneManager.LoadScene("Login");
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(DisconnectFromPhoton());
        }

        SetupButtons();
        
        //returnToTournamentButton.gameObject.SetActive(false);
    }

    IEnumerator DisconnectFromPhoton()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Debug.Log("Disconnected from Photon");
    }

    void SetupButtons()
    {
        joinButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Joinroom")));
        //createWithFriendButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("CreateFriend")));
        TournamentButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Tournament")));
        playButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("ModeCreateroom")));
        //quickplayButton.onClick.AddListener(() => SoundOnClick(OnQuickplayButtonClicked));
        logoutButton.onClick.AddListener(() => SoundOnClick(OnLogoutButtonClicked));
        profileButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Profile")));
        settingButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Setting")));
        //returnToTournamentButton.onClick.AddListener(() => SoundOnClick(ReturnToTournament));
    }

    void LoadUserData()
    {
        databaseReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    currentUsername = snapshot.Child("username").Value.ToString();
                    usernameText.text = "Welcome, " + currentUsername;
                }
                else
                {
                    DisplayFeedback("Failed to load user data.");
                }
            }
            else
            {
                DisplayFeedback("Failed to load user data.");
            }
        });
    }

    /*void CheckForOngoingTournament()
    {
        DatabaseReference tournamentsRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments");

        tournamentsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                foreach (var tournamentSnapshot in snapshot.Children)
                {
                    var bracketSnapshot = tournamentSnapshot.Child("bracket");
                    string latestMatchId = null;
                    bool isPlayerInActiveTournament = false;

                    var sortedMatches = bracketSnapshot.Children.OrderByDescending(match => 
                        int.Parse(match.Key.Split('_')[1]));

                    foreach (var matchSnapshot in sortedMatches)
                    {
                        var player1 = matchSnapshot.Child("player1").Child("username").Value?.ToString();
                        var player2 = matchSnapshot.Child("player2").Child("username").Value?.ToString();
                        var winner = matchSnapshot.Child("winner").Value?.ToString();

                        if (player1 == currentUsername || player2 == currentUsername)
                        {
                            isPlayerInActiveTournament = true;
                            if (string.IsNullOrEmpty(winner))
                            {
                                latestMatchId = matchSnapshot.Key;
                                break;
                            }
                        }
                    }

                    if (isPlayerInActiveTournament)
                    {
                        currentTournamentId = tournamentSnapshot.Key;
                        currentMatchId = latestMatchId;
                        returnToTournamentButton.gameObject.SetActive(true);
                        if (latestMatchId != null)
                        {
                            DisplayFeedback("You have an ongoing match in a tournament. Click 'Return to Tournament' to continue.");
                        }
                        else
                        {
                            DisplayFeedback("Your tournament has ended. Click 'Return to Tournament' to view results.");
                        }
                        return;
                    }
                }
            }
            else
            {
                DisplayFeedback("Failed to check for ongoing tournaments.");
            }
        });
    }*/

    /*void ReturnToTournament()
    {
        if (!string.IsNullOrEmpty(currentTournamentId))
        {
            PlayerPrefs.SetString("TournamentId", currentTournamentId);
            if (!string.IsNullOrEmpty(currentMatchId))
            {
                PlayerPrefs.SetString("CurrentMatchId", currentMatchId);
                SceneManager.LoadScene("MatchLobby");
            }
            else
            {
                SceneManager.LoadScene("TournamentBracket");
            }
        }
        else
        {
            DisplayFeedback("No ongoing tournament found.");
        }
    }*/
    

    /*void OnQuickplayButtonClicked()
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
    }*/

    /*private void SetPlayerNameAndJoinQuickplay()
    {
        string playerName = AuthManager.Instance.GetCurrentUsername();
        PhotonNetwork.NickName = playerName;
        
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", playerName }
        };
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);

        DisplayFeedback("Joining Quickplay...");
        PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable { { "GameType", "Quickplay" } }, 4);
    }*/

    void OnLogoutButtonClicked()
    {
        AuthManager.Instance.Logout();
        // Logout method in AuthManager already handles scene transition
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
    }

    /*public override void OnConnectedToMaster()
    {
        DisplayFeedback("Connected to Master. Setting up player...");
        SetPlayerNameAndJoinQuickplay();
    }*/

    /*public override void OnJoinRandomFailed(short returnCode, string message)
    {
        DisplayFeedback("Creating new Quickplay room...");
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "GameType", "Quickplay" } },
            CustomRoomPropertiesForLobby = new string[] { "GameType" }
        };
        PhotonNetwork.CreateRoom(null, roomOptions);
    }*/

    /*public override void OnJoinedRoom()
    {
        DisplayFeedback("Joined Quickplay room. Loading lobby...");
        SceneManager.LoadScene("QuickplayLobby");
    }*/

    void OnDestroy()
    {
        // Remove all listeners to prevent memory leaks
        joinButton.onClick.RemoveAllListeners();
        //createWithFriendButton.onClick.RemoveAllListeners();
        TournamentButton.onClick.RemoveAllListeners();
        //quickplayButton.onClick.RemoveAllListeners();
        logoutButton.onClick.RemoveAllListeners();
        profileButton.onClick.RemoveAllListeners();
        //returnToTournamentButton.onClick.RemoveAllListeners();
    }
}
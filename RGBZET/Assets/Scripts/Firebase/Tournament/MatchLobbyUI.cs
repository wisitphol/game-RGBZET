using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MatchLobbyUI : MonoBehaviourPunCallbacks
{
    public TMP_Text matchIdText;
    public TMP_Text player1Text;
    public TMP_Text player2Text;
    public Button backToBracketButton;
    public TMP_Text statusText;
    
    [Header("Countdown UI")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Image countdownFillImage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private string matchId;
    private DatabaseReference matchRef;
    private string currentUsername;
    private bool isCountingDown = false;
    private const float COUNTDOWN_TIME = 5f;

    void Start()
    {
        matchId = PlayerPrefs.GetString("CurrentMatchId");
        string tournamentId = PlayerPrefs.GetString("TournamentId");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(tournamentId))
        {
            Debug.LogError("matchId or tournamentId is null or empty.");
            return;
        }

        if (matchIdText != null)
            matchIdText.text = "Match ID: " + matchId;

        matchRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments")
            .Child(tournamentId)
            .Child("bracket")
            .Child(matchId);

        backToBracketButton.onClick.AddListener(() => SoundOnClick(BackToBracket));
        
        countdownPanel.SetActive(false);
        SetPlayerInLobby(true);

        LoadMatchData();
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        PhotonNetwork.NickName = currentUsername;
        statusText.text = "Connecting to server...";
        PhotonNetwork.ConnectUsingSettings();
    }

    void LoadMatchData()
    {
        matchRef.GetValueAsync().ContinueWith(task => 
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to load match data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    UpdateUI(snapshot.Value as Dictionary<string, object>);
                }
            }
        });
    }

    void UpdateUI(Dictionary<string, object> matchData)
    {
        if (matchData == null) return;

        UpdateUIForPlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined: " + newPlayer.NickName);
        UpdateUIForPlayers();

        // Start countdown when room is full
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && !isCountingDown)
        {
            StartCoroutine(StartCountdown());
        }
    }

    void UpdateUIForPlayers()
    {
        if (PhotonNetwork.PlayerList.Length > 0)
        {
            player1Text.text = PhotonNetwork.PlayerList[0].NickName;
        }

        if (PhotonNetwork.PlayerList.Length > 1)
        {
            player2Text.text = PhotonNetwork.PlayerList[1].NickName;
        }
        else
        {
            player2Text.text = "Waiting for opponent...";
        }

        statusText.text = PhotonNetwork.CurrentRoom.PlayerCount == 2 ? 
            "Starting soon..." : "Waiting for players...";
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        statusText.text = "Connected. Joining room...";
        JoinOrCreateRoom();
    }

    void JoinOrCreateRoom()
    {
        if (string.IsNullOrEmpty(matchId))
        {
            Debug.LogError("MatchId is null or empty");
            return;
        }

        RoomOptions roomOptions = new RoomOptions 
        { 
            MaxPlayers = 2, 
            PublishUserId = true, 
            IsVisible = false, 
            IsOpen = true 
        };
        
        PhotonNetwork.JoinOrCreateRoom(matchId, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        statusText.text = "Joined room. Waiting for opponent...";

        UpdateUIForPlayers();

        // Start countdown if room is already full when joining
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && !isCountingDown)
        {
            StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        if (isCountingDown) yield break;
        isCountingDown = true;

        countdownPanel.SetActive(true);
        float timeLeft = COUNTDOWN_TIME;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            int secondsLeft = Mathf.CeilToInt(timeLeft);
            
            countdownText.text = secondsLeft.ToString();
            countdownFillImage.fillAmount = timeLeft / COUNTDOWN_TIME;
            
            yield return null;
        }

        StartGame();
    }

    void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel("Card sample Tournament");
        }
    }

    void BackToBracket()
    {
        if (PhotonNetwork.InRoom)
        {   
            SetPlayerInLobby(false);
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("TournamentBracket");
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("TournamentBracket");
    }

    void SetPlayerInLobby(bool inLobby)
    {
        StartCoroutine(GetPlayerPathCoroutine(playerPath =>
        {
            matchRef.Child(playerPath).Child("inLobby").SetValueAsync(inLobby);
        }));
    }

    IEnumerator GetPlayerPathCoroutine(System.Action<string> callback)
    {
        if (matchRef == null)
        {
            Debug.LogError("matchRef is null");
            yield break;
        }

        for (int retry = 0; retry < 3; retry++)
        {
            var player1Task = matchRef.Child("player1").Child("username").GetValueAsync();
            var player2Task = matchRef.Child("player2").Child("username").GetValueAsync();
            
            yield return new WaitUntil(() => player1Task.IsCompleted && player2Task.IsCompleted);

            if (player1Task.Exception != null || player2Task.Exception != null)
            {
                Debug.LogError($"Error fetching player paths: {player1Task.Exception ?? player2Task.Exception}");
                yield break;
            }

            if (player1Task.Result.Exists && player2Task.Result.Exists)
            {
                string player1Username = player1Task.Result.Value.ToString();
                string player2Username = player2Task.Result.Value.ToString();
                
                string playerPath = player1Username == currentUsername ? "player1" : "player2";
                callback(playerPath);
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
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

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (backToBracketButton != null)
        {
            backToBracketButton.onClick.RemoveAllListeners();
        }
    }
}
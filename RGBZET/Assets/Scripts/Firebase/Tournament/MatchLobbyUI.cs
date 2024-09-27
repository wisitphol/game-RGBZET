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
    public Button startButton; // ปุ่มเริ่มเกม
    public TMP_Text statusText;

    private string matchId;
    private DatabaseReference matchRef;
    private string currentUsername;

    void Start()
    {
        matchId = PlayerPrefs.GetString("CurrentMatchId");
        string tournamentId = PlayerPrefs.GetString("TournamentId");
        currentUsername = AuthManager.Instance.GetCurrentUsername();

        if (matchIdText != null)
            matchIdText.text = "Match ID: " + matchId;

        matchRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId).Child("bracket").Child(matchId);

        backToBracketButton.onClick.AddListener(BackToBracket);
        startButton.onClick.AddListener(StartGame); // เพิ่ม Listener สำหรับปุ่มเริ่มเกม
        startButton.gameObject.SetActive(false); // ซ่อนปุ่มเริ่มเกมเริ่มต้น
        SetPlayerInLobby(true);

        LoadMatchData();
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        PhotonNetwork.NickName = currentUsername; // ตั้งชื่อผู้เล่นจาก Photon
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
                    Debug.Log("Match data: " + snapshot.GetRawJsonValue());
                    UpdateUI(snapshot.Value as Dictionary<string, object>);
                }
                else
                {
                    Debug.LogWarning("No match data found.");
                }
            }
        });
    }

    void UpdateUI(Dictionary<string, object> matchData)
    {
        if (matchData == null)
        {
            Debug.LogWarning("Match data is null");
            return;
        }

        UpdateUIForPlayers();

        // อัปเดตปุ่มเริ่มเกม
        UpdateStartButton();
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined: " + newPlayer.NickName);

        // อัปเดต UI ของผู้เล่นทั้งหมดเมื่อมีคนเข้ามาในห้อง
        UpdateUIForPlayers();
    }

    void UpdateUIForPlayers()
    {
        // อัปเดตชื่อ Player 1
        if (PhotonNetwork.PlayerList.Length > 0)
        {
            player1Text.text = PhotonNetwork.PlayerList[0].NickName; // ตั้งชื่อ Player 1
        }

        // อัปเดตชื่อ Player 2
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            player2Text.text = PhotonNetwork.PlayerList[1].NickName; // ตั้งชื่อ Player 2
        }
        else
        {
            player2Text.text = "Waiting..."; // ถ้าไม่มีผู้เล่นที่สอง แสดง "Waiting..."
        }

        // ถ้า Player 1 (Master Client) อยู่ในห้องแล้วแสดงปุ่ม Start เมื่อมีผู้เล่น 2 คน
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            startButton.gameObject.SetActive(true); // แสดงปุ่ม Start
        }
        else
        {
            startButton.gameObject.SetActive(false); // ซ่อนปุ่ม Start ถ้าจำนวนผู้เล่นไม่ครบ
        }

        Debug.Log("Player 1: " + player1Text.text);
        Debug.Log("Player 2: " + player2Text.text);
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

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2, PublishUserId = true, IsVisible = false, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom(matchId, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        statusText.text = "Joined room. Waiting for opponent...";

        // อัปเดต UI สำหรับผู้เล่น
        UpdateUIForPlayers();

        // อัปเดตปุ่มเริ่มเกม
        UpdateStartButton();
    }

    void UpdateStartButton()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient); // แสดงปุ่มเริ่มเกมถ้าเป็น Master Client
        }
        else
        {
            startButton.gameObject.SetActive(false); // ซ่อนปุ่มเริ่มเกมถ้ายังไม่ครบ
        }
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
        var player1Task = matchRef.Child("player1").Child("username").GetValueAsync();
        var player2Task = matchRef.Child("player2").Child("username").GetValueAsync();
        
        yield return new WaitUntil(() => player1Task.IsCompleted && player2Task.IsCompleted);

        if (player1Task.Exception == null && player2Task.Exception == null)
        {
            string player1Username = player1Task.Result.Value.ToString();
            string player2Username = player2Task.Result.Value.ToString();
            
            callback(player1Username == currentUsername ? "player1" : "player2");
        }
        else
        {
            Debug.LogError("Error fetching player paths");
        }
    }

    void StartGame()
    {
        PhotonNetwork.LoadLevel("Card sample Tournament");
    }

    void BackToBracket()
    {
        if (PhotonNetwork.CurrentRoom != null)
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
        if (PhotonNetwork.CurrentRoom != null)
        {
            Debug.Log("Left room: " + PhotonNetwork.CurrentRoom.Name);
        }

        SceneManager.LoadScene("TournamentBracket");
    }
}

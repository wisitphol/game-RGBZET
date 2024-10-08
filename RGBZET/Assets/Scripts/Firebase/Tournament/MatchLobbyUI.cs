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
    public Button startButton;
    public Button readyButton;
    public TMP_Text statusText;

    private string matchId;
    private DatabaseReference matchRef;
    private string currentUsername;
    private bool isReady = false;

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

        matchRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId).Child("bracket").Child(matchId);

        backToBracketButton.onClick.AddListener(BackToBracket);
        startButton.onClick.AddListener(StartGame);
        readyButton.onClick.AddListener(ToggleReady);

        startButton.gameObject.SetActive(false);
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
        UpdateStartButtonVisibility();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined: " + newPlayer.NickName);
        UpdateUIForPlayers();
    }

    void UpdateUIForPlayers()
    {
        if (PhotonNetwork.PlayerList.Length > 0)
        {
            player1Text.text = PhotonNetwork.PlayerList[0].NickName;
            bool isPlayer1Ready = PhotonNetwork.PlayerList[0].CustomProperties.TryGetValue("IsReady", out object isReady1) && (bool)isReady1;
            player1Text.text += isPlayer1Ready ? " (Ready)" : " (Not Ready)";

            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[0])
            {
                readyButton.GetComponentInChildren<TMP_Text>().text = isPlayer1Ready ? "Cancel Ready" : "Ready";
            }
        }

        if (PhotonNetwork.PlayerList.Length > 1)
        {
            player2Text.text = PhotonNetwork.PlayerList[1].NickName;
            bool isPlayer2Ready = PhotonNetwork.PlayerList[1].CustomProperties.TryGetValue("IsReady", out object isReady2) && (bool)isReady2;
            player2Text.text += isPlayer2Ready ? " (Ready)" : " (Not Ready)";

            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[1])
            {
                readyButton.GetComponentInChildren<TMP_Text>().text = isPlayer2Ready ? "Cancel Ready" : "Ready";
            }
        }
        else
        {
            player2Text.text = "Waiting for opponent...";
        }

        UpdateStartButtonVisibility();

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

        UpdateUIForPlayers();
        UpdateStartButtonVisibility();
    }

    void UpdateStartButtonVisibility()
    {
        bool allPlayersReady = true;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue("IsReady", out object isReady) || !(bool)isReady)
            {
                allPlayersReady = false;
                break;
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(allPlayersReady && PhotonNetwork.CurrentRoom.PlayerCount == 2);
            startButton.interactable = allPlayersReady && PhotonNetwork.CurrentRoom.PlayerCount == 2;
        }
        else
        {
            startButton.gameObject.SetActive(false);
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
        if (matchRef == null)
        {
            Debug.LogError("matchRef is null. Make sure it's properly initialized.");
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
                
                if (string.IsNullOrEmpty(currentUsername))
                {
                    Debug.LogError("currentUsername is null or empty.");
                    yield break;
                }

                string playerPath = player1Username == currentUsername ? "player1" : "player2";
                callback(playerPath);
                yield break;
            }

            Debug.LogWarning("Retrying to fetch player data...");
            yield return new WaitForSeconds(1f); // Retry after 1 second
        }

        Debug.LogError("Player data is missing after retries.");
        yield break;
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

    void ToggleReady()
    {
        isReady = !isReady;
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable { { "IsReady", isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Cancel Ready" : "Ready";
        UpdateUIForPlayers();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == PhotonNetwork.LocalPlayer)
        {
            if (changedProps.TryGetValue("IsReady", out object isReady))
            {
                readyButton.GetComponentInChildren<TMP_Text>().text = (bool)isReady ? "Cancel Ready" : "Ready";
            }
        }
        
        UpdateUIForPlayers();
    }
}
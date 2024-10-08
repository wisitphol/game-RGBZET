using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;

public class QuickplayLobbyUI : MonoBehaviourPunCallbacks
{
    public TMP_Text[] playerNames;
    public TMP_Text playerCountText;
    public Button leaveButton;
    public TMP_Text feedbackText;

    private const float MAX_WAIT_TIME = 60f; // 60 seconds maximum wait time
    private float waitTimer;

    void Start()
    {
        UpdatePlayerList();
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        waitTimer = MAX_WAIT_TIME;
        StartCoroutine(WaitForPlayers());
    }

    void Update()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0 && PhotonNetwork.CountOfPlayersInRooms < 4)
        {
            OnWaitTimeExceeded();
        }
    }

    void UpdatePlayerList()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < playerNames.Length; i++)
        {
            if (i < players.Length)
            {
                string username = GetPlayerUsername(players[i]);
                playerNames[i].text = username;
            }
            else
            {
                playerNames[i].text = "Waiting for player...";
            }
        }
        playerCountText.text = $"Players: {players.Length}/4";

        if (players.Length == 4 && PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }

    string GetPlayerUsername(Player player)
    {
        if (player.CustomProperties.TryGetValue("username", out object username))
        {
            return (string)username;
        }
        return player.NickName;
    }

    void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel("Card sample 2");
    }

    void OnLeaveButtonClicked()
    {
        StopAllCoroutines();
        PhotonNetwork.LeaveRoom();
    }

    IEnumerator WaitForPlayers()
    {
        while (waitTimer > 0 && PhotonNetwork.CountOfPlayersInRooms < 4)
        {
            feedbackText.text = $"Waiting for players... {Mathf.CeilToInt(waitTimer)}s";
            yield return new WaitForSeconds(1f);
        }
    }

    void OnWaitTimeExceeded()
    {
        feedbackText.text = "Not enough players. Returning to main menu...";
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        waitTimer = MAX_WAIT_TIME; // Reset timer when a new player joins
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdatePlayerList();
    }
}
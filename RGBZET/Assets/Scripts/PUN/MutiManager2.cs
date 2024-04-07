using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MutiManager2 : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            // ตรวจสอบสถานะการเชื่อมต่อก่อนเรียกใช้ ConnectUsingSettings
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Connected!");
            JoinOrCreateRoom();
        }
        
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server. Pun is ready for use!");
        JoinOrCreateRoom();
    }

    void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // จำนวนผู้เล่นสูงสุดในห้อง

        PhotonNetwork.JoinOrCreateRoom("cardsample", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjects();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjects();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjects();
    }

    void UpdatePlayerObjects()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        switch (playerCount)
        {
            case 1:
                player1.SetActive(true);
                player2.SetActive(false);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
            case 2:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
            case 3:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(true);
                player4.SetActive(false);
                break;
            case 4:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(true);
                player4.SetActive(true);
                break;
            default:
                player1.SetActive(false);
                player2.SetActive(false);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
        }
    }
}

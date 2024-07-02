using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MutiManager3 : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    private ZETManager3 zetManager3;
    private PlayerController playerController;
    private PhotonView myPhotonView;

    public GameObject Cardboard;
    public GameObject Board;
    private Dictionary<int, Vector3> playerPositions = new Dictionary<int, Vector3>();

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Connected!");
            JoinOrCreateRoom();
        }

        zetManager3 = FindObjectOfType<ZETManager3>();
        playerController = FindObjectOfType<PlayerController>();
        myPhotonView = GetComponent<PhotonView>();
    }

    private void AddPlayerPosition(int actorNumber, Vector3 position)
    {
        // เพิ่มหรืออัปเดตตำแหน่งของผู้เล่น
        if (playerPositions.ContainsKey(actorNumber))
        {
            playerPositions[actorNumber] = position;
        }
        else
        {
            playerPositions.Add(actorNumber, position);
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
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.JoinOrCreateRoom("cardsample", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjectsIN();


    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjectsIN();

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjectsOUT(otherPlayer);

    }

    void UpdatePlayerObjectsIN()
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

        // เก็บตำแหน่งของผู้เล่นทุกคน
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (playerPositions.ContainsKey(player.ActorNumber))
            {
                // หาก Dictionary มีหมายเลขผู้เล่นนี้อยู่แล้ว ให้เรียกใช้เมธอด UpdatePlayerPosition
                // เพื่ออัปเดตตำแหน่งของผู้เล่น
                UpdatePlayerPosition(player.ActorNumber, playerPositions[player.ActorNumber]);
            }
            else
            {
                // หาก Dictionary ยังไม่มีหมายเลขผู้เล่นนี้ ให้เรียกใช้เมธอด SetPlayerPosition
                // เพื่อเพิ่มตำแหน่งของผู้เล่นลงใน Dictionary
                //SetPlayerPosition(player.ActorNumber, player.Position); // ใช้ Position ของ Player แทน
            }
        }
    }

    void UpdatePlayerObjectsOUT(Player leftPlayer)
    {
        int actorNumber = leftPlayer.ActorNumber;

        // ปิดการแสดงผลผู้เล่นที่ออกจากห้อง
        switch (actorNumber)
        {
            case 1:
                player1.SetActive(false);
                break;
            case 2:
                player2.SetActive(false);
                break;
            case 3:
                player3.SetActive(false);
                break;
            case 4:
                player4.SetActive(false);
                break;
        }
    }

    // เมธอดสำหรับการอัปเดตตำแหน่งของผู้เล่น
    void UpdatePlayerPosition(int actorNumber, Vector3 position)
    {
        switch (actorNumber)
        {
            case 1:
                player1.transform.position = position;
                break;
            case 2:
                player2.transform.position = position;
                break;
            case 3:
                player3.transform.position = position;
                break;
            case 4:
                player4.transform.position = position;
                break;
        }
    }

    // เพิ่มเมธอดสำหรับการส่ง RPC เมื่อกดปุ่ม zet
    public void OnZetButtonPressed()
    {
        if (zetManager3 != null && !ZETManager3.isZETActive && PhotonNetwork.IsConnected)
        {
            myPhotonView.RPC("RPC_ZetButtonPressed_MutiManager3", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    // เพิ่มเมธอด RPC สำหรับการรับ RPC เมื่อผู้เล่นคนใดคนหนึ่งกดปุ่ม zet
    [PunRPC]
    void RPC_ZetButtonPressed_MutiManager3(int playerActorNumber)
    {
        // ตรวจสอบว่า zetManager3 ไม่ได้เป็น null
        if (zetManager3 != null)
        {
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 1
            if (playerActorNumber == 1 && player1 != null && player1.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 1
                player1.GetComponent<PlayerController>().OnZetButtonPressed();

            }
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 2
            else if (playerActorNumber == 2 && player2 != null && player2.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 2
                player2.GetComponent<PlayerController>().OnZetButtonPressed();
            }
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 3
            else if (playerActorNumber == 3 && player3 != null && player3.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 3
                player3.GetComponent<PlayerController>().OnZetButtonPressed();
            }
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 4
            else if (playerActorNumber == 4 && player4 != null && player4.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 4
                player4.GetComponent<PlayerController>().OnZetButtonPressed();
            }
        }

        // แสดง Debug.Log เพื่อบันทึกว่ามีผู้เล่นกดปุ่ม ZET
        Debug.Log("Player " + playerActorNumber + " pressed the zet button.");
    }

    
}

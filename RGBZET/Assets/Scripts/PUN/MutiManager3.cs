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

    /*[PunRPC]
    void RPC_OnBeginDrag(Vector3 startPosition, Quaternion startRotation)
    {
        // สามารถดำเนินการต่อได้ตามต้องการ เช่น แสดงการกระทำการลากในหน้าจอผู้เล่น
        Debug.Log("Player started dragging card at position: " + startPosition + " with rotation: " + startRotation);
    }*/
}

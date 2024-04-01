using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject player1; // อ็อบเจกต์ของผู้เล่นคนที่ 1
    public GameObject player2; // อ็อบเจกต์ของผู้เล่นคนที่ 2
    public GameObject player3; // อ็อบเจกต์ของผู้เล่นคนที่ 3
    public GameObject player4; // อ็อบเจกต์ของผู้เล่นคนที่ 4

    private Dictionary<int, GameObject> playerIcons = new Dictionary<int, GameObject>(); // ดิกชันนารีที่ใช้เก็บไอคอนผู้เล่นโดยใช้ ActorNumber เป็นคีย์

    // เมื่อมีการเข้าร่วมห้องเกม
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Successfully joined the room."); // เพิ่ม Debug.Log() เพื่อตรวจสอบว่าเข้าร่วมห้องเกมสำเร็จ
        // ตรวจสอบและสร้างไอคอนผู้เล่นสำหรับผู้เล่นที่เข้าร่วมห้องเกม
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CreatePlayerIcon(player);
        }
    }

    // เมื่อมีการเพิ่มผู้เล่นในห้องเกม
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player Entered Room: " + newPlayer.NickName);
        // สร้างไอคอนผู้เล่นสำหรับผู้เล่นที่เข้าร่วมห้องเกม
        CreatePlayerIcon(newPlayer);
    }

    // สร้างไอคอนผู้เล่น
    private void CreatePlayerIcon(Player player)
    {
        // ตรวจสอบว่าไอคอนผู้เล่นยังไม่มีการสร้างไว้แล้วหรือไม่
        if (!playerIcons.ContainsKey(player.ActorNumber))
        {
            // สร้างไอคอนผู้เล่น
            GameObject playerIcon;
            if (player.ActorNumber == 1)
            {
                playerIcon = Instantiate(player1);
            }
            else if (player.ActorNumber == 2)
            {
                playerIcon = Instantiate(player2);
            }
            else if (player.ActorNumber == 3)
            {
                playerIcon = Instantiate(player3);
            }
            else if (player.ActorNumber == 4)
            {
                playerIcon = Instantiate(player4);
            }   
            else
            {
                Debug.LogWarning("Cannot create player icon for player " + player.ActorNumber + ": Too many players.");
                return;
            }
            playerIcon.GetComponentInChildren<Text>().text = player.NickName;
            // เพิ่มไอคอนผู้เล่นลงในดิกชันนารี
            playerIcons.Add(player.ActorNumber, playerIcon);
        }
        else
        {
            // หากไอคอนผู้เล่นมีอยู่แล้ว ให้เปิดใช้งานไอคอนผู้เล่น
            playerIcons[player.ActorNumber].SetActive(true);
        }
    }


    // เมื่อมีการลบผู้เล่นออกจากห้องเกม
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player Left Room: " + otherPlayer.NickName);
        // ซ่อนไอคอนผู้เล่นของผู้เล่นที่ออกจากห้องเกม
        HidePlayerIcon(otherPlayer);
    }

    // ซ่อนไอคอนผู้เล่น
    private void HidePlayerIcon(Player player)
    {
        // ตรวจสอบว่ามีไอคอนผู้เล่นในดิกชันนารีหรือไม่
        if (playerIcons.ContainsKey(player.ActorNumber))
        {
            // ซ่อนไอคอนผู้เล่น
            playerIcons[player.ActorNumber].SetActive(false);
        }
    }
}

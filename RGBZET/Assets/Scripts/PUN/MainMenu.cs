using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public TMP_InputField playerNameInput; // ช่องให้กรอกชื่อผู้เล่น
    public Button startGameButton; // ปุ่มเพื่อเริ่มเกม

    void Start()
    {
        // ซ่อนปุ่มเริ่มเกมไว้ก่อน
        startGameButton.interactable = false;

        // ตรวจสอบว่าเชื่อมต่อ PUN แล้วหรือยัง
        if (!PhotonNetwork.IsConnected)
        {
            // เชื่อมต่อกับ PUN
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to PUN...");
        }
        else
        {
            Debug.Log("Already connected to PUN.");
        }
    }

    // เชื่อมต่อสำเร็จ
    public override void OnConnectedToMaster()
    {
        // สามารถกดปุ่มเริ่มเกมได้หลังจากเชื่อมต่อ PUN สำเร็จ
        startGameButton.interactable = true;
        Debug.Log("Connected to PUN.");
    }

    // เมื่อกดปุ่มเริ่มเกม
    public void StartGame()
    {
        string playerName = playerNameInput.text;

        // ตรวจสอบว่ามีชื่อผู้เล่นที่กรอกหรือไม่
        if (!string.IsNullOrEmpty(playerName))
        {
            // สร้างห้องเกมใหม่และเข้าร่วม
            PhotonNetwork.NickName = playerName;
            PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
        }
        else
        {
            Debug.Log("Please enter your name.");
        }
    }

    // เข้าร่วมห้องเกมสำเร็จ
    public override void OnJoinedRoom()
    {
        if (!string.IsNullOrEmpty(playerNameInput.text))
        {
            // โหลดหน้าเล่นเกม
            PhotonNetwork.LoadLevel("Card sample");
        }
        else
        {
            Debug.Log("Please enter your name before starting the game.");
        }
    }
}

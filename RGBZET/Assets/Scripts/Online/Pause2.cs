using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;
using System.Linq;
public class Pause2 : MonoBehaviourPunCallbacks
{
    [SerializeField] public Button pauseButton;
    [SerializeField] public GameObject pausePanel;
    [SerializeField] public Button menuButton;
    private DatabaseReference databaseReference;

    void Start()
    {
        // ซ่อนแผง pause panel ตอนเริ่มเกม
        pausePanel.SetActive(false);

        // เพิ่ม listener ให้กับปุ่ม pause
        pauseButton.onClick.AddListener(TogglePause);

        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(PlayerPrefs.GetString("RoomId"));
    }

    void TogglePause()
    {
        bool isActive = pausePanel.activeSelf;
        pausePanel.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่
        menuButton.onClick.AddListener(MenuButtonClicked);
    }

    private void MenuButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ถ้าผู้เล่นเป็น host, ลบข้อมูลห้องและเปลี่ยนไปยังเมนู
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
        else
        {
            // ถ้าผู้เล่นไม่ใช่ host, เปลี่ยนไปยังเมนูทันที
            StartCoroutine(LeaveRoomAndCheckConnection());
        }
    }

    private IEnumerator DeleteRoomAndGoToMenu()
    {
        var task = databaseReference.RemoveValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Can't delete room from Firebase.");
        }
        else
        {
            Debug.Log("Can delete room from Firebase.");
        }

        // ออกจากห้อง Photon
        PhotonNetwork.LeaveRoom();

        // ตรวจสอบสถานะการเชื่อมต่อกับ Game Server
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        // ยกเลิกการเชื่อมต่อจาก Game Server
        PhotonNetwork.Disconnect();

        // ตรวจสอบสถานะการยกเลิกการเชื่อมต่อจาก Game Server
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);


        SceneManager.LoadScene("Menu");
    }

    private IEnumerator LeaveRoomAndCheckConnection()
    {
        PhotonNetwork.LeaveRoom();

        // รอให้การออกจากห้องเสร็จสมบูรณ์
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        // ยกเลิกการเชื่อมต่อจาก Game Server
        PhotonNetwork.Disconnect();

        // ตรวจสอบสถานะการยกเลิกการเชื่อมต่อจาก Game Server
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        // เปลี่ยนไปยังเมนู
        SceneManager.LoadScene("Menu");


    }
}

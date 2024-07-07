using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerActionHandler : MonoBehaviourPunCallbacks
{
    public static PlayerActionHandler Instance;

    private void Awake()
    {
        // ตรวจสอบว่ามีอ็อบเจกต์ Instance หรือยัง
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // ถ้ามีอ็อบเจกต์ Instance อื่นอยู่แล้ว ให้ทำลายอ็อบเจกต์นี้
            Destroy(gameObject);
        }
    }

    // เมธอดสำหรับส่งข้อมูลการกระทำไปยังผู้เล่นทุกคนในห้อง
    public void SendPlayerAction(string action)
    {
        if (PhotonNetwork.IsConnected)
        {
            // ส่งข้อมูลการกระทำไปยังผู้เล่นทุกคนในห้อง
            photonView.RPC("ReceivePlayerAction", RpcTarget.All, action);
        }
    }

    // เมธอดสำหรับรับข้อมูลการกระทำจากผู้เล่นอื่นๆ
    [PunRPC]
    private void ReceivePlayerAction(string action)
    {
        // ทำสิ่งที่ต้องการเมื่อได้รับข้อมูลการกระทำ
        // ตัวอย่าง: ประมวลผลการกระทำและแสดงผลในเกม
        Debug.Log("Received player action: " + action);

        // สามารถเรียกเมธอดอื่นๆ หรือทำสิ่งอื่นๆ ตามต้องการได้ที่นี่
        // เช่น การปรับเปลี่ยนสถานะของอ็อบเจกต์ในเกม หรือแสดงผลการกระทำใน UI
    }
}

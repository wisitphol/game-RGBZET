using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    public GameObject zetText; // ตัวแปรเก็บ GameObject ของข้อความ ZET

    // เมื่อกดปุ่ม zet
    // เมื่อกดปุ่ม zet
    public void OnZetButtonPressed()
    {
        // สร้างเงื่อนไขเพื่อเปิด/ปิดการแสดงข้อความ ZET ตามสถานะการกดปุ่ม zet
        if (ZETManager.isZETActive)
        {
            zetText.SetActive(true); // เปิดการแสดงข้อความ ZET เมื่อกดปุ่ม
        }
        else
        {
            zetText.SetActive(false); // ปิดการแสดงข้อความ ZET เมื่อไม่ได้กดปุ่ม
        }

        if (ZETManager.isZETActive && PhotonNetwork.IsConnected)
        {
            // ส่ง RPC ไปยัง ZETManager เพื่อแจ้งเหตุการณ์การกดปุ่ม zet และส่งตำแหน่งของผู้เล่น
            photonView.RPC("RPC_ZetButtonPressed_PlayerScript", RpcTarget.All, transform.position);
        }

        
    }

    [PunRPC]
    void RPC_ZetButtonPressed_PlayerScript(Vector3 playerPosition)
    {
        // นำข้อมูลตำแหน่งของผู้เล่นมาใช้งาน เช่น แสดงรูป zet ในตำแหน่งที่ผู้เล่นอยู่
        // ในที่นี้คุณสามารถใช้ playerPosition ในการกำหนดตำแหน่งของ zetText ได้
        zetText.transform.position = playerPosition;
        zetText.SetActive(true); // เปิดการแสดงข้อความ ZET

        if (ZETManager.isZETActive)
        {
            zetText.SetActive(true); // เปิดการแสดงข้อความ ZET เมื่อกดปุ่ม
        }
        else
        {
            zetText.SetActive(false); // ปิดการแสดงข้อความ ZET เมื่อไม่ได้กดปุ่ม
        }
    }
}

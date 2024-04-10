using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    public TMP_Text nameText;
    public GameObject zetText;
    private bool playerNameHasBeenSet = false;

    private void Start()
    {
        // ตั้งค่าชื่อผู้เล่นเมื่อการเชื่อมต่อสำเร็จ
        string playerName = PlayerPrefs.GetString("PlayerName", "DefaultName"); // ดึงชื่อผู้เล่นจาก PlayerPrefs โดยใช้ค่าเริ่มต้นเป็น "DefaultName" หากไม่พบชื่อผู้เล่น

        SetPlayerName(playerName); // เรียกเมธอด SetPlayerName() เพื่อแสดงชื่อผู้เล่น

        if (photonView.IsMine)
        {
            // ลงทะเบียน zetText ของผู้เล่นเมื่อเริ่มเกม
            ZETManager2.instance.RegisterZetText(photonView.ViewID, zetText);
        }

    }

    [PunRPC]
    public void SetPlayerName(string playerName)
    {
        nameText.text = playerName;
        playerNameHasBeenSet = true; 
        Debug.Log("SetPlayerName has been called. Player name: " + playerName);
    }

    // เมื่อกดปุ่ม ZET
    public void OnZetButtonPressed()
    {
        // เปิด/ปิดการแสดงข้อความ ZET ตามสถานะของ isZETActive ที่เก็บไว้ใน ZETManager2
        zetText.SetActive(ZETManager2.isZETActive);

        // ส่ง RPC เพื่อเปิด/ปิดการแสดงข้อความ ZET ให้กับผู้เล่นทุกคน
        photonView.RPC("ToggleZetText", RpcTarget.All, photonView.ViewID, ZETManager2.isZETActive);
    }

    // RPC เพื่อเปิด/ปิดการแสดงข้อความ ZET
    [PunRPC]
    private void ToggleZetText(int viewID, bool show)
    {
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        if (targetPhotonView != null)
        {
            GameObject zetTextObject = ZETManager2.instance.GetZetText(targetPhotonView.ViewID);
            if (zetTextObject != null)
            {
                zetTextObject.SetActive(show);
            }
        }
    }
}

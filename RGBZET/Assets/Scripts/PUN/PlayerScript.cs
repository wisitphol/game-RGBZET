using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    public TMP_Text nameText;
    public GameObject zetText;
    //private bool playerNameHasBeenSet = false;

    private void Start()
    {
        // ตรวจสอบว่ามีชื่อผู้เล่นที่ถูกเก็บไว้หรือไม่
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "DefaultName");
            SetPlayerName(playerName);
        }



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
        //playerNameHasBeenSet = true;
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
    private void ToggleZetText(int photonId, bool show)
    {
        PhotonView targetPhotonView = PhotonView.Find(photonId);
        if (targetPhotonView != null)
        {
            GameObject zetTextObject = ZETManager2.instance.GetZetText(targetPhotonView.ViewID);
            if (zetTextObject != null)
            {
                zetTextObject.SetActive(show);
            }
            else
            {
                Debug.LogWarning("ZetText not found for PhotonViewID: " + photonId);
            }
        }
        else
        {
            Debug.LogWarning("PhotonView not found for photonId: " + photonId);
        }
    }
}

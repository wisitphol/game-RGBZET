using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    public TMP_Text nameText;
    public TMP_Text scoreText;
    public GameObject zetText; // ตัวแปรเก็บ GameObject ของข้อความ ZET

    private void Start()
    {
        if (photonView.IsMine)
        {
            ZETManager2.instance.RegisterZetText(photonView.ViewID, zetText);
        }
    }

    // เมื่อกดปุ่ม zet
    public void OnZetButtonPressed()
    {
        // สร้างเงื่อนไขเพื่อเปิด/ปิดการแสดงข้อความ ZET ตามสถานะการกดปุ่ม zet
        if (ZETManager2.isZETActive)
        {
            zetText.SetActive(true);
        }
        else
        {
            zetText.SetActive(false);
        }

        photonView.RPC("ToggleZetText", RpcTarget.All, true);

        if (ZETManager2.instance != null)
        {
            ZETManager2.instance.ToggleZetText(photonView.ViewID, true);
        }
    }

}

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerCon : MonoBehaviourPunCallbacks
{
    public GameObject zettext; // อ้างอิงไปยัง object zet ใน player
    //private bool isZetActive = false; // สถานะการแสดง zet
    public TMP_Text NameText;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    private float cooldownTimer = 0f; // เวลาที่เหลือจากการ cooldown
    private Button zetButton;
    private int playerID;

    void Start()
    {
         zetButton = GetComponent<Button>();

        if (zetButton != null)
        {
            // ซ่อน object zet เมื่อเริ่มต้น
            zettext.SetActive(false);
        }
        else
        {
            //Debug.LogError("zetButton is null");
        }

    }

    void Update()
    {
        // ตรวจสอบว่าเวลา cooldown ของปุ่มเสร็จสิ้นหรือยัง
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                // เมื่อเวลา cooldown เสร็จสิ้น ซ่อน zet
                zettext.SetActive(false);
            }
        }
    }


    // เมื่อกดปุ่ม zet
    public void OnZetButtonPressed()
    {
        // เปลี่ยนสถานะการแสดง zet และส่ง RPC เพื่ออัพเดตสถานะนี้ให้กับผู้เล่นอื่น ๆ
        //isZetActive = !isZetActive;

        //ToggleZet(isZetActive);

        zettext.SetActive(true);

        photonView.RPC("ToggleZet", RpcTarget.All, true);

        cooldownTimer = cooldownTime;


    }

    // RPC เพื่อเปิด/ปิดการแสดง zet บน object player
    [PunRPC]
    private void ToggleZet(bool show)
    {
        zettext.SetActive(show);
    }

    public void SetPlayerName(string playerName)
    {
        if (NameText != null)
        {
            NameText.text = playerName;
            Debug.Log("Player ID: " + playerID + " updated name to: " + playerName);
        }
        else
        {
            Debug.LogError("NameText is null");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public GameObject zettext; // อ้างอิงไปยัง object zet ใน player
    private bool isZetActive = false; // สถานะการแสดง zet

    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    private float cooldownTimer = 0f; // เวลาที่เหลือจากการ cooldown
    private Button zetButton;

    void Start()
    {
        // ซ่อน object zet เมื่อเริ่มต้น
        zettext.SetActive(false);

        if (zetButton != null)
        {
            // กำหนดค่าให้กับ zetButton โดยอ้างอิงจาก Button component บน GameObject นี้
            zetButton = GetComponent<Button>();
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
}

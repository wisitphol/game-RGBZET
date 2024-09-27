using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerCon : MonoBehaviour
{
    public GameObject zettext; // อ้างอิงไปยัง object zet ใน player
    public TMP_Text NameText;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    private float cooldownTimer = 0f; // เวลาที่เหลือจากการ cooldown
    [SerializeField] public Button zetButton;

    void Start()
    {
        zettext.SetActive(false);

        zetButton.onClick.AddListener(OnZetButtonPressed);
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
        zettext.SetActive(true);

        cooldownTimer = cooldownTime;
    }
}

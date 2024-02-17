using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Button1 : MonoBehaviour
{
    public static bool isZetActive = false; // ตรวจสอบว่ามีผู้เล่นกดปุ่ม ZET หรือยัง
    public Button zetButton; // ปุ่ม ZET ใน UI

    // ฟังก์ชันที่เรียกเมื่อปุ่ม ZET ถูกกด
    public void OnZetButtonPressed()
    {
        if (!isZetActive)
        {
            isZetActive = true; // ตั้งค่าว่ามีผู้เล่นกำลังใช้งาน ZET
            zetButton.interactable = false; // ปิดใช้งานปุ่มเพื่อไม่ให้ผู้เล่นอื่นกด
            Debug.Log("ZET activated by a player.");
            // TODO: ล็อกการเคลื่อนย้ายการ์ดของผู้เล่นอื่น

            // เรียกใช้ Coroutine เพื่อจำลองการกระทำที่ผู้เล่นต้องทำ
            StartCoroutine(PlayerActionCompleted());
        }
    }

    // Coroutine ที่รอการกระทำของผู้เล่นเสร็จสิ้น
    IEnumerator PlayerActionCompleted()
    {
        // จำลองการกระทำที่ใช้เวลา 5 วินาที
        yield return new WaitForSeconds(10);

        // หลังจากผู้เล่นเสร็จสิ้นการกระทำ
        isZetActive = false; // รีเซ็ตสถานะ
        zetButton.interactable = true; // เปิดใช้งานปุ่มอีกครั้ง
        Debug.Log("Player action completed, ZET is now available again.");
        // TODO: ปลดล็อกการเคลื่อนย้ายการ์ดสำหรับผู้เล่นทุกคน
    }

    void Start()
    {
        // ตั้งค่าเริ่มต้นให้ปุ่ม ZET สามารถใช้งานได้
        zetButton.interactable = true;
    }
}


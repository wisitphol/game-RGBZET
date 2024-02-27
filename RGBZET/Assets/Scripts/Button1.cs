using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Button1 : MonoBehaviour
{
    public static bool isZetActive = false; // ตรวจสอบว่ามีผู้เล่นกดปุ่ม ZET หรือยัง
    public Button zetButton; // ปุ่ม ZET ใน UI
   
    void Start()
    {
        // ตั้งค่าเริ่มต้นให้ปุ่ม ZET สามารถใช้งานได้
        zetButton.interactable = true;
    }


    // ฟังก์ชันที่เรียกเมื่อปุ่ม ZET ถูกกด
    public void OnZetButtonPressed()
    {
        if (!isZetActive)
        {
            isZetActive = true;
            zetButton.interactable = false;
            Debug.Log("ZET activated by a player.");

        

            
        }
    }

    public void CheckSetConditionsCompleted()
    {
        if (isZetActive)
        {
            isZetActive = false; // รีเซ็ตสถานะ
            zetButton.interactable = true; // เปิดใช้งานปุ่มอีกครั้ง
            Debug.Log("Player action completed, ZET is now available again.");
        }
    }

   
}


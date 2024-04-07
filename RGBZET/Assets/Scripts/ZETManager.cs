using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ZETManager : MonoBehaviour
{
    
    public static bool isZETActive = false;
    public Button zetButton;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    public GameObject ZETText; // เพิ่มตัวแปรเก็บ GameObject ของข้อความ ZET


    private void Start()
    {
        zetButton.interactable = true;
        ZETText.SetActive(false); // เริ่มต้นปิดการแสดงข้อความ ZET
    }

    public void OnZetButtonPressed()
    {
        if (!isZETActive)
        {
            StartCoroutine(ActivateZetWithCooldown());

           
        }
    }

    private IEnumerator ActivateZetWithCooldown()
    {
        isZETActive = true;
        zetButton.interactable = false;
        ZETText.SetActive(true); // เปิดการแสดงข้อความ ZET เมื่อกดปุ่ม
        Debug.Log("ZET activated by a player.");

        yield return new WaitForSeconds(cooldownTime);

        isZETActive = false;
        zetButton.interactable = true;
        ZETText.SetActive(false); // เปิดการแสดงข้อความ ZET เมื่อกดปุ่ม
        Debug.Log("ZET is now available again after cooldown.");
    }
}
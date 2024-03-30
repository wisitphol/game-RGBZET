using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ZETButton : MonoBehaviour
{
    
    public static bool isZetActive = false;
    public Button zetButton;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    public GameObject ZetText; // เพิ่มตัวแปรเก็บ GameObject ของข้อความ ZET
    private void Start()
    {
        zetButton.interactable = true;
        ZetText.SetActive(false); // เริ่มต้นปิดการแสดงข้อความ ZET
    }

    public void OnZetButtonPressed()
    {
        if (!isZetActive)
        {
            StartCoroutine(ActivateZetWithCooldown());
        }
    }

    private IEnumerator ActivateZetWithCooldown()
    {
        isZetActive = true;
        zetButton.interactable = false;
        ZetText.SetActive(true); // เปิดการแสดงข้อความ ZET เมื่อกดปุ่ม
        Debug.Log("ZET activated by a player.");

        yield return new WaitForSeconds(cooldownTime);

        isZetActive = false;
        zetButton.interactable = true;
        ZetText.SetActive(false); // เปิดการแสดงข้อความ ZET เมื่อกดปุ่ม
        Debug.Log("ZET is now available again after cooldown.");
    }
}
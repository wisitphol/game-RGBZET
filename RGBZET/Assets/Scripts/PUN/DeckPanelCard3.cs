using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckPanelCard3 : MonoBehaviour
{
    public GameObject cardBack3;

    void Update()
    {
         // ตรวจสอบว่ามีการ์ดอยู่ในพาเนลหรือไม่
        if (transform.childCount > 0)
        {
            // มีการ์ดอยู่ในพาเนล ให้เปิดการแสดงของ cardBack
            cardBack3.SetActive(true);
        }
        else
        {
            // ไม่มีการ์ดอยู่ในพาเนล ให้ปิดการแสดงของ cardBack
            cardBack3.SetActive(false);
        }
    }
}

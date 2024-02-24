using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckPanelCard : MonoBehaviour
{
    public GameObject cardBack;

    void Update()
    {
         // ตรวจสอบว่ามีการ์ดอยู่ในพาเนลหรือไม่
        if (transform.childCount > 0)
        {
            // มีการ์ดอยู่ในพาเนล ให้เปิดการแสดงของ cardBack
            cardBack.SetActive(true);
        }
        else
        {
            // ไม่มีการ์ดอยู่ในพาเนล ให้ปิดการแสดงของ cardBack
            cardBack.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

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
            //Debug.Log("Card(s) detected in the panel. Activating cardBack.");
        }
        else
        {
            // ไม่มีการ์ดอยู่ในพาเนล ให้ปิดการแสดงของ cardBack
            cardBack.SetActive(false);
           // Debug.Log("No card detected in the panel. Deactivating cardBack.");
        }
    }
}

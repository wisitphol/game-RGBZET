using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public List<GameObject> cardZone; // เก็บรายการ GameObject ของพื้นที่การ์ด

    // เพิ่มการ์ดเข้าไปในพื้นที่การ์ด
    public void AddCard()
    {
        // ตรวจสอบว่ายังมีพื้นที่ว่างใน cardZone หรือไม่
        if (cardZone.Count < 3)
        {
            // เพิ่มการ์ดเข้าไปในพื้นที่การ์ด
            // เรียกเมธอดหรือทำการกระทำที่เกี่ยวข้องเมื่อการ์ดถูกเพิ่มเข้าไป
            Debug.Log("Card added to the card zone.");
        }
        else
        {
            Debug.Log("Cannot add more cards. The card zone is full.");
        }
    }

    // ลบการ์ดออกจากพื้นที่การ์ด
    public void RemoveCard()
    {
        // ตรวจสอบว่ามีการ์ดอยู่ในพื้นที่การ์ดหรือไม่
        if (cardZone.Count > 0)
        {
            // ลบการ์ดออกจากพื้นที่การ์ด
            // เรียกเมธอดหรือทำการกระทำที่เกี่ยวข้องเมื่อการ์ดถูกลบออก
            Debug.Log("Card removed from the card zone.");
        }
        else
        {
            Debug.Log("No cards to remove from the card zone.");
        }
    }

    // ตรวจสอบว่าพื้นที่การ์ดมีการ์ดครบตามที่กำหนดหรือไม่
    public bool IsCardZoneFull()
    {
        return cardZone.Count >= 3;
    }
}

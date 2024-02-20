using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CardZone2 : MonoBehaviour
{
    private List<GameObject> objectsInZone = new List<GameObject>(); // เก็บ GameObject ที่อยู่ในโซน

    // เมื่อ GameObject เข้ามาในโซน
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("YourObjectType")) // ตรวจสอบ tag ของ Object เพื่อให้แน่ใจว่าเป็น Object ที่เราต้องการ
        {
            if (objectsInZone.Count < 3) // เช็คว่ามี Object ในโซนน้อยกว่า 3 ชิ้นหรือไม่
            {
                // เพิ่ม Object เข้าไปใน List ของ Objects ในโซน
                objectsInZone.Add(other.gameObject);
                Debug.Log("Object added to zone.");
            }
            else
            {
                Debug.Log("Cannot add more objects. Zone is full."); // แสดงข้อความเมื่อโซนเต็มแล้ว
            }
        }
    }

    // เมื่อ GameObject ออกจากโซน
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("YourObjectType")) // ตรวจสอบ tag ของ Object เพื่อให้แน่ใจว่าเป็น Object ที่เราต้องการ
        {
            // ลบ Object ที่ออกจากโซนออกจาก List
            objectsInZone.Remove(other.gameObject);
            Debug.Log("Object removed from zone.");
        }
    }
}

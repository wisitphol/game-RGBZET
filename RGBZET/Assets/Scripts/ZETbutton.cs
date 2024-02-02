using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ZETbutton : MonoBehaviour
{
    private List<Card> selectedCards = new List<Card>();

    // สมมติว่าคุณมีวิธีการเพื่อทำให้การ์ดเป็น "selected"
    // ฟังก์ชันนี้จะถูกเรียกเมื่อผู้เล่นกดปุ่ม ZET
    public void OnZETButtonPressed()
    {
        // อาจต้องการรหัสเพิ่มเติมเพื่อการเลือกการ์ด
        selectedCards = SelectCards();

        // ตรวจสอบเงื่อนไข
        if (CheckSelectedCards(selectedCards))
        {
            // ทำลายหรือย้ายการ์ดที่เลือก
            RemoveOrDisableSelectedCards(selectedCards);
        }
        else
        {
            // คืนสถานะการ์ด
            ResetSelectedCards(selectedCards);
        }
    }

    private List<Card> SelectCards()
    {
        // รหัสสำหรับการเลือกการ์ด
        List<Card> cards = new List<Card>();
        // จำลองการเลือกการ์ด
        return cards;
    }

    private bool CheckSelectedCards(List<Card> cards)
    {
        // ตรวจสอบเงื่อนไขของการ์ดที่ถูกเลือก
        // สมมติว่าคุณมีเงื่อนไขการตรวจสอบที่นี่
        return true; // หรือ false ขึ้นอยู่กับเงื่อนไข
    }

    private void RemoveOrDisableSelectedCards(List<Card> cards)
    {
        // รหัสสำหรับการลบหรือย้ายการ์ดที่ถูกเลือก
        foreach (Card card in cards)
        {
            // Destroy(card.gameObject); หรือ card.gameObject.SetActive(false);
        }
    }

    private void ResetSelectedCards(List<Card> cards)
    {
        // รหัสสำหรับการคืนสถานะการ์ด
        foreach (Card card in cards)
        {
            // ล้างการเลือกหรือคืนค่าสถานะที่เป็นไปได้
        }
    }
}


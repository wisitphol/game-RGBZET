using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ZETbutton : MonoBehaviour
{
    
    private List<Card> selectedCards = new List<Card>();
    public List<DisplayCard> allDisplayCards;

    public void CardClicked(Card card)
    {
        // Assuming you have a way to get the corresponding DisplayCard component
        DisplayCard display = allDisplayCards.Find(d => d.Id == card.Id);

        if (display != null)
        {
            bool highlight = selectedCards.Contains(card);
            display.HighlightCard(highlight);
        }

        if (selectedCards.Contains(card))
        {
            // Card is already selected, deselect it
            selectedCards.Remove(card);
            display.HighlightCard(false); // Update visual state
        }
        else if (selectedCards.Count < 3)
        {
            // Select the new card
            selectedCards.Add(card);
            display.HighlightCard(true); // Update visual state
        }

        // Debug.Log เมื่อมีการเลือกการ์ด
        Debug.Log("Selected Cards: " + selectedCards.Count);
    }

    // ฟังก์ชันนี้จะถูกเรียกเมื่อผู้เล่นกดปุ่ม ZET
    public void OnZETButtonPressed()
    {
        if (selectedCards.Count == 3)
        {
            // ทำอะไรก็ตามกับการ์ดที่เลือก
            // ตัวอย่าง: อาจมีโค้ดที่ส่งการ์ดที่เลือกไปที่ฟังก์ชันอื่น
            Debug.Log("ZET button pressed with 3 selected cards.");
            
            // เรียกฟังก์ชันเช็คการ์ดที่ถูกเลือก
            CheckSelectedCards(selectedCards);
            
            // ล้างการ์ดที่ถูกเลือกหลังจากการกดปุ่ม "zet"
            selectedCards.Clear();
            
            // รีเซ็ตสถานะการ์ดที่ถูกเลือก
            ResetSelectedCards(allDisplayCards);
        }
        else
        {
            // แสดงข้อความเตือนหรือแจ้งเตือนผู้เล่นว่าต้องเลือกการ์ด 3 ใบ
            Debug.Log("Please select 3 cards to use ZET.");
        }
    }

    private void CheckSelectedCards(List<Card> cards)
    {
        // ตรวจสอบเงื่อนไขของการ์ดที่ถูกเลือก
        // สมมติว่าคุณมีเงื่อนไขการตรวจสอบที่นี่
        // ตัวอย่าง: ให้เรียกฟังก์ชันสำหรับการตรวจสอบการ์ดที่ถูกเลือก
    }

    private void ResetSelectedCards(List<DisplayCard> displayCards)
    {
        // รีเซ็ตสถานะการ์ดที่ถูกเลือก
        foreach (DisplayCard display in displayCards)
        {
            display.HighlightCard(false);
        }
    }
}

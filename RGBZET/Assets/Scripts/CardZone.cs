using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardZone : MonoBehaviour
{
    public int maxCardsAllowed = 3; // จำนวนการ์ดสูงสุดที่สามารถวางได้ในโซนนี้
    private int currentCardCount = 0; // จำนวนการ์ดปัจจุบันในโซนนี้

    // เพิ่มจำนวนการ์ดในโซน
    public void AddCard()
    {
        if (currentCardCount < maxCardsAllowed)
        {
            currentCardCount++;
            Debug.Log("Card added. Current count: " + currentCardCount);
        }
        else
        {
            Debug.Log("Cannot add more cards. Maximum limit reached.");
        }
    }

    // ลบจำนวนการ์ดในโซน
    public void RemoveCard()
    {
        if (currentCardCount > 0)
        {
            currentCardCount--;
            Debug.Log("Card removed. Current count: " + currentCardCount);
        }
        else
        {
            Debug.Log("No cards to remove.");
        }
    }
}

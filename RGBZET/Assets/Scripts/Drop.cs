using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Drop : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private List<Card> droppedCards = new List<Card>(); // เก็บคุณสมบัติของการ์ดที่ลากมาวางลงในพื้นที่ drop
    private DisplayCard displayCard;
    public Text scoreText; // อ้างอิงไปยัง Text UI สำหรับแสดงคะแนน

    public void OnPointerEnter(PointerEventData eventData)
    {
        // เมื่อมี pointer เข้ามาในระยะ
        if (eventData.pointerDrag == null)
        return;

        //Debug.Log("Pointer entered drop zone");
    }

    public void OnDrop(PointerEventData eventData)
    {
         // ตรวจสอบก่อนว่าปุ่ม ZET ถูกกดแล้วหรือยัง
        if (!Button1.isZetActive)
        {
            //Debug.Log("Cannot drop. ZET button has not been pressed.");
            return; // ยกเลิกการ drop ถ้าปุ่ม ZET ยังไม่ถูกกด
        }

        //Debug.Log("OnDrop event detected");

        int numberOfCardsOnDropZone = CheckNumberOfCardsOnDropZone();

        if (numberOfCardsOnDropZone == 3)
        {
            Debug.Log("Cannot drop. Maximum number of cards reached.");
            return;
        }

        // ตรวจสอบว่ามี object ที่ลากมาวางลงหรือไม่
        if (eventData.pointerDrag != null)
        {
            // เรียกใช้สคริปต์ Drag ของ object ที่ลากมา
            Drag draggable = eventData.pointerDrag.GetComponent<Drag>();
            if (draggable != null)
            {
                // กำหนด parent ใหม่ให้กับ object ที่ลากมา เพื่อให้วางลงใน panel นี้
                draggable.parentToReturnTo = this.transform;
            }

            DisplayCard displayCard = eventData.pointerDrag.GetComponent<DisplayCard>();
            if (displayCard != null)
            {
                droppedCards.Add(displayCard.displayCard[0]);
            }

            CheckSetConditions();
        }

        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // เมื่อ pointer ออกจากระยะ
        if (eventData.pointerDrag == null)
        return;

        //Debug.Log("Pointer exited drop zone");
    }

    private int CheckNumberOfCardsOnDropZone()
    {
        int numberOfCards = this.transform.childCount;
        return numberOfCards;
    }

    private void CheckSetConditions()
    {
        
        // เรียกเมธอดหรือทำงานเพิ่มเติมที่ต้องการเมื่อมีการ์ดทั้ง 3 ใบในพื้นที่ drop
        // ตรวจสอบว่าการ์ดทั้งสามใบตรงเงื่อนไขหรือไม่ ตามกฎของเกม Set

        if(droppedCards.Count == 3)
        {
            bool isSet = CheckCardsAreSet(droppedCards[0], droppedCards[1], droppedCards[2]);

            if (isSet)
            {
                // คำนวณคะแนนรวมของการ์ด 3 ใบ
                int totalScore = CalculateTotalScore(droppedCards[0], droppedCards[1], droppedCards[2]);
            
                // แสดงคะแนนที่ Text UI
                scoreText.text =  totalScore.ToString();
                Debug.Log("Set found! Remove the cards from the game.");
                // ลบการ์ดที่ตรงเงื่อนไขออกจากเกม
                
                
            }
            else
            {
                Debug.Log("Not a valid Set. Return the cards to their original position.");
                // นำการ์ดที่ไม่ตรงเงื่อนไขกลับไปที่ตำแหน่งเดิม
                
            }
        }
         else
        {
            Debug.Log("Unexpected number of cards: " + droppedCards.Count); // Debug Log เมื่อมีจำนวนการ์ดไม่ถูกต้อง
        }
        
    }

    private bool CheckCardsAreSet(Card card1, Card card2, Card card3)
    {
        // ตรวจสอบคุณสมบัติของการ์ดทั้งสามใบ
        bool letterSet = ArePropertiesEqual(card1.LetterType, card2.LetterType, card3.LetterType);
        bool colorSet = ArePropertiesEqual(card1.ColorType, card2.ColorType, card3.ColorType);
        bool sizeSet = ArePropertiesEqual(card1.SizeType, card2.SizeType, card3.SizeType);
        bool textureSet = ArePropertiesEqual(card1.TextureType, card2.TextureType, card3.TextureType);

        // ตรวจสอบว่ามีการ์ดทั้งสามใบมีคุณสมบัติเหมือนหรือต่างกันทั้งหมดหรือไม่
        return (letterSet && colorSet && sizeSet && textureSet);
    }

    // ตรวจสอบว่าคุณสมบัติของการ์ดทั้งสามใบเหมือนหรือต่างกันทั้งหมดหรือไม่
    private bool ArePropertiesEqual(string property1, string property2, string property3)
    {
        return (property1 == property2 && property2 == property3) || (property1 != property2 && property2 != property3 && property1 != property3);
    }

    private void RemoveCardsFromGame()
    {
        // ตรวจสอบจำนวนการ์ดที่อยู่ในพื้นที่ drop
        Debug.Log("Number of cards in droppedCards: " + droppedCards.Count);

        // ตรวจสอบว่ามีการ์ดทั้งสามใบอยู่ในพื้นที่ drop แล้วหรือไม่
        if (droppedCards.Count == 3)
        {
            // ลบการ์ดที่ถูก drop จากเกม
            foreach (Transform cardTransform in transform)
            {
                // ตรวจสอบว่าการ์ดที่ต้องการจะลบได้ยังอยู่ในลิสต์ droppedCards หรือไม่
                DisplayCard displayCard = cardTransform.GetComponent<DisplayCard>();
                if (displayCard != null && droppedCards.Contains(displayCard.displayCard[0]))
                {
                    // ลบการ์ดออกจากลิสต์ droppedCards
                    droppedCards.Remove(displayCard.displayCard[0]);

                    // ลบการ์ดที่ถูก drop ออกจากเกม
                    Destroy(cardTransform.gameObject);
                }
            }

            // ตรวจสอบว่าการ์ดทั้งสามถูกลบออกจากเกมหรือไม่
            if (droppedCards.Count == 0)
            {
                Debug.Log("All cards removed successfully.");
            }
            else
            {
                Debug.Log("Error: Not all cards were removed.");
            
                // ลบการ์ดที่เหลืออยู่อีกครั้ง
                RemoveRemainingCards();
            }
        }
        else
        {
            // หากไม่มีการ์ดทั้งสามใบอยู่ในพื้นที่ drop ให้แสดงข้อความเตือนใน Debug Log
            Debug.Log("Cannot remove cards. Number of cards in the drop zone is not 3.");
        }
    }


    private void RemoveRemainingCards()
    {
        // ลบการ์ดที่เหลืออยู่ออกจากเกม
        foreach (Transform cardTransform in transform)
        {
            // ตรวจสอบว่าการ์ดที่ต้องการจะลบได้ยังอยู่ในลิสต์ droppedCards หรือไม่
            DisplayCard displayCard = cardTransform.GetComponent<DisplayCard>();
            if (displayCard != null && droppedCards.Contains(displayCard.displayCard[0]))
            {
                // ลบการ์ดออกจากลิสต์ droppedCards
                droppedCards.Remove(displayCard.displayCard[0]);

                // ลบการ์ดที่ถูก drop ออกจากเกม
                Destroy(cardTransform.gameObject);
            }
        }

        // ตรวจสอบว่าการ์ดทั้งสามถูกลบออกจากเกมหรือไม่
        if (droppedCards.Count == 0)
        {
            Debug.Log("All remaining cards removed successfully.");
        }
        else
        {
            Debug.Log("Error: Not all remaining cards were removed.");
        }
    }

    private int CalculateTotalScore(Card card1, Card card2, Card card3)
    {
        // คำนวณคะแนนรวมของการ์ดทั้ง 3 ใบ
        int totalScore = card1.Point + card2.Point + card3.Point;
        return totalScore;
    }


    private void ReturnCardsToOriginalPosition()
    {
        
    }

}

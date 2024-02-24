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
    private List<Vector3> originalPositions = new List<Vector3>(); // เพิ่มส่วนนี้
    private List<Quaternion> originalRotations = new List<Quaternion>(); // เพิ่มส่วนนี้
   
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

        StoreOriginalPositionsAndRotations(); // เรียกเมทอดนี้เพื่อเก็บตำแหน่งและการหมุนเริ่มต้นของการ์ดที่วางลงบนบอร์ด

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

            DisplayCard displayCardComponent = eventData.pointerDrag.GetComponent<DisplayCard>();
            if (displayCardComponent != null)
            {
                
                droppedCards.Add(displayCardComponent.displayCard[0]);
            }

           
            
        }

        CheckSetConditions();   
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
         Debug.Log("number of cards: " + droppedCards.Count);
        if(droppedCards.Count == 3)
        {
           
            bool isSet = CheckCardsAreSet(droppedCards[0], droppedCards[1], droppedCards[2]);

            if (isSet)
            {
                // คำนวณคะแนนรวมของการ์ด 3 ใบ
                int totalScore = CalculateTotalScore(droppedCards[0], droppedCards[1], droppedCards[2]);
            
                // แสดงคะแนนที่ Text UI
                scoreText.text =  totalScore.ToString();
                //Debug.Log("Set found! Remove the cards from the game.");
                // ลบการ์ดที่ตรงเงื่อนไขออกจากเกม
                RemoveAllCardsFromGame();
                
                
            }
            else
            {
                Debug.Log("Not a valid Set. Return the cards to their original position.");
                // นำการ์ดที่ไม่ตรงเงื่อนไขกลับไปที่ตำแหน่งเดิม
                ReturnCardsToOriginalPosition();
                
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

    private int CalculateTotalScore(Card card1, Card card2, Card card3)
    {
        // คำนวณคะแนนรวมของการ์ดทั้ง 3 ใบ
        int totalScore = card1.Point + card2.Point + card3.Point;
        return totalScore;
    }

/*
    private void ReturnCardsToOriginalPosition2()
    {
       
        foreach (var droppedCard in droppedCards)
        {
            // หาตำแหน่งและการหมุนเริ่มต้นของการ์ดจากลิสต์ originalPositions และ originalRotations
            int index = droppedCards.IndexOf(droppedCard);
            Vector3 originalPosition = originalPositions[index];
            Quaternion originalRotation = originalRotations[index];

            // นำการ์ดกลับไปยังตำแหน่งและการหมุนเริ่มต้น
            for (int i = 0; i < droppedCards.Count; i++)
            {
                droppedCards[i].transform.position = originalPositions[i];
                droppedCards[i].transform.rotation = originalRotations[i];
            }
        }
        
        // เคลียร์รายการการ์ดที่มีในเขตตรวจสอบ
        droppedCards.Clear();

    }*/

    private void ReturnCardsToOriginalPosition3()
    {
        foreach (Transform cardTransform in transform)
        {
            DisplayCard displayCard = cardTransform.GetComponent<DisplayCard>();
            if (displayCard != null)
            {
                displayCard.SetBlocksRaycasts(true); // เปิดการใช้งาน blocksRaycasts เมื่อคืนตำแหน่ง
            }
            cardTransform.localPosition = Vector3.zero;
        }
        droppedCards.Clear();
    }
    private IEnumerator RemoveCardsFromGameCoroutine()
    {
        Debug.Log($"Attempting to remove {droppedCards.Count} cards with delay.");
        foreach (Transform cardTransform in transform)
        {
            DisplayCard displayCard = cardTransform.GetComponent<DisplayCard>();
            if (displayCard != null && droppedCards.Contains(displayCard.displayCard[0]))
            {
                //Debug.Log($"Removing card with delay: {displayCard.displayCard[0].Id}");
                // หน่วงเวลาก่อนการลบ
                yield return new WaitForSeconds(1f);
                Destroy(cardTransform.gameObject);
            }
        }

        droppedCards.Clear();
        //Debug.Log("All specified cards removed with delay.");
        
    }

    // เรียกใช้ Coroutine
    private void RemoveCardsFromGame()
    {
        StartCoroutine(RemoveCardsFromGameCoroutine());
    }

    private void RemoveAllCardsFromGame()
    {
        foreach (Transform cardTransform in transform)
        {
            Destroy(cardTransform.gameObject);
        }
        droppedCards.Clear();
    }


    private void StoreOriginalPositionsAndRotations()
    {
        foreach (Transform cardTransform in transform)
        {
            DisplayCard displayCard = cardTransform.GetComponent<DisplayCard>();
            if (displayCard != null)
            {
                displayCard.StoreOriginalPositionAndRotation();
                displayCard.SetBlocksRaycasts(false); // ปิดการใช้งาน blocksRaycasts เมื่อลาก
            }
        }
    }

    private void ReturnCardsToOriginalPosition()
    {
        foreach (Transform cardTransform in transform)
        {
            DisplayCard displayCard = cardTransform.GetComponent<DisplayCard>();
            if (displayCard != null)
            {
                displayCard.ReturnToOriginalPosition();
                displayCard.SetBlocksRaycasts(true); // เปิดการใช้งาน blocksRaycasts เมื่อคืนตำแหน่ง
            }
        }
    }
}

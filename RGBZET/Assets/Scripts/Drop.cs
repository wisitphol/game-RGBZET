using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Drop : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text scoreText; // อ้างอิงไปยัง Text UI สำหรับแสดงคะแนน
    
    [HideInInspector]
    public List<Card> droppedCards = new List<Card>(); // เก็บคุณสมบัติของการ์ดที่ลากมาวางลงในพื้นที่ drop
    [HideInInspector]
    public Transform parentToReturnTo;
    [HideInInspector]
    public Deck deck;
    [HideInInspector]
    public int currentScore;


    public void Start() 
    {
        deck = FindObjectOfType<Deck>(); // หรือใช้วิธีการค้นหาที่สอดคล้องกับโครงสร้างของโปรเจคของคุณ
        currentScore = 0; // เพิ่มบรรทัดนี้เพื่อกำหนดค่าเริ่มต้นของ currentScore
        
    }

    public void Update()
    {
        // เมื่อปุ่ม ZET ไม่ได้ถูกกด
        if (!ZETbutton.isZetActive)
        {
            // ตรวจสอบจำนวนการ์ดใน Checkzone
            int numberOfCardsInCheckZone = transform.childCount;

            // ถ้ามีการ์ดใน Checkzone และไม่ได้ถูกกด ZET
            if (numberOfCardsInCheckZone > 0 && numberOfCardsInCheckZone < 3)
            {
                // วนลูปเพื่อนำการ์ดทั้งหมดใน Checkzone กลับไปยัง Boardzone
                for (int i = numberOfCardsInCheckZone - 1; i >= 0; i--) // เริ่มจากตัวสุดท้าย
                {
                    Transform cardTransform = transform.GetChild(i);
                    if (cardTransform != null)
                    {
                        cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                        cardTransform.localPosition = Vector3.zero;

                        DisplayCard displayCardComponent = cardTransform.GetComponent<DisplayCard>();
                        if (displayCardComponent != null)
                        {
                            displayCardComponent.SetBlocksRaycasts(true);
                        }
                    }
                }

                // ล้างรายการการ์ดใน Checkzone
                droppedCards.Clear();

                Debug.Log("Returned cards to Boardzone because ZET button was not pressed.");
            }
        }
    }





    public void OnPointerEnter(PointerEventData eventData)
    {
        // เมื่อมี pointer เข้ามาในระยะ
        if (eventData.pointerDrag == null)
        return;

    }

    public void OnDrop(PointerEventData eventData)
    {
        // ตรวจสอบก่อนว่าปุ่ม ZET ถูกกดแล้วหรือยัง
        if (!ZETbutton.isZetActive)
        {
            
            //Debug.Log("Cannot drop. ZET button has not been pressed.");
            return; // ยกเลิกการ drop ถ้าปุ่ม ZET ยังไม่ถูกกด
        }

        parentToReturnTo = transform.parent;

       

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

            DisplayCard displayCardComponent = eventData.pointerDrag.GetComponent<DisplayCard>();
            if (displayCardComponent != null)
            {
                
                droppedCards.Add(displayCardComponent.displayCard[0]);
            }
            
        }

        StartCoroutine(CheckSetWithDelay());
        
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // เมื่อ pointer ออกจากระยะ
        if (eventData.pointerDrag == null)
        return;

    }

    public int CheckNumberOfCardsOnDropZone()
    {
        int numberOfCards = this.transform.childCount;
        return numberOfCards;
    }

    public void CheckSetConditions()
    {
        
        // เรียกเมธอดหรือทำงานเพิ่มเติมที่ต้องการเมื่อมีการ์ดทั้ง 3 ใบในพื้นที่ drop
        // ตรวจสอบว่าการ์ดทั้งสามใบตรงเงื่อนไขหรือไม่ ตามกฎของเกม Set
         //Debug.Log("number of cards: " + droppedCards.Count);
        if(droppedCards.Count == 3)
        {
           
            bool isSet = CheckCardsAreSet(droppedCards[0], droppedCards[1], droppedCards[2]);

            if (isSet)
            {
                // คำนวณคะแนนรวมของการ์ด 3 ใบ
                int TotalScore = CalculateTotalScore(droppedCards[0], droppedCards[1], droppedCards[2]);
            
                UpdateScore(TotalScore);
                
                //scoreText.text =  totalScore.ToString();
               
                RemoveCardsFromGame();

                deck.DrawCards(3);
            }
            else
            {
                //Debug.Log("Not a valid Set. Return the cards to their original position.");
                // นำการ์ดที่ไม่ตรงเงื่อนไขกลับไปที่ตำแหน่งเดิม
                ReturnCardsToOriginalPosition();

                UpdateScore(-1);
                
            }
            //FindObjectOfType<ZETbutton>().CheckSetConditionsCompleted();
            
        }
        else
        {
            //Debug.Log("Unexpected number of cards: " + droppedCards.Count); // Debug Log เมื่อมีจำนวนการ์ดไม่ถูกต้อง
        }

    }

    public bool CheckCardsAreSet(Card card1, Card card2, Card card3)
    {
        // ตรวจสอบคุณสมบัติของการ์ดทั้งสามใบ
        bool letterSet = ArePropertiesEqual(card1.LetterType, card2.LetterType, card3.LetterType);
        bool colorSet = ArePropertiesEqual(card1.ColorType, card2.ColorType, card3.ColorType);
        bool amountSet = ArePropertiesEqual(card1.AmountType, card2.AmountType, card3.AmountType);
        bool fontSet = ArePropertiesEqual(card1.FontType, card2.FontType, card3.FontType);

        // ตรวจสอบว่ามีการ์ดทั้งสามใบมีคุณสมบัติเหมือนหรือต่างกันทั้งหมดหรือไม่
        return (letterSet && colorSet && amountSet && fontSet);
    }

    // ตรวจสอบว่าคุณสมบัติของการ์ดทั้งสามใบเหมือนหรือต่างกันทั้งหมดหรือไม่
    public bool ArePropertiesEqual(string property1, string property2, string property3)
    {
        return (property1 == property2 && property2 == property3) || (property1 != property2 && property2 != property3 && property1 != property3);
    }

    public int CalculateTotalScore(Card card1, Card card2, Card card3)
    {
        // คำนวณคะแนนรวมของการ์ดทั้ง 3 ใบ
        int totalScore = card1.Point + card2.Point + card3.Point;
        return totalScore;
    }

    public IEnumerator CheckSetWithDelay()
    {
        // หน่วงเวลา 3 วินาที
        yield return new WaitForSeconds(4f);

        // ตรวจสอบเงื่อนไขการ์ดหลังจากหน่วงเวลา 3 วินาที
        CheckSetConditions();
    }

    public void ReturnCardsToOriginalPosition()
    {
        if (transform.childCount > 0)
        {
            // เก็บ parent และ local position ของการ์ดที่ต้องการย้ายกลับไปยัง boardzone
            List<Transform> cardsToReturn = new List<Transform>();

            foreach (Transform cardTransform in transform)
            {
                cardsToReturn.Add(cardTransform);
            }

            Debug.Log("Number of cards to return: " + cardsToReturn.Count);

            // ย้ายการ์ดทั้งหมดกลับไปยัง boardzone
            foreach (Transform cardTransform in cardsToReturn)
            {
                cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                cardTransform.localPosition = Vector3.zero;

                DisplayCard displayCardComponent = cardTransform.GetComponent<DisplayCard>();
                if (displayCardComponent != null)
                {
                    displayCardComponent.SetBlocksRaycasts(true);
                }
            }

            droppedCards.Clear();
        }
    }

    public void RemoveCardsFromGame()
    {
        // ตรวจสอบจำนวนการ์ดที่อยู่ในพื้นที่ drop
        int numberOfCards = transform.childCount;

        // ลูปเพื่อลบการ์ดที่อยู่ในพื้นที่ drop ทั้งหมด
        for (int i = 0; i < numberOfCards; i++)
        {
            Transform cardTransform = transform.GetChild(i);
            DisplayCard displayCard = cardTransform.GetComponent<DisplayCard>();
            if (displayCard != null && droppedCards.Contains(displayCard.displayCard[0]))
            {
                // ลบการ์ดที่ถูก drop ออกจากเกม
                Destroy(cardTransform.gameObject);

                 // ลบการ์ดออกจากลิสต์ droppedCards
                droppedCards.Remove(displayCard.displayCard[0]);

            }
        }

        droppedCards.Clear();

        // ตรวจสอบว่าทุกการ์ดที่ต้องการลบออกไปได้หรือไม่
        if (droppedCards.Count == 0)
        {
            //Debug.Log("All cards removed successfully.");
        }
        else
        {
            Debug.Log("Error: Not all cards were removed.");

            // Debug สำหรับตรวจสอบ droppedCards ที่ยังคงอยู่หลังจากลบ
            foreach (var card in droppedCards)
            {
                Debug.Log("Remaining card in droppedCards: " + card);
            }
        }
    }

    public void UpdateScore(int addScore)
    {

        currentScore += addScore;
        // Update the score text UI with the total score
        if (currentScore < 0)
        {
            currentScore = 0; // เซ็ตคะแนนใหม่เป็น 0 ถ้าค่าลบ
        }
        scoreText.text = "Score: " + currentScore.ToString();
    }

    public void CheckAndReturnCardsToBoardZone()
    {
        // ตรวจสอบว่าปุ่ม ZET ไม่ได้ถูกกด
        if (!ZETbutton.isZetActive)
        {
            // ตรวจสอบจำนวนการ์ดใน Checkzone
            int numberOfCardsInCheckZone = transform.childCount;

            // ถ้ามีการ์ดใน Checkzone
            if (numberOfCardsInCheckZone > 0 && numberOfCardsInCheckZone < 3)
            {
                // วนลูปเพื่อนำการ์ดทั้งหมดใน Checkzone กลับไปยัง Boardzone
                for (int i = numberOfCardsInCheckZone - 1; i >= 0; i--) // เริ่มจากตัวสุดท้าย
                {
                    Transform cardTransform = transform.GetChild(i);
                    if (cardTransform != null)
                    {
                        cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                        cardTransform.localPosition = Vector3.zero;

                        DisplayCard displayCardComponent = cardTransform.GetComponent<DisplayCard>();
                        if (displayCardComponent != null)
                        {
                            displayCardComponent.SetBlocksRaycasts(true);
                        }
                    }
                }

                // ล้างรายการการ์ดใน Checkzone
                droppedCards.Clear();

                Debug.Log("Returned cards to Boardzone because ZET button was not pressed.");
            }
        }
    }




}

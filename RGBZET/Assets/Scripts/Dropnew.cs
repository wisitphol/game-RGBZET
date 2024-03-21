using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dropnew : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private List<Card> droppedCards = new List<Card>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!Button1.isZetActive)
        {
            return;
        }

        int numberOfCardsOnDropZone = CheckNumberOfCardsOnDropZone();

        if (numberOfCardsOnDropZone == 3)
        {
            Debug.Log("Cannot drop. Maximum number of cards reached.");
            return;
        }

        if (eventData.pointerDrag != null)
        {
            Drag draggable = eventData.pointerDrag.GetComponent<Drag>();
            if (draggable != null)
            {
                draggable.parentToReturnTo = this.transform;
            }

            // เพิ่มการ์ดลงใน droppedCards
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
        if (eventData.pointerDrag == null)
            return;
    }

    private int CheckNumberOfCardsOnDropZone()
    {
        int numberOfCards = this.transform.childCount;
        return numberOfCards;
    }

    private void CheckSetConditions()
    {
        if (droppedCards.Count == 3)
        {
            bool isSet = CheckCardsAreSet(droppedCards[0], droppedCards[1], droppedCards[2]);

            if (isSet)
            {
                Debug.Log("Set found! Remove the cards from the game.");
                // ลบการ์ดที่ตรงเงื่อนไขออกจากเกม
            }
            else
            {
                Debug.Log("Not a valid Set. Return the cards to their original position.");
                // นำการ์ดที่ไม่ตรงเงื่อนไขกลับไปที่ตำแหน่งเดิม
            }
        }
    }

    private bool CheckCardsAreSet(Card card1, Card card2, Card card3)
    {
        bool letterSet = ArePropertiesEqual(card1.LetterType, card2.LetterType, card3.LetterType);
        bool colorSet = ArePropertiesEqual(card1.ColorType, card2.ColorType, card3.ColorType);
        bool amountSet = ArePropertiesEqual(card1.AmountType, card2.AmountType, card3.AmountType);
        bool fontSet = ArePropertiesEqual(card1.FontType, card2.FontType, card3.FontType);

        return (letterSet && colorSet && amountSet && fontSet);
    }

    private bool ArePropertiesEqual(string property1, string property2, string property3)
    {
        return (property1 == property2 && property2 == property3) || (property1 != property2 && property2 != property3 && property1 != property3);
    }
}

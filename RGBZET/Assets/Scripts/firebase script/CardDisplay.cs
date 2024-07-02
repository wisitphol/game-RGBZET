using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    public int Id;
    public string LetterType;
    public string ColorType;
    public string AmountType;
    public string FontType;
    public int Point;

    public void SetCard(Card card)
    {
        Id = card.Id;
        LetterType = card.LetterType;
        ColorType = card.ColorType;
        AmountType = card.AmountType;
        FontType = card.FontType;
        Point = card.Point;

        // Update the UI elements or any other components with the card data
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class Card 
{
    public int Id;
    public string LetterType;   
    public string ColorType;    
    public string AmountType ;    
    public string FontType;  
    public int Point;
    public Sprite Spriteimg;
    public Transform originalParent;

    public Card()
    {

    }

    public Card (int id,string letter,string color,string amount,string font,int point,Sprite spriteimg)
    {
        Id = id;
        LetterType = letter;
        ColorType = color;
        AmountType = amount;
        FontType = font;
        Point = point;
        Spriteimg = spriteimg;
    }
}

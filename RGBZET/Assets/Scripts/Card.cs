using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class Card 
{
    public int Id;
    public string LetterType;   
    public string ColorType;    
    public string SizeType ;    
    public string TextureType;  
    public int Point;
    public Sprite Spriteimg;

    public Card()
    {

    }

    public Card (int id,string letter,string color,string size,string texture,int point,Sprite spriteimg)
    {
        Id = id;
        LetterType = letter;
        ColorType = color;
        SizeType = size;
        TextureType = texture;
        Point = point;
        Spriteimg = spriteimg;
    }
}

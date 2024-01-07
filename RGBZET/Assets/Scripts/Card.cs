using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class Card 
{
    public int Id;
    public string LetterType;   //{R,G,B};
    public string ColorType;    //{Red,Green,Blue};
    public string SizeType ;    //{Small,Normal,Big};
    public string TextureType;  //{Thru,Thin,Thick};
    public Sprite Spriteimg;

    public Card()
    {

    }

    public Card (int id,string letter,string color,string size,string texture,Sprite spriteimg)
    {
        Id = id;
        LetterType = letter;
        ColorType = color;
        SizeType = size;
        TextureType = texture;
        Spriteimg = spriteimg;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carddata : MonoBehaviour
{
    
    public enum LetterType
    {
        R,
        G,
        B
    }

    public enum ColorType
    {
        Red,
        Green,
        Blue
    }

    public enum SizeType
    {
        Small,
        Normal,
        Big
    }

    public enum TextureType
    {
        Type1,
        Type2,
        Type3
    }

    public LetterType letter;
    public ColorType color;
    public SizeType size;
    public TextureType texture;

    


}

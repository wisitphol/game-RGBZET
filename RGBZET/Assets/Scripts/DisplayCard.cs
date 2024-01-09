using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DisplayCard : MonoBehaviour
{
    public List<Card> displayCard = new List<Card>();

    public int displayId;

    public int Id;
    public string LetterType; 
    public string ColorType; 
    public string SizeType ;    
    public string TextureType; 
    public Sprite Spriteimg;

    public Text IdText;
    public Image ArtImage;


    // Start is called before the first frame update
    void Start()
    {
       // if (displayId >= 0 && displayId < CardData.cardList.Count)
       // {
            
        displayCard[0] = CardData.cardList[displayId];

       // }
        //else
        //{
        //    Debug.LogError("displayId is out of range.");
        //}
    }

    // Update is called once per frame
    void Update()
    {
            Id = displayCard[0].Id;
            LetterType = displayCard[0].LetterType;
            ColorType = displayCard[0].ColorType;
            SizeType = displayCard[0].SizeType;
            TextureType = displayCard[0].TextureType;
            Spriteimg = displayCard[0].Spriteimg;

            IdText.text = " " + Id;
            ArtImage.sprite = Spriteimg;
    }
}

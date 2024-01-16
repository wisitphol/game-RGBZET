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

    public bool cardBack;
    public static bool staticCardBack;

    public GameObject Hand;
    public int numberOfCardInDeck;



    // Start is called before the first frame update
    void Start()
    {
        numberOfCardInDeck = Deck.deckSize;
        
        Debug.Log("CardList Count: " + CardData.cardList.Count);
        Debug.Log("DisplayId: " + displayId);

        if (displayId >= 0 && displayId < CardData.cardList.Count)
        {
            //displayCard.Add(CardData.cardList[displayId]);
            displayCard[0] = CardData.cardList[displayId];
        }
        else
        {
            Debug.LogError("displayId is out of range.");
        }
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



        Hand = GameObject.Find("Hand");
        if(this.transform.parent == Hand.transform.parent)
        {
            //cardBack = false;

        }

        staticCardBack = cardBack;

        if(this.tag == "Clone")
        {
            displayCard[0] = Deck.staticDeck[numberOfCardInDeck - 1];
            numberOfCardInDeck -= 1;
            Deck.deckSize -= 1;
            //cardBack = false;
            this.tag = "Untagged";
        }
    }
    }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class DisplayCard : MonoBehaviour
{
    public List<Card> displayCard = new List<Card>();
    public int displayId;

    public int Id;
    public string LetterType;
    public string ColorType;
    public string AmountType;
    public string FontType;
    public int Point;
    public Sprite Spriteimg;

    private Text IdText;
    public Image ArtImage;

    public bool cardBack;
    public static bool staticCardBack;

    public GameObject Board;
    public int numberOfCardInDeck;



    // Start is called before the first frame update
    void Start()
    {
        numberOfCardInDeck = Deck.deckSize;

        //Debug.Log("CardList Count: " + CardData.cardList.Count);
        //Debug.Log("DisplayId: " + displayId);

        if (displayId >= 0 && displayId < CardData.cardList.Count)
        {
            displayCard.Add(CardData.cardList[displayId]);
            //displayCard[0] = CardData.cardList[displayId];

        }
        else
        {
            Debug.LogError("displayId is out of range.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (displayCard.Count > 0)
        {
            DisplayCardData(displayCard[0]);

            if (Board == null)
            {
                Board = GameObject.Find("Boardzone");
            }

            if (this.transform.parent == Board.transform.parent)
            {
                cardBack = false;
            }

            staticCardBack = cardBack;

            if (this.tag == "Clone" && numberOfCardInDeck > 0)
            {
                displayCard[0] = Deck.staticDeck[numberOfCardInDeck - 1];
                numberOfCardInDeck -= 1;
                Deck.deckSize -= 1;
                cardBack = false;
                this.tag = "Untagged";
            }

        }
        else
        {
            Debug.LogError("displayCard is empty.");
        }
    }

    public void DisplayCardData(Card card)
    {

        Id = displayCard[0].Id;
        LetterType = displayCard[0].LetterType;
        ColorType = displayCard[0].ColorType;
        AmountType = displayCard[0].AmountType;
        FontType = displayCard[0].FontType;
        Point = displayCard[0].Point;
        Spriteimg = displayCard[0].Spriteimg;

        //IdText.text = " " + Id;
        ArtImage.sprite = Spriteimg;
    }


    public void SetBlocksRaycasts(bool blocksRaycasts)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = blocksRaycasts;
    }

}

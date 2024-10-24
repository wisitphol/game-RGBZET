using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;


public class DisplayCardT : MonoBehaviour
{
    public List<Card> displayCard = new List<Card>();
    public int displayId;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

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
        numberOfCardInDeck = DeckT.deckSize;

        //Debug.Log("CardList Count: " + CardData.cardList.Count);
        //Debug.Log("DisplayId: " + displayId);


        if (CardData.cardList == null || CardData.cardList.Count == 0)
        {
            Debug.LogError("CardData.cardList is null or empty.");
            return;
        }

        if (displayId >= 0 && displayId < CardData.cardList.Count)
        {
            displayCard.Add(CardData.cardList[displayId]);
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
                if (numberOfCardInDeck <= DeckT.staticDeck.Count && numberOfCardInDeck > 0)
                {
                    displayCard[0] = DeckT.staticDeck[numberOfCardInDeck - 1];
                    numberOfCardInDeck--;
                    DeckT.deckSize--;
                    cardBack = false;
                    this.tag = "Untagged";
                }
                else
                {
                    Debug.LogError("numberOfCardInDeck is out of range.");
                }
            }
        }
        else
        {
            Debug.LogError("displayCard is empty.");
        }
    }

    public void DisplayCardData(Card card)
    {
        if (card != null && displayCard.Count > 0)
        {
            // ตรวจสอบขนาดของ displayCard ก่อนเข้าถึงดัชนี
            if (displayCard.Count > 0)
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
            else
            {
                Debug.LogError("displayCard is empty.");
            }
        }
        else
        {
            Debug.LogError("Card is null or displayCard is empty.");
        }
    }

    public void SetBlocksRaycasts(bool blocksRaycasts)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = blocksRaycasts;
        }
        else
        {
            Debug.LogError("CanvasGroup component missing.");
        }
    }

}
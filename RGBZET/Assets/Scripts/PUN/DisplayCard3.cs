using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class DisplayCard3 : MonoBehaviour
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
        numberOfCardInDeck = DeckFire.deckSize;

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
                displayCard[0] = DeckFire.staticDeck[numberOfCardInDeck - 1];
                numberOfCardInDeck -= 1;
                DeckFire.deckSize -= 1;
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

    public void HighlightCard(bool highlight)
    {
        if (highlight)
        {
            // เปลี่ยนขอบหรือสีของการ์ดเพื่อบ่งบอกว่ามันถูกเลือก
            // นี่เป็นเพียงตัวอย่าง: คุณจะต้องอ้างอิงถึงองค์ประกอบ UI จริง
            this.GetComponent<Image>().color = Color.yellow; // สีเน้น
        }
        else
        {
            // คืนค่าการเปลี่ยนแปลงทางภาพถ้าการ์ดไม่ถูกเลือกอีกต่อไป
            this.GetComponent<Image>().color = Color.white; // สีปกติ
        }
    }

    public void StoreOriginalPositionAndRotation()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;

    }

    // เมื่อต้องการคืนการ์ดกลับไปที่ตำแหน่งเดิม
    public void ReturnToOriginalPosition()
    {
        originalParent = transform.parent;
        transform.position = originalPosition;
        transform.rotation = originalRotation;

    }

    public void SetBlocksRaycasts(bool blocksRaycasts)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = blocksRaycasts;
    }

}

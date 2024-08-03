using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck2 : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public List<Card> deck = new List<Card>();
    public int x;
    public static int deckSize;
    public static List<Card> staticDeck = new List<Card>();

    public GameObject CardInDeck;

    public GameObject CardPrefab;
    public GameObject[] Clones;
    public GameObject Board;
    private BoardCheck2 boardCheckScript;



    // Start is called before the first frame update
    void Start()
    {
        // Initialize deckSize and cardList if not already done
        deck = new List<Card>(CardData.cardList);
        deckSize = deck.Count;
        Shuffle(deck);

        // Now deck is shuffled and ready to use
        StartCoroutine(StartGame());

        boardCheckScript = FindObjectOfType<BoardCheck2>();

    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if (deckSize <= 0)
        {
            CardInDeck.SetActive(false);
            // boardCheckScript.CheckBoardEnd();
            
        }
        else
        {
            CardInDeck.SetActive(true); // เพิ่มบรรทัดนี้เพื่อให้แน่ใจว่า CardInDeck ถูกเปิดใช้งานเมื่อ deckSize มากกว่า 0
           
        }

    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(0.5f);
            GameObject newCard = Instantiate(CardPrefab, transform.position, transform.rotation); //as GameObject;
            newCard.transform.SetParent(Board.transform, false);
            newCard.SetActive(true);
        }
    }


    private void Shuffle(List<Card> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Card temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public IEnumerator Draw(int x)
    {
        for (int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(1);

            if (deckSize > 0)
            {
                // สร้างการ์ดและเพิ่มลงบอร์ด
                GameObject newCard = Instantiate(CardPrefab, transform.position, transform.rotation);
                newCard.transform.SetParent(Board.transform, false);
                newCard.SetActive(true);


                Debug.Log("Number of cards in deck: " + RemainingCardsCount());
            }
            else
            {
                //Debug.Log("No more cards in the deck. Cannot draw.");
                break; // หยุดการจั่วการ์ดเมื่อสำรับการ์ดใน deck หมดลงแล้ว
            }

        }

        if (deckSize <= 0)
        {
            boardCheckScript.CheckBoardEnd();
        }

    }

    public void DrawCards(int numberOfCards)
    {
        StartCoroutine(Draw(numberOfCards));
    }

    public int RemainingCardsCount()
    {
        return deckSize;
    }


}

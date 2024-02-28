using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public List<Card> deck = new List<Card>();
    public int x;
    public static int deckSize;
    public static List<Card> staticDeck = new List<Card>();

    public GameObject CardInDeck;

    public GameObject CardToBoard;
    public GameObject[] Clones;
    public GameObject Board;

   
   
    // Start is called before the first frame update
    void Start()
    {
        // Initialize deckSize and cardList if not already done
        deckSize = CardData.cardList.Count;
        deck = new List<Card>(CardData.cardList);
        Shuffle(deck);

        // Now deck is shuffled and ready to use
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if(deckSize < 1)
        {
            CardInDeck.SetActive(false);
        }

       
       
    }

    IEnumerator StartGame()
    {
        for(int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(1);
            GameObject newCard = Instantiate(CardToBoard, transform.position, transform.rotation) as GameObject;
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
        for(int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(1);

            if (deckSize > 0)
            {
                // สร้างการ์ดและเพิ่มลงบอร์ด
                GameObject newCard = Instantiate(CardToBoard, transform.position, transform.rotation);
                newCard.transform.SetParent(Board.transform, false);
                newCard.SetActive(true);

                // ลดขนาดของสำรับการ์ดลงตามจำนวนการ์ดที่ถูกจั่ว
                deckSize--;

                //เรียกใช้ Shuffle หากสำรับการ์ดใน deck ใช้หมด
            /*    if (deckSize <= 0)
                {
                    Shuffle(deck);
                    deckSize = deck.Count;
                }  */
            }
            else
            {
                Debug.Log("No more cards in the deck. Cannot draw.");
                break; // หยุดการจั่วการ์ดเมื่อสำรับการ์ดใน deck หมดลงแล้ว
            } 
        }
    }

    public void DrawCards(int numberOfCards)
    {
        StartCoroutine(Draw(numberOfCards));
    }
    
}

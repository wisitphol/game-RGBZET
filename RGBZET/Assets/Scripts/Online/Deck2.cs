using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Deck2 : MonoBehaviourPunCallbacks
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

        if (Board == null)
        {
            Board = GameObject.Find("Boardzone"); // หาบอร์ดโซน
        }

        if (PhotonNetwork.IsMasterClient)
        {
            InitializeDeck();
            // Convert deck to card IDs
            int[] deckCardIds = new int[deck.Count];
            for (int i = 0; i < deck.Count; i++)
            {
                deckCardIds[i] = deck[i].Id;  // Assuming Id is an integer, if it's a string, use a different approach
            }

            // Sync deck with other players
            photonView.RPC("ReceiveDeck", RpcTarget.OthersBuffered, deckCardIds);

            // Vector3[] cardPositions = GetCardPositions(); // Method to get positions
         //   photonView.RPC("ReceiveBoard", RpcTarget.OthersBuffered, deckCardIds, cardPositions);

          

            // Start the game
            StartCoroutine(StartGame());



        }
        else
        {
            // Wait for the master client to sync the deck
            //StartCoroutine(WaitForDeckSync());
            boardCheckScript.FindCard();
        }
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
            GameObject newCard = PhotonNetwork.Instantiate(CardPrefab.name, transform.position, transform.rotation);
            newCard.transform.SetParent(Board.transform, false);
            newCard.SetActive(true);

        }

    }

    private void InitializeDeck()
    {
        // Initialize deckSize and cardList if not already done
        deck = new List<Card>(CardData.cardList);
        deckSize = deck.Count;
        Shuffle(deck);
        Debug.Log("Deck initialized with " + deckSize + " cards.");
    }

    IEnumerator WaitForDeckSync()
    {
        while (deck.Count == 0)
        {
            yield return null;
        }

        // Deck has been synced, start the game
        StartCoroutine(StartGame());
    }

    [PunRPC]
    private void ReceiveDeck(int[] cardIds)
    {
        deck.Clear();
        foreach (int cardId in cardIds)
        {
            Card card = CardData.cardList.Find(c => c.Id == cardId);
            if (card != null)
            {
                deck.Add(card);
                //Debug.Log("Added card to deck with ID: " + card.Id);
            }
            else
            {
                //Debug.LogWarning("Card not found with ID: " + cardId);
            }
        }
        deckSize = deck.Count;
        Debug.Log("Deck synchronized with " + deckSize + " cards.");
    }

  

    [PunRPC]
    private void ReceiveBoard(int[] cardIds, Vector3[] positions, Quaternion[] rotations, string[] letterTypes, string[] colorTypes, string[] amountTypes, string[] fontTypes, int[] points, string[] spriteNames)
    {
        GameObject boardzone = GameObject.Find("Boardzone");

        if (boardzone == null)
        {
            Debug.LogError("Boardzone not found.");
            return;
        }

        for (int i = 0; i < cardIds.Length; i++)
        {
            // หาตำแหน่งและการหมุนของการ์ดจากข้อมูลที่ได้รับ
            Vector3 position = positions[i];
            Quaternion rotation = rotations[i];

            // สร้างการ์ดใหม่หรือหา gameObject ของการ์ดที่มีอยู่แล้ว
            GameObject cardObject = Instantiate(CardPrefab, position, rotation);
            cardObject.transform.SetParent(boardzone.transform, false);
            cardObject.transform.localPosition = position;
            cardObject.transform.localRotation = rotation;
            cardObject.SetActive(true);

            // กำหนดค่า DisplayCard3 หรือ DisplayCard2
            DisplayCard2 displayCard2 = cardObject.GetComponent<DisplayCard2>();
            if (displayCard2 != null)
            {
                displayCard2.displayId = cardIds[i];
                displayCard2.LetterType = letterTypes[i];
                displayCard2.ColorType = colorTypes[i];
                displayCard2.AmountType = amountTypes[i];
                displayCard2.FontType = fontTypes[i];
                displayCard2.Point = points[i];
                displayCard2.Spriteimg = Resources.Load<Sprite>(spriteNames[i]);
                displayCard2.ArtImage.sprite = displayCard2.Spriteimg;
            }
            else
            {
                Debug.LogWarning("DisplayCard2 component missing on cardObject.");
            }
        }
    }




    private Vector3[] GetCardPositions()
    {
        // Method to get positions of cards in the boardzone
        Vector3[] positions = new Vector3[deck.Count];
        for (int i = 0; i < deck.Count; i++)
        {
            // Assume cards are placed in a grid or specific positions
            // This is just an example and should be replaced with actual logic
            positions[i] = new Vector3(i * 2.0f, 0, 0); // Adjust based on your actual board layout
        }
        return positions;
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
                GameObject newCard = PhotonNetwork.Instantiate(CardPrefab.name, transform.position, transform.rotation);
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

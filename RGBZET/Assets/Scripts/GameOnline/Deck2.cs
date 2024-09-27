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
    [SerializeField] public AudioSource audioSource;  // ตัวแปร AudioSource ที่จะเล่นเสียง
    [SerializeField] public AudioClip drawCardSound;  // เสียงที่ต้องการเล่นตอนจั่วการ์ด



    // Start is called before the first frame update
    void Start()
    {
        boardCheckScript = FindObjectOfType<BoardCheck2>();



        if (Board == null)
        {
            Board = GameObject.Find("Boardzone"); // หาบอร์ดโซน
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("This player is the MasterClient.");

            InitializeDeck();
            // Convert deck to card IDs
            int[] deckCardIds = new int[deck.Count];
            for (int i = 0; i < deck.Count; i++)
            {
                deckCardIds[i] = deck[i].Id;  // Assuming Id is an integer, if it's a string, use a different approach
            }
            // Sync deck with other players
            photonView.RPC("ReceiveDeck", RpcTarget.Others, deckCardIds);

            RPC_StartGame();

        }
        else
        {
            Debug.Log("This player is NOT the MasterClient.");
        }

        if (Board.transform.childCount == 13)
        {
            boardCheckScript.CheckBoard();
        }
    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if (deckSize <= 0)
        {
            CardInDeck.SetActive(false);
        }
        else
        {
            CardInDeck.SetActive(true); // เพิ่มบรรทัดนี้เพื่อให้แน่ใจว่า CardInDeck ถูกเปิดใช้งานเมื่อ deckSize มากกว่า 0
        }

    }

    [PunRPC]
    public void RPC_StartGame()
    {
        Debug.Log("Deck2: RPC_StartGame called.");
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {

        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(0.6f);

            if (deckSize > 0)
            {
                GameObject newCard = PhotonNetwork.Instantiate(CardPrefab.name, transform.position, transform.rotation);
                int viewID = newCard.GetComponent<PhotonView>().ViewID;
                // เรียกใช้ RPC เพื่อให้ผู้เล่นอื่นๆ จัดการกับการ์ดที่ถูกสร้างใหม่
                photonView.RPC("RPC_SetCardParent", RpcTarget.AllBuffered, viewID);
                // Debug.Log("Deck2: Card created and RPC_SetCardParent called with viewID: " + viewID);

                // เล่นเสียงตอนจั่วการ์ด
                if (audioSource != null && drawCardSound != null)
                {
                    audioSource.PlayOneShot(drawCardSound);
                }

            }
            else
            {
                Debug.LogWarning("Deck is empty, no more cards to draw.");
                break; // หยุดการจั่วการ์ดเมื่อสำรับการ์ดหมด
            }

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
    private void RPC_SetCardParent(int viewID)
    {
        GameObject card = PhotonView.Find(viewID)?.gameObject;
        if (card != null)
        {
            card.transform.SetParent(Board.transform, false);
            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.identity;
            card.SetActive(true);
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
        Debug.Log("Deck2: Draw coroutine started with " + x + " cards to draw.");
        for (int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(0.9f);

            if (deckSize > 0)
            {
                // สร้างการ์ดและเพิ่มลงบอร์ด
                GameObject newCard = PhotonNetwork.Instantiate(CardPrefab.name, transform.position, transform.rotation);
                newCard.transform.SetParent(Board.transform, false);
                newCard.transform.localPosition = new Vector3(0, 0, 0); // ตั้งค่า localPosition ให้ถูกต้อง
                newCard.transform.localRotation = Quaternion.identity; // ตั้งค่า localRotation ให้ถูกต้อง
                newCard.SetActive(true);

                int viewID = newCard.GetComponent<PhotonView>().ViewID;
                photonView.RPC("RPC_SetCardParent", RpcTarget.AllBuffered, viewID);

                 // เล่นเสียงตอนจั่วการ์ด
                if (audioSource != null && drawCardSound != null)
                {
                    audioSource.PlayOneShot(drawCardSound);
                }

                //  Debug.Log("Number of cards in deck: " + RemainingCardsCount());
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

    [PunRPC]
    public void DrawCards(int numberOfCards)
    {
        // Debug.Log("Deck2: DrawCards called with " + numberOfCards + " cards to draw.");
        StartCoroutine(Draw(numberOfCards));
    }

    [PunRPC]
    public void RequestDrawCardsFromMaster(int numberOfCards)
    {
        // Debug.Log(" RequestDrawCardsFromMaster called");
        if (PhotonNetwork.IsMasterClient)
        {
            DrawCards(numberOfCards); // เรียกฟังก์ชันการจั่วการ์ด
        }
    }

    [PunRPC]
    void RemoveCardFromDeck(int cardId)
    {
        Card cardToRemove = deck.Find(c => c.Id == cardId);
        if (cardToRemove != null)
        {
            deck.Remove(cardToRemove);
            deckSize = deck.Count; // อัพเดท deckSize หลังจากลบการ์ด
        }
    }

    public int RemainingCardsCount()
    {
        return deckSize;
    }

}

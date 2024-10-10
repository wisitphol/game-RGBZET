using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DeckQ : MonoBehaviourPunCallbacks
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
    private BoardCheckQ boardCheckScript;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip drawCardSound;

    void Start()
    {
        boardCheckScript = FindObjectOfType<BoardCheckQ>();

        if (Board == null)
        {
            Board = GameObject.Find("Boardzone");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            InitializeDeck();
            int[] deckCardIds = new int[deck.Count];
            for (int i = 0; i < deck.Count; i++)
            {
                deckCardIds[i] = deck[i].Id;
            }
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

    void Update()
    {
        staticDeck = deck;

        if (deckSize <= 0)
        {
            CardInDeck.SetActive(false);
        }
        else
        {
            CardInDeck.SetActive(true);
        }
    }

    [PunRPC]
    public void RPC_StartGame()
    {
        Debug.Log("DeckQ: RPC_StartGame called.");
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
                photonView.RPC("RPC_SetCardParent", RpcTarget.AllBuffered, viewID);

                if (audioSource != null && drawCardSound != null)
                {
                    audioSource.PlayOneShot(drawCardSound);
                }
            }
            else
            {
                Debug.LogWarning("Deck is empty, no more cards to draw.");
                break;
            }
        }
    }

    private void InitializeDeck()
    {
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
            }
            else
            {
                Debug.LogWarning("Card not found with ID: " + cardId);
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
        Debug.Log("DeckQ: Draw coroutine started with " + x + " cards to draw.");
        for (int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(0.9f);

            if (deckSize > 0)
            {
                GameObject newCard = PhotonNetwork.Instantiate(CardPrefab.name, transform.position, transform.rotation);
                newCard.transform.SetParent(Board.transform, false);
                newCard.transform.localPosition = new Vector3(0, 0, 0);
                newCard.transform.localRotation = Quaternion.identity;
                newCard.SetActive(true);

                int viewID = newCard.GetComponent<PhotonView>().ViewID;
                photonView.RPC("RPC_SetCardParent", RpcTarget.AllBuffered, viewID);

                if (audioSource != null && drawCardSound != null)
                {
                    audioSource.PlayOneShot(drawCardSound);
                }
            }
            else
            {
                Debug.Log("No more cards in the deck. Cannot draw.");
                break;
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
        StartCoroutine(Draw(numberOfCards));
    }

    [PunRPC]
    public void RequestDrawCardsFromMaster(int numberOfCards)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            DrawCards(numberOfCards);
        }
    }

    [PunRPC]
    void RemoveCardFromDeck(int cardId)
    {
        Card cardToRemove = deck.Find(c => c.Id == cardId);
        if (cardToRemove != null)
        {
            deck.Remove(cardToRemove);
            deckSize = deck.Count;
        }
    }

    public int RemainingCardsCount()
    {
        return deckSize;
    }
}
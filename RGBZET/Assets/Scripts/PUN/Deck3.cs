using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Deck3 : MonoBehaviourPunCallbacks
{
    public List<Card> deck = new List<Card>();
    public static int deckSize;
    public static List<Card> staticDeck = new List<Card>();
    public GameObject CardInDeck;
    public GameObject CardPrefab;
    public GameObject Board;
    private BoardCheck3 boardCheckScript;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            // Check if already in a room
            if (PhotonNetwork.InRoom)
            {
                InitializeDeck();
            }
            else
            {
                // Wait until the client joins or creates a room
                PhotonNetwork.AddCallbackTarget(this);
            }
        }
        else
        {
            InitializeDeckLocally();
        }
    }

    [PunRPC]
    void InitializeDeck()
    {
        deck = new List<Card>(CardData.cardList);
        deckSize = deck.Count;
        Shuffle(deck);

        StartCoroutine(StartGame());
        boardCheckScript = FindObjectOfType<BoardCheck3>();
    }

    void InitializeDeckLocally()
    {
        deck = new List<Card>(CardData.cardList);
        deckSize = deck.Count;
        Shuffle(deck);

        StartCoroutine(StartGame());
        boardCheckScript = FindObjectOfType<BoardCheck3>();
    }

    void Update()
    {
        if (deckSize <= 0)
        {
            // Disable the card object if no more cards in the deck
            CardInDeck.SetActive(false);
        }
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < 12; i++)
        {
            Debug.Log("CardPrefab: " + CardPrefab);
            yield return new WaitForSeconds(0.5f);
            GameObject newCard = PhotonNetwork.Instantiate(CardPrefab.name, transform.position, transform.rotation);
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

    public int RemainingCardsCount()
    {
        return deckSize;
    }

    public override void OnJoinedRoom()
    {
        InitializeDeck();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnCreatedRoom()
    {
        InitializeDeck();
        PhotonNetwork.RemoveCallbackTarget(this);
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

    
}

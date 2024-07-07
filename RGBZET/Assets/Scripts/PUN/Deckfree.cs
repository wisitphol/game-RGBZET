using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Deckfree : MonoBehaviourPunCallbacks
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
    private BoardCheck3 boardCheckScript;


    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        boardCheckScript = FindObjectOfType<BoardCheck3>();

    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if (deckSize <= 0)
        {
            CardInDeck.SetActive(false);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        StartCoroutine(JoinLobbyWhenReady());
    }

    private IEnumerator JoinLobbyWhenReady()
    {
        while (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("RoomName", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        base.OnJoinedRoom();

        if (PhotonNetwork.IsMasterClient)
        {
            
            // Initialize deckSize and cardList if not already done
            deck = new List<Card>(CardData.cardList);
            deckSize = deck.Count;
            Shuffle(deck);

            // Now deck is shuffled and ready to use
            StartCoroutine(StartGame());

            boardCheckScript = FindObjectOfType<BoardCheck3>();
        }
        else
        {
            // ผู้เล่นคนอื่นไม่ต้องทำอะไรเพิ่มเติม
            photonView.RPC("RequestInitialDeckData", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New Player Joined: " + newPlayer.NickName);
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(0.5f);
            GameObject newCard = PhotonNetwork.Instantiate("CardBoardOn1", transform.position, transform.rotation); 
            newCard.transform.SetParent(Board.transform, false);
            newCard.SetActive(true);
            Card cardDat = deck[i];
            photonView.RPC("SendCardDataToOthers", RpcTarget.Others, 
            cardDat.Id, cardDat.LetterType, cardDat.ColorType, cardDat.AmountType, cardDat.FontType, cardDat.Point);
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
                GameObject newCard = PhotonNetwork.Instantiate("CardBoardOn1", transform.position, transform.rotation);
                newCard.transform.SetParent(Board.transform, false);
                newCard.SetActive(true);
                Card cardDat = deck[deckSize - 1];
                photonView.RPC("SendCardDataToOthers", RpcTarget.Others, 
                cardDat.Id, cardDat.LetterType, cardDat.ColorType, cardDat.AmountType, cardDat.FontType, cardDat.Point);

                Debug.Log("Number of cards in deck: " + RemainingCardsCount()); 
            }
            else
            {
                break;
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

    [PunRPC]
    private void RequestInitialDeckData(Player requestingPlayer)
    {
        for (int i = 0; i < 12; i++)
        {
            Card cardDat = deck[i];
            photonView.RPC("SendCardDataToPlayer", requestingPlayer,
             cardDat.Id, cardDat.LetterType, cardDat.ColorType, cardDat.AmountType, cardDat.FontType, cardDat.Point);
        }

        // ส่งข้อมูล deck ทั้งหมดไปยังผู้เล่นคนใหม่
        photonView.RPC("ReceiveDeckData", requestingPlayer, deckSize, deck);
    }

    [PunRPC]
    private void SendCardDataToPlayer(int id, string letterType, string colorType, string amountType, string fontType, int point)
    {
        InstantiateCardOnBoard(id, letterType, colorType, amountType, fontType, point);
    }

    [PunRPC]
    private void SendCardDataToOthers(int id, string letterType, string colorType, string amountType, string fontType, int point)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            InstantiateCardOnBoard(id, letterType, colorType, amountType, fontType, point);
        }
    }

    [PunRPC]
    private void ReceiveDeckData(int receivedDeckSize, List<Card> receivedDeck)
    {
        deckSize = receivedDeckSize;
        deck = receivedDeck;

        for (int i = 0; i < 12; i++)
        {
            Card cardDat = deck[i];
            InstantiateCardOnBoard(cardDat.Id, cardDat.LetterType, cardDat.ColorType, cardDat.AmountType, cardDat.FontType, cardDat.Point);
        }
    }

     private void InstantiateCardOnBoard(int id, string letterType, string colorType, string amountType, string fontType, int point)
    {
        GameObject newCard = PhotonNetwork.Instantiate("CardBoardOn1", transform.position, transform.rotation);
        newCard.transform.SetParent(Board.transform, false);
        newCard.SetActive(true);

        // Set the card data here, for example:
        Card cardDat = new Card();
        cardDat.Id = id;
        cardDat.LetterType = letterType;
        cardDat.ColorType = colorType;
        cardDat.AmountType = amountType;
        cardDat.FontType = fontType;
        cardDat.Point = point;

        newCard.GetComponent<CardDisplay>().SetCard(cardDat); 
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DeckFire : MonoBehaviourPunCallbacks
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
    private List<GameObject> cardList = new List<GameObject>();
    private PhotonView localPhotonView;

    void Awake()
    {
        localPhotonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        PhotonNetwork.PrefabPool = new CustomPrefabPool();
        PhotonNetwork.ConnectUsingSettings();

        boardCheckScript = FindObjectOfType<BoardCheck3>();
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
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinOrCreateRoom("RoomName", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        base.OnJoinedRoom();

        if (PhotonNetwork.IsMasterClient)
        {
            deck = new List<Card>(CardData.cardList); // ดึงข้อมูลจาก CardData
            deckSize = deck.Count;
            Shuffle(deck);
            photonView.RPC("StartGameRPC", RpcTarget.AllBuffered);
            //SyncDeckWithAllClients();
        }
        else
        {
            // ซิงค์เด็คการ์ดจาก MasterClient
            SyncDeckWithAllClients();
        }
    }

    void Update()
    {
        staticDeck = deck;

        if (deckSize <= 0)
        {
            CardInDeck.SetActive(false);
        }
    }

    [PunRPC]
    private void StartGameRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartGame());
        }
    }

    IEnumerator StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 12; i++)
            {
                yield return new WaitForSeconds(0.5f);
                CreateCard(i);
            }
            SyncDeckWithAllClients();
        }
        else
        {
            // ซิงค์เด็คการ์ดจาก MasterClient

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
                CreateCard(deckSize - 1);
                SyncDeckSizeWithAllClients();
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

    private void CreateCard(int cardIndex)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot Instantiate before the client joined/created a room.");
            return;
        }

        GameObject newCard = PhotonNetwork.Instantiate("CardBoardOn1", transform.position, transform.rotation, 0);
        if (newCard != null)
        {
            newCard.transform.SetParent(Board.transform, false);  // Set the parent to Board with worldPositionStays set to false
            newCard.transform.localPosition = Vector3.zero;      // Reset the local position of the card
            newCard.SetActive(true);
            cardList.Add(newCard);

            Card card = deck[cardIndex];
            PhotonView cardPhotonView = newCard.GetComponent<PhotonView>();
            if (cardPhotonView != null)
            {
                photonView.RPC("SyncCardState", RpcTarget.AllBuffered, cardPhotonView.ViewID, card.Id, card.LetterType, card.ColorType, card.AmountType, card.FontType, card.Point, card.Spriteimg.name);
            }
            else
            {
                Debug.LogError("PhotonView component not found on the instantiated card.");
            }
        }
    }

    [PunRPC]
    public void SyncCardState(int viewID, int cardId, string letterType, string colorType, string amountType, string fontType, int point, string spriteName)
    {
        GameObject cardObj = PhotonView.Find(viewID)?.gameObject;
        if (cardObj != null)
        {
            Card card = new Card(cardId, letterType, colorType, amountType, fontType, point, Resources.Load<Sprite>(spriteName));
            DisplayCard3 displayCard = cardObj.GetComponent<DisplayCard3>();
            if (displayCard != null)
            {
                displayCard.Initialize(card);
            }

            cardObj.transform.SetParent(Board.transform, false);  // Set the parent to Board with worldPositionStays set to false
            cardObj.transform.localPosition = Vector3.zero;      // Reset the local position of the card
            cardObj.SetActive(true);
            cardList.Add(cardObj);
        }
    }


    public void DrawCards(int numberOfCards)
    {
        photonView.RPC("DrawMultipleCards", RpcTarget.All, numberOfCards);
    }

    [PunRPC]
    private void DrawMultipleCards(int numberOfCards)
    {
        StartCoroutine(Draw(numberOfCards));
    }

    private void SyncDeckWithAllClients()
    {
        List<int> cardIds = new List<int>();
        foreach (Card card in deck)
        {
            cardIds.Add(card.Id);
        }
        photonView.RPC("ReceiveDeck", RpcTarget.All, cardIds.ToArray());
    }

    [PunRPC]
    private void ReceiveDeck(int[] cardIds)
    {
        deck.Clear();
        foreach (int id in cardIds)
        {
            Card card = CardData.cardList.Find(c => c.Id == id);
            if (card != null)
            {
                deck.Add(card);
                //Debug.Log("Added card to deck with ID: " + id);
            }
        }
        deckSize = deck.Count;
        Debug.Log("Deck synchronized with " + deckSize + " cards.");
    }

    public void SyncDeckSizeWithAllClients()
    {
        photonView.RPC("SyncDeckSize", RpcTarget.AllBuffered, deckSize);
    }

    [PunRPC]
    public void SyncDeckSize(int newSize)
    {
        deckSize = newSize;
    }

    private int RemainingCardsCount()
    {
        return deckSize;
    }

    
   
}

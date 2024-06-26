using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class DeckFire3 : MonoBehaviourPunCallbacks
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
    private PhotonView localphotonView;

    DatabaseReference reference;

    void Awake()
    {
        localphotonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        PhotonNetwork.PrefabPool = new CustomPrefabPool(); // กำหนดค่า CustomPrefabPool
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
            yield return null; // Wait until the client is connected and ready
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
            deck = new List<Card>(CardData.cardList);
            deckSize = deck.Count;
            Shuffle(deck);
            StartCoroutine(StartGame());
        }
        else
        {
            SyncCardsWithMasterClient();
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

    IEnumerator StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 12; i++)
            {
                yield return new WaitForSeconds(0.5f);
                CreateCard();
            }
        }
        else
        {
            SyncCardsWithMasterClient();
        }
    }

    private void SyncCardsWithMasterClient()
    {
        if (localphotonView != null)
        {
            localphotonView.RPC("RequestCardSync", RpcTarget.MasterClient);
            Debug.Log("Requesting card sync from Master Client.");
        }
        else
        {
            Debug.LogError("photonView is null in SyncCardsWithMasterClient");
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
                CreateCard();
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

    private void CreateCard()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot Instantiate before the client joined/created a room.");
            return;
        }

        GameObject newCard = PhotonNetwork.Instantiate("CardBoardOn1", transform.position, transform.rotation, 0);
        if (newCard != null)
        {
            newCard.transform.SetParent(Board.transform, false);
            newCard.SetActive(true);
            cardList.Add(newCard);

            PhotonView cardPhotonView = newCard.GetComponent<PhotonView>();
            if (cardPhotonView != null)
            {
                //Debug.Log("Creating card with ViewID: " + cardPhotonView.ViewID);
                photonView.RPC("SyncCardState", RpcTarget.AllBuffered, cardPhotonView.ViewID, newCard.transform.position, newCard.transform.rotation);
            }
            else
            {
                Debug.LogError("PhotonView component not found on the instantiated card.");
            }
        }
    }


    [PunRPC]
    public void SyncCardState(int viewID, Vector3 position, Quaternion rotation)
    {
        GameObject card = PhotonView.Find(viewID).gameObject;
        if (card != null)
        {
            card.transform.position = position;
            card.transform.rotation = rotation;
        }
    }


   


    [PunRPC]
    private void RequestCardSync()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client processing card sync request.");
            foreach (GameObject card in cardList)
            {
                PhotonView cardPhotonView = card.GetComponent<PhotonView>();
                if (cardPhotonView != null)
                {
                    photonView.RPC("ReceiveCard", RpcTarget.OthersBuffered, cardPhotonView.ViewID, card.transform.position, card.transform.rotation);
                    Debug.Log("Syncing card with ViewID: " + cardPhotonView.ViewID);
                }
            }

            // Send deck information to other players
            List<int> cardIds = new List<int>();
            foreach (Card card in deck)
            {
                cardIds.Add(card.Id); // Assuming each card has a unique Id
            }
            photonView.RPC("ReceiveDeck", RpcTarget.OthersBuffered, cardIds.ToArray());
            Debug.Log("Syncing deck with " + cardIds.Count + " cards.");
        }
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
                Debug.Log("Added card to deck with ID: " + id);
            }
        }
        deckSize = deck.Count;
        Debug.Log("Deck synchronized with " + deckSize + " cards.");
    }

    [PunRPC]
    private void ReceiveCard(int viewID, Vector3 position, Quaternion rotation)
    {
        GameObject card = PhotonView.Find(viewID)?.gameObject;
        if (card != null)
        {
            cardList.Add(card);
            card.transform.SetParent(Board.transform, false);
            card.transform.position = position;
            card.transform.rotation = rotation;
            card.SetActive(true);
            Debug.Log("Received card with ViewID: " + viewID);
        }
        else
        {
            Debug.LogError("Could not find card with ViewID: " + viewID);
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
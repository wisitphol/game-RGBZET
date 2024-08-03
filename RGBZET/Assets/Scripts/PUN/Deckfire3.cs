using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class DeckFire : MonoBehaviourPunCallbacks
{
    public List<Card> container = new List<Card>();
    public List<Card> deck = new List<Card>();
    public int y;
    public static int deckSize;
    public static List<Card> staticDeck = new List<Card>();

    public GameObject CardInDeck;

    public GameObject CardPrefab;
    public GameObject[] Clones;
    public GameObject Board;
    private BoardCheck3 boardCheckScript;
    private List<GameObject> cardList = new List<GameObject>();
    private PhotonView localphotonView;
    public List<Card> boardcard = new List<Card>();

    DatabaseReference reference;

    void Awake()
    {
        localphotonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        //PhotonNetwork.PrefabPool = new CustomPrefabPool(); // กำหนดค่า CustomPrefabPool
        PhotonNetwork.ConnectUsingSettings();

        boardCheckScript = FindObjectOfType<BoardCheck3>();

         deck = new List<Card>(CardData.cardList);
            deckSize = deck.Count;
            Shuffle(deck);
            StartCoroutine(StartGame());

            SyncCardsWithMasterClient();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        //StartCoroutine(JoinLobbyWhenReady());
    }
    /*
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
        //Debug.Log("Joined Lobby");
        PhotonNetwork.JoinOrCreateRoom("RoomName", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }
    */
    public override void OnJoinedRoom()
    {
        //Debug.Log("Joined Room");
        base.OnJoinedRoom();

        
       
        if (PhotonNetwork.IsMasterClient)
        {
            
           // deck = new List<Card>(CardData.cardList);
          //  deckSize = deck.Count;
          //  Shuffle(deck);
          //  StartCoroutine(StartGame());
            
        }
        else
        {
            //SyncCardsWithMasterClient();
            

        }

    }


    void Update()
    {
        
       // Debug.Log("deckSize: " + deckSize);
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

    IEnumerator StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 12; i++)
            {
                yield return new WaitForSeconds(0.5f);
                CreateCard();
            }
          /*   foreach (GameObject card in cardList)
             {
                 PhotonView cardPhotonView = card.GetComponent<PhotonView>();
                 DisplayCard3 cardComponent = card.GetComponent<DisplayCard3>();
                 if (cardPhotonView != null && cardComponent != null)
                 {
                     photonView.RPC("SyncCardState", RpcTarget.OthersBuffered,
                         cardPhotonView.ViewID, card.transform.position, card.transform.rotation,
                         cardComponent.LetterType, cardComponent.ColorType, cardComponent.AmountType, cardComponent.FontType,
                         cardComponent.Point, cardComponent.Spriteimg.name);
                 }
             }*/
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

    public void CallCreateCard()
    {
        photonView.RPC("CreateCard", RpcTarget.MasterClient);
    }


    private void CreateCard()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot Instantiate before the client joined/created a room.");
            return;
        }

        if (Board == null)
        {
            Debug.LogError("Board is not assigned.");
            return;
        }


        GameObject newCard = PhotonNetwork.Instantiate("CardBoardOn1", transform.position, transform.rotation, 0);
        if (newCard != null)
        {
            newCard.transform.SetParent(Board.transform, false);
            newCard.transform.position = new Vector3(0, 0, 0); // ตำแหน่งที่ต้องการใน Board
            newCard.transform.rotation = Quaternion.identity;
            newCard.SetActive(true);
            cardList.Add(newCard);

            PhotonView cardPhotonView = newCard.GetComponent<PhotonView>();
            DisplayCard3 cardComponent = newCard.GetComponent<DisplayCard3>();

           

        }
        else
        {
            Debug.LogError("PhotonNetwork.Instantiate failed to create a new card.");
        }
    }



    [PunRPC]
    private void SyncCardState(int viewID, Vector3 position, Quaternion rotation, string letterType, string colorType, string amountType, string fontType, int point, string spriteName)
    {
        GameObject card = PhotonView.Find(viewID)?.gameObject;
        if (card != null)
        {
            card.transform.SetParent(Board.transform, false);
            card.transform.position = position;
            card.transform.rotation = rotation;
            card.SetActive(true);

            DisplayCard3 cardComponent = card.GetComponent<DisplayCard3>();
            if (cardComponent != null)
            {
                cardComponent.LetterType = letterType;
                cardComponent.ColorType = colorType;
                cardComponent.AmountType = amountType;
                cardComponent.FontType = fontType;
                cardComponent.Point = point;
                cardComponent.Spriteimg = Resources.Load<Sprite>(spriteName); // Ensure the sprite is loaded from resources
                cardComponent.ArtImage.sprite = cardComponent.Spriteimg; // Update the Image component with the sprite

                Debug.Log("Card state synchronized with ViewID: " + viewID);
            }
            else
            {
                Debug.LogError("DisplayCard3 component not found on the card.");
            }
        }
        else
        {
            Debug.LogError("Could not find card with ViewID: " + viewID);
        }
    }



    [PunRPC]
    private void RequestCardSync()
    {
        if (PhotonNetwork.IsMasterClient)
        {
         /*  Debug.Log("Master Client processing card sync request.");
            foreach (GameObject card in cardList)
            {
                PhotonView cardPhotonView = card.GetComponent<PhotonView>();
                DisplayCard3 cardComponent = card.GetComponent<DisplayCard3>();
                if (cardPhotonView != null && cardComponent != null)
                {
                    photonView.RPC("ReceiveCard", RpcTarget.OthersBuffered, cardPhotonView.ViewID, cardComponent.Id, card.transform.position, card.transform.rotation);
                    Debug.Log("Syncing card with ViewID: " + cardPhotonView.ViewID + " and CardID: " + cardComponent.Id);
                }
            }
            
            // ส่งข้อมูลเด็คไปให้ผู้เล่นคนอื่น
            List<int> cardIds = new List<int>();
            foreach (Card card in deck)
            {
                cardIds.Add(card.Id);
            }
            photonView.RPC("ReceiveDeck", RpcTarget.OthersBuffered, cardIds.ToArray());
            Debug.Log("Syncing deck with " + cardIds.Count + " cards.");*/

            CallUpdateCard();


        }
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
    private void ReceiveCard(int viewID, int cardID, Vector3 position, Quaternion rotation)
    {
        Debug.Log("ReceiveCard called");
        GameObject card = PhotonView.Find(viewID)?.gameObject;
        if (card != null)
        {
            if (!cardList.Contains(card))
            {
                cardList.Add(card);
                card.transform.SetParent(Board.transform, false);
                card.transform.position = position;
                card.transform.rotation = rotation;
                card.SetActive(true);
                Debug.Log("Received card with ViewID: " + viewID + " and CardID: " + cardID);


                DisplayCard3 cardComponent = card.GetComponent<DisplayCard3>();
                if (cardComponent != null)
                {
                    Debug.Log("Searching for card in boardzone with cardID: " + cardID);
                    Card masterCard = boardcard.Find(c => c.Id == cardID);
                    if (masterCard != null)
                    {
                        Debug.Log("Master card found in boardzone: " + masterCard);
                        cardComponent.LetterType = masterCard.LetterType;
                        cardComponent.ColorType = masterCard.ColorType;
                        cardComponent.AmountType = masterCard.AmountType;
                        cardComponent.FontType = masterCard.FontType;
                        cardComponent.Point = masterCard.Point;
                        cardComponent.Spriteimg = masterCard.Spriteimg;

                        Debug.Log($"Card data synchronized for card with ViewID: {viewID} and CardID: {cardID}. " +
                             $"LetterType: {masterCard.LetterType}, ColorType: {masterCard.ColorType}, " +
                             $"AmountType: {masterCard.AmountType}, FontType: {masterCard.FontType}, Point: {masterCard.Point}");
                    }
                    else
                    {
                        //Debug.LogWarning("Master card not found for card with ID: " + cardID);
                    }
                }
                else
                {
                    //Debug.LogError("Card component not found on the received card.");
                }
            }
            else
            {
                Debug.LogWarning("Card with ViewID: " + viewID + " is already in the cardList.");
            }

        }
        else
        {
            //Debug.LogError("Could not find card with ViewID: " + viewID);
        }
    }

    public void CallUpdateCard()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            List<int> cardIds = new List<int>();
            List<string> letterTypes = new List<string>();
            List<string> colorTypes = new List<string>();
            List<string> amountTypes = new List<string>();
            List<string> fontTypes = new List<string>();
            List<int> points = new List<int>();
            List<string> spriteNames = new List<string>();

            foreach (Card card in boardcard)
            {
                cardIds.Add(card.Id);
                letterTypes.Add(card.LetterType);
                colorTypes.Add(card.ColorType);
                amountTypes.Add(card.AmountType);
                fontTypes.Add(card.FontType);
                points.Add(card.Point);
                spriteNames.Add(card.Spriteimg.name);
            }

            photonView.RPC("UpdateCard", RpcTarget.AllBuffered, cardIds.ToArray(), letterTypes.ToArray(), colorTypes.ToArray(), amountTypes.ToArray(), fontTypes.ToArray(), points.ToArray(), spriteNames.ToArray());
        }
        else
        {
            //localphotonView.RPC("RequestCardSync", RpcTarget.MasterClient);
        }
    }

[PunRPC]
private void UpdateCard(int[] cardIds, string[] letterTypes, string[] colorTypes, string[] amountTypes, string[] fontTypes, int[] points, string[] spriteNames)
{
    for (int i = 0; i < cardIds.Length; i++)
    {
        // ตรวจสอบว่ามีการ์ดที่ตำแหน่งนี้หรือไม่
        Card existingCard = boardcard.FirstOrDefault(c => c.Id == cardIds[i]);
        if (existingCard != null)
        {
            // อัปเดตการ์ดที่มีอยู่
            existingCard.LetterType = letterTypes[i];
            existingCard.ColorType = colorTypes[i];
            existingCard.AmountType = amountTypes[i];
            existingCard.FontType = fontTypes[i];
            existingCard.Point = points[i];
            existingCard.Spriteimg = Resources.Load<Sprite>(spriteNames[i]);
        }
        else
        {
            // สร้างการ์ดใหม่
            GameObject newCard = Instantiate(CardPrefab);
            newCard.transform.SetParent(Board.transform, false); // ตั้งค่า parent เป็น Board
            newCard.transform.localPosition = Vector3.zero; // ปรับตำแหน่งตามความต้องการ
            newCard.transform.localRotation = Quaternion.identity;
            newCard.SetActive(true);

            // ตั้งค่าข้อมูลการ์ด
            Card card = newCard.GetComponent<Card>();
            card.Id = cardIds[i];
            card.LetterType = letterTypes[i];
            card.ColorType = colorTypes[i];
            card.AmountType = amountTypes[i];
            card.FontType = fontTypes[i];
            card.Point = points[i];
            card.Spriteimg = Resources.Load<Sprite>(spriteNames[i]);

            // เพิ่มการ์ดใน list boardcard
            boardcard.Add(card);

            // เพิ่มการ์ดใน list cardList เพื่อจัดการการ์ดที่สร้างใหม่
            cardList.Add(newCard);
        }
    }
    Debug.Log("Boardcard updated with " + boardcard.Count + " cards.");
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

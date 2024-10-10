using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class DropQ : MonoBehaviourPunCallbacks, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private List<Card> droppedCards = new List<Card>();
    private Transform parentToReturnTo;
    private DeckQ deck;
    private int currentScore;
    private MutimanageQ muti;

    public void Start()
    {
        deck = FindObjectOfType<DeckQ>();
        currentScore = 0;
        muti = FindAnyObjectByType<MutimanageQ>();
    }

    public void Update()
    {
        if (!MutimanageQ.isZETActive)
        {
            int numberOfCardsInCheckZone = transform.childCount;

            if (numberOfCardsInCheckZone > 0 && numberOfCardsInCheckZone < 3)
            {
                for (int i = numberOfCardsInCheckZone - 1; i >= 0; i--)
                {
                    Transform cardTransform = transform.GetChild(i);
                    if (cardTransform != null)
                    {
                        cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                        cardTransform.localPosition = Vector3.zero;

                        DisplayCardQ displayCardComponent = cardTransform.GetComponent<DisplayCardQ>();
                        if (displayCardComponent != null)
                        {
                            displayCardComponent.SetBlocksRaycasts(true);
                        }
                    }
                }

                droppedCards.Clear();

                Debug.Log("Returned cards to Boardzone because ZET button was not pressed.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!MutimanageQ.isZETActive)
        {
            return;
        }

        parentToReturnTo = transform.parent;

        int numberOfCardsOnDropZone = CheckNumberOfCardsOnDropZone();

        if (numberOfCardsOnDropZone == 3)
        {
            Debug.Log("Cannot drop. Maximum number of cards reached.");
            return;
        }

        if (eventData.pointerDrag != null)
        {
            DragQ draggable = eventData.pointerDrag.GetComponent<DragQ>();
            if (draggable != null)
            {
                draggable.parentToReturnTo = this.transform;
                photonView.RPC("UpdateCardParent", RpcTarget.AllBuffered, draggable.GetComponent<PhotonView>().ViewID, photonView.ViewID);
            }

            DisplayCardQ displayCardComponent = eventData.pointerDrag.GetComponent<DisplayCardQ>();
            if (displayCardComponent != null)
            {
                droppedCards.Add(displayCardComponent.displayCard[0]);
            }
        }
        StartCoroutine(CheckSetWithDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }
    }

    public int CheckNumberOfCardsOnDropZone()
    {
        int numberOfCards = this.transform.childCount;
        return numberOfCards;
    }

    public void CheckSetConditions()
    {
        if (droppedCards.Count == 3)
        {
            bool isSet = CheckCardsAreSet(droppedCards[0], droppedCards[1], droppedCards[2]);

            if (isSet)
            {
                int TotalScore = CalculateTotalScore(droppedCards[0], droppedCards[1], droppedCards[2]);

                UpdateScore(TotalScore);

                RemoveCardsFromGameRPC();
                if (PhotonNetwork.IsMasterClient)
                {
                    deck.DrawCards(3);
                }
                else
                {
                    deck.photonView.RPC("RequestDrawCardsFromMaster", RpcTarget.MasterClient, 3);
                }
            }
            else
            {
                photonView.RPC("ReturnCardsToOriginalPositionRPC", RpcTarget.AllBuffered);

                UpdateScore(-1);
            }
        }
        else
        {
            Debug.Log("Unexpected number of cards: " + droppedCards.Count);
        }
    }

    public bool CheckCardsAreSet(Card card1, Card card2, Card card3)
    {
        bool letterSet = ArePropertiesEqual(card1.LetterType, card2.LetterType, card3.LetterType);
        bool colorSet = ArePropertiesEqual(card1.ColorType, card2.ColorType, card3.ColorType);
        bool amountSet = ArePropertiesEqual(card1.AmountType, card2.AmountType, card3.AmountType);
        bool fontSet = ArePropertiesEqual(card1.FontType, card2.FontType, card3.FontType);

        return (letterSet && colorSet && amountSet && fontSet);
    }

    public bool ArePropertiesEqual(string property1, string property2, string property3)
    {
        return (property1 == property2 && property2 == property3) || (property1 != property2 && property2 != property3 && property1 != property3);
    }

    public int CalculateTotalScore(Card card1, Card card2, Card card3)
    {
        int totalScore = card1.Point + card2.Point + card3.Point;
        return totalScore;
    }

    public IEnumerator CheckSetWithDelay()
    {
        yield return new WaitForSeconds(4f);
        CheckSetConditions();
    }

    public void UpdateScore(int addScore)
    {
        currentScore += addScore;
        if (currentScore < 0)
        {
            currentScore = 0;
        }
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        muti.photonView.RPC("UpdatePlayerScore", RpcTarget.All, actorNumber, currentScore);
    }

    public void CheckAndReturnCardsToBoardZone()
    {
        if (!MutimanageQ.isZETActive)
        {
            int numberOfCardsInCheckZone = transform.childCount;

            if (numberOfCardsInCheckZone > 0 && numberOfCardsInCheckZone < 3)
            {
                for (int i = numberOfCardsInCheckZone - 1; i >= 0; i--)
                {
                    Transform cardTransform = transform.GetChild(i);
                    if (cardTransform != null)
                    {
                        cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                        cardTransform.localPosition = Vector3.zero;

                        DisplayCardQ displayCardComponent = cardTransform.GetComponent<DisplayCardQ>();
                        if (displayCardComponent != null)
                        {
                            displayCardComponent.SetBlocksRaycasts(true);
                        }
                    }
                }

                droppedCards.Clear();

                Debug.Log("Returned cards to Boardzone because ZET button was not pressed.");
            }
        }
    }

    [PunRPC]
    public void ReturnCardsToOriginalPositionRPC()
    {
        int numberOfCards = transform.childCount;

        for (int i = numberOfCards - 1; i >= 0; i--)
        {
            Transform cardTransform = transform.GetChild(i);
            if (cardTransform != null)
            {
                cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                cardTransform.localPosition = Vector3.zero;

                DisplayCardQ displayCardComponent = cardTransform.GetComponent<DisplayCardQ>();
                if (displayCardComponent != null)
                {
                    displayCardComponent.SetBlocksRaycasts(true);
                }

                Card card = displayCardComponent.displayCard[0];
                if (droppedCards.Contains(card))
                {
                    droppedCards.Remove(card);
                }
            }
        }

        photonView.RPC("ClearDroppedCards", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RemoveCardsFromGameRPC()
    {
        int numberOfCards = transform.childCount;

        for (int i = numberOfCards - 1; i >= 0; i--)
        {
            Transform cardTransform = transform.GetChild(i);
            if (cardTransform != null)
            {
                DisplayCardQ displayCard = cardTransform.GetComponent<DisplayCardQ>();
                if (displayCard != null && droppedCards.Contains(displayCard.displayCard[0]))
                {
                    if (PhotonNetwork.IsMasterClient || photonView.Owner == PhotonNetwork.LocalPlayer)
                    {
                        PhotonNetwork.Destroy(cardTransform.gameObject);
                        droppedCards.Remove(displayCard.displayCard[0]);
                    }
                    else
                    {
                        photonView.RPC("RemoveCardByMasterClient", RpcTarget.MasterClient, cardTransform.gameObject.GetPhotonView().ViewID);
                    }
                }
            }
        }

        photonView.RPC("ClearDroppedCards", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RemoveCardByMasterClient(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView targetView = PhotonView.Find(viewID);
            if (targetView != null)
            {
                PhotonNetwork.Destroy(targetView.gameObject);
            }
        }
    }

    [PunRPC]
    public void ClearDroppedCards()
    {
        droppedCards.Clear();
    }

    [PunRPC]
    public void UpdateCardParent(int cardViewID, int parentViewID)
    {
        PhotonView cardPhotonView = PhotonView.Find(cardViewID);
        PhotonView parentPhotonView = PhotonView.Find(parentViewID);

        if (cardPhotonView != null && parentPhotonView != null)
        {
            cardPhotonView.transform.SetParent(parentPhotonView.transform);
            cardPhotonView.transform.localPosition = Vector3.zero;
        }
    }
}
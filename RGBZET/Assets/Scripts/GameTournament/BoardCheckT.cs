using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class BoardCheckT : MonoBehaviourPunCallbacks
{
    private DropT dropScript;
    private DeckT deck;
    public GameObject Board;
    private MutiManageT mutiManageT;

    public void Start()
    {
        dropScript = FindObjectOfType<DropT>();
        deck = FindObjectOfType<DeckT>();
        mutiManageT = FindObjectOfType<MutiManageT>();
        Board = GameObject.Find("Boardzone");
    }

    public void CheckBoard()
    {
        List<Card> cardsOnBoard = new List<Card>();

        for (int i = 0; i < Board.transform.childCount; i++)
        {
            DisplayCardT displayCard = Board.transform.GetChild(i).GetComponent<DisplayCardT>();
            if (displayCard != null && displayCard.displayCard.Count > 0)
            {
                Card card = displayCard.displayCard[0];
                cardsOnBoard.Add(card);
            }
        }

        if (cardsOnBoard.Count == 12)
        {
            bool isSet = CheckSetForAllCards(cardsOnBoard);

            if (isSet)
            {
                Debug.Log("The cards on the board form a set!");
            }
            else
            {
                Debug.Log("The cards on the board do not form a set. Drawing three more cards...");

                if (PhotonNetwork.IsMasterClient)
                {
                    deck.DrawCards(3);
                }
                else
                {
                    deck.photonView.RPC("RequestDrawCardsFromMaster", RpcTarget.MasterClient, 3);
                }
            }
        }
        else
        {
            Debug.Log("Not enough cards on the board to check for a set.");
        }
    }

    public void CheckBoardEnd()
    {
        List<Card> cardsOnBoard = new List<Card>();

        for (int i = 0; i < Board.transform.childCount; i++)
        {
            DisplayCardT displayCard = Board.transform.GetChild(i).GetComponent<DisplayCardT>();
            if (displayCard != null && displayCard.displayCard.Count > 0)
            {
                Card card = displayCard.displayCard[0];
                cardsOnBoard.Add(card);
            }
        }

        if (cardsOnBoard.Count < 12)
        {
            bool isSet = CheckSetForAllCards(cardsOnBoard);

            if (!isSet)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    mutiManageT.EndGame();
                }
            }
        }
    }

    [PunRPC]
    public void RPC_LoadEndScene()
    {
       PhotonNetwork.LoadLevel("ResultTournament");
    }

    private bool CheckSetForAllCards(List<Card> cards)
    {
        for (int i = 0; i < cards.Count - 2; i++)
        {
            for (int j = i + 1; j < cards.Count - 1; j++)
            {
                for (int k = j + 1; k < cards.Count; k++)
                {
                    bool isSet = dropScript.CheckCardsAreSet(cards[i], cards[j], cards[k]);
                    if (isSet)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
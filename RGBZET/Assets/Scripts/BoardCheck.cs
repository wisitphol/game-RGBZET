using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoardCheck: MonoBehaviour
{
    [HideInInspector]
    public Drop dropScript;
    [HideInInspector]
    public Deck deck;

    public void Start()
    {
        dropScript = FindObjectOfType<Drop>();
        deck = FindObjectOfType<Deck>();
    }

    public void CheckBoard()
    {
        List<Card> cardsOnBoard = new List<Card>();

        GameObject Board = GameObject.Find("Boardzone");

        // เช็คการ์ดใน Boardzone
        for (int i = 0; i < Board.transform.childCount; i++)
        {
            Card card = Board.transform.GetChild(i).GetComponent<DisplayCard>().displayCard[0];
            cardsOnBoard.Add(card);
        }

        // ถ้ามีการ์ดใน Boardzone 12 ใบ
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
                StartCoroutine(deck.Draw(3));
            }
        }
        else
        {
            Debug.Log("Not enough cards on the board to check for a set.");
        }
    }

    private bool CheckSetForAllCards(List<Card> cards)
    {
        // ตรวจสอบเซตของการ์ดทั้ง 12 ใบ
        for (int i = 0; i < cards.Count - 2; i++)
        {
            for (int j = i + 1; j < cards.Count - 1; j++)
            {
                for (int k = j + 1; k < cards.Count; k++)
                {
                    bool isSet = dropScript.CheckCardsAreSet(cards[i], cards[j], cards[k]);
                    if (isSet)
                    {
                        return true; // เมื่อพบเซต ให้ส่งค่า true กลับ
                    }
                }
            }
        }
        return false; // หากไม่พบเซตใดๆ ให้ส่งค่า false กลับ
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardCheck2 : MonoBehaviour
{
    
    private Drop2 dropScript;
    private Deck2 deck;
    public GameObject Board;
    
    public void Start()
    {
        dropScript = FindObjectOfType<Drop2>();
        deck = FindObjectOfType<Deck2>();
        Board = GameObject.Find("Boardzone");
    }

    public void CheckBoard()
    {
        List<Card> cardsOnBoard = new List<Card>();

        // Check cards in Boardzone
        for (int i = 0; i < Board.transform.childCount; i++)
        {
            DisplayCard2 displayCard = Board.transform.GetChild(i).GetComponent<DisplayCard2>();
            if (displayCard != null && displayCard.displayCard.Count > 0)
            {
                Card card = displayCard.displayCard[0];
                cardsOnBoard.Add(card);
            }
        }

        // Count only the cards that are shown on the board when running
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

    public void CheckBoardEnd()
    {
        List<Card> cardsOnBoard = new List<Card>();

        // Check cards in Boardzone
        for (int i = 0; i < Board.transform.childCount; i++)
        {
            DisplayCard2 displayCard = Board.transform.GetChild(i).GetComponent<DisplayCard2>();
            if (displayCard != null && displayCard.displayCard.Count > 0)
            {
                Card card = displayCard.displayCard[0];
                cardsOnBoard.Add(card);
            }
        }

        // Check if there are less than 12 cards on the board
        if(cardsOnBoard.Count < 12)
        {
            bool isSet = CheckSetForAllCards(cardsOnBoard);

            // If there are less than 12 cards and they form a set, the game continues
            if (isSet)
            {
                Debug.Log("The cards on the board form a set!");
            }
            else
            {
                SceneManager.LoadScene("Endscene");
            }
        }
        else
        {
        // If there are 12 cards on the board, the game continues
        }
        
    }

    

    private bool CheckSetForAllCards(List<Card> cards)
    {
        // Check for sets of cards across all 12 cards
        for (int i = 0; i < cards.Count - 2; i++)
        {
            for (int j = i + 1; j < cards.Count - 1; j++)
            {
                for (int k = j + 1; k < cards.Count; k++)
                {
                    bool isSet = dropScript.CheckCardsAreSet(cards[i], cards[j], cards[k]);
                    if (isSet)
                    {
                        return true; // Return true when a set is found
                    }
                }
            }
        }
        return false; // Return false if no set is found
    }


    public void FindCard()
    {
        List<GameObject> cardsOnBoard = new List<GameObject>();

        // ตรวจสอบการ์ดใน Boardzone
        for (int i = 0; i < Board.transform.childCount; i++)
        {
            GameObject card = Board.transform.GetChild(i).gameObject;
            if (card != null)
            {
                cardsOnBoard.Add(card);
            }
        }

        // ถ้ามีการ์ดใน Boardzone มากกว่า 1 ใบ ไม่ต้องทำอะไร
        if (cardsOnBoard.Count > 1)
        {
            Debug.Log("There are already enough cards on the board.");
            return;
        }
        else
        {
            // หา GameObjects ที่เป็นการ์ดนอก canvas (หรือพื้นที่อื่นๆ)
            GameObject[] allCards = GameObject.FindGameObjectsWithTag("Clone");
            foreach (GameObject card in allCards)
            {
                if (card.transform.parent != Board.transform)
                {
                    // ย้ายการ์ดมาที่ Boardzone
                    card.transform.SetParent(Board.transform, false);
                    card.transform.localScale = Vector3.one;
                    card.transform.localPosition = Vector3.zero;
                    card.transform.localEulerAngles = Vector3.zero;
                    Debug.Log("Moved card to board!");
                    return;
                }
            }
            Debug.Log("No cards found outside the board.");
        }
    }
}

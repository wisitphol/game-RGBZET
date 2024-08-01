using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardCheck3 : MonoBehaviour
{
    
    private Drop3 dropScript;
    
    //private DeckFire deck;
    private DeckFire deck;
   
    private GameObject Board;
    
    public void Start()
    {
        dropScript = FindObjectOfType<Drop3>();
        deck = FindObjectOfType<DeckFire>();
        Board = GameObject.Find("Boardzone");
    }

    public void CheckBoard()
    {
        Debug.Log("CheckBoard() called.");
        List<Card> cardsOnBoard = new List<Card>();

        // Check cards in Boardzone
        for (int i = 0; i < Board.transform.childCount; i++)
        {
            DisplayCard3 displayCard = Board.transform.GetChild(i).GetComponent<DisplayCard3>();
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
            DisplayCard3 displayCard = Board.transform.GetChild(i).GetComponent<DisplayCard3>();
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
                //deck.DeleteDeckFromDatabase();
            }
        }
        else
        {
        // If there are 12 cards on the board, the game continues
        }
        
    }

    public void CheckBoardSame()
    {
        Debug.Log("CheckBoardSame() called.");
        List<Card> cardsOnBoard = new List<Card>();

        // Check cards in Boardzone
        for (int i = 0; i < Board.transform.childCount; i++)
        {
            DisplayCard3 displayCard = Board.transform.GetChild(i).GetComponent<DisplayCard3>();
            if (displayCard != null && displayCard.displayCard.Count > 0)
            {
                Card card = displayCard.displayCard[0];
                cardsOnBoard.Add(card);
            }
        }

        // Count only the cards that are shown on the board when running
        if (cardsOnBoard.Count == 12)
        {
            bool isSet = CheckSameForAllCards(cardsOnBoard);

            if (isSet)
            {
                // ลบการ์ดใน Boardzone ทิ้ง
                for (int i = 0; i < Board.transform.childCount; i++)
                {
                    Destroy(Board.transform.GetChild(i).gameObject);
                }

                // จั่วการ์ดใหม่ 12 ใบ
                if (deck != null)
                {
                    StartCoroutine(deck.Draw(12));
                }
                else
                {
                    Debug.LogError("DeckFire script not found.");
                }
            }
            else
            {
                // ไม่ต้องทำอะไร
            }
        }
        else
        {
            Debug.Log("Not enough cards on the board to check for a set.");
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

    public bool CheckSameForAllCards(List<Card> cards)
    {
        if (cards.Count == 0)
            return false;

        // ดึงข้อมูลการ์ดใบแรกมาเป็นตัวอย่าง
        Card firstCard = cards[0];

        foreach (Card currentCard in cards)
        {
            // เช็คว่าค่าของการ์ดไม่เท่ากับการ์ดใบแรก
            if (currentCard.LetterType != firstCard.LetterType ||
                currentCard.ColorType != firstCard.ColorType ||
                currentCard.AmountType != firstCard.AmountType ||
                currentCard.FontType != firstCard.FontType)
            {
                return false; // ถ้าค่าต่างกันคืนค่า false
            }
        }

        return true; // ถ้าค่าทั้งหมดเหมือนกันคืนค่า true
    }


}

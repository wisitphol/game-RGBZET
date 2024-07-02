using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardCheck3 : MonoBehaviour
{
    
    private Drop3 dropScript;
    
    //private DeckFire deck;
    private Deckfree deck;
   
    private GameObject Board;
    
    public void Start()
    {
        dropScript = FindObjectOfType<Drop3>();
        deck = FindObjectOfType<Deckfree>();
        Board = GameObject.Find("Boardzone");
    }

    public void CheckBoard()
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


}

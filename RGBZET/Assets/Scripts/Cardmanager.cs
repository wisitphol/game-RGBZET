using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cardmanager : MonoBehaviour
{
   
    public Carddata[] allCards; // Array to store 81 cards

    void Start()
    {
        InitializeCards();
    }

    void InitializeCards()
    {
        // Initialize the array with 81 cards
        allCards = new Carddata[81];

        for (int i = 0; i < allCards.Length; i++)
        {
            // Create an instance of CardData
            Carddata card = new Carddata();

            // Set properties for the card (customize based on your game rules)
            card.letter = (Carddata.LetterType)(i % 3); // Assigning a letter type based on index
            card.color = (Carddata.ColorType)(i % 3);   // Assigning a color type based on index
            card.size = (Carddata.SizeType)(i % 3);     // Assigning a size type based on index

            // Add the card to the array
            allCards[i] = card;
        }
    }


}

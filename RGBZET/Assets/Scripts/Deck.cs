using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public List<Card> deck = new List<Card>();
    public int x;
    public static int deckSize;
    public static List<Card> staticDeck = new List<Card>();

    public GameObject CardInDeck;

    public GameObject CardToHand;
    public GameObject[] Clones;
    public GameObject Hand;

    private List<Card> selectedCards = new List<Card>();
    public CardSelection cardSelection;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize deckSize and cardList if not already done
        deckSize = CardData.cardList.Count;
        deck = new List<Card>(CardData.cardList);
        Shuffle(deck);

        // Now deck is shuffled and ready to use
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if(deckSize < 1)
        {
            CardInDeck.SetActive(false);
        }
        
       // if(TurnSystem.startTurn == true)
      //  {
      //      StartCoroutine(Draw(1));
      //      TurnSystem.startTurn = false;
       // }
    }

   IEnumerator StartGame()
{
    for(int i = 0; i < 12; i++)
    {
        yield return new WaitForSeconds(1);
        GameObject newCard = Instantiate(CardToHand, transform.position, transform.rotation) as GameObject;
        newCard.transform.SetParent(Hand.transform, false);
        newCard.SetActive(true);
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

    IEnumerator Draw(int x)
    {
        for(int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(1);

            GameObject newCard = Instantiate(CardToHand, transform.position, transform.rotation);
            
            //Debug.Log("New Card created: " + newCard);
        }
    }

    
}

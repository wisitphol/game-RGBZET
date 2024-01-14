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

    public GameObject[] Clones;
    public GameObject Hand;

    // Start is called before the first frame update
    void Start()
    {
        x = 0;
        deckSize = 80;
        for(int i = 0; i < 80; i++)
        {
            x = Random.Range(0,26);
            deck[i] = CardData.cardList[x];
        }
        
        //StartCotroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;
    }

    //IEnumerator StartGame()
    //{
    //    for(int i = 0; i <= 4;i++)
    //    {
     //       yield return new WaitFordSeconds(1);

            

      //      Instantiate(CardToHand, transform.position, transform.rotation);
    //    }
  //  }

    public void Shuffle()
    {
        for(int i = 0; i < deckSize; i++)
        {
            container[0] = deck[i];
            int randomIndex = Random.Range(i,deckSize);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = container[0];
        }
    }


}

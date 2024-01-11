using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public List<Card> deck = new List<Card>();
    public int x;
    public int deckSize;

    // Start is called before the first frame update
    void Start()
    {
        x = 0;

        for(int i = 0; i < 80; i++)
        {
            x = Random.Range(0,26);
            deck[i] = CardData.cardList[x];
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


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

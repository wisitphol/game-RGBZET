using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cardmanager : MonoBehaviour
{

    public GameObject card1;
    public GameObject card2;
    public GameObject card3;

    void Start()
    {
        Carddata card1Data = card1.GetComponent<Carddata>();
        Carddata card2Data = card2.GetComponent<Carddata>();
        Carddata card3Data = card3.GetComponent<Carddata>();

        // ตัวอย่างการเข้าถึงข้อมูลของการ์ด
        Debug.Log("Card 1: " + card1Data.letter + ", " + card1Data.color + ", " + card1Data.size + ", " + card1Data.texture);
        Debug.Log("Card 2: " + card2Data.letter + ", " + card2Data.color + ", " + card2Data.size + ", " + card2Data.texture);
        Debug.Log("Card 3: " + card3Data.letter + ", " + card3Data.color + ", " + card3Data.size + ", " + card3Data.texture);
    }




}

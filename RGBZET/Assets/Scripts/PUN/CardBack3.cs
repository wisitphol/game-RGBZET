using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBack3 : MonoBehaviour
{
    public GameObject cardBack3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(DisplayCard3.staticCardBack == true)
        {
            cardBack3.SetActive(true);
        }
        else
        {
            cardBack3.SetActive(false);
        }


    }
}

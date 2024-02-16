using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardToBoard : MonoBehaviour
{
    public GameObject Hand;
    public GameObject HandCard;

    /*
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Hand = GameObject.Find("Hand");
        HandCard.transform.SetParent(Hand.transform);
        //Debug.Log("Cardboard set to Hand: " + HandCard);
        HandCard.transform.localScale = Vector3.one;
        HandCard.transform.position = new Vector3(transform.position.x,transform.position.y, -48);
        HandCard.transform.eulerAngles = new Vector3(25, 0, 0);
    }
    */


    // Start is called before the first frame update
    void Start()
    {
        Hand = GameObject.Find("Hand");
    }

    public void MoveCardToHand()
    {
        HandCard.transform.SetParent(Hand.transform, false);
        HandCard.transform.localScale = Vector3.one;
        // Use localPosition and localRotation for UI elements
        HandCard.transform.localPosition = new Vector3(0, 0, -48);
        HandCard.transform.localEulerAngles = new Vector3(25, 0, 0);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardToBoard : MonoBehaviour
{
    public GameObject Board;
    public GameObject BoardCard;
    private BoardCheck boardCheckScript;


    // Start is called before the first frame update
    void Start()
    {
        Board = GameObject.Find("Boardzone");
        boardCheckScript = FindObjectOfType<BoardCheck>();
    }

    public void MoveCardToBoard()
    {
        if (!Button1.isZetActive)
        {
            Debug.Log("ZET is not active. Cannot move card to board.");
            return;
        }
        
        BoardCard.transform.SetParent(Board.transform, false);
        BoardCard.transform.localScale = Vector3.one;
        // Use localPosition and localRotation for UI elements
        BoardCard.transform.localPosition = new Vector3(0, 0, -48);
        BoardCard.transform.localEulerAngles = new Vector3(25, 0, 0);

        if (Board.transform.childCount == 12)
        {
            boardCheckScript.CheckBoard();
        }
        

       
    }


}

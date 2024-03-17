using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardToBoard : MonoBehaviour
{
    public GameObject Board;
    public GameObject BoardCard;
    private BoardCheck boardCheckScript;
    private Deck deck;


    // Start is called before the first frame update
    void Start()
    {
        Board = GameObject.Find("Boardzone");
        boardCheckScript = FindObjectOfType<BoardCheck>();
        
        //นับทุกการ์ดทีอยู่ใน boardzone
        if (Board.transform.childCount == 13)
        {
             boardCheckScript.CheckBoard();
        }

        if(Deck.deckSize <= 0)
        {
            boardCheckScript.CheckBoardEnd();
        }
        
    }

    public void MoveCardToBoard()
    {
        if (!ZETbutton.isZetActive)
        {
            Debug.Log("ZET is not active. Cannot move card to board.");
            return;
        }
        
        BoardCard.transform.SetParent(Board.transform, false);
        BoardCard.transform.localScale = Vector3.one;
        // Use localPosition and localRotation for UI elements
        BoardCard.transform.localPosition = new Vector3(0, 0, -48);
        BoardCard.transform.localEulerAngles = new Vector3(25, 0, 0);

        Debug.Log("Card moved to board!"); // เพิ่ม Debug Log เพื่อตรวจสอบการเรียกใช้งาน


        
        

       
    }


}

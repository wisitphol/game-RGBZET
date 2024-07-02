using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardToBoard3 : MonoBehaviour
{
    public GameObject Board;
    public GameObject BoardCard;
    private BoardCheck3 boardCheckScript;
    private PhotonView photonView;


    // Start is called before the first frame update
    void Start()
    {
        Board = GameObject.Find("Boardzone");
        boardCheckScript = FindObjectOfType<BoardCheck3>();
        
        //นับทุกการ์ดทีอยู่ใน boardzone
        if (Board.transform.childCount == 13)
        {
             boardCheckScript.CheckBoard();
        }
        
        photonView = GetComponent<PhotonView>();
        
    }

    public void MoveCardToBoard()
    {
        if (!ZETManager3.isZETActive)
        {
            Debug.Log("ZET is not active. Cannot move card to board.");
            return;
        }

        // เรียก Photon RPC เพื่อซิงค์การ์ดกับผู้เล่นคนอื่น
        photonView.RPC("RPC_MoveCardToBoard", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_MoveCardToBoard()
    {
        BoardCard.transform.SetParent(Board.transform, false);
        BoardCard.transform.localScale = Vector3.one;
        // ใช้ localPosition และ localRotation สำหรับ UI elements
        BoardCard.transform.localPosition = new Vector3(0, 0, -48);
        BoardCard.transform.localEulerAngles = new Vector3(25, 0, 0);

        Debug.Log("Card moved to board!"); // เพิ่ม Debug Log เพื่อตรวจสอบการเรียกใช้งาน

        // นับการ์ดใหม่ในบอร์ด
        if (Board.transform.childCount == 13)
        {
            boardCheckScript.CheckBoard();
        }
    }


}

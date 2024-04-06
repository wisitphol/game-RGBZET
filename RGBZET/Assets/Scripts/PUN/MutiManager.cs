using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MutiManager : MonoBehaviourPunCallbacks
{
   public GameObject cardPrefab; // Prefab ของการ์ด
    public Transform boardTransform; // ตำแหน่งที่จะสร้างการ์ด
    public int numberOfCards = 12; // จำนวนการ์ดที่จะสร้าง
    private List<Card> cardList; // รายการข้อมูลของการ์ด

   void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            // ตรวจสอบสถานะการเชื่อมต่อก่อนเรียกใช้ ConnectUsingSettings
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Already connected to Photon Master Server. Pun is ready for use!");
            if (PhotonNetwork.IsMasterClient)
            {
                // ถ้าเป็น Master Client ให้สร้างการ์ดและส่งข้อมูลไปยังผู้เล่นอื่น
                CreateCards();
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server. Pun is ready for use!");
        if (PhotonNetwork.IsMasterClient)
        {
            // ถ้าเป็น Master Client ให้สร้างการ์ดและส่งข้อมูลไปยังผู้เล่นอื่น
            CreateCards();
        }
    }

    void CreateCards()
    {
        // สุ่มการ์ดและส่งข้อมูลไปยังผู้เล่นอื่น
        cardList = new List<Card>(CardData.cardList);
        Shuffle(cardList);
        photonView.RPC("RPC_CreateCards", RpcTarget.AllBuffered, cardList.ToArray());
    }

    [PunRPC]
    void RPC_CreateCards(Card[] cards)
    {
        foreach (Card cardData in cards)
        {
            GameObject newCard = Instantiate(cardPrefab, boardTransform);

            // กำหนด PhotonView ID ใหม่
            PhotonView photonView = newCard.GetComponent<PhotonView>();

            photonView.ViewID = PhotonNetwork.AllocateViewID(PhotonNetwork.LocalPlayer.ActorNumber);

            
            // ส่ง RPC เพื่อขอเป็นเจ้าของของ PhotonView
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

            newCard.GetComponent<DisplayCard>().DisplayCardData(cardData);
        }
    }

    void Shuffle(List<Card> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Card temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        // ทำสิ่งที่คุณต้องการหลังจากเข้าร่วมห้อง
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a room, creating a new one...");
        CreateRoom();
    }
}

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

        // สร้างการ์ดโดยใช้ Instantiate() และนำเข้า PhotonNetwork
        foreach (Card cardData in cardList)
        {
            PhotonNetwork.Instantiate(cardPrefab.name, boardTransform.position, boardTransform.rotation, 0, new object[] { cardData.Id });
        }
    }

    private Dictionary<int, PhotonView> photonViewDictionary = new Dictionary<int, PhotonView>(); // สร้าง Dictionary เพื่อเก็บ PhotonView ที่สร้างขึ้น

    int GenerateUniqueViewID()
    {
        int newViewID = 0;
        bool isUnique = false;

        // Loop จนกว่าจะพบ ID ที่ไม่ซ้ำซ้อน
        while (!isUnique)
        {
            newViewID = Random.Range(1000, 1000000); // สร้างเลขสุ่ม
            isUnique = !photonViewDictionary.ContainsKey(newViewID); // ตรวจสอบว่าเลขนี้ซ้ำหรือไม่
        }

        return newViewID;
    }
    [PunRPC]
    void RPC_SetupCard(int cardId)
    {
        GameObject newCardObject = Instantiate(cardPrefab, boardTransform.position, Quaternion.identity);
        PhotonView photonView = newCardObject.GetComponent<PhotonView>();

        // สร้าง ViewID ใหม่และกำหนดให้กับ PhotonView
        //photonView.ViewID = PhotonNetwork.AllocateViewID();

        // กำหนดเจ้าของของ PhotonView เป็นผู้เล่นที่เชื่อมต่ออยู่ในปัจจุบัน
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

        // แสดงข้อมูลของการ์ด
        Card cardData = CardData.cardList[cardId];
        newCardObject.GetComponent<DisplayCard>().DisplayCardData(cardData);
    }


    void OnDestroy()
    {
        // คืนทรัพยากรการ์ดที่ถูกทำลายกลับไปยัง Pool เพื่อให้สามารถใช้งานได้ใหม่
        foreach (var kvp in photonViewDictionary)
        {
            PhotonNetwork.Destroy(kvp.Value);
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

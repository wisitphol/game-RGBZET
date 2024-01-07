using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager1 : MonoBehaviour
{
    public GameObject cardnormal; // Prefab ของการ์ด
    public Transform board; // กระดานที่จะวางการ์ด

    private List<GameObject> deck; // กองการ์ด
    private List<GameObject> selectedCards; // การ์ดที่ผู้เล่นเลือก
    
    void Start()
    {
        InitializeDeck();
        ShuffleDeck();
        DrawCardsOnBoard(12);
        DestroyOldCardClones();
    }

    // สร้างกองการ์ด
    void InitializeDeck()
    {
        deck = new List<GameObject>();
       
        // เพิ่มการ์ดลงในกอง
        // เช่น Instantiate(cardPrefab) และเพิ่มลงใน deck
        // ทำซ้ำจนครบจำนวนที่ต้องการ
       // เพิ่ม Prefab ของการ์ดลงใน deck
        int numberOfCardsInDeck = 81;
        for (int i = 0; i < numberOfCardsInDeck; i++)
        {
            GameObject cardInstance = Instantiate(cardnormal);
            // สร้าง instance ของ Prefab และเพิ่มลงใน deck
            deck.Add(cardInstance);
        }


    }

    // สลับการ์ดในกอง
    void ShuffleDeck()
    {
        int deckSize = deck.Count;
        // ใช้ลูปเพื่อสลับตำแหน่งของการ์ด
        for (int i = 0; i < deckSize; i++)
        {
            int randomIndex = Random.Range(i, deckSize); // เลือกตำแหน่งที่จะสลับ
            // สลับตำแหน่งการ์ด
            GameObject temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // จั่วการ์ดออกมาบนกระดาน
    void DrawCardsOnBoard(int numCards)
    {
        selectedCards = new List<GameObject>();
        for (int i = 0; i < numCards; i++)
        {
            if (deck.Count > 0)
            {
                // สุ่มการ์ดจาก deck
                int randomIndex = Random.Range(0, deck.Count);
                GameObject selectedCard = deck[randomIndex];

                // ลบการ์ดจาก deck และเพิ่มลงใน selectedCards
                deck.RemoveAt(randomIndex);
                selectedCards.Add(selectedCard);

                // นำ selectedCard มาแสดงบนกระดาน
                Instantiate(selectedCard, board.position + new Vector3(i * 1.5f, 0f, 0f), Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("ไม่มีการ์ดในกอง");
            }
        }
    }

    // ตรวจสอบการ์ดที่ผู้เล่นเลือก
    void CheckSelectedCards()
    {
        // ตรวจสอบ selectedCards ตามกฎของเกม
        // สามารถเพิ่มเงื่อนไขต่าง ๆ ได้ตามกฎของเกม
    }

    void DestroyOldCardClones()
{
    // หา Prefab clone ที่อยู่บน Board และลบทิ้ง
    GameObject[] oldCardClones = GameObject.FindGameObjectsWithTag("cardnormal(Clone)");
    foreach (GameObject oldCardClone in oldCardClones)
    {
        Destroy(oldCardClone);
    }
}
}

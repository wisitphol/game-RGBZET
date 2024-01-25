using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRowManager : MonoBehaviour
{
    public GameObject cardPrefab;  // ลาก Prefab ของการ์ดลงไปใน Inspector
    public int cardsPerRow = 4;     // จำนวนการ์ดในแถว
    public Transform rowParent;     // ลาก GameObject ที่ถือ `HorizontalLayoutGroup` ลงไปใน Inspector

    private List<GameObject> currentRow = new List<GameObject>();

    // เพิ่มการ์ดลงในแถว
    public void AddCardToRow()
    {
        GameObject newCard = Instantiate(cardPrefab, rowParent);
        currentRow.Add(newCard);

        // ถ้าจำนวนการ์ดในแถวเต็ม, สร้างแถวใหม่
        if (currentRow.Count == cardsPerRow)
        {
            CreateNewRow();
        }
    }

    // สร้างแถวใหม่
    private void CreateNewRow()
    {
        currentRow.Clear();
    }
}

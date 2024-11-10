using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private DisplayCard displayCard;

    private Vector3 savedPosition;
    private int savedIndex;

    // ตัวแปร GridLayoutGroup สำหรับ BoardZone
    public float cardWidth = 200f; // ความกว้างของการ์ด
    public float cardHeight = 130f; // ความสูงของการ์ด
    public int columns = 1; // จำนวนคอลัมน์ที่ต้องการจัดเรียง
                            // ใช้ RectTransform เพื่อจัดการตำแหน่ง
    private RectTransform parentRectTransform;

    void Start()
    {
        displayCard = GetComponent<DisplayCard>();
        //displayCard = GetComponentInParent<DisplayCard>();

        parentRectTransform = parentToReturnTo.GetComponent<RectTransform>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ZETManage.isZETActive) // ให้เริ่มลากเฉพาะเมื่อ ZET ถูก activate
        {
            Debug.Log("Dragging enabled.");

            // เก็บตำแหน่งเริ่มต้นและ siblingIndex ของการ์ด
            savedPosition = this.transform.localPosition;
            savedIndex = this.transform.GetSiblingIndex();

            // ย้ายการ์ดออกจาก parent เพื่อทำการลาก
            this.transform.SetParent(this.transform.parent.parent);
            GetComponent<CanvasGroup>().blocksRaycasts = false;

            // บันทึกตำแหน่งเริ่มต้น
            startPosition = this.transform.localPosition;
            startRotation = this.transform.localRotation;
        }
        else
        {
            Debug.Log("Dragging disabled. ZET button has not been pressed.");
            eventData.pointerDrag = null; // ป้องกันการลาก
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ZETManage.isZETActive)
        {
            this.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ตรวจสอบว่าต้องการให้การ์ดกลับไปที่ BoardZone หรือไม่
        if (parentToReturnTo != null)
        {
            // กลับการ์ดไปที่ parent เดิม
            this.transform.SetParent(parentToReturnTo);

            // คืนตำแหน่งที่บันทึกไว้
            this.transform.localPosition = savedPosition;

            // กลับ siblingIndex ของการ์ดกลับไปที่ตำแหน่งเดิมใน BoardZone
            this.transform.SetSiblingIndex(savedIndex);

            // รีเซ็ตตำแหน่งของการ์ดใหม่
            RepositionCardsInBoardzone();

            GetComponent<CanvasGroup>().blocksRaycasts = true; // เปิดการตรวจจับ Raycasts ของการ์ดที่ถูกลาก
            Debug.Log("Returning to original position.");
        }
    }

    // ฟังก์ชันนี้ใช้คำนวณตำแหน่งใหม่ของการ์ดใน BoardZone
    private void RepositionCardsInBoardzone()
    {
        int totalCards = parentToReturnTo.childCount;

        // คำนิยามตำแหน่งเริ่มต้นของการ์ดใบแรก
        float startX = 100f; // ตำแหน่งเริ่มต้น x
        float startY = -65f; // ตำแหน่งเริ่มต้น y

        float horizontalSpacing = 10f; // ระยะห่างระหว่างการ์ดในแนวนอน
        float verticalSpacing = 10f;   // ระยะห่างระหว่างการ์ดในแนวตั้ง

        for (int i = 0; i < totalCards; i++)
        {
            Transform cardTransform = parentToReturnTo.GetChild(i);

            // คำนวณตำแหน่งของการ์ดในแถวและคอลัมน์
            int row = i / columns;
            int column = i % columns;

            // คำนวณตำแหน่งของการ์ดจากตำแหน่งเริ่มต้นที่กำหนด
            float xPosition = startX + (column * (cardWidth + horizontalSpacing));
            float yPosition = startY - (row * (cardHeight + verticalSpacing));

            // ตั้งตำแหน่งใหม่ของการ์ด
            cardTransform.localPosition = new Vector3(xPosition, yPosition, cardTransform.localPosition.z);
        }
    }
}

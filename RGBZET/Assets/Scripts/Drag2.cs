using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Drag2 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    private Vector3 startPosition;
    private Quaternion startRotation; // เพิ่มเติมสำหรับการจัดเก็บการหมุนเริ่มต้น

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Button1.isZetActive) // ให้เริ่มลากเฉพาะเมื่อ ZET ถูก activate
        {
            Debug.Log("Dragging enabled.");
            parentToReturnTo = this.transform.parent;
            startPosition = this.transform.localPosition; // จัดเก็บตำแหน่งเริ่มต้น
            startRotation = this.transform.localRotation; // จัดเก็บการหมุนเริ่มต้น
            
            // ย้ายการ์ดออกจาก parent เพื่อทำการลาก
            this.transform.SetParent(this.transform.parent.parent);
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            Debug.Log("Dragging disabled. ZET button has not been pressed.");
            eventData.pointerDrag = null; // ป้องกันการลาก
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Button1.isZetActive)
        {
            this.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(parentToReturnTo);
        // ใช้ตำแหน่งและการหมุนเริ่มต้นเพื่อให้กลับไปยังสถานะเดิม
        this.transform.localPosition = startPosition;
        this.transform.localRotation = startRotation;
        
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        Debug.Log("Returning to original position.");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Drop : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        // เมื่อมี pointer เข้ามาในระยะ
        if (eventData.pointerDrag == null)
        return;

        //Debug.Log("Pointer entered drop zone");
    }

    public void OnDrop(PointerEventData eventData)
    {
         // ตรวจสอบก่อนว่าปุ่ม ZET ถูกกดแล้วหรือยัง
        if (!Button1.isZetActive)
        {
            Debug.Log("Cannot drop. ZET button has not been pressed.");
            return; // ยกเลิกการ drop ถ้าปุ่ม ZET ยังไม่ถูกกด
        }

        Debug.Log("OnDrop event detected");

        // ตรวจสอบว่ามี object ที่ลากมาวางลงหรือไม่
        if (eventData.pointerDrag != null)
        {
            // เรียกใช้สคริปต์ Drag ของ object ที่ลากมา
            Drag draggable = eventData.pointerDrag.GetComponent<Drag>();
            if (draggable != null)
            {
                // กำหนด parent ใหม่ให้กับ object ที่ลากมา เพื่อให้วางลงใน panel นี้
                draggable.parentToReturnTo = this.transform;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // เมื่อ pointer ออกจากระยะ
        if (eventData.pointerDrag == null)
        return;

        //Debug.Log("Pointer exited drop zone");
    }

    
}

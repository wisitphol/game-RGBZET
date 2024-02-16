using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Dropnew : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        // เมื่อมี pointer เข้ามาในระยะ
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag; // แก้ไขตรงนี้เป็น pointerDrag
        if (dropped != null)
        {
            Dragnew draggableItem = dropped.GetComponent<Dragnew>();
            if (draggableItem != null)
            {
                draggableItem.parentAfterDrag = transform; // ตั้งค่า parent ใหม่หลังจากลาก
                Debug.Log("Drop");
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // เมื่อ pointer ออกจากระยะ
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

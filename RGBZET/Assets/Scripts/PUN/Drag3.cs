using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class Drag3 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private DisplayCard3 displayCard;
    private PhotonView photonView; // เพิ่มตัวแปร PhotonView สำหรับการสื่อสารผ่านเครือข่าย

    void Start()
    {
        displayCard = GetComponent<DisplayCard3>();

        photonView = GetComponent<PhotonView>(); // กำหนดค่าให้กับ photonView
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ZETManager3.isZETActive) // ให้เริ่มลากเฉพาะเมื่อ ZET ถูก activate
        {
            //Debug.Log("Dragging enabled.");
            parentToReturnTo = this.transform.parent;
            startPosition = this.transform.localPosition; // จัดเก็บตำแหน่งเริ่มต้น
            startRotation = this.transform.localRotation; // จัดเก็บการหมุนเริ่มต้น

            // ย้ายการ์ดออกจาก parent เพื่อทำการลาก
            this.transform.SetParent(this.transform.parent.parent);
            GetComponent<CanvasGroup>().blocksRaycasts = false;

            // เพิ่มการบันทึกตำแหน่งและการหมุนเริ่มต้นของการ์ด
            startPosition = this.transform.localPosition;
            startRotation = this.transform.localRotation;

            // เรียกใช้งาน RPC เพื่อส่งข้อมูลการย้ายการ์ดไปยังผู้เล่นอื่น ๆ ผ่านทางเครือข่าย Photon
            //photonView.RPC("RPC_OnBeginDrag", RpcTarget.All, startPosition, startRotation);
        }
        else
        {
            Debug.Log("Dragging disabled. ZET button has not been pressed.");
            eventData.pointerDrag = null; // ป้องกันการลาก
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ZETManager3.isZETActive)
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
        //Debug.Log("Returning to original position.");
    }

    // RPC สำหรับการย้ายการ์ด
    [PunRPC]
    private void RPC_OnBeginDrag(Vector3 startPosition, Quaternion startRotation)
    {
        // กำหนดตำแหน่งและการหมุนเริ่มต้นของการ์ดตามข้อมูลที่ได้รับผ่านทางเครือข่าย
        this.startPosition = startPosition;
        this.startRotation = startRotation;
    }
}

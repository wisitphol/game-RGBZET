using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Drag2 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    private Vector3 startPosition;
    private Quaternion startRotation; 
    private DisplayCard2 displayCard;
    private PhotonView photonView;

    void Start()
    {
        displayCard = GetComponent<DisplayCard2>();
      
        photonView = GetComponent<PhotonView>();
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (MutiManage2.isZETActive && photonView.Owner == MutiManage2.playerWhoActivatedZET) // ตรวจสอบผู้เล่นที่กดปุ่ม ZET
        {
            Debug.Log("Dragging enabled.");
            parentToReturnTo = this.transform.parent;
            startPosition = this.transform.localPosition; // จัดเก็บตำแหน่งเริ่มต้น
            startRotation = this.transform.localRotation; // จัดเก็บการหมุนเริ่มต้น
            
            // ย้ายการ์ดออกจาก parent เพื่อทำการลาก
            this.transform.SetParent(this.transform.parent.parent);
            GetComponent<CanvasGroup>().blocksRaycasts = false;

           // เพิ่มการบันทึกตำแหน่งและการหมุนเริ่มต้นของการ์ด
            startPosition = this.transform.localPosition;
            startRotation = this.transform.localRotation;
             photonView.RPC("RPC_OnBeginDrag", RpcTarget.All, startPosition, startRotation);
        }
        else
        {
            Debug.Log("Dragging disabled. ZET button has not been pressed.");
            eventData.pointerDrag = null; // ป้องกันการลาก
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (MutiManage2.isZETActive && photonView.Owner == MutiManage2.playerWhoActivatedZET) // ตรวจสอบผู้เล่นที่กดปุ่ม ZET
        {
            this.transform.position = eventData.position;

            photonView.RPC("RPC_OnDrag", RpcTarget.AllBuffered, (Vector2)eventData.position);
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

        photonView.RPC("RPC_OnEndDrag", RpcTarget.AllBuffered, startPosition, startRotation);

    }

    [PunRPC]
    private void RPC_OnBeginDrag(Vector3 startPosition, Quaternion startRotation)
    {
        // กำหนดตำแหน่งและการหมุนเริ่มต้นของการ์ดตามข้อมูลที่ได้รับผ่านทางเครือข่าย
        this.startPosition = startPosition;
        this.startRotation = startRotation;
    }

    // RPC สำหรับการอัปเดตการลากการ์ด
    [PunRPC]
    private void RPC_OnDrag(Vector2 position)
    {
        this.transform.position = position;
    }

    // RPC สำหรับการย้ายกลับการ์ด
    [PunRPC]
    private void RPC_OnEndDrag(Vector3 startPosition, Quaternion startRotation)
    {
        this.startPosition = startPosition;
        this.startRotation = startRotation;
        this.transform.localPosition = startPosition;
        this.transform.localRotation = startRotation;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Drag : MonoBehaviour , IBeginDragHandler , IDragHandler , IEndDragHandler
{
    public Transform parentToReturnTo = null;


    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Current parent: " + this.transform.parent.name);
        parentToReturnTo = this.transform.parent;
        this.transform.SetParent(this.transform.parent.parent);
        //Debug.Log("BeginDrag");
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
        //Debug.Log("Drag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(parentToReturnTo);
        Debug.Log("Returning to parent: " + parentToReturnTo.name);
        //Debug.Log("EndDrag");
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    
}

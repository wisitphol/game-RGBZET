using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Dragnew : MonoBehaviour , IBeginDragHandler , IDragHandler , IEndDragHandler
{
    public Image image;
    
    //Transform parentToReturnTo = null;
    [HideInInspector] public Transform parentAfterDrag;
    public void OnBeginDrag(PointerEventData eventData)
    {
        //parentToReturnTo = this.transform.parent;
        //this.transform.SetParent(this.transform.parent.parent);
        Debug.Log("BeginDrag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;

    }

    public void OnDrag(PointerEventData eventData)
    {
        //this.transform.position = eventData.position;
        
        Debug.Log("Drag");
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //this.transform.SetParent(parentToReturnTo);
        Debug.Log("EndDrag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
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

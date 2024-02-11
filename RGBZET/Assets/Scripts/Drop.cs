using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Drop : MonoBehaviour, IDropHandler , IPointerEnterHandler , IPointerExitHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnDrop(PointerEventData eventData)
    {
        
        Debug.Log("Drop");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
        Debug.Log("EndDrag");
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

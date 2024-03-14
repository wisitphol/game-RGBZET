using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ZETbutton : MonoBehaviour
{
    
    public static bool isZetActive = false;
    public Button zetButton;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown

    private void Start()
    {
        zetButton.interactable = true;
    }

    public void OnZetButtonPressed()
    {
        if (!isZetActive)
        {
            StartCoroutine(ActivateZetWithCooldown());
        }
    }

    private IEnumerator ActivateZetWithCooldown()
    {
        isZetActive = true;
        zetButton.interactable = false;
        Debug.Log("ZET activated by a player.");

        yield return new WaitForSeconds(cooldownTime);

        isZetActive = false;
        zetButton.interactable = true;
        Debug.Log("ZET is now available again after cooldown.");
    }
}
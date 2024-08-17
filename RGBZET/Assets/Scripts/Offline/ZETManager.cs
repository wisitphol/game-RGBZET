using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class ZETManager : MonoBehaviour
{
    
    public static bool isZETActive = false;
    public Button zetButton;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    
    public PlayerCon playercon;

    private void Start()
    {
        zetButton.interactable = true;
        
    }

    public void OnZetButtonPressed()
    {
        if (!isZETActive)
        {
            StartCoroutine(ActivateZetWithCooldown());
           
           
        }
    }

    private IEnumerator ActivateZetWithCooldown()
    {
        isZETActive = true;
        zetButton.interactable = false;
        Debug.Log("ZET activated by a player.");

        yield return new WaitForSeconds(cooldownTime);

        isZETActive = false;
        zetButton.interactable = true;
        Debug.Log("ZET is now available again after cooldown.");
    }
}
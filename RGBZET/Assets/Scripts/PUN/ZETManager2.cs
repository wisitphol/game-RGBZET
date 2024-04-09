using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class ZETManager2 : MonoBehaviourPunCallbacks
{
    
    public static bool isZETActive = false;
    public Button zetButton;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    
    public static ZETManager2 instance; // ตัวแทนสำหรับ ZETManager เพื่อให้สามารถเรียกใช้ฟังก์ชั่นจากที่อื่นได้
    private Dictionary<int, GameObject> zetTexts = new Dictionary<int, GameObject>(); // เก็บ zetText ของแต่ละผู้เล่น
    private void Awake()
    {
        instance = this;
    }
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

        GetComponent<PhotonView>().RPC("ToggleZetText", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, false);
    }

    public void RegisterZetText(int photonId, GameObject zetText)
    {
        if (!zetTexts.ContainsKey(photonId))
        {
            zetTexts.Add(photonId, zetText);
        }
    }
    [PunRPC]
    public void ToggleZetText(int photonId, bool show)
    {
        if (zetTexts.ContainsKey(photonId))
        {
            zetTexts[photonId].SetActive(show);
        }
    }
}
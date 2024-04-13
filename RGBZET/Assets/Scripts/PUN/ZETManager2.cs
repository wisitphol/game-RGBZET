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

    int photonId = PhotonNetwork.LocalPlayer.ActorNumber;

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

        int photonId = PhotonNetwork.LocalPlayer.ActorNumber; // หา photonId ของผู้เล่นที่เกี่ยวข้อง
        GetComponent<PhotonView>().RPC("ToggleZetText", RpcTarget.All, photonId, false);
    }

    public void RegisterZetText(int photonId, GameObject zetText)
    {
        if (!zetTexts.ContainsKey(photonId))
        {
            zetTexts.Add(photonId, zetText);
            Debug.Log("ZETText registered for PhotonViewID: " + photonId);
        }
        else
        {
            Debug.LogWarning("ZETText already registered for PhotonViewID: " + photonId);
        }
    }


    [PunRPC]
    public void ToggleZetText(int photonId, bool show)
    {
        // ค้นหา PhotonView ของผู้เล่นที่มี photonId ที่ระบุ
        PhotonView targetPhotonView = PhotonView.Find(photonId);
        if (targetPhotonView != null)
        {
            // หากพบ PhotonView ให้เรียกใช้เมทอด GetZetText เพื่อหา GameObject ของ ZETText
            GameObject zetTextObject = GetZetText(targetPhotonView.ViewID);
            if (zetTextObject != null)
            {
                // เปิดหรือปิด ZETText ตามค่า show
                zetTextObject.SetActive(show);
                Debug.Log("ZETText toggled for PhotonViewID: " + photonId + ". Show: " + show);
            }
            else
            {
                Debug.LogWarning("ZetText not found for PhotonViewID: " + photonId);
            }
        }
        else
        {
            Debug.LogWarning("PhotonView not found for photonId: " + photonId);
        }
    }


    public GameObject GetZetText(int photonViewID)
    {
        if (zetTexts.ContainsKey(photonViewID))
        {
            return zetTexts[photonViewID];
        }
        else
        {
            Debug.LogError("ZetText not found for PhotonViewID: " + photonViewID);
            return null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ZETManager3 : MonoBehaviourPunCallbacks
{

    public static bool isZETActive = false;
    public Button zetButton;
    public float cooldownTime = 7f;

    private PhotonView photonView3; // เพิ่มตัวแปร PhotonView

    private void Start()
    {
        zetButton.interactable = true;
        photonView3 = GetComponent<PhotonView>(); // กำหนดค่าให้กับ photonView

    }

    public void OnZetButtonPressed()
    {

        if (!isZETActive)
        {
            StartCoroutine(ActivateZetWithCooldown());
            //photonView3.RPC("RPC_ZetButtonPressed", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.ActorNumber);

            // แสดง Debug.Log เพื่อตรวจสอบว่าหมายเลข Actor ถูกส่งไปถูกต้องหรือไม่

        }
        Debug.Log("Sending RPC with player actor number: " + PhotonNetwork.LocalPlayer.ActorNumber);
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

    [PunRPC]
    private void RPC_ZetButtonPressed(int playerActorNumber)
    {
        // จัดการกับการแสดง zet ใน object player ของผู้เล่นที่กดปุ่ม ZET เท่านั้น
        GameObject playerObject = GameObject.Find("Player_" + playerActorNumber);
        if (playerObject != null && playerObject.GetComponent<PlayerController>() != null)
        {
            playerObject.GetComponent<PlayerController>().OnZetButtonPressed();
        }
    }

    [PunRPC]
    private void RPC_OnBeginDrag(Vector3 startPosition, Quaternion startRotation)
    {
        // สามารถดำเนินการต่อได้ตามต้องการ เช่น แสดงการกระทำการลากในหน้าจอผู้เล่น
        Debug.Log("Player started dragging card at position: " + startPosition + " with rotation: " + startRotation);
    }

}

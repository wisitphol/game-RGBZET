using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class MutiManage2 : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public Button zetButton;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    public static bool isZETActive = false;
    public static Player playerWhoActivatedZET = null;


    void Start()
    {
        UpdatePlayerList();

        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);


    }

    void UpdatePlayerList()
    {
        Debug.Log("UpdatePlayerList called.");

        // สร้าง array สำหรับ player gameObjects
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            // ตรวจสอบว่า index ของ playerObjects ไม่เกินจำนวนผู้เล่นในห้อง
            if (i < players.Length && playerObjects[i] != null)
            {
                // แสดง gameObject สำหรับผู้เล่น
                playerObjects[i].SetActive(true);

                // เข้าถึง PlayerCon2 component ของ playerObject
                PlayerCon2 playerCon = playerObjects[i].GetComponent<PlayerCon2>();
                if (playerCon != null)
                {
                    // ดึงชื่อผู้เล่นจาก CustomProperties หรือ NickName
                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;

                    // ตั้งค่าคะแนนของผู้เล่น
                    string score = players[i].CustomProperties.ContainsKey("score") ? players[i].CustomProperties["score"].ToString() : "score : 0";

                    // ตัวอย่างการตั้งค่า zettext (คุณอาจต้องปรับให้เหมาะสมตามความต้องการ)
                    bool zetActive = false; // ตัวอย่างการกำหนดค่า zettext, ปรับตามความต้องการ

                    // อัปเดตข้อมูลใน PlayerCon2
                    playerCon.UpdatePlayerInfo(username, score, zetActive);

                    Debug.Log($"Updating Player {i + 1}: Name={username}, Score={score}, ZetActive={zetActive}");
                }
            }
            else
            {
                // ซ่อน gameObject หากไม่มีผู้เล่น
                if (playerObjects[i] != null)
                {
                    playerObjects[i].SetActive(false);
                }
            }
        }
    }



    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        Debug.Log($"{newPlayer.NickName} player In");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
        Debug.Log($"{otherPlayer.NickName} player Out");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdatePlayerList();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string username = PlayerPrefs.GetString("username");
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "username", username }, { "isHost", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            Debug.Log($"โฮสต์: {username}");
        }
        else
        {
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "isHost", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());

        UpdatePlayerList();
    }

    public void OnZetButtonPressed()
    {
        if (photonView != null && !isZETActive)
        {
            photonView.RPC("RPC_ActivateZET", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    public void RPC_ActivateZET(int playerActorNumber)
    {
        playerWhoActivatedZET = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
        StartCoroutine(ActivateZetWithCooldown(playerActorNumber));
    }

    private IEnumerator ActivateZetWithCooldown(int playerActorNumber)
    {
        isZETActive = true;
        zetButton.interactable = false;
        Debug.Log("ZET activated.");

        // ค้นหา player object ที่สอดคล้องกับ playerActorNumber
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        PlayerCon2 activatedPlayerCon = null;
        // ตรวจสอบว่า playerObjects และ PlayerList มีขนาดที่ตรงกัน
        int playerCount = Mathf.Min(playerObjects.Length, PhotonNetwork.PlayerList.Length);

        for (int i = 0; i < playerCount; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            PlayerCon2 playerCon = playerObjects[i].GetComponent<PlayerCon2>();

            if (player.ActorNumber == playerActorNumber && playerCon != null)
            {
                // เปิดใช้งาน zettext สำหรับผู้เล่นที่กดปุ่ม ZET
                playerCon.ActivateZetText();
                activatedPlayerCon = playerCon;
            }
          //  else if (playerCon != null)
          //  {
                // ซ่อน zettext สำหรับผู้เล่นที่ไม่ได้กดปุ่ม ZET
          //      playerCon.DeactivateZetText();
          //  }
        }

        yield return new WaitForSeconds(cooldownTime);

        // ซ่อน zettext สำหรับผู้เล่นที่กดปุ่ม ZET หลังจากหมดเวลาคูลดาวน์
        if (activatedPlayerCon != null)
        {
            activatedPlayerCon.DeactivateZetText();
        }

        isZETActive = false;
        zetButton.interactable = true;
        Debug.Log("ZET is now available again after cooldown.");
    }
}

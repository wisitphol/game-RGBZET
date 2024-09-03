using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;

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
    private DatabaseReference databaseRef;
    private string roomId;


    void Start()
    {
        roomId = PlayerPrefs.GetString("RoomId");
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(roomId);

        UpdatePlayerList();

        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);

        LogServerConnectionStatus();
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
                    playerCon.SetActorNumber(players[i].ActorNumber);

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
        UpdatePlayerListInFirebase(); // อัปเดตรายชื่อผู้เล่นใน Firebase
    
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
       // UpdatePlayerListInFirebase(); 
    }

    public override void OnJoinedRoom()
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            string username = PlayerPrefs.GetString("username");
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "username", username }, { "isHost", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            Debug.Log($"host: {username}");
        }
        else
        {
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "isHost", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());

        UpdatePlayerList();
        UpdatePlayerListInFirebase(); 
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

    [PunRPC]
    public void UpdatePlayerScore(int actorNumber, int newScore)
    {
        Debug.Log($"Updating score for actorNumber: {actorNumber} with newScore: {newScore}");

        string scoreWithPrefix = "score : " + newScore.ToString();

         PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "score", scoreWithPrefix } });

        GameObject[] players = { player1, player2, player3, player4 };

        foreach (GameObject player in players)
        {
            PlayerCon2 playerComponent = player.GetComponent<PlayerCon2>();
            if (playerComponent != null && playerComponent.ActorNumber == actorNumber)
            {
                playerComponent.UpdateScore(newScore);
                Debug.Log($"Score updated for {playerComponent.NameText.text} to {newScore}");

                UpdatePlayerInfoInFirebase(actorNumber, playerComponent.NameText.text, newScore);
                break;
            }
        }
    }

    // เพิ่มฟังก์ชันนี้เพื่ออัปเดตข้อมูลผู้เล่นใน Firebase
    void UpdatePlayerInfoInFirebase(int actorNumber, string playerName, int score)
    {
        // สร้างข้อมูลผู้เล่น
        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "name", playerName },
            { "score", score }
        };

        // กำหนด path ของข้อมูลผู้เล่นใน Firebase
        string playerKey = "player_" + actorNumber;
        databaseRef.Child("players").Child(playerKey).UpdateChildrenAsync(playerData)
            .ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to update player data in Firebase.");
                }
                else
                {
                    Debug.Log($"Player data updated in Firebase: {playerName}, Score: {score}");
                }
            });
    }

    void UpdatePlayerListInFirebase()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string playerName = player.CustomProperties.ContainsKey("username") ? player.CustomProperties["username"].ToString() : player.NickName;
            int score = player.CustomProperties.ContainsKey("score") ? (int)player.CustomProperties["score"] : 0;
            UpdatePlayerInfoInFirebase(player.ActorNumber, playerName, score);
        }
    }

    private void LogServerConnectionStatus()
    {
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Currently connected to Master Server.");
        }
        else if (PhotonNetwork.InRoom)
        {
            Debug.Log("Currently connected to Game Server.");
        }
        else
        {
            Debug.Log("Not connected to Master Server or Game Server.");
        }
    }

}

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
using System.Linq;

public class MutiManage2 : MonoBehaviourPunCallbacks
{
    [SerializeField] public GameObject player1;
    [SerializeField] public GameObject player2;
    [SerializeField] public GameObject player3;
    [SerializeField] public GameObject player4;
    [SerializeField] public Button zetButton;
    public float cooldownTime = 7f; // เวลาที่ใช้ในการ cooldown
    public static bool isZETActive = false;
    public static Player playerWhoActivatedZET = null;
    private DatabaseReference databaseRef;
    private string roomId;
    private BoardCheck2 boardCheck;
    private float timer;
    public TMP_Text timerText; // เพิ่ม TextMeshProUGUI เพื่อแสดงเวลา
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    public float gameDuration; // เวลาเกมที่จะได้รับจาก CreateRoomUI
    private bool isUnlimitedTime = false; // เพิ่มตัวแปรนี้

    void Start()
    {
        roomId = PhotonNetwork.CurrentRoom.CustomProperties["roomId"].ToString();
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(roomId);

        UpdatePlayerList();
        ResetPlayerData();
        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);
        boardCheck = FindObjectOfType<BoardCheck2>(); // หา component ของ BoardCheck2

        if (PhotonNetwork.InRoom)
        {
            // ตรวจสอบว่ามีการตั้งค่าเวลาจากห้อง
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gameTime"))
            {
                int gameTimeMinutes = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameTime"];
                if (gameTimeMinutes == -1) // สมมุติว่า -1 เป็นค่าที่แสดงถึงเวลาไม่จำกัด
                {
                    isUnlimitedTime = true; // ตั้งเวลาเป็นไม่จำกัด
                    timerText.text = "NoTime"; // แสดงข้อความว่า NoTime
                }
                else
                {
                    gameDuration = gameTimeMinutes * 60f; // แปลงเป็นวินาที
                    timer = gameDuration;
                    StartCoroutine(GameTimer());
                    UpdateTimerUI();
                }
            }
            else
            {
                // Handle the case where the game time is not found (default)
                gameDuration = 120f; // ค่า default
                timer = gameDuration;
                StartCoroutine(GameTimer());
                UpdateTimerUI();
            }
        }
        else
        {
            Debug.LogError("Not in a room.");
        }
    }

    void Update()
    {
        // ตรวจสอบว่าเวลาเป็นไม่จำกัดหรือไม่
        if (!isUnlimitedTime)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                UpdateTimerUI(); // อัปเดต UI เมื่อเริ่มเกม

                if (timer <= 0)
                {
                    TimeUp();


                    GoToEndScene();

                }
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null && !isUnlimitedTime)
        {
            // แปลงเวลาที่เหลือเป็นนาทีและวินาที
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);

            // แสดงผลเวลาในรูปแบบ mm:ss
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdatePlayerList()
    {
        // Debug.Log("UpdatePlayerList called.");

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
                PlayerControl2 playerCon = playerObjects[i].GetComponent<PlayerControl2>();
                if (playerCon != null)
                {
                    playerCon.SetActorNumber(players[i].ActorNumber);

                    // ดึงชื่อผู้เล่นจาก CustomProperties หรือ NickName
                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;

                    // ตั้งค่าคะแนนของผู้เล่น
                    string score = players[i].CustomProperties.ContainsKey("score") ? players[i].CustomProperties["score"].ToString() : "0";

                    // ตัวอย่างการตั้งค่า zettext (คุณอาจต้องปรับให้เหมาะสมตามความต้องการ)
                    bool zetActive = false; // ตัวอย่างการกำหนดค่า zettext, ปรับตามความต้องการ

                    // อัปเดตข้อมูลใน PlayerCon2
                    playerCon.UpdatePlayerInfo(username, score, zetActive);

                    // อัปเดตรูปไอคอนตาม Custom Properties ของ Photon
                    if (players[i].CustomProperties.ContainsKey("iconId"))
                    {
                        int iconId = (int)players[i].CustomProperties["iconId"];
                        playerCon.UpdatePlayerIcon(iconId);
                    }

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
        if (PhotonNetwork.IsMasterClient && otherPlayer == PhotonNetwork.MasterClient)
        {
            // ถ้า MasterClient ออกจากห้อง, ให้บังคับให้ทุกคนออกจากห้องและลบข้อมูลห้องจาก Firebase
            photonView.RPC("RPC_ForceLeaveAndDestroyRoom", RpcTarget.All);
            Debug.Log($"{otherPlayer.NickName} (MasterClient) left. All players will leave and the room will be destroyed.");
        }
        else
        {
            // ถ้าไม่ใช่ MasterClient, ออกจากห้องแค่ผู้เล่นคนนั้น
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Menu");
            Debug.Log($"{otherPlayer.NickName} left the room.");
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdatePlayerList();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string username = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("username") ? PhotonNetwork.LocalPlayer.CustomProperties["username"].ToString() : "Guest";
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
        audioSource.PlayOneShot(buttonSound);

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
        //   Debug.Log("ZET activated.");

        // ค้นหา player object ที่สอดคล้องกับ playerActorNumber
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        PlayerControl2 activatedPlayerCon = null;
        // ตรวจสอบว่า playerObjects และ PlayerList มีขนาดที่ตรงกัน
        int playerCount = Mathf.Min(playerObjects.Length, PhotonNetwork.PlayerList.Length);

        for (int i = 0; i < playerCount; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            PlayerControl2 playerCon = playerObjects[i].GetComponent<PlayerControl2>();

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
        //  Debug.Log("ZET is now available again after cooldown.");
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
            PlayerControl2 playerComponent = player.GetComponent<PlayerControl2>();
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

    void ResetPlayerData()
    {
        isZETActive = false; // รีเซ็ตสถานะ isZETActive ให้เป็น false
        playerWhoActivatedZET = null; // รีเซ็ต playerWhoActivatedZET ให้เป็น null
        zetButton.interactable = true; // ทำให้ปุ่ม zet กดได้อีกครั้ง
        Debug.Log("ResetPlayerData called.");

        // รีเซ็ตข้อมูลผู้เล่นทั้งหมด
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        foreach (var playerObject in playerObjects)
        {
            if (playerObject != null)
            {
                PlayerControl2 playerCon = playerObject.GetComponent<PlayerControl2>();
                if (playerCon != null)
                {
                    // รีเซ็ตข้อมูลใน PlayerCon2
                    playerCon.ResetScore();
                    playerCon.ResetZetStatus();

                    // รีเซ็ตข้อมูลคะแนนใน CustomProperties ของ PhotonPlayer
                    int actorNumber = playerCon.ActorNumber;
                    ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "score", "score : 0" } // รีเซ็ตคะแนนเป็น "score : 0"
                };

                    PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber)?.SetCustomProperties(newProperties);
                }
            }
        }
    }

    [PunRPC]
    void RPC_ForceLeaveAndDestroyRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DestroyRoomAndReturnToMainGame());
        }
        else
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Menu");
        }
    }

    private IEnumerator DestroyRoomAndReturnToMainGame()
    {
        var task = databaseRef.RemoveValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Failed to remove room data from Firebase.");
        }
        else
        {
            Debug.Log("Room data removed from Firebase successfully.");
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene("Menu");
    }

    IEnumerator GameTimer()
    {
        // รอจนกว่าจะครบเวลาเกม
        while (timer > 0)
        {
            yield return null; // รอ frame ถัดไป
        }
    }

    void GoToEndScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Time's up! Going to EndScene.");
            StartCoroutine(WaitAndGoToEndScene());
        }
        else
        {
            return;
        }
    }

    private IEnumerator WaitAndGoToEndScene()
    {
        yield return new WaitForSeconds(1f); // หน่วงเวลา 3 วินาทีก่อนเปลี่ยน Scene
        boardCheck.photonView.RPC("RPC_LoadResult", RpcTarget.AllBuffered); // เรียกใช้ฟังก์ชัน RPC_LoadEndScene
    }

    private void TimeUp()
    {
        // ซ่อน timerText เมื่อเวลาหมด
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

}

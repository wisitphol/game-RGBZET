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

public class MutiManageT : MonoBehaviourPunCallbacks
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
    private BoardCheckT boardCheck;
//    private float gameDuration = 120f; // 5 นาทีในหน่วยวินาที (5 นาที = 300 วินาที)
    private float timer;
    public TMP_Text timerText; // เพิ่ม TextMeshProUGUI เพื่อแสดงเวลา
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        UpdatePlayerList();
        ResetPlayerData();
        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);
        boardCheck = FindObjectOfType<BoardCheckT>();

        //    timer = gameDuration;
        //    StartCoroutine(GameTimer());  ***เกี่ยวกับเวลา
        //    UpdateTimerUI(); 
    }

    /* void Update()
     {
             // ลดค่า timer ลงตามเวลาที่ผ่านไป
             timer -= Time.deltaTime;

             UpdateTimerUI(); // อัปเดต UI เมื่อเริ่มเกม
             // ตรวจสอบว่าเวลาเหลือ 0 หรือไม่
             if (timer <= 0)
             {
                 GoToEndScene(); // เรียกฟังก์ชันเปลี่ยนไปหน้า EndScene
             }

     }*/

    /* void UpdateTimerUI()
     {
         if (timerText != null)
         {
             // แปลงเวลาที่เหลือเป็นนาทีและวินาที
             int minutes = Mathf.FloorToInt(timer / 60);
             int seconds = Mathf.FloorToInt(timer % 60);

             // แสดงผลเวลาในรูปแบบ mm:ss
             timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
         }
     }*/

    void UpdatePlayerList()
    {
        // Debug.Log("UpdatePlayerList called.");

        GameObject[] playerObjects = { player1, player2, player3, player4 };
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (i < players.Length && playerObjects[i] != null)
            {
                playerObjects[i].SetActive(true);
                PlayerControlT playerCon = playerObjects[i].GetComponent<PlayerControlT>();
                if (playerCon != null)
                {
                    playerCon.SetActorNumber(players[i].ActorNumber);
                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;
                    string score = players[i].CustomProperties.ContainsKey("score") ? players[i].CustomProperties["score"].ToString() : "0";
                    bool zetActive = false; 

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
        if (otherPlayer.IsMasterClient)
        {
            Debug.Log("The player who left is the MasterClient.");
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
        else
        {
            UpdatePlayerList();
            Debug.Log($"{otherPlayer.NickName} player Out");
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
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        PlayerControlT activatedPlayerCon = null;

        int playerCount = Mathf.Min(playerObjects.Length, PhotonNetwork.PlayerList.Length);

        for (int i = 0; i < playerCount; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            PlayerControlT playerCon = playerObjects[i].GetComponent<PlayerControlT>();

            if (player.ActorNumber == playerActorNumber && playerCon != null)
            {
                playerCon.ActivateZetText();
                activatedPlayerCon = playerCon;
            }

        }

        yield return new WaitForSeconds(cooldownTime);

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
            PlayerControlT playerComponent = player.GetComponent<PlayerControlT>();
            if (playerComponent != null && playerComponent.ActorNumber == actorNumber)
            {
                playerComponent.UpdateScore(newScore);
                Debug.Log($"Score updated for {playerComponent.NameText.text} to {newScore}");
                break;
            }
        }
    } 

    void ResetPlayerData()
    {
        isZETActive = false; // รีเซ็ตสถานะ isZETActive ให้เป็น false
        playerWhoActivatedZET = null; // รีเซ็ต playerWhoActivatedZET ให้เป็น null
        zetButton.interactable = true; // ทำให้ปุ่ม zet กดได้อีกครั้ง
        Debug.Log("ResetPlayerData called.");

        GameObject[] playerObjects = { player1, player2, player3, player4 };
        foreach (var playerObject in playerObjects)
        {
            if (playerObject != null)
            {
                PlayerControlT playerCon = playerObject.GetComponent<PlayerControlT>();
                if (playerCon != null)
                {
                    playerCon.ResetScore();
                    playerCon.ResetZetStatus();

                    int actorNumber = playerCon.ActorNumber;
                    ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "score", "score : 0" } 
                };

                    PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber)?.SetCustomProperties(newProperties);
                }
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
    }

    private IEnumerator DeleteRoomAndGoToMenu()
    {
        Debug.Log("Started DeleteRoomAndGoToMenu coroutine.");

        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");  // เปลี่ยนชื่อ Scene เป็น "Menu" หรือตามที่คุณกำหนด
    }

/*    IEnumerator GameTimer()   *** เกี่ยวกับเวลา ***
    {
        yield return new WaitForSeconds(gameDuration);

        GoToEndScene();
    }*/

/*    void GoToEndScene()    *** เกี่ยวกับเวลา ***
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("time up go to EndScene.");
            boardCheck.photonView.RPC("RPC_LoadEndScene", RpcTarget.AllBuffered); // เรียกใช้ฟังก์ชัน RPC_LoadEndScene
        }
        else
        {
            return;
        }
    }*/
}

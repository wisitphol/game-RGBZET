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

public class MutimanageQ : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public Button zetButton;
    public float cooldownTime = 7f;
    public static bool isZETActive = false;
    public static Player playerWhoActivatedZET = null;
    private DatabaseReference databaseRef;
    private string roomId;
    private BoardCheckQ boardCheck;
    private float timer;
    public TMP_Text timerText;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    public float gameDuration;
    private bool isUnlimitedTime = false;

    void Start()
    {
        roomId = PhotonNetwork.CurrentRoom.CustomProperties["roomId"].ToString();
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("quickplay").Child(roomId);

        UpdatePlayerList();
        ResetPlayerData();
        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);
        boardCheck = FindObjectOfType<BoardCheckQ>();

        gameDuration = 30f * 60f; // ค่า default
        timer = gameDuration;
        StartCoroutine(GameTimer());
        UpdateTimerUI();

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
        GameObject[] playerObjects = { player1, player2, player3, player4 };
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (i < players.Length && playerObjects[i] != null)
            {
                playerObjects[i].SetActive(true);

                PlayerControlQ playerCon = playerObjects[i].GetComponent<PlayerControlQ>();
                if (playerCon != null)
                {
                    playerCon.SetActorNumber(players[i].ActorNumber);

                    string username = players[i].CustomProperties.ContainsKey("username") ? players[i].CustomProperties["username"].ToString() : players[i].NickName;

                    string score = players[i].CustomProperties.ContainsKey("score") ? players[i].CustomProperties["score"].ToString() : "0";

                    bool zetActive = false;

                    playerCon.UpdatePlayerInfo(username, score, zetActive);

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
        UpdatePlayerListInFirebase();

        Debug.Log($"{newPlayer.NickName} player In");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // ตรวจสอบว่าเป็น MasterClient คนเก่าที่ออกจากห้อง
       // if (otherPlayer.IsMasterClient)
       // {
            // ถ้า MasterClient ออกจากห้อง, ให้บังคับให้ทุกคนออกจากห้องและลบข้อมูลห้องจาก Firebase
            photonView.RPC("RPC_ForceLeaveAndDestroyRoom", RpcTarget.All);
            Debug.Log($"{otherPlayer.NickName} (MasterClient) left. All players will leave and the room will be destroyed.");
       // }
       /* else
        {
            // ถ้าไม่ใช่ MasterClient และเป็นผู้เล่นคนที่ออกจากห้องนั้นเอง จะออกจากห้องและกลับไปที่เมนู
            if (PhotonNetwork.LocalPlayer == otherPlayer)
            {
                PhotonNetwork.LeaveRoom();
                SceneManager.LoadScene("Menu");
                Debug.Log($"{otherPlayer.NickName} left the room.");
            }
            UpdatePlayerList();
        }*/
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

        GameObject[] playerObjects = { player1, player2, player3, player4 };
        PlayerControlQ activatedPlayerCon = null;
        int playerCount = Mathf.Min(playerObjects.Length, PhotonNetwork.PlayerList.Length);

        for (int i = 0; i < playerCount; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            PlayerControlQ playerCon = playerObjects[i].GetComponent<PlayerControlQ>();

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
            PlayerControlQ playerComponent = player.GetComponent<PlayerControlQ>();
            if (playerComponent != null && playerComponent.ActorNumber == actorNumber)
            {
                playerComponent.UpdateScore(newScore);
                Debug.Log($"Score updated for {playerComponent.NameText.text} to {newScore}");

                UpdatePlayerInfoInFirebase(actorNumber, playerComponent.NameText.text, newScore);
                break;
            }
        }
    }

    void UpdatePlayerInfoInFirebase(int actorNumber, string playerName, int score)
    {
        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "name", playerName },
            { "score", score }
        };

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
        isZETActive = false;
        playerWhoActivatedZET = null;
        zetButton.interactable = true;
        Debug.Log("ResetPlayerData called.");

        GameObject[] playerObjects = { player1, player2, player3, player4 };
        foreach (var playerObject in playerObjects)
        {
            if (playerObject != null)
            {
                PlayerControlQ playerCon = playerObject.GetComponent<PlayerControlQ>();
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
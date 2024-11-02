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
    //private float timer;
    public TMP_Text timerText;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    //public float gameDuration;
    //private bool isUnlimitedTime = false;

    void Start()
    {
        roomId = PlayerPrefs.GetString("RoomId");
        databaseRef = FirebaseDatabase.DefaultInstance.GetReference("quickplay").Child(roomId);

        UpdatePlayerList();
        ResetPlayerData();
        zetButton.interactable = true;
        zetButton.onClick.AddListener(OnZetButtonPressed);
        boardCheck = FindObjectOfType<BoardCheckQ>();

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
        var task = databaseRef.RemoveValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Can't delete room from Firebase.");
        }
        else
        {
            Debug.Log("Can delete room from Firebase.");
        }

        PhotonNetwork.LeaveRoom();

        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
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
        yield return new WaitForSeconds(1f);
        boardCheck.photonView.RPC("RPC_LoadResult", RpcTarget.AllBuffered);
    }

}
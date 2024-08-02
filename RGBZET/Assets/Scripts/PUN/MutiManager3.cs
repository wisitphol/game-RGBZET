using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class MutiManager3 : MonoBehaviourPunCallbacks
{
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    private ZETManager3 zetManager3;
    private PhotonView myPhotonView;

    public GameObject Cardboard;
    public GameObject Board;
    private Dictionary<int, Vector3> playerPositions = new Dictionary<int, Vector3>();
    private FirebaseAuth auth;
    private DatabaseReference usersRef;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Connected!");
           // JoinOrCreateRoom();
        }

        zetManager3 = FindObjectOfType<ZETManager3>();
        myPhotonView = GetComponent<PhotonView>();

        Debug.Log("Firebase Initialized: " + (auth != null ? "Yes" : "No"));
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server. Pun is ready for use!");
       // PhotonNetwork.JoinLobby();  // Join the default lobby
    }
    /*
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        JoinOrCreateRoom();
    }

    void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.JoinOrCreateRoom("cardsample", roomOptions, TypedLobby.Default);
    }
    */
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        
        LoadUserData(PhotonNetwork.LocalPlayer);
       
        UpdatePlayerObjectsIN();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjectsIN();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjectsOUT(otherPlayer);
    }

    void UpdatePlayerObjectsIN()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        switch (playerCount)
        {
            case 1:
                player1.SetActive(true);
                player2.SetActive(false);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
            case 2:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
            case 3:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(true);
                player4.SetActive(false);
                break;
            case 4:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(true);
                player4.SetActive(true);
                break;
            default:
                player1.SetActive(false);
                player2.SetActive(false);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (playerPositions.ContainsKey(player.ActorNumber))
            {
                UpdatePlayerPosition(player.ActorNumber, playerPositions[player.ActorNumber]);
            }
        }
    }

    void UpdatePlayerObjectsOUT(Player leftPlayer)
    {
        int actorNumber = leftPlayer.ActorNumber;

        switch (actorNumber)
        {
            case 1:
                player1.SetActive(false);
                break;
            case 2:
                player2.SetActive(false);
                break;
            case 3:
                player3.SetActive(false);
                break;
            case 4:
                player4.SetActive(false);
                break;
        }
    }

    void UpdatePlayerPosition(int actorNumber, Vector3 position)
    {
        switch (actorNumber)
        {
            case 1:
                player1.transform.position = position;
                break;
            case 2:
                player2.transform.position = position;
                break;
            case 3:
                player3.transform.position = position;
                break;
            case 4:
                player4.transform.position = position;
                break;
        }
    }

    public void OnZetButtonPressed()
    {
        if (zetManager3 != null && !ZETManager3.isZETActive && PhotonNetwork.IsConnected)
        {
            myPhotonView.RPC("RPC_ZetButtonPressed_MutiManager3", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    void RPC_ZetButtonPressed_MutiManager3(int playerActorNumber)
    {
        if (zetManager3 != null)
        {
            if (playerActorNumber == 1 && player1 != null && player1.GetComponent<PlayerController>() != null)
            {
                zetManager3.OnZetButtonPressed();
                player1.GetComponent<PlayerController>().OnZetButtonPressed();
            }
            else if (playerActorNumber == 2 && player2 != null && player2.GetComponent<PlayerController>() != null)
            {
                zetManager3.OnZetButtonPressed();
                player2.GetComponent<PlayerController>().OnZetButtonPressed();
            }
            else if (playerActorNumber == 3 && player3 != null && player3.GetComponent<PlayerController>() != null)
            {
                zetManager3.OnZetButtonPressed();
                player3.GetComponent<PlayerController>().OnZetButtonPressed();
            }
            else if (playerActorNumber == 4 && player4 != null && player4.GetComponent<PlayerController>() != null)
            {
                zetManager3.OnZetButtonPressed();
                player4.GetComponent<PlayerController>().OnZetButtonPressed();
            }
        }

        Debug.Log("Player " + playerActorNumber + " pressed the zet button.");
    }

    private void LoadUserData(Player player)
    {
        if (auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            Debug.Log("Loading user data for userId: " + userId);

            usersRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);
            usersRef.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    Debug.Log("Firebase response received. Snapshot exists: " + snapshot.Exists);

                    if (snapshot.Exists)
                    {
                        if (snapshot.HasChild("username"))
                        {
                            string playerName = snapshot.Child("username").Value.ToString();
                            Debug.Log("Username found: " + playerName);

                            myPhotonView.RPC("UpdatePlayerName", RpcTarget.AllBuffered, player.ActorNumber, playerName);
                            
                            Debug.Log("User data loaded successfully for player: " + playerName);
                        }
                        else
                        {
                            Debug.LogError("Username not found in snapshot for userId: " + userId);
                        }
                    }
                    else
                    {
                        Debug.LogError("Snapshot does not exist for userId: " + userId);
                    }
                }
                else
                {
                    Debug.LogError("Failed to get user data from Firebase for userId: " + userId);
                }
            });
        }
        else
        {
            Debug.LogError("CurrentUser is null");
        }
    }

    [PunRPC]
    private void UpdatePlayerName(int actorNumber, string username)
    {
        PlayerController playerController = null;

        if (actorNumber == 1 && player1 != null)
        {
            playerController = player1.GetComponent<PlayerController>();
        }
        else if (actorNumber == 2 && player2 != null)
        {
            playerController = player2.GetComponent<PlayerController>();
        }
        else if (actorNumber == 3 && player3 != null)
        {
            playerController = player3.GetComponent<PlayerController>();
        }
        else if (actorNumber == 4 && player4 != null)
        {
            playerController = player4.GetComponent<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.SetPlayerName(username);
        }
    }
}

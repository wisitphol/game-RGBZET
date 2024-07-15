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
            JoinOrCreateRoom();
        }

        zetManager3 = FindObjectOfType<ZETManager3>();
        myPhotonView = GetComponent<PhotonView>();

        Debug.Log("Firebase Initialized: " + (auth != null ? "Yes" : "No"));
    }

    private void AddPlayerPosition(int actorNumber, Vector3 position)
    {
        if (playerPositions.ContainsKey(actorNumber))
        {
            playerPositions[actorNumber] = position;
        }
        else
        {
            playerPositions.Add(actorNumber, position);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server. Pun is ready for use!");
        JoinOrCreateRoom();
    }

    void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.JoinOrCreateRoom("cardsample", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjectsIN();

        // Load user data for the current player
        LoadUserData(PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered room! Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        UpdatePlayerObjectsIN();
        LoadUserData(newPlayer);
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

    // เพิ่มเมธอด RPC สำหรับการรับ RPC เมื่อผู้เล่นคนใดคนหนึ่งกดปุ่ม zet
    [PunRPC]
    void RPC_ZetButtonPressed_MutiManager3(int playerActorNumber)
    {
        // ตรวจสอบว่า zetManager3 ไม่ได้เป็น null
        if (zetManager3 != null)
        {
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 1
            if (playerActorNumber == 1 && player1 != null && player1.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 1
                player1.GetComponent<PlayerController>().OnZetButtonPressed();

            }
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 2
            else if (playerActorNumber == 2 && player2 != null && player2.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 2
                player2.GetComponent<PlayerController>().OnZetButtonPressed();
            }
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 3
            else if (playerActorNumber == 3 && player3 != null && player3.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 3
                player3.GetComponent<PlayerController>().OnZetButtonPressed();
            }
            // หากผู้เล่นที่กดปุ่ม ZET เป็นผู้เล่นที่มีหมายเลข Actor เท่ากับ 4
            else if (playerActorNumber == 4 && player4 != null && player4.GetComponent<PlayerController>() != null)
            {
                // เรียกใช้งานเมธอด OnZetButtonPressed ของ ZETManager โดยตรง
                zetManager3.OnZetButtonPressed();

                // แสดง zettext สำหรับผู้เล่นที่มีหมายเลข Actor เท่ากับ 4
                player4.GetComponent<PlayerController>().OnZetButtonPressed();
            }
        }

        // แสดง Debug.Log เพื่อบันทึกว่ามีผู้เล่นกดปุ่ม ZET
        Debug.Log("Player " + playerActorNumber + " pressed the zet button.");
    }

    private void LoadUserData(Player player)
    {
        if (auth.CurrentUser != null)
        {
            string email = auth.CurrentUser.Email;
            Debug.Log("Loading user data for email: " + email);

            usersRef = FirebaseDatabase.DefaultInstance.GetReference("users");
            usersRef.OrderByChild("email").EqualTo(email).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    Debug.Log("Firebase response received. Snapshot exists: " + snapshot.Exists);

                    if (snapshot.Exists && snapshot.ChildrenCount > 0)
                    {
                        foreach (DataSnapshot userSnapshot in snapshot.Children)
                        {
                            if (userSnapshot.HasChild("username"))
                            {
                                string playerName = userSnapshot.Child("username").Value.ToString();
                                Debug.Log("Username found: " + playerName);

                                // แสดงชื่อผู้ใช้ใน PlayerController ที่เกี่ยวข้อง
                                ShowPlayerName(player.ActorNumber, playerName);

                                Debug.Log("User data loaded successfully for player: " + playerName);
                                return;
                            }
                        }
                        Debug.LogError("Username not found in snapshot for email: " + email);
                    }
                    else
                    {
                        Debug.LogError("Snapshot does not exist for email: " + email);
                    }
                }
                else
                {
                    Debug.LogError("Failed to get user data from Firebase for email: " + email);
                }
            });
        }
        else
        {
            Debug.LogError("User email is null or empty");
        }
    }

    private void ShowPlayerName(int actorNumber, string playerName)
    {
        switch (actorNumber)
        {
            case 1:
                player1.GetComponent<PlayerController>().SetPlayerName(playerName);
                break;
            case 2:
                player2.GetComponent<PlayerController>().SetPlayerName(playerName);
                break;
            case 3:
                player3.GetComponent<PlayerController>().SetPlayerName(playerName);
                break;
            case 4:
                player4.GetComponent<PlayerController>().SetPlayerName(playerName);
                break;
        }
    }
}

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
public class EndGame2 : MonoBehaviour
{
    [SerializeField] private Button backToMenu;
    // Start is called before the first frame update
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;

    private GameObject[] playerObjects;
    private PlayerResult[] playerResults;
    private DatabaseReference databaseReference;

    void Start()
    {
        backToMenu.onClick.AddListener(OnBackToMenuButtonClicked);

        playerObjects = new GameObject[] { player1, player2, player3, player4 };
        playerResults = new PlayerResult[playerObjects.Length];

        // เริ่มต้นคอมโพเนนต์ PlayerResult
        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] != null)
            {
                playerResults[i] = playerObjects[i].GetComponent<PlayerResult>();
            }
        }

        // เริ่มต้นการเชื่อมต่อ Firebase
        // ตรวจสอบให้แน่ใจว่า path นี้ตรงกับที่ใช้ใน MutiManage2
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(PlayerPrefs.GetString("RoomId"));

        // ดึงและแสดงผลลัพธ์ของผู้เล่น
        FetchPlayerDataFromFirebase();
    }

    void FetchPlayerDataFromFirebase()
    {
        // Get the current room ID from Photon or other relevant source
        string roomID = PhotonNetwork.CurrentRoom.Name;

        // Reference to the correct room in "withfriends"
        databaseReference.Child("withfriends").Child(roomID).Child("players").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve data from Firebase.");
                return;
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                List<PlayerResult> activePlayers = new List<PlayerResult>();

                int index = 0;

                foreach (DataSnapshot playerSnapshot in snapshot.Children)
                {
                    if (index >= playerResults.Length) break;

                    string playerName = playerSnapshot.Child("name").Value.ToString();
                    string playerScore = playerSnapshot.Child("score").Value.ToString();

                    Debug.Log($"Player {index + 1}: Name = {playerName}, Score = {playerScore}");

                    playerResults[index].UpdatePlayerResult(playerName, playerScore, "");

                    activePlayers.Add(playerResults[index]);
                    index++;
                }

                // Deactivate any unused player objects
                for (int i = 0; i < playerObjects.Length; i++)
                {
                    if (i < activePlayers.Count)
                    {
                        playerObjects[i].SetActive(true);
                    }
                    else
                    {
                        playerObjects[i].SetActive(false);
                    }
                }

                // Sort players by score (assuming the score is an integer)
                activePlayers.Sort((a, b) =>
                {
                    int scoreA = int.Parse(a.ScoreText.text);
                    int scoreB = int.Parse(b.ScoreText.text);

                    return scoreB.CompareTo(scoreA); // Descending order
                });

                // Set win status for the player with the highest score
                if (activePlayers.Count > 0)
                {
                    activePlayers[0].UpdatePlayerResult(activePlayers[0].NameText.text, activePlayers[0].ScoreText.text, "Winner");
                }
            }
        });
    }



    private void OnBackToMenuButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ถ้าผู้เล่นเป็น host, ลบข้อมูลห้องและเปลี่ยนไปยังเมนู
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
        else
        {
            // ถ้าผู้เล่นไม่ใช่ host, เปลี่ยนไปยังเมนูทันที
            SceneManager.LoadScene("menu");
        }
    }

    private IEnumerator DeleteRoomAndGoToMenu()
    {
        // ลบข้อมูลห้องจาก Firebase
        var task = databaseReference.RemoveValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Can't delete room from Firebase.");
        }
        else
        {
            Debug.Log("Can delete room from Firebase.");
        }

        // ออกจากห้อง Photon
        PhotonNetwork.LeaveRoom();

        // เปลี่ยนไปยังเมนูหลังจากออกจากห้อง
        yield return new WaitForSeconds(1f); // รอเพื่อให้แน่ใจว่าผู้เล่นออกจากห้องแล้ว

        SceneManager.LoadScene("menu");
    }
}

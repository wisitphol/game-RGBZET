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
                Debug.Log($"player{i + 1} is not null. Checking for PlayerResult component.");
                playerResults[i] = playerObjects[i].GetComponent<PlayerResult>();

                if (playerResults[i] != null)
                {
                    Debug.Log($"PlayerResult component found on player{i + 1}.");
                }
                else
                {
                    Debug.LogWarning($"PlayerResult component NOT found on player{i + 1}.");
                }
            }
            else
            {
                Debug.LogWarning($"player{i + 1} is null.");
            }
        }

        // เริ่มต้นการเชื่อมต่อ Firebase
        // ตรวจสอบให้แน่ใจว่า path นี้ตรงกับที่ใช้ใน MutiManage2
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(PlayerPrefs.GetString("RoomId"));

        FetchPlayerDataFromPhoton();

    }

    /*  void FetchPlayerDataFromFirebase()
      {
          string roomID = PhotonNetwork.CurrentRoom.Name;
          Debug.Log("Room ID: " + roomID); // ตรวจสอบ Room ID

          databaseReference.Child(roomID).Child("players").GetValueAsync().ContinueWith(task =>
          {
              if (task.IsFaulted)
              {
                  Debug.LogError("Failed to retrieve data from Firebase.");
                  return;
              }
              else if (task.IsCompleted)
              {
                  DataSnapshot snapshot = task.Result;
                  Debug.Log("Data retrieved from Firebase."); // ตรวจสอบการดึงข้อมูลสำเร็จ

                  // Clear all player objects first
                  foreach (var playerObject in playerObjects)
                  {
                      playerObject.SetActive(false);
                  }

                  int index = 0;

                  foreach (DataSnapshot playerSnapshot in snapshot.Children)
                  {
                      if (index >= playerResults.Length) break;

                      string playerName = playerSnapshot.Child("name").Value.ToString();
                      string playerScore = playerSnapshot.Child("score").Value.ToString();

                      Debug.Log($"Player {index + 1}: Name = {playerName}, Score = {playerScore}");

                      // Update player results and set active
                      playerResults[index].UpdatePlayerResult(playerName, playerScore, "");
                      playerObjects[index].SetActive(true);

                      index++;
                  }

                  // Sort players by score
                  List<PlayerResult> activePlayers = new List<PlayerResult>(playerResults);
                  activePlayers.Sort((a, b) =>
                  {
                      int scoreA = int.Parse(a.ScoreText.text);
                      int scoreB = int.Parse(b.ScoreText.text);
                      return scoreB.CompareTo(scoreA); // Descending order
                  });

                  // Mark the player with the highest score as the winner
                  if (activePlayers.Count > 0)
                  {
                      activePlayers[0].UpdatePlayerResult(activePlayers[0].NameText.text, activePlayers[0].ScoreText.text, "Winner");
                  }
                  else
                  {
                      Debug.Log("No active players found.");
                  }
              }
          });
      }*/

    void FetchPlayerDataFromPhoton()
    {
        Debug.Log("Fetching player data from Photon.");

        // Clear all player objects first
        foreach (var playerObject in playerObjects)
        {
            playerObject.SetActive(false);
        }

        Player[] players = PhotonNetwork.PlayerList;
        int index = 0;
        int highestScore = int.MinValue;
        List<int> highestScoreIndices = new List<int>();

        foreach (Player player in players)
        {
            if (index >= playerResults.Length) break;

            string playerName = player.CustomProperties.ContainsKey("username") ? player.CustomProperties["username"].ToString() : player.NickName;
            string playerScoreStr = player.CustomProperties.ContainsKey("score") ? player.CustomProperties["score"].ToString() : "0";

            // Clean score string and parse it to integer
            int playerScore;
            string cleanScoreStr = System.Text.RegularExpressions.Regex.Replace(playerScoreStr, @"\D", ""); // Remove non-digit characters

            if (!int.TryParse(cleanScoreStr, out playerScore))
            {
                Debug.LogError($"Failed to parse score '{playerScoreStr}' for player '{playerName}'. Defaulting to 0.");
                playerScore = 0;
            }

            Debug.Log($"Player {index + 1}: Name = {playerName}, Score = {playerScore}");

            // Check if this player has the highest score
            if (playerScore > highestScore)
            {
                highestScore = playerScore;
                highestScoreIndices.Clear();
                highestScoreIndices.Add(index);
            }
            else if (playerScore == highestScore)
            {
                highestScoreIndices.Add(index);
            }

            // Update player results and set active
            playerResults[index].UpdatePlayerResult(playerName, playerScore.ToString(), "");
            playerObjects[index].SetActive(true);

            index++;
        }

        // Mark all players with the highest score as winners
        foreach (int i in highestScoreIndices)
        {
            playerResults[i].UpdatePlayerResult(playerResults[i].NameText.text, playerResults[i].ScoreText.text, "Winner");
        }
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

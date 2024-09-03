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
    private FirebaseUserId firebaseUserId;

    void Start()
    {
        backToMenu.onClick.AddListener(OnBackToMenuButtonClicked);

        firebaseUserId = FindObjectOfType<FirebaseUserId>();
        if (firebaseUserId == null)
        {
            Debug.LogError("FirebaseUserId script not found in the scene.");
        }
        else
        {
            Debug.Log("FirebaseUserId script found successfully.");
        }

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
            playerResults[index].UpdatePlayerResult(playerName, "score: " + playerScore.ToString(), "");
            playerObjects[index].SetActive(true);

            index++;
        }

        // Mark all players with the highest score as winners
        foreach (int i in highestScoreIndices)
        {
            playerResults[i].UpdatePlayerResult(playerResults[i].NameText.text, playerResults[i].ScoreText.text, "Winner");
        }

       // UpdateGameResultsInDatabase();
    }

    void UpdateGameResultsInDatabase()
    {
        Debug.Log("Updating game results in database.");

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            string userId = players[i].CustomProperties.ContainsKey("FirebaseUserId") ? players[i].CustomProperties["FirebaseUserId"].ToString() : null;

            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning($"UserId is null or empty for player {i}, skipping update.");
                continue;
            }
            else
            {
                Debug.Log($"FirebaseUserId for player {i}: {userId}");
            }

            DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);

            // เพิ่มจำนวนเกมที่เล่นโดยใช้ Transaction
            userRef.Child("gamescount").RunTransaction(mutableData =>
            {
                int currentGamesPlayed = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                mutableData.Value = currentGamesPlayed + 1;
                return TransactionResult.Success(mutableData);
            }).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Game count updated successfully.");
                }
                else
                {
                    Debug.LogError("Failed to update game count.");
                }
            });

            // ตรวจสอบว่าผู้เล่นคนนี้เป็นผู้ชนะหรือไม่
            bool isWinner = false;

            if (playerResults[i] != null && playerResults[i].WinText.text == "Winner")
            {
                isWinner = true;
            }

            // อัปเดตจำนวนชัยชนะหรือการแพ้
            if (isWinner)
            {
                userRef.Child("gameswin").RunTransaction(mutableData =>
                {
                    int currentWins = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                    mutableData.Value = currentWins + 1;
                    return TransactionResult.Success(mutableData);
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Wins count updated successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to update wins count.");
                    }
                });
            }
            else
            {
                userRef.Child("gameslose").RunTransaction(mutableData =>
                {
                    int currentLosses = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                    mutableData.Value = currentLosses + 1;
                    return TransactionResult.Success(mutableData);
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Losses count updated successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to update losses count.");
                    }
                });
            }
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

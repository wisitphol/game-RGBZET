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
public class SumT : MonoBehaviour
{
    [SerializeField] private Button backToMenu;
    // Start is called before the first frame update


    public GameObject playervictory;
    private PlayerResultT playerResult;
    private DatabaseReference databaseReference;
    private FirebaseUserId firebaseUserId;
    private string tournamentId;
    private string currentMatchId;
    [SerializeField] public AudioSource audioSource;  // ตัวแปร AudioSource ที่จะเล่นเสียง
    [SerializeField] public AudioClip endgameSound;
    [SerializeField] public AudioClip buttonSound;
    // Start is called before the first frame update
    void Start()
    {
        backToMenu.interactable = false;
        StartCoroutine(EnableBackButtonAfterDelay(3f));
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

        tournamentId = PlayerPrefs.GetString("TournamentId");
        currentMatchId = PlayerPrefs.GetString("CurrentMatchId");
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        if (playervictory != null)
        {
            playerResult = playervictory.GetComponent<PlayerResultT>();

            if (playerResult != null)
            {
                Debug.Log("PlayerResult component found on player1.");
            }
            else
            {
                Debug.LogWarning("PlayerResult component NOT found on player1.");
            }
        }
        else
        {
            Debug.LogWarning("player1 is null.");
        }

        StartCoroutine(DelayedUpdateGameResults());

        if (audioSource != null && endgameSound != null)
        {
            audioSource.PlayOneShot(endgameSound);
        }
    }

    void FetchPlayerDataFromPhoton()
    {
        Debug.Log("Fetching player data from Photon.");

        playervictory.SetActive(false);

        Player[] players = PhotonNetwork.PlayerList;

        if (players.Length > 0)
        {
            Player player = players[0]; // ใช้ข้อมูลจาก player1 เท่านั้น

            string playerName = player.CustomProperties.ContainsKey("username") ? player.CustomProperties["username"].ToString() : player.NickName;
            string playerScoreStr = player.CustomProperties.ContainsKey("score") ? player.CustomProperties["score"].ToString() : "0";

            int playerScore;
            string cleanScoreStr = System.Text.RegularExpressions.Regex.Replace(playerScoreStr, @"\D", "");

            if (!int.TryParse(cleanScoreStr, out playerScore))
            {
                Debug.LogError($"Failed to parse score '{playerScoreStr}' for player '{playerName}'. Defaulting to 0.");
                playerScore = 0;
            }

            Debug.Log($"Player 1: Name = {playerName}, Score = {playerScore}");

            if (player.CustomProperties.ContainsKey("iconId"))
            {
                int iconId = (int)player.CustomProperties["iconId"];
                playerResult.UpdatePlayerIcon(iconId); // อัปเดตรูปภาพ
            }

            playerResult.UpdatePlayerResult(playerName, " ", "WINNER");
            playervictory.SetActive(true);
        }
    }

     IEnumerator DelayedUpdateGameResults()
    {
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateGameResultsInDatabase();
        }
    }

    void UpdateGameResultsInDatabase()
    {
        Debug.Log("Updating game results in database.");

        Player player = PhotonNetwork.PlayerList[0]; // ใช้ข้อมูลจาก player1 เท่านั้น

        string userId = player.CustomProperties.ContainsKey("FirebaseUserId") ? player.CustomProperties["FirebaseUserId"].ToString() : null;

        if (!string.IsNullOrEmpty(userId))
        {
            Debug.Log($"FirebaseUserId for player 1: {userId}");

            DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);

            // เพิ่มจำนวนเกมที่เล่นโดยใช้ Transaction
            userRef.Child("gamescount").RunTransaction(mutableData =>
            {
                int currentGamesPlayed = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                mutableData.Value = currentGamesPlayed + 1;
                return TransactionResult.Success(mutableData);
            });

            // อัปเดตจำนวนชัยชนะ
            userRef.Child("gameswintournament").RunTransaction(mutableData =>
            {
                int currentWins = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                mutableData.Value = currentWins + 1;
                return TransactionResult.Success(mutableData);
            });
        }
    }



    IEnumerator EnableBackButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        backToMenu.interactable = true;  // Enable the button after delay
    }
    private void OnBackToMenuButtonClicked()
    {
        audioSource.PlayOneShot(buttonSound);

        // รอเวลาให้เสียงเล่นจบก่อนเปลี่ยน Scene
        StartCoroutine(WaitAndGoToMenu(1.0f)); // 1.0f คือเวลาของเสียงที่ต้องการให้เล่นจบ
    }

    private IEnumerator WaitAndGoToMenu(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        StartCoroutine(DeleteRoomAndGoToMenu());

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

        // ตรวจสอบสถานะการเชื่อมต่อกับ Game Server
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        // ยกเลิกการเชื่อมต่อจาก Game Server
        PhotonNetwork.Disconnect();

        // ตรวจสอบสถานะการยกเลิกการเชื่อมต่อจาก Game Server
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
    }


}

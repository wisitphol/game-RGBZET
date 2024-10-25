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
    private DatabaseReference databaseReference2;
    private FirebaseUserId firebaseUserId;
    private string tournamentId;
    [SerializeField] public AudioSource audioSource;  // ตัวแปร AudioSource ที่จะเล่นเสียง
    [SerializeField] public AudioClip endgameSound;
    [SerializeField] public AudioClip buttonSound;
    // Start is called before the first frame update
    void Start()
    {
        if (backToMenu != null)
        {
            backToMenu.interactable = false;
            StartCoroutine(EnableBackButtonAfterDelay(3f));
            backToMenu.onClick.AddListener(OnBackToMenuButtonClicked);
        }
        else
        {
            Debug.LogWarning("backToMenu button is not assigned.");
        }

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
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        string userId = AuthManager.Instance.GetCurrentUserId();
        databaseReference2 = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);

        if (playervictory != null)
        {
            playerResult = playervictory.GetComponent<PlayerResultT>();

            if (playerResult != null)
            {
                Debug.Log("PlayerResult component found on playervictory.");
            }
            else
            {
                Debug.LogWarning("PlayerResult component NOT found on player1.");
            }
        }
        else
        {
            Debug.LogWarning("playervictory is null.");
        }

        StartCoroutine(FetchPlayerData());

        StartCoroutine(DelayedUpdateGameResults());

        if (audioSource != null && endgameSound != null)
        {
            audioSource.PlayOneShot(endgameSound);
        }
    }

    private IEnumerator FetchPlayerData()
    {
        var task = databaseReference2.GetValueAsync();  // ใช้ databaseReference2 ที่ตั้งค่า userId ไว้แล้ว
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCompleted && !task.IsFaulted && task.Result != null)
        {
            // ดึงข้อมูลผู้เล่นจาก Firebase
            var playerData = task.Result;
            string playerName = playerData.Child("username")?.Value?.ToString();
            int iconId = playerData.Child("icon") != null ? int.Parse(playerData.Child("icon").Value.ToString()) : 0;

            Debug.Log($"Player Name: {playerName}, Icon ID: {iconId}");

            // อัปเดตข้อมูลใน playerResult
            if (playerResult != null)
            {
                playerResult.UpdatePlayerResult(playerName, " ", "WINNER");
                playerResult.UpdatePlayerIcon(iconId);
                playervictory.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Failed to fetch player data or data is null.");
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

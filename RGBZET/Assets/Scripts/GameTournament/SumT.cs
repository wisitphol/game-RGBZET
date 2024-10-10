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

    private GameObject[] playerObjects;
    private PlayerResult2[] playerResults;
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

        tournamentId = PlayerPrefs.GetString("TournamentId");
        currentMatchId = PlayerPrefs.GetString("CurrentMatchId");
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        
        if (audioSource != null && endgameSound != null)
        {
            audioSource.PlayOneShot(endgameSound);
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

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
        else
        {
            StartCoroutine(LeaveRoomAndCheckConnection());
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

        // ตรวจสอบสถานะการเชื่อมต่อกับ Game Server
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        // ยกเลิกการเชื่อมต่อจาก Game Server
        PhotonNetwork.Disconnect();

        // ตรวจสอบสถานะการยกเลิกการเชื่อมต่อจาก Game Server
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
    }

    private IEnumerator LeaveRoomAndCheckConnection()
    {
        PhotonNetwork.LeaveRoom();

        // รอให้การออกจากห้องเสร็จสมบูรณ์
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        // ยกเลิกการเชื่อมต่อจาก Game Server
        PhotonNetwork.Disconnect();

        // ตรวจสอบสถานะการยกเลิกการเชื่อมต่อจาก Game Server
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        // เปลี่ยนไปยังเมนู
        SceneManager.LoadScene("Menu");


    }
}

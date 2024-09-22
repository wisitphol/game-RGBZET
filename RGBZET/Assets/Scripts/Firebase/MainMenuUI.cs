using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createWithFriendButton;
    [SerializeField] private Button createTournamentButton;
    [SerializeField] private Button createQuickplayButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button profileButton;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    private DatabaseReference userRef;
    private string currentUsername;

    void Start()
    {
        if (AuthManager.Instance.IsUserLoggedIn())
        {
            string userId = AuthManager.Instance.GetCurrentUserId();
            userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);
            LoadUserData();
        }
        else
        {
            SceneManager.LoadScene("Login");
            return;
        }

        joinButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Joinroom")));
        createQuickplayButton.onClick.AddListener(() => SoundOnClick(() => {/* TODO: Implement Quickplay functionality */}));
        createWithFriendButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("CreateFriend")));
        createTournamentButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Tournament")));
        logoutButton.onClick.AddListener(() => SoundOnClick(OnLogoutButtonClicked));


       profileButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Profile")));

    }

    void LoadUserData()
    {
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    currentUsername = snapshot.Child("username").Value.ToString();
                    usernameText.text = "Welcome, " + currentUsername;
                }
                else
                {
                    DisplayFeedback("Failed to load user data.");
                }
            }
            else
            {
                DisplayFeedback("Failed to load user data.");
            }
        });
    }

    void OnLogoutButtonClicked()
    {
        AuthManager.Instance.Logout();
        // Logout method in AuthManager already handles scene transition
    }
       
    void SoundOnClick(System.Action buttonAction)
    {
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
            // รอให้เสียงเล่นเสร็จก่อนที่จะทำการเปลี่ยน scene
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            // ถ้าไม่มีเสียงให้เล่น ให้ทำงานทันที
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        // รอความยาวของเสียงก่อนที่จะทำงาน
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createWithFriendButton;
    [SerializeField] private Button createTournamentButton;
    [SerializeField] private Button createQuickplayButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button proButton;

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

        joinButton.onClick.AddListener(() => SceneManager.LoadScene("Joinroom"));
        createQuickplayButton.onClick.AddListener(() => {/* TODO: Implement Quickplay functionality */});
        createWithFriendButton.onClick.AddListener(() => SceneManager.LoadScene("CreateFriend"));
        createTournamentButton.onClick.AddListener(() => SceneManager.LoadScene("Tournament"));
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);


        proButton.onClick.AddListener(() => SceneManager.LoadScene("Profile"));
        
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

    

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }
}

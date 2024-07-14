using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class MainMenuUI : MonoBehaviour
{
    public Text usernameText;
    public Button joinTournamentButton;
    public Button createTournamentButton;
    public Button logoutButton;
    public Text feedbackText;

    private FirebaseAuth auth;
    private DatabaseReference userRef;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(auth.CurrentUser.UserId);
            LoadUserData();
        }

        joinTournamentButton.onClick.AddListener(() => 
        {
            SceneManager.LoadScene("JoinTournament");
        });
        createTournamentButton.onClick.AddListener(() => 
        {
            SceneManager.LoadScene("TournamentCreation");
        });
        logoutButton.onClick.AddListener(() => 
        {
            AuthManager.Instance.Logout();
            SceneManager.LoadScene("Login");
        });
    }

    void LoadUserData()
    {
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string username = snapshot.Child("username").Value.ToString();
                    usernameText.text = "Welcome, " + username;
                }
                else
                {
                    feedbackText.text = "Failed to load user data.";
                }
            }
            else
            {
                feedbackText.text = "Failed to load user data.";
            }
        });
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }
}

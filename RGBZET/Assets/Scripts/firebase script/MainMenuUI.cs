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
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private Button editUsernameButton;
    [SerializeField] private Button saveUsernameButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createWithFriendButton;
    [SerializeField] private Button createTournamentButton;
    [SerializeField] private Button createQuickplayButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TMP_Text feedbackText;

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

        editUsernameButton.onClick.AddListener(OnEditUsernameButtonClicked);
        saveUsernameButton.onClick.AddListener(OnSaveUsernameButtonClicked);

        // Initially hide the username input field and save button
        usernameInputField.gameObject.SetActive(false);
        saveUsernameButton.gameObject.SetActive(false);
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

    void OnEditUsernameButtonClicked()
    {
        usernameText.gameObject.SetActive(false);
        editUsernameButton.gameObject.SetActive(false);
        usernameInputField.gameObject.SetActive(true);
        saveUsernameButton.gameObject.SetActive(true);
        usernameInputField.text = currentUsername;
    }

    async void OnSaveUsernameButtonClicked()
    {
        string newUsername = usernameInputField.text.Trim();
        if (string.IsNullOrEmpty(newUsername))
        {
            DisplayFeedback("Username cannot be empty.");
            return;
        }

        if (newUsername == currentUsername)
        {
            CancelUsernameEdit();
            return;
        }

        bool isAvailable = await CheckUsernameAvailability(newUsername);
        if (!isAvailable)
        {
            DisplayFeedback("This username is already taken.");
            return;
        }

        UpdateUsername(newUsername);
    }

    async Task<bool> CheckUsernameAvailability(string username)
    {
        var snapshot = await FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .OrderByChild("username")
            .EqualTo(username)
            .GetValueAsync();

        return !snapshot.Exists;
    }

    void UpdateUsername(string newUsername)
    {
        AuthManager.Instance.UpdateUserProfile(newUsername, (success) => 
        {
            if (success)
            {
                userRef.Child("username").SetValueAsync(newUsername).ContinueWith(task => 
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        currentUsername = newUsername;
                        usernameText.text = "Welcome, " + newUsername;
                        DisplayFeedback("Username updated successfully.");
                        CancelUsernameEdit();
                    }
                    else
                    {
                        DisplayFeedback("Failed to update username in the database.");
                    }
                });
            }
            else
            {
                DisplayFeedback("Failed to update username.");
            }
        });
    }

    void CancelUsernameEdit()
    {
        usernameText.gameObject.SetActive(true);
        editUsernameButton.gameObject.SetActive(true);
        usernameInputField.gameObject.SetActive(false);
        saveUsernameButton.gameObject.SetActive(false);
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }
}

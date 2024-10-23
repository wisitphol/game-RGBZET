using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class AuthManager : MonoBehaviour
{
    // Singleton instance
    public static AuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference databaseRef;
    private bool isQuitting = false;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Firebase components
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        Application.quitting += OnApplicationQuit;
    }

    // Called when the application is quitting
    void OnApplicationQuit()
    {
        isQuitting = true;
        if (auth.CurrentUser != null)
        {
            SetUserLoginStatusImmediate(auth.CurrentUser.UserId, false);
        }
    }

    // Called when the behaviour becomes disabled or inactive
    void OnDisable()
    {
        if (isQuitting && auth.CurrentUser != null)
        {
            SetUserLoginStatusImmediate(auth.CurrentUser.UserId, false);
        }
    }

    // Login method
    public void Login(string email, string password, LoginUI loginUI)
    {
        StartCoroutine(LoginCoroutine(email, password, loginUI));
    }

    // Coroutine for login process
    private IEnumerator LoginCoroutine(string email, string password, LoginUI loginUI)
    {
        // Attempt to sign in
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            // Login failed
            loginUI.DisplayFeedback("Login failed. Please try again.");
        }
        else
        {
            // Login successful
            user = loginTask.Result.User;

            // Check if user is already logged in elsewhere
            var userStatusTask = CheckUserLoginStatus(user.UserId);
            yield return new WaitUntil(() => userStatusTask.IsCompleted);

            if (userStatusTask.Result)
            {
                loginUI.DisplayFeedback("Account already logged in elsewhere.");
                auth.SignOut();
            }
            else
            {
                // Set user status to logged in
                yield return StartCoroutine(SetUserLoginStatus(user.UserId, true));

                loginUI.DisplayFeedback("Login successful!");
                SaveUserData(user);
                SceneManager.LoadScene("Menu");
            }
        }
    }

    // Registration method
    public void Register(string email, string password, string username, RegisterUI registerUI)
    {
        StartCoroutine(RegisterCoroutine(email, password, username, registerUI));
    }

    // Coroutine for registration process
    private IEnumerator RegisterCoroutine(string email, string password, string username, RegisterUI registerUI)
    {
        // Attempt to create user
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            // Registration failed
            registerUI.ShowNotification("Registration failed");
        }
        else
        {
            // Registration successful
            var authResult = registerTask.Result;
            user = authResult.User;
            UserProfile profile = new UserProfile { DisplayName = username };

            // Update user profile with username
            var updateProfileTask = user.UpdateUserProfileAsync(profile);
            yield return new WaitUntil(() => updateProfileTask.IsCompleted);

            if (updateProfileTask.Exception != null)
            {
               registerUI.ShowNotification("Failed to set username");
            }
            else
            {
                // Save additional user data to database
                DatabaseReference userRef = databaseRef.Child("users").Child(user.UserId);
                userRef.Child("username").SetValueAsync(username);
                userRef.Child("email").SetValueAsync(email);
                userRef.Child("isLoggedIn").SetValueAsync(true);

                registerUI.ShowNotification("Registration successful!");
                SaveUserData(user);
                SceneManager.LoadScene("Menu");  // Changed from "Login" to "Menu"
            }
        }
    }

    // Logout method
    public void Logout()
    {
        if (auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            StartCoroutine(SetUserLoginStatus(userId, false));
        }
        auth.SignOut();
        user = null;
        SceneManager.LoadScene("Login");
    }

    // Check user login status
    private async Task<bool> CheckUserLoginStatus(string userId)
    {
        var snapshot = await databaseRef.Child("users").Child(userId).Child("isLoggedIn").GetValueAsync();
        return snapshot.Exists && (bool)snapshot.Value;
    }

    // Set user login status
    private IEnumerator SetUserLoginStatus(string userId, bool status)
    {
        var task = databaseRef.Child("users").Child(userId).Child("isLoggedIn").SetValueAsync(status);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to set user login status: {task.Exception.Message}");
        }
    }

    // Set user login status immediately (used when quitting)
    private void SetUserLoginStatusImmediate(string userId, bool status)
    {
        databaseRef.Child("users").Child(userId).Child("isLoggedIn").SetValueAsync(status).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to set user login status: {task.Exception.Message}");
            }
        });
    }

    // Save user data to database
    private void SaveUserData(FirebaseUser user)
    {
        DatabaseReference userRef = databaseRef.Child("users").Child(user.UserId);
        userRef.Child("email").SetValueAsync(user.Email);
        userRef.Child("username").SetValueAsync(user.DisplayName);
    }

    // Get current user ID
    public string GetCurrentUserId()
    {
        return auth.CurrentUser != null ? auth.CurrentUser.UserId : null;
    }

    // Get current username
    public string GetCurrentUsername()
    {
        if (auth.CurrentUser != null)
        {
            return auth.CurrentUser.DisplayName ?? auth.CurrentUser.Email;
        }
        return null;
    }

    // Get user ID by username
    public async Task<string> GetUserIdByUsername(string username)
    {
        var snapshot = await databaseRef.Child("users").OrderByChild("username").EqualTo(username).GetValueAsync();
        if (snapshot.Exists && snapshot.ChildrenCount > 0)
        {
            return snapshot.Children.First().Key;
        }
        return null;
    }

    // Get username by user ID
    public async Task<string> GetUsernameById(string userId)
    {
        var snapshot = await databaseRef.Child("users").Child(userId).Child("username").GetValueAsync();
        if (snapshot.Exists)
        {
            return snapshot.Value.ToString();
        }
        return null;
    }

    // Check if user is logged in
    public bool IsUserLoggedIn()
    {
        return auth.CurrentUser != null;
    }

    // Reset password
    public void ResetPassword(string email, System.Action<bool> callback)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            callback(!task.IsCanceled && !task.IsFaulted);
        });
    }

    // Update user profile
    public void UpdateUserProfile(string newUsername, System.Action<bool> callback)
    {
        if (auth.CurrentUser != null)
        {
            UserProfile profile = new UserProfile { DisplayName = newUsername };
            auth.CurrentUser.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (!task.IsCanceled && !task.IsFaulted)
                {
                    // Update username in the database as well
                    databaseRef.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(newUsername)
                        .ContinueWith(dbTask =>
                        {
                            callback(!dbTask.IsCanceled && !dbTask.IsFaulted);
                        });
                }
                else
                {
                    callback(false);
                }
            });
        }
        else
        {
            callback(false);
        }
    }
}
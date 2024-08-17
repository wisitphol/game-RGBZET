using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference databaseRef;

    void Awake()
    {
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

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Login(string email, string password, LoginUI loginUI)
    {
        StartCoroutine(LoginCoroutine(email, password, loginUI));
    }

    private IEnumerator LoginCoroutine(string email, string password, LoginUI loginUI)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            loginUI.DisplayFeedback($"Failed to login: {loginTask.Exception.Message}");
        }
        else
        {
            user = loginTask.Result.User;
            
            // Check if the user is already logged in
            var userStatusTask = CheckUserLoginStatus(user.UserId);
            yield return new WaitUntil(() => userStatusTask.IsCompleted);

            if (userStatusTask.Result)
            {
                loginUI.DisplayFeedback("This account is already logged in elsewhere.");
                auth.SignOut();
            }
            else
            {
                // Set user status to logged in
                yield return StartCoroutine(SetUserLoginStatus(user.UserId, true));

                loginUI.DisplayFeedback($"Logged in successfully: {user.Email}");
                SaveUserData(user);
                SceneManager.LoadScene("menu");
            }
        }
    }

    public void Register(string email, string password, string username, RegisterUI registerUI)
    {
        StartCoroutine(RegisterCoroutine(email, password, username, registerUI));
    }

    private IEnumerator RegisterCoroutine(string email, string password, string username, RegisterUI registerUI)
    {
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            registerUI.DisplayFeedback($"Failed to register: {registerTask.Exception.Message}");
        }
        else
        {
            var authResult = registerTask.Result;
            user = authResult.User;
            UserProfile profile = new UserProfile { DisplayName = username };

            var updateProfileTask = user.UpdateUserProfileAsync(profile);
            yield return new WaitUntil(() => updateProfileTask.IsCompleted);

            if (updateProfileTask.Exception != null)
            {
                registerUI.DisplayFeedback($"Failed to update profile: {updateProfileTask.Exception.Message}");
            }
            else
            {
                DatabaseReference userRef = databaseRef.Child("users").Child(user.UserId);
                userRef.Child("username").SetValueAsync(username);
                userRef.Child("email").SetValueAsync(email);
                userRef.Child("isLoggedIn").SetValueAsync(false);

                registerUI.DisplayFeedback($"Registered successfully: {user.Email}");
                SceneManager.LoadScene("Login");
            }
        }
    }

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

    private async Task<bool> CheckUserLoginStatus(string userId)
    {
        var snapshot = await databaseRef.Child("users").Child(userId).Child("isLoggedIn").GetValueAsync();
        return snapshot.Exists && (bool)snapshot.Value;
    }

    private IEnumerator SetUserLoginStatus(string userId, bool status)
    {
        var task = databaseRef.Child("users").Child(userId).Child("isLoggedIn").SetValueAsync(status);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to set user login status: {task.Exception.Message}");
        }
    }

    private void SaveUserData(FirebaseUser user)
    {
        DatabaseReference userRef = databaseRef.Child("users").Child(user.UserId);
        userRef.Child("email").SetValueAsync(user.Email);
        userRef.Child("username").SetValueAsync(user.DisplayName);
    }

    public string GetCurrentUserId()
    {
        return auth.CurrentUser != null ? auth.CurrentUser.UserId : null;
    }

    public string GetCurrentUsername()
    {
        return auth.CurrentUser != null ? auth.CurrentUser.DisplayName : null;
    }

    public async Task<string> GetUserIdByUsername(string username)
    {
        var snapshot = await databaseRef.Child("users").OrderByChild("username").EqualTo(username).GetValueAsync();
        if (snapshot.Exists && snapshot.ChildrenCount > 0)
        {
            return snapshot.Children.First().Key;
        }
        return null;
    }

    public async Task<string> GetUsernameById(string userId)
    {
        var snapshot = await databaseRef.Child("users").Child(userId).Child("username").GetValueAsync();
        if (snapshot.Exists)
        {
            return snapshot.Value.ToString();
        }
        return null;
    }

    public bool IsUserLoggedIn()
    {
        return auth.CurrentUser != null;
    }

    public void ResetPassword(string email, System.Action<bool> callback)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback(false);
            }
            else
            {
                callback(true);
            }
        });
    }

    public void UpdateUserProfile(string newUsername, System.Action<bool> callback)
    {
        if (auth.CurrentUser != null)
        {
            UserProfile profile = new UserProfile { DisplayName = newUsername };
            auth.CurrentUser.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled || task.IsFaulted)
                {
                    callback(false);
                }
                else
                {
                    // Update username in the database as well
                    databaseRef.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(newUsername)
                        .ContinueWith(dbTask => {
                            callback(!dbTask.IsCanceled && !dbTask.IsFaulted);
                        });
                }
            });
        }
        else
        {
            callback(false);
        }
    }
}

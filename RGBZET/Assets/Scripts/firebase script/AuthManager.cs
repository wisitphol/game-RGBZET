using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System.Collections;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseUser user;

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
            loginUI.DisplayFeedback($"Logged in successfully: {user.Email}");
            SaveUserData(user);
            SceneManager.LoadScene("Menu");
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
                // เก็บข้อมูลผู้ใช้ลงใน Firebase
                DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(user.UserId);
                userRef.Child("username").SetValueAsync(username);
                userRef.Child("email").SetValueAsync(email);

                registerUI.DisplayFeedback($"Registered successfully: {user.Email}");
                SceneManager.LoadScene("Login");
            }
        }
    }

    public void Logout()
    {
        auth.SignOut();
        user = null;
        SceneManager.LoadScene("Login");
    }

    private void SaveUserData(FirebaseUser user)
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(user.UserId);
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
}

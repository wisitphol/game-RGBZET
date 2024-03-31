using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;


public class AuthManager : MonoBehaviour
{
    public InputField loginEmailField;
    public InputField loginPasswordField;
    public InputField registerEmailField;
    public InputField registerPasswordField;
    public InputField confirmPasswordField;

    private FirebaseAuth auth;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.GetAuth(app);
        });
    }

    public async void Register()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();

        string email = registerEmailField.text;
        string password = registerPasswordField.text;
        string confirmPassword = confirmPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            Debug.LogError("Please fill in all fields.");
            return;
        }

        if (password != confirmPassword)
        {
            Debug.LogError("Passwords do not match.");
            return;
        }

        try
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            await registerTask;
            AuthResult authResult = registerTask.Result;
            FirebaseUser user = authResult.User;
            Debug.Log($"Registration successful: {user.Email}");

            SceneManager.LoadScene("Login");
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Registration failed: {e.Message}");
        }
    }

    public async void Login()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();

        string email = loginEmailField.text;
        string password = loginPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Please fill in both email and password.");
            return;
        }

        try
        {
            var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
            await loginTask;
            AuthResult authResult = loginTask.Result;
            FirebaseUser user = authResult.User;
            Debug.Log($"Login successful: {user.Email}");

            SceneManager.LoadScene("Menu");
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Login failed: {e.Message}");
        }
    }
}

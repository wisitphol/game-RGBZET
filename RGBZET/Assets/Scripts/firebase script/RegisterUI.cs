using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backToLoginButton;
    [SerializeField] private TMP_Text feedbackText;

    public void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        backToLoginButton.onClick.AddListener(OnBackToLoginButtonClicked);
    }

    private void OnRegisterButtonClicked()
    {
        string username = usernameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (password != confirmPassword)
        {
            DisplayFeedback("Passwords do not match.");
            return;
        }

        // ใช้ AuthManager.Instance.Register โดยส่ง this (RegisterUI) เป็นพารามิเตอร์
        AuthManager.Instance.Register(email, password, username, this);
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }

    private void OnBackToLoginButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
}

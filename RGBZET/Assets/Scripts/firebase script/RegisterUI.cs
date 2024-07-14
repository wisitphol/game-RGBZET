using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameInput;
    public Button registerButton;
    public Text feedbackText;

    void Start()
    {
        registerButton.onClick.AddListener(() => 
        {
            AuthManager.Instance.Register(emailInput.text, passwordInput.text, usernameInput.text, this);
        });
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }
}

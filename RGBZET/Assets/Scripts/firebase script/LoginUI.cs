using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Text feedbackText;

    void Start()
    {
        loginButton.onClick.AddListener(() => 
        {
            AuthManager.Instance.Login(emailInput.text, passwordInput.text, this);
        });
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }

    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene("Register");
    }
}

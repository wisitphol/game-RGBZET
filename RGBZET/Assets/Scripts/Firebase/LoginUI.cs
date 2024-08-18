using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button goToRegisterButton;
    [SerializeField] private TMP_Text feedbackText;

    public void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        goToRegisterButton.onClick.AddListener(OnGoToRegisterButtonClicked);
    }

    private void OnLoginButtonClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        // ใช้ AuthManager.Instance.Login โดยส่ง this (LoginUI) เป็นพารามิเตอร์
        AuthManager.Instance.Login(email, password, this);
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
        StartCoroutine(ClearFeedbackAfterDelay(5f));
    }

    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
    }

    private void OnGoToRegisterButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Register");
    }
}

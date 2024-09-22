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


public class RegisterUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button LoginButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    public void Start()
    {
        registerButton.onClick.AddListener(() => SoundOnClick(OnRegisterButtonClicked));
        LoginButton.onClick.AddListener(() => SoundOnClick(OnBackToLoginButtonClicked));
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
        SceneManager.LoadScene("Login");
    }

      void SoundOnClick(System.Action buttonAction)
    {
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
            // รอให้เสียงเล่นเสร็จก่อนที่จะทำการเปลี่ยน scene
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            // ถ้าไม่มีเสียงให้เล่น ให้ทำงานทันที
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        // รอความยาวของเสียงก่อนที่จะทำงาน
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }
}

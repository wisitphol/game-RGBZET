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
    [SerializeField] private Button RegisterButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    public void Start()
    {
        loginButton.onClick.AddListener(() => SoundOnClick(OnLoginButtonClicked));
        RegisterButton.onClick.AddListener(() => SoundOnClick(OnGoToRegisterButtonClicked));
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
        SceneManager.LoadScene("Register");
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

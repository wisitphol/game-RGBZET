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
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    public void Start()
    {
        registerButton.onClick.AddListener(() => SoundOnClick(OnRegisterButtonClicked));
        LoginButton.onClick.AddListener(() => SoundOnClick(OnBackToLoginButtonClicked));
        notificationPopup.SetActive(false);
    }

    private void OnRegisterButtonClicked()
    {
        string username = usernameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (password != confirmPassword)
        {
            ShowNotification("Passwords don't match");
            return;
        }

        AuthManager.Instance.Register(email, password, username, this);
    }

    public void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationPopup.SetActive(true);
        StartCoroutine(HideNotificationAfterDelay(3f));
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPopup.SetActive(false);
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
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }
}
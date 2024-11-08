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

    private const int MAX_USERNAME_LENGTH = 10;

    public void Start()
    {
        // จำกัดจำนวนตัวอักษรสำหรับ username
        usernameInput.characterLimit = MAX_USERNAME_LENGTH;
        
        // เพิ่ม listener สำหรับการเปลี่ยนแปลงข้อความ
        usernameInput.onValueChanged.AddListener(OnUsernameInputChanged);
        
        registerButton.onClick.AddListener(() => SoundOnClick(OnRegisterButtonClicked));
        LoginButton.onClick.AddListener(() => SoundOnClick(OnBackToLoginButtonClicked));
        notificationPopup.SetActive(false);
    }

    private void OnUsernameInputChanged(string value)
    {
        if (value.Length > MAX_USERNAME_LENGTH)
        {
            usernameInput.text = value.Substring(0, MAX_USERNAME_LENGTH);
            ShowNotification($"Username cannot exceed {MAX_USERNAME_LENGTH} characters");
        }
    }

    private void OnRegisterButtonClicked()
    {
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        // ตรวจสอบ username
        if (string.IsNullOrEmpty(username))
        {
            ShowNotification("Username cannot be empty");
            return;
        }

        if (username.Length > MAX_USERNAME_LENGTH)
        {
            ShowNotification($"Username cannot exceed {MAX_USERNAME_LENGTH} characters");
            return;
        }

        // ตรวจสอบ email
        if (string.IsNullOrEmpty(email))
        {
            ShowNotification("Email cannot be empty");
            return;
        }

        // ตรวจสอบ password
        if (string.IsNullOrEmpty(password))
        {
            ShowNotification("Password cannot be empty");
            return;
        }

        if (password != confirmPassword)
        {
            ShowNotification("Passwords don't match");
            return;
        }

        // ถ้าผ่านการตรวจสอบทั้งหมด ทำการสมัครสมาชิก
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

    void OnDestroy()
    {
        // ทำความสะอาด listeners
        if (usernameInput != null)
        {
            usernameInput.onValueChanged.RemoveAllListeners();
        }
        if (registerButton != null)
        {
            registerButton.onClick.RemoveAllListeners();
        }
        if (LoginButton != null)
        {
            LoginButton.onClick.RemoveAllListeners();
        }
    }
}
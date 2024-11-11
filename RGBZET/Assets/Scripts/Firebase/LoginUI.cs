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
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button RegisterButton;
    [SerializeField] private GameObject feedbackPopup;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private void Start()
    {
        loginButton.onClick.AddListener(() => SoundOnClick(OnLoginButtonClicked));
        RegisterButton.onClick.AddListener(() => SoundOnClick(OnGoToRegisterButtonClicked));
        
        // Hide the feedback popup initially
        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(false);
        }
    }

    private void OnLoginButtonClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        AuthManager.Instance.Login(email, password, this);
    }

    public void DisplayFeedback(string message)
    {
        if (feedbackPopup != null && feedbackText != null)
        {
            feedbackText.text = message;
            feedbackPopup.SetActive(true);
            StartCoroutine(HideFeedbackAfterDelay(3f));
        }
        else
        {
            Debug.LogError("Feedback popup or text is not assigned in the inspector");
        }
    }

    private IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(false);
        }
    }

    private void OnGoToRegisterButtonClicked()
    {
        SceneManager.LoadScene("Register");
    }

    private void SoundOnClick(System.Action buttonAction)
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
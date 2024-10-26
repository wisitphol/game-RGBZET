using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button TournamentButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button guideButton;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    
    [SerializeField] private GameObject feedbackPopup;
    [SerializeField] private TMP_Text feedbackText;
    
    private DatabaseReference databaseReference;
    private string currentUsername;
    private string currentTournamentId;
    private string currentMatchId;

    void Start()
    {
        if (AuthManager.Instance.IsUserLoggedIn())
        {
            string userId = AuthManager.Instance.GetCurrentUserId();
            databaseReference = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);
            LoadUserData();
        }
        else
        {
            SceneManager.LoadScene("Login");
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(DisconnectFromPhoton());
        }

        SetupButtons();
        
        feedbackPopup.SetActive(false);
    }

    IEnumerator DisconnectFromPhoton()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Debug.Log("Disconnected from Photon");
    }

    void SetupButtons()
    {
        joinButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Joinroom")));
        TournamentButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Tournament")));
        playButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("ModeCreateroom")));
        logoutButton.onClick.AddListener(() => SoundOnClick(OnLogoutButtonClicked));
        profileButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Profile")));
        settingButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Setting")));
    }

    void LoadUserData()
    {
        databaseReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    currentUsername = snapshot.Child("username").Value.ToString();
                    usernameText.text = currentUsername;
                }
                else
                {
                    DisplayFeedback("Failed to load user data.");
                }
            }
            else
            {
                DisplayFeedback("Failed to load user data.");
            }
        });
    }

    void OnLogoutButtonClicked()
    {
        AuthManager.Instance.Logout();
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

    public void DisplayFeedback(string message)
    {
        StartCoroutine(ShowFeedbackPopup(message));
    }

    private IEnumerator ShowFeedbackPopup(string message)
    {
        feedbackText.text = message;
        feedbackPopup.SetActive(true);

        CanvasGroup canvasGroup = feedbackPopup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = feedbackPopup.AddComponent<CanvasGroup>();
        }

        // Fade in
        canvasGroup.alpha = 0f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * 2;
            yield return null;
        }

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Fade out
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * 2;
            yield return null;
        }

        feedbackPopup.SetActive(false);
    }

    void OnDestroy()
    {
        joinButton.onClick.RemoveAllListeners();
        TournamentButton.onClick.RemoveAllListeners();
        logoutButton.onClick.RemoveAllListeners();
        profileButton.onClick.RemoveAllListeners();
        settingButton.onClick.RemoveAllListeners();
    }
}
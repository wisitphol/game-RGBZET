using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TournamentUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button createTournamentButton;
    [SerializeField] private Button returnToTournamentButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button ruleButton;

    [Header("Notification")]
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private TMP_Text notificationText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference databaseReference;
    private string currentUsername;
    private string currentTournamentId;
    private string currentMatchId;
    private bool hasActiveTournament = false;

    void Start()
    {
        if (notificationPopup == null || notificationText == null)
        {
            Debug.LogError("Notification UI elements not assigned!");
            return;
        }

        currentUsername = AuthManager.Instance.GetCurrentUsername();
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("tournaments");
        SetupButtons();
        CheckForOngoingTournament();

        returnToTournamentButton.gameObject.SetActive(false);
        notificationPopup.SetActive(false);
    }

    void SetupButtons()
    {
        createTournamentButton.onClick.AddListener(() => SoundOnClick(() => 
        {
            if (hasActiveTournament)
            {
                ShowNotification("Cannot create new tournament while in active tournament");
            }
            else
            {
                SceneManager.LoadScene("CreateTournament");
            }
        }));
        
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
        ruleButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Rule")));
        returnToTournamentButton.onClick.AddListener(() => SoundOnClick(ReturnToTournament));
    }

    void CheckForOngoingTournament()
    {
        ShowNotification("Checking tournaments...");

        DatabaseReference tournamentsRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments");

        tournamentsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                ShowNotification("Check failed");
                UpdateButtonStates(false);
                return;
            }

            if (!task.Result.Exists)
            {
                ShowNotification("No tournaments found");
                UpdateButtonStates(false);
                return;
            }

            DataSnapshot snapshot = task.Result;
            bool foundActiveTournament = false;

            foreach (var tournamentSnapshot in snapshot.Children)
            {
                var bracketSnapshot = tournamentSnapshot.Child("bracket");
                string latestMatchId = null;
                bool isPlayerInActiveTournament = false;

                var sortedMatches = bracketSnapshot.Children.OrderByDescending(match => 
                    int.Parse(match.Key.Split('_')[1]));

                foreach (var matchSnapshot in sortedMatches)
                {
                    var player1 = matchSnapshot.Child("player1").Child("username").Value?.ToString();
                    var player2 = matchSnapshot.Child("player2").Child("username").Value?.ToString();
                    var winner = matchSnapshot.Child("winner").Value?.ToString();

                    if (player1 == currentUsername || player2 == currentUsername)
                    {
                        isPlayerInActiveTournament = true;
                        if (string.IsNullOrEmpty(winner))
                        {
                            latestMatchId = matchSnapshot.Key;
                            break;
                        }
                    }
                }

                if (isPlayerInActiveTournament)
                {
                    foundActiveTournament = true;
                    currentTournamentId = tournamentSnapshot.Key;
                    currentMatchId = latestMatchId;
                    hasActiveTournament = true;
                    
                    returnToTournamentButton.gameObject.SetActive(true);
                    
                    if (latestMatchId != null)
                    {
                        ShowNotification("Active match found");
                    }
                    else
                    {
                        ShowNotification("Tournament ended");
                        hasActiveTournament = false;
                    }
                    break;
                }
            }

            UpdateButtonStates(foundActiveTournament);

            if (!foundActiveTournament)
            {
                ShowNotification("No active tournaments");
            }
        });
    }

    void UpdateButtonStates(bool hasActiveTournament)
    {
        createTournamentButton.interactable = !hasActiveTournament;
        if (hasActiveTournament)
        {
            createTournamentButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1f); // Grayed out
        }
        else
        {
            createTournamentButton.GetComponent<Image>().color = Color.white; // Normal color
        }
    }

    void ReturnToTournament()
    {
        if (string.IsNullOrEmpty(currentTournamentId))
        {
            ShowNotification("No tournament found");
            return;
        }

        PlayerPrefs.SetString("TournamentId", currentTournamentId);
        if (!string.IsNullOrEmpty(currentMatchId))
        {
            PlayerPrefs.SetString("CurrentMatchId", currentMatchId);
            ShowNotification("Loading match...");
            SceneManager.LoadScene("MatchLobby");
        }
        else
        {
            ShowNotification("Loading bracket...");
            SceneManager.LoadScene("TournamentBracket");
        }
    }

    void ShowNotification(string message)
    {
        if (notificationPopup == null || notificationText == null)
        {
            Debug.LogError($"Cannot show notification: {message} - UI elements missing!");
            return;
        }

        Debug.Log($"Showing notification: {message}");
        notificationText.text = message;
        notificationPopup.SetActive(true);
        StartCoroutine(HideNotificationAfterDelay(3f));
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        if (notificationPopup == null)
        {
            Debug.LogError("Cannot hide notification - popup is null!");
            yield break;
        }

        yield return new WaitForSeconds(delay);
        notificationPopup.SetActive(false);
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
        createTournamentButton.onClick.RemoveAllListeners();
        if (returnToTournamentButton != null)
            returnToTournamentButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }
}
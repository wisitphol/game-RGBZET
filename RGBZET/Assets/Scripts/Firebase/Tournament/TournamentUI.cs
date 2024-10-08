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
    [SerializeField] private Button createTournamentButton;
    [SerializeField] private Button returnToTournamentButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference databaseReference;
    private string currentUsername;
    private string currentTournamentId;
    private string currentMatchId;

    void Start()
    {
        currentUsername = AuthManager.Instance.GetCurrentUsername();
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("tournaments");
        SetupButtons();
        CheckForOngoingTournament();

        returnToTournamentButton.gameObject.SetActive(false);
    }

    void SetupButtons()
    {
        createTournamentButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("CreateTournament")));
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
        returnToTournamentButton.onClick.AddListener(() => SoundOnClick(ReturnToTournament));
    }

    void CheckForOngoingTournament()
    {
        DatabaseReference tournamentsRef = FirebaseDatabase.DefaultInstance.GetReference("tournaments");

        tournamentsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
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
                        currentTournamentId = tournamentSnapshot.Key;
                        currentMatchId = latestMatchId;
                        returnToTournamentButton.gameObject.SetActive(true);
                        if (latestMatchId != null)
                        {
                            DisplayFeedback("You have an ongoing match in a tournament. Click 'Return to Tournament' to continue.");
                        }
                        else
                        {
                            DisplayFeedback("Your tournament has ended. Click 'Return to Tournament' to view results.");
                        }
                        return;
                    }
                }
            }
            else
            {
                DisplayFeedback("Failed to check for ongoing tournaments.");
            }
        });
    }

    void ReturnToTournament()
    {
        if (!string.IsNullOrEmpty(currentTournamentId))
        {
            PlayerPrefs.SetString("TournamentId", currentTournamentId);
            if (!string.IsNullOrEmpty(currentMatchId))
            {
                PlayerPrefs.SetString("CurrentMatchId", currentMatchId);
                SceneManager.LoadScene("MatchLobby");
            }
            else
            {
                SceneManager.LoadScene("TournamentBracket");
            }
        }
        else
        {
            DisplayFeedback("No ongoing tournament found.");
        }
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
        feedbackText.text = message;
    }

    void OnDestroy()
    {
        createTournamentButton.onClick.RemoveAllListeners();
        //returnToTournamentButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using System.Linq;

public class EndGameT : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button backToMenu;
    [SerializeField] private Button nextRoundButton;
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;

    private GameObject[] playerObjects;
    private PlayerResultT[] playerResults;
    private DatabaseReference databaseReference;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip endgameSound;
    [SerializeField] public AudioClip buttonSound;

    private string tournamentId;
    private string currentMatchId;
    private Player winningPlayer;

    void Start()
    {
        backToMenu.interactable = false;
        nextRoundButton.gameObject.SetActive(false);
        StartCoroutine(EnableBackButtonAfterDelay(3f));
        backToMenu.onClick.AddListener(OnBackToMenuButtonClicked);
        nextRoundButton.onClick.AddListener(OnNextRoundButtonClicked);

        tournamentId = PlayerPrefs.GetString("TournamentId");
        currentMatchId = PlayerPrefs.GetString("CurrentMatchId");
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("tournaments").Child(tournamentId);

        playerObjects = new GameObject[] { player1, player2, player3, player4 };
        playerResults = new PlayerResultT[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] != null)
            {
                playerResults[i] = playerObjects[i].GetComponent<PlayerResultT>();
            }
        }

        FetchPlayerDataFromPhoton();

        if (audioSource != null && endgameSound != null)
        {
            audioSource.PlayOneShot(endgameSound);
        }
    }

    void FetchPlayerDataFromPhoton()
    {
        foreach (var playerObject in playerObjects)
        {
            playerObject.SetActive(false);
        }

        Player[] players = PhotonNetwork.PlayerList;
        int index = 0;
        int highestScore = int.MinValue;
        List<int> highestScoreIndices = new List<int>();

        foreach (Player player in players)
        {
            if (index >= playerResults.Length) break;

            string playerName = player.CustomProperties.ContainsKey("username") ? player.CustomProperties["username"].ToString() : player.NickName;
            string playerScoreStr = player.CustomProperties.ContainsKey("score") ? player.CustomProperties["score"].ToString() : "0";

            int playerScore;
            string cleanScoreStr = System.Text.RegularExpressions.Regex.Replace(playerScoreStr, @"\D", "");

            if (!int.TryParse(cleanScoreStr, out playerScore))
            {
                Debug.LogError($"Failed to parse score '{playerScoreStr}' for player '{playerName}'. Defaulting to 0.");
                playerScore = 0;
            }

            if (playerScore > highestScore)
            {
                highestScore = playerScore;
                highestScoreIndices.Clear();
                highestScoreIndices.Add(index);
                winningPlayer = player;
            }
            else if (playerScore == highestScore)
            {
                highestScoreIndices.Add(index);
            }

            playerResults[index].UpdatePlayerResult(playerName, "score: " + playerScore.ToString(), "");
            playerObjects[index].SetActive(true);

            index++;
        }

        if (highestScoreIndices.Count == 1)
        {
            playerResults[highestScoreIndices[0]].UpdatePlayerResult(
                playerResults[highestScoreIndices[0]].NameText.text,
                playerResults[highestScoreIndices[0]].ScoreText.text,
                "WIN"
            );
            if (PhotonNetwork.LocalPlayer == winningPlayer)
            {
                nextRoundButton.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (int i in highestScoreIndices)
            {
                playerResults[i].UpdatePlayerResult(
                    playerResults[i].NameText.text,
                    playerResults[i].ScoreText.text,
                    "DRAW"
                );
            }
            // Handle draw situation (e.g., replay the match or use a tiebreaker)
        }

        UpdateTournamentBracket();
    }

    void UpdateTournamentBracket()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string winnerUsername = winningPlayer.CustomProperties["username"].ToString();
            databaseReference.Child("bracket").Child(currentMatchId).Child("winner").SetValueAsync(winnerUsername)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        MoveWinnerToNextMatch(winnerUsername);
                    }
                    else
                    {
                        Debug.LogError("Failed to update winner in the database: " + task.Exception);
                    }
                });
        }
    }

    void MoveWinnerToNextMatch(string winnerUsername)
    {
        databaseReference.Child("bracket").Child(currentMatchId).Child("nextMatchId").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Value != null)
            {
                string nextMatchId = task.Result.Value.ToString();
                if (nextMatchId != "final")
                {
                    UpdateNextMatch(nextMatchId, winnerUsername);
                }
                else
                {
                    HandleTournamentEnd(winnerUsername);
                }
            }
            else
            {
                Debug.LogError("Failed to get next match ID: " + task.Exception);
            }
        });
    }

    void UpdateNextMatch(string nextMatchId, string winnerUsername)
    {
        databaseReference.Child("bracket").Child(nextMatchId).RunTransaction(mutableData =>
        {
            Dictionary<string, object> match = mutableData.Value as Dictionary<string, object>;
            if (match != null)
            {
                if (match["player1"] is Dictionary<string, object> player1 && string.IsNullOrEmpty(player1["username"] as string))
                {
                    player1["username"] = winnerUsername;
                }
                else if (match["player2"] is Dictionary<string, object> player2 && string.IsNullOrEmpty(player2["username"] as string))
                {
                    player2["username"] = winnerUsername;
                }
                mutableData.Value = match;
            }
            return TransactionResult.Success(mutableData);
        }).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                PlayerPrefs.SetString("CurrentMatchId", nextMatchId);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("Failed to update next match: " + task.Exception);
            }
        });
    }

    void HandleTournamentEnd(string winnerUsername)
    {
        databaseReference.Child("winner").SetValueAsync(winnerUsername).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Tournament ended. Winner: " + winnerUsername);
                // Update UI or transition to a tournament end screen
            }
            else
            {
                Debug.LogError("Failed to set tournament winner: " + task.Exception);
            }
        });
    }

    private void OnBackToMenuButtonClicked()
    {
        audioSource.PlayOneShot(buttonSound);
        StartCoroutine(WaitAndGoToMenu(1.0f));
    }

    private void OnNextRoundButtonClicked()
    {
        audioSource.PlayOneShot(buttonSound);
        SceneManager.LoadScene("TournamentBracket");
    }

    private IEnumerator WaitAndGoToMenu(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(LeaveRoomAndGoToMenu());
        }
        else
        {
            StartCoroutine(LeaveRoomAndCheckConnection());
        }
    }

    private IEnumerator LeaveRoomAndGoToMenu()
    {
        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
    }

    private IEnumerator LeaveRoomAndCheckConnection()
    {
        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
    }

    IEnumerator EnableBackButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        backToMenu.interactable = true;
    }
}
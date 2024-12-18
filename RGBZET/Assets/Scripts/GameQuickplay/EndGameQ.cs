using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;

public class EndGameQ : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button backToMenu;
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    private string roomId;
    private GameObject[] playerObjects;
    private PlayerResultQ[] playerResults;
    private DatabaseReference databaseReference;
    private FirebaseUserId firebaseUserId;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip endgameSound;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        backToMenu.interactable = false;
        StartCoroutine(EnableBackButtonAfterDelay(3f));
        backToMenu.onClick.AddListener(OnBackToMenuButtonClicked);

        firebaseUserId = FindObjectOfType<FirebaseUserId>();
        if (firebaseUserId == null)
        {
            Debug.LogError("FirebaseUserId script not found in the scene.");
        }
        else
        {
            Debug.Log("FirebaseUserId script found successfully.");
        }

        playerObjects = new GameObject[] { player1, player2, player3, player4 };
        playerResults = new PlayerResultQ[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] != null)
            {
                Debug.Log($"player{i + 1} is not null. Checking for PlayerResult component.");
                playerResults[i] = playerObjects[i].GetComponent<PlayerResultQ>();

                if (playerResults[i] != null)
                {
                    Debug.Log($"PlayerResult component found on player{i + 1}.");
                }
                else
                {
                    Debug.LogWarning($"PlayerResult component NOT found on player{i + 1}.");
                }
            }
            else
            {
                Debug.LogWarning($"player{i + 1} is null.");
            }
        }

        roomId = PhotonNetwork.CurrentRoom.CustomProperties["roomId"].ToString();
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("withfriends").Child(roomId);

        LogServerConnectionStatus();

        FetchPlayerDataFromPhoton();

        StartCoroutine(DelayedUpdateGameResults());

        if (audioSource != null && endgameSound != null)
        {
            audioSource.PlayOneShot(endgameSound);
        }
    }

    IEnumerator EnableBackButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        backToMenu.interactable = true;
    }

    void FetchPlayerDataFromPhoton()
    {
        Debug.Log("Fetching player data from Photon.");

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

            Debug.Log($"Player {index + 1}: Name = {playerName}, Score = {playerScore}");

            if (player.CustomProperties.ContainsKey("iconId"))
            {
                int iconId = (int)player.CustomProperties["iconId"];
                playerResults[index].UpdatePlayerIcon(iconId);
            }

            if (playerScore > highestScore)
            {
                highestScore = playerScore;
                highestScoreIndices.Clear();
                highestScoreIndices.Add(index);
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
        }
    }

    IEnumerator DelayedUpdateGameResults()
    {
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateGameResultsInDatabase();
        }
    }

    void UpdateGameResultsInDatabase()
    {
        Debug.Log("Updating game results in database.");

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            string userId = players[i].CustomProperties.ContainsKey("FirebaseUserId") ? players[i].CustomProperties["FirebaseUserId"].ToString() : null;

            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning($"UserId is null or empty for player {i}, skipping update.");
                continue;
            }
            else
            {
                Debug.Log($"FirebaseUserId for player {i}: {userId}");
            }

            DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);

            userRef.Child("gamescount").RunTransaction(mutableData =>
            {
                int currentGamesPlayed = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                mutableData.Value = currentGamesPlayed + 1;
                return TransactionResult.Success(mutableData);
            }).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Game count updated successfully.");
                }
                else
                {
                    Debug.LogError("Failed to update game count.");
                }
            });

            bool isWinner = false;
            bool isDraw = false;

            if (playerResults[i] != null)
            {
                if (playerResults[i].ResultText.text == "WIN")
                {
                    isWinner = true;
                }
                else if (playerResults[i].ResultText.text == "DRAW")
                {
                    isDraw = true;
                }
            }

            if (isWinner)
            {
                userRef.Child("gameswin").RunTransaction(mutableData =>
                {
                    int currentWins = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                    mutableData.Value = currentWins + 1;
                    return TransactionResult.Success(mutableData);
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Wins count updated successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to update wins count.");
                    }
                });
            }
            else if (isDraw)
            {
                userRef.Child("gamesdraw").RunTransaction(mutableData =>
                {
                    int currentDraws = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                    mutableData.Value = currentDraws + 1;
                    return TransactionResult.Success(mutableData);
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Draws count updated successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to update draws count.");
                    }
                });
            }
            else
            {
                userRef.Child("gameslose").RunTransaction(mutableData =>
                {
                    int currentLosses = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
                    mutableData.Value = currentLosses + 1;
                    return TransactionResult.Success(mutableData);
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Losses count updated successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to update losses count.");
                    }
                });
            }
        }
    }

    private void OnBackToMenuButtonClicked()
    {
        audioSource.PlayOneShot(buttonSound);
        StartCoroutine(WaitAndGoToMenu(1.0f));
    }

    private IEnumerator WaitAndGoToMenu(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
        else
        {
            StartCoroutine(LeaveRoomAndCheckConnection());
        }
    }

    private IEnumerator DeleteRoomAndGoToMenu()
    {
        var task = databaseReference.RemoveValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Can't delete room from Firebase.");
        }
        else
        {
            Debug.Log("Can delete room from Firebase.");
        }

        PhotonNetwork.LeaveRoom();

        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        LogServerConnectionStatus();
        SceneManager.LoadScene("Menu");
    }

    private void LogServerConnectionStatus()
    {
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Currently connected to Master Server.");
        }
        else if (PhotonNetwork.InRoom)
        {
            Debug.Log("Currently connected to Game Server.");
        }
        else
        {
            Debug.Log("Not connected to Master Server or Game Server.");
        }
    }

    private IEnumerator LeaveRoomAndCheckConnection()
    {
        PhotonNetwork.LeaveRoom();

        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        LogServerConnectionStatus();
        SceneManager.LoadScene("Menu");
    }
}
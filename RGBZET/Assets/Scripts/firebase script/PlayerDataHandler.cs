using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;

public class PlayerDataHandler : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text playerScoreText;

    DatabaseReference databaseReference;

    // Data structure to hold player information
    private struct PlayerInfo
    {
        public string playerName;
        public int playerScore;
    }

    // List to store all player data
    private List<PlayerInfo> playerDataList = new List<PlayerInfo>();

    void Start()
    {
        // Connect to Firebase Realtime Database
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
                return;
            }

            // If connection is successful, get FirebaseApp instance and enable Realtime Database
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

            // Call a function to retrieve player data
            RetrievePlayerData();

            Debug.Log("Firebase connected successfully!");
        });

         //Debug.Log("PlayerDataHandler script has been called.");
    }

    void RetrievePlayerData()
    {
        // Retrieve player data from Realtime Database
        databaseReference.Child("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve player data: " + task.Exception);
                return;
            }

            // If data retrieval is successful
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // Clear existing player data
                playerDataList.Clear();

                // Iterate through each player's data
                foreach (DataSnapshot playerSnapshot in snapshot.Children)
                {
                    // Retrieve player name and score
                    string playerName = playerSnapshot.Child("username").Value.ToString();
                    int playerScore = int.Parse(playerSnapshot.Child("score").Value.ToString());

                    // Add player data to the list
                    playerDataList.Add(new PlayerInfo { playerName = playerName, playerScore = playerScore });
                }

                // Display player data
                DisplayPlayerData();
            }
        });
    }

    void DisplayPlayerData()
    {
        // Clear existing text
        playerNameText.text = "";
        playerScoreText.text = "";

        // Iterate through player data list and display each player's info
        foreach (PlayerInfo playerInfo in playerDataList)
        {
            playerNameText.text += playerInfo.playerName + "\n";
            playerScoreText.text += "Score: " + playerInfo.playerScore + "\n";
        }
    }
}

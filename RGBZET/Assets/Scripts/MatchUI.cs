using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MatchUI : MonoBehaviour
{
    public TMP_Text player1Text;
    public TMP_Text player2Text;
    public Image player1StatusImage;
    public Image player2StatusImage;
    public TMP_Text matchIdText;
    public TMP_Text roundText;
    public TMP_Text winnerText;

    public string matchId { get; private set; }
    private string currentUsername;
    private Dictionary<string, object> matchData;

    // Define status colors
    private readonly Color NotInLobbyColor = Color.red;        // ยังไม่เข้า lobby
    private readonly Color InLobbyColor = new Color(1f, 0.5f, 0f); // สีส้ม - อยู่ใน lobby
    private readonly Color PlayingColor = Color.green;         // กำลังเล่น
    private readonly Color CompletedColor = Color.cyan;        // เล่นเสร็จแล้ว

    public void Initialize(string id, Dictionary<string, object> data, string username)
    {
        matchId = id;
        currentUsername = username;

        if (matchIdText != null)
            matchIdText.text = "Match: " + matchId;

        UpdateMatchData(data);
        UpdateRoundInfo();
    }

    public void UpdateMatchData(Dictionary<string, object> data)
    {
        matchData = data;
        if (matchData == null) return;

        Dictionary<string, object> player1Data = matchData["player1"] as Dictionary<string, object>;
        Dictionary<string, object> player2Data = matchData["player2"] as Dictionary<string, object>;

        UpdatePlayerText(player1Text, player1Data);
        UpdatePlayerText(player2Text, player2Data);

        UpdatePlayerStatusColors(player1StatusImage, player2StatusImage, player1Data, player2Data);
        UpdateWinnerText();
    }

    private void UpdatePlayerStatusColors(Image player1Image, Image player2Image, 
        Dictionary<string, object> player1Data, Dictionary<string, object> player2Data)
    {
        if (player1Image != null && player1Data != null)
        {
            UpdatePlayerStatusColor(player1Image, player1Data);
        }

        if (player2Image != null && player2Data != null)
        {
            UpdatePlayerStatusColor(player2Image, player2Data);
        }
    }

    private void UpdatePlayerStatusColor(Image statusImage, Dictionary<string, object> playerData)
    {
        if (string.IsNullOrEmpty(playerData["username"] as string))
        {
            statusImage.color = Color.gray;
            return;
        }

        bool inLobby = playerData.ContainsKey("inLobby") && (bool)playerData["inLobby"];
        bool isPlaying = playerData.ContainsKey("isPlaying") && (bool)playerData["isPlaying"];
        bool hasCompleted = playerData.ContainsKey("hasCompleted") && (bool)playerData["hasCompleted"];

        if (hasCompleted)
        {
            statusImage.color = CompletedColor;
        }
        else if (isPlaying)
        {
            statusImage.color = PlayingColor;
        }
        else if (inLobby)
        {
            statusImage.color = InLobbyColor;
        }
        else
        {
            statusImage.color = NotInLobbyColor;
        }
    }

    private void UpdatePlayerText(TMP_Text playerText, Dictionary<string, object> playerData)
    {
        if (playerData == null || !playerData.ContainsKey("username") || 
            string.IsNullOrEmpty(playerData["username"] as string))
        {
            playerText.text = "TBD";
            playerText.color = Color.gray;
        }
        else
        {
            string username = playerData["username"] as string;
            playerText.text = username;
            playerText.color = (username == currentUsername) ? Color.green : Color.white;
        }
    }

    private void UpdateWinnerText()
    {
        if (winnerText == null)
        {
            Debug.LogWarning("winnerText is null, skipping update.");
            return;
        }

        if (matchData.ContainsKey("winner") && matchData["winner"] is string winner && 
            !string.IsNullOrEmpty(winner))
        {
            winnerText.text = "Winner: " + winner;
            winnerText.gameObject.SetActive(true);

            // Update player text colors for winner
            if (player1Text != null && player1Text.text == winner)
            {
                player1Text.color = Color.yellow;
            }
            else if (player2Text != null && player2Text.text == winner)
            {
                player2Text.color = Color.yellow;
            }

            // Set both status images to completed color
            if (player1StatusImage != null) player1StatusImage.color = CompletedColor;
            if (player2StatusImage != null) player2StatusImage.color = CompletedColor;
        }
        else
        {
            winnerText.gameObject.SetActive(false);
        }
    }

    private void UpdateRoundInfo()
    {
        if (roundText != null)
        {
            string[] matchInfo = matchId.Split('_');
            if (matchInfo.Length >= 2)
            {
                int round = int.Parse(matchInfo[1]) + 1;
                roundText.text = "Round " + round;
            }
            else
            {
                roundText.text = "Final";
            }
        }
    }
}
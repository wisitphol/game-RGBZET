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

        UpdatePlayerStatus(player1StatusImage, player1Data);
        UpdatePlayerStatus(player2StatusImage, player2Data);

        UpdateWinnerText();
    }

    private void UpdatePlayerText(TMP_Text playerText, Dictionary<string, object> playerData)
    {
        if (playerData == null || !playerData.ContainsKey("username") || string.IsNullOrEmpty(playerData["username"] as string))
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

    private void UpdatePlayerStatus(Image statusImage, Dictionary<string, object> playerData)
    {
        if (statusImage == null) return;

        if (playerData == null || !playerData.ContainsKey("username") || string.IsNullOrEmpty(playerData["username"] as string))
        {
            statusImage.color = Color.gray;
            return;
        }

        bool inLobby = playerData.ContainsKey("inLobby") && (bool)playerData["inLobby"];
        statusImage.color = inLobby ? Color.green : Color.red;
    }

    private void UpdateWinnerText()
    {
        if (winnerText == null)
        {
            Debug.LogWarning("winnerText is null, skipping update.");
            return;
        }

        if (matchData.ContainsKey("winner") && matchData["winner"] is string winner && !string.IsNullOrEmpty(winner))
        {
            winnerText.text = "Winner: " + winner;
            winnerText.gameObject.SetActive(true);

            if (player1Text.text == winner)
                player1Text.color = Color.yellow;
            else if (player2Text.text == winner)
                player2Text.color = Color.yellow;
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

    public bool IsPlayerInMatch(string username)
    {
        if (matchData == null) return false;

        Dictionary<string, object> player1Data = matchData["player1"] as Dictionary<string, object>;
        Dictionary<string, object> player2Data = matchData["player2"] as Dictionary<string, object>;

        return (player1Data != null && player1Data["username"] as string == username) ||
               (player2Data != null && player2Data["username"] as string == username);
    }

    public bool IsPlayerInLobby(string username)
    {
        if (matchData == null) return false;

        Dictionary<string, object> player1Data = matchData["player1"] as Dictionary<string, object>;
        Dictionary<string, object> player2Data = matchData["player2"] as Dictionary<string, object>;

        if (player1Data != null && player1Data["username"] as string == username)
        {
            return player1Data.ContainsKey("inLobby") && (bool)player1Data["inLobby"];
        }
        else if (player2Data != null && player2Data["username"] as string == username)
        {
            return player2Data.ContainsKey("inLobby") && (bool)player2Data["inLobby"];
        }

        return false;
    }

    public bool IsMatchCompleted()
    {
        return matchData != null && matchData.ContainsKey("winner") && !string.IsNullOrEmpty(matchData["winner"] as string);
    }

    public void SetWinnerLoser(string winner)
    {
        if (player1Text.text == winner)
        {
            player1Text.color = Color.green;
            player2Text.color = Color.red;
        }
        else if (player2Text.text == winner)
        {
            player2Text.color = Color.green;
            player1Text.color = Color.red;
        }
    }
}
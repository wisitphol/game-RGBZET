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
    private readonly Color NotInLobbyColor = Color.red;
    private readonly Color InLobbyColor = new Color(1f, 0.5f, 0f); // Orange
    private readonly Color PlayingColor = Color.green;
    private readonly Color CompletedColor = Color.cyan;
    private readonly Color DefaultColor = Color.white;

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
        if (player1Data != null)
        {
            UpdateSinglePlayerStatus(player1Image, player1Data);
        }

        if (player2Data != null)
        {
            UpdateSinglePlayerStatus(player2Image, player2Data);
        }
    }

    private void UpdateSinglePlayerStatus(Image statusImage, Dictionary<string, object> playerData)
    {
        if (statusImage == null || playerData == null) return;

        string username = playerData["username"] as string;
        if (string.IsNullOrEmpty(username))
        {
            statusImage.color = Color.gray;
            return;
        }

        bool inLobby = playerData.ContainsKey("inLobby") && (bool)playerData["inLobby"];
        bool isPlaying = playerData.ContainsKey("isPlaying") && (bool)playerData["isPlaying"];
        bool hasCompleted = playerData.ContainsKey("hasCompleted") && (bool)playerData["hasCompleted"];

        // ตรวจสอบว่ามีผู้ชนะแล้วหรือไม่
        string winner = matchData.ContainsKey("winner") ? matchData["winner"] as string : null;
        
        if (!string.IsNullOrEmpty(winner))
        {
            // ถ้าเป็นผู้ชนะให้เป็นสีฟ้า แพ้ให้เป็นสีเทา
            statusImage.color = (username == winner) ? CompletedColor : Color.gray;
        }
        else if (hasCompleted)
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
        if (playerText == null) return;

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

            // สีข้อความปกติสำหรับผู้เล่นทั่วไป
            playerText.color = DefaultColor;

            // ถ้าเป็นผู้เล่นปัจจุบัน ให้เป็นสีเขียว
            if (username == currentUsername)
            {
                playerText.color = Color.green;
            }

            // ถ้ามีผู้ชนะแล้ว ให้ชื่อผู้ชนะเป็นสีเหลือง
            if (matchData.ContainsKey("winner") && matchData["winner"] as string == username)
            {
                playerText.color = Color.yellow;
            }
        }
    }

    private void UpdateWinnerText()
    {
        if (winnerText == null) return;

        if (matchData.ContainsKey("winner") && !string.IsNullOrEmpty(matchData["winner"] as string))
        {
            string winner = matchData["winner"] as string;
            winnerText.text = "Winner: " + winner;
            winnerText.gameObject.SetActive(true);
        }
        else
        {
            winnerText.gameObject.SetActive(false);
        }
    }

    private void UpdateRoundInfo()
    {
        if (roundText == null) return;

        string[] matchInfo = matchId.Split('_');
        if (matchInfo.Length >= 2)
        {
            int round = int.Parse(matchInfo[1]) + 1;
            roundText.text = round == 1 ? "Final" : $"Round {round}";
        }
    }
}
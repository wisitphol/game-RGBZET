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
        if (playerData == null)
        {
            playerText.text = "TBD";
            playerText.color = Color.gray;
            return;
        }

        string username = playerData["username"] as string;
        if (string.IsNullOrEmpty(username))
        {
            playerText.text = "TBD";
            playerText.color = Color.gray;
        }
        else
        {
            playerText.text = username;
            playerText.color = (username == currentUsername) ? Color.green : Color.white;
        }
    }

    private void UpdatePlayerStatus(Image statusImage, Dictionary<string, object> playerData)
    {
        if (statusImage == null) return; // ตรวจสอบว่า statusImage ไม่เป็น null

        if (playerData == null)
        {
            statusImage.color = Color.red; // Default status if data is null
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

        // ตรวจสอบว่ามี key 'winner' หรือไม่ และว่าค่านั้นเป็น null หรือไม่
        if (matchData.ContainsKey("winner"))
        {
            string winner = matchData["winner"] as string;

            if (!string.IsNullOrEmpty(winner))
            {
                winnerText.text = "Winner: " + winner;
                winnerText.gameObject.SetActive(true);
            }
            else
            {
                // หาก winner เป็นค่า null หรือว่าง, ซ่อน Text นี้
                winnerText.gameObject.SetActive(false);
            }
        }
        else
        {
            // ถ้าไม่มี key 'winner', ซ่อน Text นี้
            winnerText.gameObject.SetActive(false);
        }
    }

    private void UpdateRoundInfo()
    {
        if (roundText != null) // ตรวจสอบว่า roundText ไม่เป็น null
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
        else
        {
            Debug.LogWarning("roundText is null, skipping update."); // แสดงข้อความเตือนหาก roundText เป็น null
        }
    }

    public bool IsPlayerInMatch(string username)
    {
        if (matchData == null) return false; // ตรวจสอบว่า matchData ไม่เป็น null

        Dictionary<string, object> player1Data = matchData["player1"] as Dictionary<string, object>;
        Dictionary<string, object> player2Data = matchData["player2"] as Dictionary<string, object>;

        return (player1Data["username"] as string == username) || (player2Data["username"] as string == username);
    }

    public bool IsPlayerInLobby(string username)
    {
        if (matchData == null) return false; // ตรวจสอบว่า matchData ไม่เป็น null

        Dictionary<string, object> player1Data = matchData["player1"] as Dictionary<string, object>;
        Dictionary<string, object> player2Data = matchData["player2"] as Dictionary<string, object>;

        if (player1Data["username"] as string == username)
        {
            return player1Data.ContainsKey("inLobby") && (bool)player1Data["inLobby"];
        }
        else if (player2Data["username"] as string == username)
        {
            return player2Data.ContainsKey("inLobby") && (bool)player2Data["inLobby"];
        }

        return false;
    }
}

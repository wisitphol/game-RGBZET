using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayerResult2 : MonoBehaviourPunCallbacks
{
    public TMP_Text NameText;
    public TMP_Text ScoreText;
    public TMP_Text ResultText;
    public Image playerIconImage; 
    public Sprite[] iconSprites;    

    void Start()
    {
        if (NameText == null || ScoreText == null || ResultText == null)
        {
            Debug.LogError("PlayerResult: One or more TMP_Text components are not assigned.");
        }
    }
    public void UpdatePlayerResult(string name, string score, string result)
    {
        Debug.Log($"Updating Player Result: Name = {name}, Score = {score}, Win = {result}");

        if (NameText != null)
        {
            NameText.text = name;
        }

        if (ScoreText != null)
        {
            ScoreText.text = score;
        }

        if (ResultText != null)
        {
            ResultText.text = result;

          /*  if (win == "Winner")
            {
                WinText.color = Color.green; // สีสำหรับ Winner
            }
            else if (win == "Draw")
            {
                WinText.color = Color.yellow; // สีสำหรับ Draw
            }
            else
            {
                WinText.color = Color.white; // สีเริ่มต้น
            }*/
        }
    }

    public void UpdatePlayerIcon(int iconId)
    {
        if (iconId >= 0 && iconId < iconSprites.Length)
        {
            playerIconImage.sprite = iconSprites[iconId];
        }
        else
        {
            Debug.LogError("Invalid iconId. Unable to update player icon.");
        }
    }
}

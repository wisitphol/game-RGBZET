using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayerResult : MonoBehaviourPunCallbacks
{
    public TMP_Text NameText;
    public TMP_Text ScoreText;
    public TMP_Text WinText;

    void Start()
    {
        if (NameText == null || ScoreText == null || WinText == null)
        {
            Debug.LogError("PlayerResult: One or more TMP_Text components are not assigned.");
        }
    }
    public void UpdatePlayerResult(string name, string score, string win)
    {
        Debug.Log($"Updating Player Result: Name = {name}, Score = {score}, Win = {win}");
        
        if (NameText != null)
        {
            NameText.text = name;
        }

        if (ScoreText != null)
        {
            ScoreText.text = score;
        }

        if (WinText != null)
        {
            WinText.text = win;

            if (win == "Winner")
            {
                WinText.color = Color.green; // เปลี่ยนเป็นสีเขียว
            }
            else
            {
                WinText.color = Color.white; // สีเริ่มต้น
            }
        }
    }
}

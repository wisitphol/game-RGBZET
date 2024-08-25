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

    }
    public void UpdatePlayerResult(string name, string score, string win)
    {

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

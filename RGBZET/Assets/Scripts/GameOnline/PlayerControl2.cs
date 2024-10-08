using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayerControl2 : MonoBehaviourPunCallbacks
{

    public TMP_Text NameText;
    public TMP_Text ScoreText;
    public GameObject zettext;
    public Image playerIconImage; 
    public Sprite[] iconSprites;    

    public int ActorNumber { get; private set; }
    private int currentScore = 0; // เก็บคะแนนของผู้เล่น
    
    void Start()
    {
        zettext.SetActive(false); // ซ่อน zettext ในตอนเริ่มต้น
    }

    public void ActivateZetText()
    {
        zettext.SetActive(true); // แสดง zettext
    }

    public void DeactivateZetText()
    {
        zettext.SetActive(false); // ซ่อน zettext
    }

    public void UpdatePlayerInfo(string name, string score, bool zetActive)
    {

        if (NameText != null)
        {
            NameText.text = name;
        }

        if (ScoreText != null)
        {
            ScoreText.text = score;
        }

        if (zettext != null)
        {
            zettext.SetActive(zetActive);
        }
    }

    public void UpdateScore(int newScore)
    {
        currentScore = newScore; // อัปเดตคะแนน
        if (currentScore < 0)
        {
            currentScore = 0; // ป้องกันคะแนนติดลบ
        }
        ScoreText.text = "Score: " + currentScore.ToString(); // แสดงคะแนนใหม่
    }

    public void ResetScore()
    {
        currentScore = 0; // รีเซ็ตคะแนน
        ScoreText.text = "Score: " + currentScore.ToString(); // อัปเดตคะแนนที่แสดง
    }

    public void ResetZetStatus()
    {
        zettext.SetActive(false); // ซ่อน zettext
    }
    
    public void SetActorNumber(int actorNumber)
    {
        ActorNumber = actorNumber;
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

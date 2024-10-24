using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayerControlT : MonoBehaviourPunCallbacks
{
    public TMP_Text NameText;
    public TMP_Text ScoreText;
    public GameObject zettext;
    public Image playerIconImage;
    public Sprite[] iconSprites;
    public int ActorNumber { get; private set; }
    private int currentScore = 0; 
    
    void Start()
    {
        zettext.SetActive(false); 
    }

    public void ActivateZetText()
    {
        zettext.SetActive(true); 
    }

    public void DeactivateZetText()
    {
        zettext.SetActive(false); 
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
        currentScore = newScore; 
        if (currentScore < 0)
        {
            currentScore = 0; 
        }
        ScoreText.text = "Score: " + currentScore.ToString(); 
    }

    public void ResetScore()
    {
        currentScore = 0; 
        ScoreText.text = "Score: " + currentScore.ToString(); 
    }

    public void ResetZetStatus()
    {
        zettext.SetActive(false); 
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
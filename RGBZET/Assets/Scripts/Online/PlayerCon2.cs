using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayerCon2 : MonoBehaviourPunCallbacks
{
    
    public TMP_Text NameText;
    public TMP_Text ScoreText;
    public GameObject zettext; 

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

    
}

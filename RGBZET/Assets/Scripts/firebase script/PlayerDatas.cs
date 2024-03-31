using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDatas : MonoBehaviour
{
    public TMP_Text UsernameText;
    public TMP_Text ScoreText;

    public void NewDatas (string username, int score)
    {
        UsernameText.text = username;
        ScoreText.text = score.ToString();
       
    }

}

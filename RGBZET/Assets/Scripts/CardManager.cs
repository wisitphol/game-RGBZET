using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardManager : MonoBehaviour
{
     public Text scoreText; // อ้างอิง Text UI สำหรับแสดงคะแนน

    private int score = 0; // คะแนนเริ่มต้น

    void Start()
    {
        UpdateScoreText(); // เรียกใช้เมื่อเริ่มเกมเพื่อแสดงคะแนนเริ่มต้น
    }

    // เพิ่มคะแนนและอัปเดต UI Text
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    // อัปเดต UI Text ด้วยคะแนนปัจจุบัน
    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}

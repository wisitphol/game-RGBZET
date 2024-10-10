using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayerLobbyQ : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text ReadyText;
    public Image playerIconImage;  // เพิ่มตัวแปรสำหรับแสดงไอคอน
    public Sprite[] iconSprites;    // เพิ่มตัวแปรสำหรับเก็บสไปต์ไอคอน
    public int ActorNumber { get; private set; }

    public void UpdatePlayerInfo(string name, string ready)
    {

        if (NameText != null)
        {
            NameText.text = name;
        }

        if (ReadyText != null)
        {
            ReadyText.text = ready;
        }
    }
    public void SetActorNumber(int actorNumber)
    {
        ActorNumber = actorNumber;
    }

       // ฟังก์ชันสำหรับอัปเดตรูปไอคอนของผู้เล่น
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

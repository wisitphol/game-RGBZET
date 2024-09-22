using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayerLobby2 : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text ReadyText;
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
}

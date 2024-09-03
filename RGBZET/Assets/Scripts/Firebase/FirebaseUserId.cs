using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseUserId : MonoBehaviourPunCallbacks
{
    public string UserId { get; private set; }

    void Start()
    {
        // ดึงข้อมูล Firebase User ID
        UserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        // ตรวจสอบว่า Firebase User ID ถูกดึงมาได้อย่างถูกต้องหรือไม่
        if (!string.IsNullOrEmpty(UserId))
        {
            // ดึงข้อมูลอื่นๆ ของผู้เล่น เช่น Display Name และ Email
            string displayName = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
            string email = FirebaseAuth.DefaultInstance.CurrentUser.Email;

            Debug.Log("Firebase User ID retrieved: " + UserId);
            Debug.Log("User Display Name: " + displayName);
            Debug.Log("User Email: " + email);

            // เก็บ Firebase User ID ใน Custom Properties ของ Photon Player
            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "FirebaseUserId", UserId }
        };
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

            // Debug log เพื่อยืนยันว่าข้อมูลถูกบันทึกใน Photon Player's Custom Properties
            Debug.Log("Firebase User ID successfully set in Photon Player's Custom Properties: " + PhotonNetwork.LocalPlayer.CustomProperties["FirebaseUserId"]);
        }
        else
        {
            Debug.LogError("Failed to retrieve Firebase User ID.");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

public class PlayerIcon : MonoBehaviourPunCallbacks
{
    [SerializeField] private Image playerIconImage;
    [SerializeField] private Sprite[] iconSprites;
    private DatabaseReference userRef;

    void Start()
    {
        if (AuthManager.Instance.IsUserLoggedIn())
        {
            string userId = AuthManager.Instance.GetCurrentUserId();
            Debug.Log($"User ID: {userId}");

            // ตรวจสอบว่าค่า userId เป็น null หรือไม่
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("User ID is null or empty.");
                return;
            }

            // สร้าง userRef จาก userId
            userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);
            Debug.Log($"User Reference: {userRef}");

            LoadUserIcon();
        }
        else
        {
            Debug.LogError("User is not logged in.");
        }
    }

    public void LoadUserIcon()
    {
        if (iconSprites == null || iconSprites.Length == 0)
        {
            Debug.LogError("Icon Sprites are not assigned or empty.");
            return;
        }

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    if (snapshot.Child("icon").Exists)
                    {
                        int iconId = int.Parse(snapshot.Child("icon").Value.ToString());

                        // อัปเดต Sprite ของตัวเอง
                        playerIconImage.sprite = iconSprites[iconId];

                        // แชร์ iconId ผ่าน Photon Custom Properties
                        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
                        playerProperties["iconId"] = iconId;
                        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties); // อัปเดตข้อมูลไปยัง Photon
                    }
                }
                else
                {
                    Debug.Log("Failed to load user data.");
                }
            }
            else
            {
                Debug.Log("Failed to load user data.");
            }
        });
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("iconId"))
        {
            int iconId = (int)changedProps["iconId"];

            if (targetPlayer == PhotonNetwork.LocalPlayer)
            {
                // ถ้าเป็นผู้เล่นคนปัจจุบัน
                playerIconImage.sprite = iconSprites[iconId];
            }
            else
            {
                // ถ้าต้องการแสดงรูปผู้เล่นอื่นใน object อื่น ๆ สามารถเพิ่มโค้ดตรงนี้ได้
                Debug.Log($"Player {targetPlayer.NickName} has updated their icon to {iconId}");
            }
        }
    }
}

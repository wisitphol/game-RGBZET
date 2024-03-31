using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;

public class PlayerDataHandler : MonoBehaviour
{
    DatabaseReference databaseReference;

    void Start()
    {
        // เชื่อมต่อ Firebase Realtime Database
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
                return;
            }

            // เมื่อเชื่อมต่อสำเร็จ จะได้รับอินสแตนซ์ FirebaseApp และเปิดใช้งาน Realtime Database
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        });
    }

    // ฟังก์ชันเพื่อบันทึกข้อมูลผู้เล่น
    public void SavePlayerData(string playerName, string playerImageURL, int score)
    {
        // สร้าง object ข้อมูลผู้เล่น
        PlayerData playerData = new PlayerData(playerName, playerImageURL, score);

        // แปลงข้อมูลผู้เล่นเป็น JSON format
        string json = JsonUtility.ToJson(playerData);

        // บันทึกข้อมูลผู้เล่นลงใน Firebase Realtime Database
        databaseReference.Child("players").Child(playerName).SetRawJsonValueAsync(json);
    }
}

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public string playerImageURL;
    public int score;

    public PlayerData(string playerName, string playerImageURL, int score)
    {
        this.playerName = playerName;
        this.playerImageURL = playerImageURL;
        this.score = score;
    }
}

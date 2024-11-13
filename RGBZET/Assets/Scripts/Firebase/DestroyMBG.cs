using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyMBG : MonoBehaviour
{
    public string[] scenesWithoutMusic;  // รายชื่อ Scene ที่ไม่ต้องการเพลงพื้นหลัง

    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // ตรวจสอบว่า Scene ที่กำลังใช้งานอยู่มีชื่อในรายการหรือไม่
        foreach (string sceneName in scenesWithoutMusic)
        {
            if (currentSceneName == sceneName)
            {
                DestroyBackgroundMusic();
                return;
            }
        }
    }

    void DestroyBackgroundMusic()
    {
         BackgroundMusic backgroundMusic = FindObjectOfType<BackgroundMusic>();
       if (backgroundMusic != null)
        {
            Destroy(backgroundMusic.gameObject);
            Debug.Log("Destroyed Background Music in scene: " + SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("No Background Music found to destroy.");
        }
    }
}

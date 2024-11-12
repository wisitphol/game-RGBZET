using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Framerate : MonoBehaviour
{
    //private float deltaTime = 0.0f;
    public string firstSceneName = "SplashScreen";  // เปลี่ยนชื่อ scene แรกตามที่คุณใช้
    void Start()
    {
         // Set the target frame rate to 60 frames per second
        Application.targetFrameRate = 60;
    }

   /* void Update()
    {
        // คำนวณ deltaTime สำหรับการคำนวณ FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f; 
    }

    void OnGUI()
    {
        // เช็คว่าเป็น scene แรกหรือไม่
        if (SceneManager.GetActiveScene().name == firstSceneName)
        {
            // แสดง FPS ที่มุมบนซ้าย
            int fps = Mathf.CeilToInt(1.0f / deltaTime);
            GUI.Label(new Rect(10, 10, 100, 20), fps + " FPS");
        }
    }*/
}

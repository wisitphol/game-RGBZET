using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;  // ใช้สำหรับเก็บ instance เดียว
    public AudioSource backgroundMusicSource;  // ตัวแปรสำหรับ AudioSource ของเพลงพื้นหลัง

    void Awake()
    {

        // ตรวจสอบว่า instance ของเพลงพื้นหลังมีอยู่แล้วหรือไม่
        if (instance != null)
        {
            Debug.Log("BackgroundMusic: Destroying duplicate object.");
            // ถ้ามี instance อื่นแล้ว ให้ทำลาย GameObject นี้
            Destroy(gameObject);
        }
        else
        {
            // ถ้าไม่มี instance อื่นตั้งไว้ ให้ตั้งให้เป็น instance เดียวกัน
            instance = this;
            DontDestroyOnLoad(gameObject);  // ทำให้ GameObject นี้ไม่ถูกทำลายเมื่อเปลี่ยน Scene
        }
    }

    void Start()
    {
        // อ่านค่าระดับเสียงจาก PlayerPrefs และปรับเสียงให้เหมาะสม
        float savedVolume = PlayerPrefs.GetFloat("backgroundMusicVolume", 0.3f);  // ถ้าไม่มีจะใช้ค่าเริ่มต้นที่ 1
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = savedVolume;  // ปรับระดับเสียงเพลงพื้นหลัง
        }
    }

    // ฟังก์ชันสำหรับการปรับระดับเสียงจาก Setting
    public void SetVolume(float volume)
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = volume;
        }
        // บันทึกค่าระดับเสียงใน PlayerPrefs
        PlayerPrefs.SetFloat("backgroundMusicVolume", volume);
        PlayerPrefs.Save();  // บันทึกการเปลี่ยนแปลง
    }
}

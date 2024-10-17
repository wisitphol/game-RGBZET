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
using System.Linq;
public class SettingUI : MonoBehaviour
{
    [SerializeField] public Button backToMenu;
    [SerializeField] public Button soundButton;
    [SerializeField] public Button accountButton;
    [SerializeField] public Button quitButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button NoButton;
    [SerializeField] private Button YesButton;
    [SerializeField] public GameObject soundPanel;
    [SerializeField] public GameObject accountPanel;
    [SerializeField] public GameObject quitPanel;
    [SerializeField] public Slider volumeSlider;  // ตัวเลื่อนสำหรับควบคุมเสียง
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        backToMenu.onClick.AddListener(() => SoundOnClick(BackButtonClicked));
        soundPanel.SetActive(false);
        accountPanel.SetActive(false);
        quitPanel.SetActive(false);

        float savedVolume = PlayerPrefs.GetFloat("volume", 1f); // ดึงค่าที่บันทึกไว้, ถ้าไม่มีจะใช้ค่าเริ่มต้นเป็น 1
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume; // ตั้งค่าเริ่มต้นให้ตรงกับระดับเสียงที่บันทึกไว้

        soundButton.onClick.AddListener(() => SoundOnClick(ToggleSound));
        accountButton.onClick.AddListener(() => SoundOnClick(ToggleAccount));
        quitButton.onClick.AddListener(() => SoundOnClick(ToggleQuit));

        // ตั้งค่า Slider เริ่มต้น
        volumeSlider.onValueChanged.AddListener(AdjustVolume);
        volumeSlider.value = AudioListener.volume; // ตั้งค่าเริ่มต้นให้ตรงกับระดับเสียงปัจจุบัน

        logoutButton.onClick.AddListener(() => SoundOnClick(OnLogoutButtonClicked));

        YesButton.onClick.AddListener(() => SoundOnClick(QuitGame)); // เพิ่ม Listener ให้ปุ่ม Yes
        NoButton.onClick.AddListener(() => SoundOnClick(ToggleQuit)); // ปุ่ม No จะปิด quitPanel
    }

    private void BackButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    void ToggleSound()
    {
        bool isActive = soundPanel.activeSelf;
        soundPanel.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่

        accountPanel.SetActive(false);
        quitPanel.SetActive(false);

    }

    void ToggleAccount()
    {
        bool isActive = accountPanel.activeSelf;
        accountPanel.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่

        soundPanel.SetActive(false);
        quitPanel.SetActive(false);
    }

    void ToggleQuit()
    {
        bool isActive = quitPanel.activeSelf;
        quitPanel.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่

        soundPanel.SetActive(false);
        accountPanel.SetActive(false);
    }

    void AdjustVolume(float value)
    {
        AudioListener.volume = value; // ปรับระดับเสียงตามค่า Slider
        PlayerPrefs.SetFloat("volume", value); // บันทึกค่าเสียง
        PlayerPrefs.Save(); // บันทึกการเปลี่ยนแปลงใน PlayerPrefs
    }

    void OnLogoutButtonClicked()
    {
        AuthManager.Instance.Logout();
        // Logout method in AuthManager already handles scene transition
    }

    void SoundOnClick(System.Action buttonAction)
    {
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
            // รอให้เสียงเล่นเสร็จก่อนที่จะทำการเปลี่ยน scene
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            // ถ้าไม่มีเสียงให้เล่น ให้ทำงานทันที
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        // รอความยาวของเสียงก่อนที่จะทำงาน
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }

    void QuitGame()
    {
        Application.Quit(); // ออกจากเกม
        // สำหรับการทดสอบใน Unity Editor คุณสามารถใช้
        UnityEditor.EditorApplication.isPlaying = false;
    }
}

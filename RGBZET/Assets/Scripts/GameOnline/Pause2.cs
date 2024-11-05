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
public class Pause2 : MonoBehaviourPunCallbacks
{
    [SerializeField] public Button pauseButton;
    [SerializeField] public GameObject pausePanel;
    [SerializeField] public Button menuButton;
    private DatabaseReference databaseReference;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;
    void Start()
    {
        // ซ่อนแผง pause panel ตอนเริ่มเกม
        pausePanel.SetActive(false);

        // เพิ่ม listener ให้กับปุ่ม pause
        pauseButton.onClick.AddListener(() => SoundOnClick(TogglePause));

    }

    void TogglePause()
    {
        bool isActive = pausePanel.activeSelf;
        pausePanel.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่
        menuButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
    }

     void SoundOnClick(System.Action buttonAction)
    {
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
            StartCoroutine(WaitForSound(buttonAction));
        }
        else
        {
            buttonAction.Invoke();
        }
    }

    private IEnumerator WaitForSound(System.Action buttonAction)
    {
        yield return new WaitForSeconds(buttonSound.length);
        buttonAction.Invoke();
    }
}

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

public class PauseQ : MonoBehaviourPunCallbacks
{
    [SerializeField] public Button pauseButton;
    [SerializeField] public GameObject pausePanel;
    [SerializeField] public Button menuButton;
    [SerializeField] public Button guideButton;
    [SerializeField] public GameObject guidePanel;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        pausePanel.SetActive(false);
        pauseButton.onClick.AddListener(() => SoundOnClick(TogglePause));

        guidePanel.SetActive(false);
        guideButton.onClick.AddListener(() => SoundOnClick(ToggleGuide));
    }

    void TogglePause()
    {
        bool isActive = pausePanel.activeSelf;
        pausePanel.SetActive(!isActive);
        menuButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
    }

      void ToggleGuide()
    {
        bool isActive = guidePanel.activeSelf;
        guidePanel.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่
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
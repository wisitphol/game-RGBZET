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
    private DatabaseReference databaseReference;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        pausePanel.SetActive(false);
        pauseButton.onClick.AddListener(() => SoundOnClick(TogglePause));
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference("quickplay").Child(PlayerPrefs.GetString("RoomId"));
    }

    void TogglePause()
    {
        bool isActive = pausePanel.activeSelf;
        pausePanel.SetActive(!isActive);
        menuButton.onClick.AddListener(() => SoundOnClick(MenuButtonClicked));
    }

    private void MenuButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DeleteRoomAndGoToMenu());
        }
        else
        {
            StartCoroutine(LeaveRoomAndCheckConnection());
        }
    }

    private IEnumerator DeleteRoomAndGoToMenu()
    {
        yield return new WaitForSeconds(1f); 
        var task = databaseReference.RemoveValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Can't delete room from Firebase.");
        }
        else
        {
            Debug.Log("Can delete room from Firebase.");
        }

        PhotonNetwork.LeaveRoom();

        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
    }

    private IEnumerator LeaveRoomAndCheckConnection()
    {
        yield return new WaitForSeconds(1f); 
        PhotonNetwork.LeaveRoom();

        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        SceneManager.LoadScene("Menu");
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
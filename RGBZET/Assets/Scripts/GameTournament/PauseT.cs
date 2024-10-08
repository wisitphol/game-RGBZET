using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using Firebase.Database;

public class PauseT : MonoBehaviourPunCallbacks
{
    [SerializeField] public Button pauseButton;
    [SerializeField] public GameObject pausePanel;
    [SerializeField] public Button menuButton;
    private MutiManageT mutiManageT;

    void Start()
    {
        pausePanel.SetActive(false);
        pauseButton.onClick.AddListener(TogglePause);
        mutiManageT = FindObjectOfType<MutiManageT>();
    }

    void TogglePause()
    {
        bool isActive = pausePanel.activeSelf;
        pausePanel.SetActive(!isActive);
        menuButton.onClick.RemoveAllListeners(); // Remove previous listeners to prevent stacking
        menuButton.onClick.AddListener(MenuButtonClicked);
    }

    private void MenuButtonClicked()
    {
        // Treat this like a disconnect, making the current player a loser
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            StartCoroutine(HandlePlayerDisconnection());
        }
    }

    private IEnumerator HandlePlayerDisconnection()
    {
        // Notify MutiManageT to handle the disconnection as if the player has lost
        mutiManageT.EndGameDueToDisconnection();

        yield return new WaitForSeconds(1f); // Wait for the game to process the disconnection

        // Leave the Photon room and disconnect
        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        // Load the menu scene after disconnecting
        SceneManager.LoadScene("Menu");
    }
}
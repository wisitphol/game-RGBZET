using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;

public class ModeCreateroomUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button quickplayButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        createRoomButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("CreateFriend")));
        quickplayButton.onClick.AddListener(() => SoundOnClick(OnQuickplayButtonClicked));
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
    }

    void OnQuickplayButtonClicked()
    {
        DisplayFeedback("Connecting to Quickplay...");
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            SetPlayerNameAndJoinQuickplay();
        }
    }

    private void SetPlayerNameAndJoinQuickplay()
    {
        string playerName = AuthManager.Instance.GetCurrentUsername();
        PhotonNetwork.NickName = playerName;
        
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "username", playerName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        DisplayFeedback("Joining Quickplay...");
        PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable { { "GameType", "Quickplay" } }, 4);
    }

    public override void OnConnectedToMaster()
    {
        DisplayFeedback("Connected to Master. Setting up player...");
        SetPlayerNameAndJoinQuickplay();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        DisplayFeedback("Creating new Quickplay room...");
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "GameType", "Quickplay" } },
            CustomRoomPropertiesForLobby = new string[] { "GameType" }
        };
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        DisplayFeedback("Joined Quickplay room. Loading lobby...");
        SceneManager.LoadScene("QuickplayLobby");
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

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }

    void OnDestroy()
    {
        createRoomButton.onClick.RemoveAllListeners();
        quickplayButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }
}
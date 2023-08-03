using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject registrationPanel;

    [SerializeField]
    private GameObject gamePanel;

    [SerializeField]
    private GameObject SearchRoomPanel;

    [SerializeField]
    private GameObject CreateRoomPanel;

    [SerializeField]
    private GameObject UIRoomPanel;

    [SerializeField]
    private GameObject UIGamePanel;
    

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void ClearUI(){
        registrationPanel.SetActive(false);
        loginPanel.SetActive(false);
        gamePanel.SetActive(false);
        SearchRoomPanel.SetActive(false);
        CreateRoomPanel.SetActive(false);
        UIRoomPanel.SetActive(false);
        UIGamePanel.SetActive(false);
    }

    public void OpenLoginPanel()
    {
        ClearUI();
        loginPanel.SetActive(true);
    }

    public void OpenRegistrationPanel()
    {
        ClearUI();
        registrationPanel.SetActive(true);
    }

    public void OpenGamePanel(){
        ClearUI();
        gamePanel.SetActive(true);
    }

    public void OpenSearchRoomPanel()
    {
        ClearUI();
        SearchRoomPanel.SetActive(true); 
    }

    public void OpenCreateRoomPanel()
    {
        ClearUI();
        CreateRoomPanel.SetActive(true); 
    }

    public void OpenUIRoomPanel()
    {
        ClearUI();
        UIRoomPanel.SetActive(true); 
    }

    public void OpenUIGamePanel()
    {
        ClearUI();
        UIGamePanel.SetActive(true); 
    }
}

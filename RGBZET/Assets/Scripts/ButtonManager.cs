using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void OnPlayButtonClicked()
    {
        // Load the Play scene when the "Play" button is clicked
        SceneManager.LoadScene("Lobby"); //create room auto
    }
    public void OnCreateAndJoinButtonClicked()
    {
        // Handle the Create and Join button click, e.g., create a new game room
        // or join an existing one.
        // เพิ่มโค้ดที่คุณต้องการในส่วนนี้
    }

    public void OnTournamentButtonClicked()
    {
        // Handle the Tournament button click, e.g., load the tournament scene.
        // เพิ่มโค้ดที่คุณต้องการในส่วนนี้
        //SceneManager.LoadScene("Tournament"); // Auto check user 1.create room or 2.go to tournament
    }

    public void OnCreateNewAccountClicked() //open scene register
    {
        SceneManager.LoadScene("Register");
    }
    // Add other functions and logic as needed
}


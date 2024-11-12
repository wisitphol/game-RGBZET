using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] public Button backButton;
    [SerializeField] public Button Button1;
    [SerializeField] public Button Button2;
    [SerializeField] public GameObject Tutorial1;
    [SerializeField] public GameObject Tutorial2;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip buttonSound;

    void Start()
    {
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
        Tutorial1.SetActive(true);
        Tutorial2.SetActive(false);
       

        Button1.onClick.AddListener(() => SoundOnClick(ToggleTutorial1));
        Button2.onClick.AddListener(() => SoundOnClick(ToggleTutorial2));
        
    }

    void ToggleTutorial1()
    {
        bool isActive = Tutorial1.activeSelf;
        Tutorial1.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่

        Tutorial2.SetActive(false);
        
    }

    void ToggleTutorial2()
    {
        bool isActive = Tutorial2.activeSelf;
        Tutorial2.SetActive(!isActive); // แสดงถ้าถูกซ่อน, ซ่อนถ้าแสดงอยู่

        Tutorial1.SetActive(false);
        
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
    
}

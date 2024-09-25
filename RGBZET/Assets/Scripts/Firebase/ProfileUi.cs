using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ProfileUi : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private Button editUsernameButton;
    [SerializeField] private Button saveUsernameButton;
    [SerializeField] private TMP_Text matchText;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text loseText;
    [SerializeField] private TMP_Text drawText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button BackButton;
    [SerializeField] private Button saveIconButton;
    [SerializeField] private TMP_Dropdown iconDropdown;
    [SerializeField] private Image playerIconImage;
    [SerializeField] private Sprite[] iconSprites;

    private DatabaseReference userRef;
    private string currentUsername;
    void Start()
    {
        if (AuthManager.Instance.IsUserLoggedIn())
        {
            string userId = AuthManager.Instance.GetCurrentUserId();
            userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);
            LoadUserData();

            PopulateIconDropdown();
        }
        usernameInputField.interactable = false;

        BackButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        editUsernameButton.onClick.AddListener(OnEditUsernameButtonClicked);
        saveUsernameButton.onClick.AddListener(OnSaveUsernameButtonClicked);

        saveIconButton.onClick.AddListener(OnSaveIconButtonClicked);  // เพิ่มการตั้งค่าให้กับปุ่มบันทึกไอคอน

        // ตั้งค่าตัวเลือกใน dropdown ตามจำนวนไอคอน
        iconDropdown.onValueChanged.AddListener(OnIconDropdownChanged);
    }
    void LoadUserData()
    {
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    currentUsername = snapshot.Child("username").Value.ToString();
                    usernameText.text = currentUsername;

                    // ดึงข้อมูลจำนวนเกมที่เล่น ชนะ แพ้ และเสมอ
                    // ดึงข้อมูลจำนวนเกมที่เล่น ชนะ แพ้ และเสมอ พร้อมแสดงข้อความที่ต้องการ
                    matchText.text = snapshot.Child("gamescount").Value != null ? "Matches: " + snapshot.Child("gamescount").Value.ToString() : "Matches: 0";
                    winText.text = snapshot.Child("gameswin").Value != null ? "Win: " + snapshot.Child("gameswin").Value.ToString() : "Win: 0";
                    loseText.text = snapshot.Child("gameslose").Value != null ? "Lose: " + snapshot.Child("gameslose").Value.ToString() : "Lose: 0";
                    drawText.text = snapshot.Child("gamesdraw").Value != null ? "Draw: " + snapshot.Child("gamesdraw").Value.ToString() : "Draw: 0";

                    if (snapshot.Child("icon").Value != null)
                    {
                        int iconId = int.Parse(snapshot.Child("icon").Value.ToString());
                        playerIconImage.sprite = iconSprites[iconId];  // ตั้งค่าไอคอนใน UI
                        iconDropdown.value = iconId;  // ตั้งค่าให้ dropdown แสดงค่าไอคอนที่ถูกเลือก
                    }
                }
                else
                {
                    DisplayFeedback("Failed to load user data.");
                }
            }
            else
            {
                DisplayFeedback("Failed to load user data.");
            }
        });
    }



    void OnEditUsernameButtonClicked()
    {
        usernameText.gameObject.SetActive(false);
        //editUsernameButton.gameObject.SetActive(false);
        //usernameInputField.gameObject.SetActive(true);
        //saveUsernameButton.gameObject.SetActive(true);

        usernameInputField.interactable = true;
        usernameInputField.text = currentUsername;
    }

    async void OnSaveUsernameButtonClicked()
    {
        string newUsername = usernameInputField.text.Trim();
        if (string.IsNullOrEmpty(newUsername))
        {
            DisplayFeedback("Username cannot be empty.");
            return;
        }

        if (newUsername == currentUsername)
        {
            CancelUsernameEdit();
            return;
        }

        bool isAvailable = await CheckUsernameAvailability(newUsername);
        if (!isAvailable)
        {
            DisplayFeedback("This username is already taken.");
            return;
        }

        UpdateUsername(newUsername);
    }

    async Task<bool> CheckUsernameAvailability(string username)
    {
        var snapshot = await FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .OrderByChild("username")
            .EqualTo(username)
            .GetValueAsync();

        return !snapshot.Exists;
    }

    void UpdateUsername(string newUsername)
    {
        AuthManager.Instance.UpdateUserProfile(newUsername, (success) =>
        {
            if (success)
            {
                userRef.Child("username").SetValueAsync(newUsername).ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        currentUsername = newUsername;
                        usernameText.text = newUsername;
                        DisplayFeedback("Username updated successfully.");
                        CancelUsernameEdit();
                    }
                    else
                    {
                        DisplayFeedback("Failed to update username in the database.");
                    }
                });
            }
            else
            {
                DisplayFeedback("Failed to update username.");
            }
        });
    }

    void CancelUsernameEdit()
    {
        usernameText.gameObject.SetActive(true);
        //editUsernameButton.gameObject.SetActive(true);
        //usernameInputField.gameObject.SetActive(false);
        //saveUsernameButton.gameObject.SetActive(false);

        usernameInputField.interactable = false;
    }

    private void OnIconDropdownChanged(int iconId)
    {
        playerIconImage.sprite = iconSprites[iconId]; // อัปเดตรูปไอคอนใน UI
    }

    // ฟังก์ชันสำหรับบันทึกไอคอนที่เลือก
    private void OnSaveIconButtonClicked()
    {
        int selectedIconId = iconDropdown.value; // รับค่าไอคอนที่เลือกจาก dropdown
        userRef.Child("icon").SetValueAsync(selectedIconId).ContinueWith(task => // บันทึกค่า iconId ลง Firebase
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                // อัปเดตรูปไอคอนใน UI
                playerIconImage.sprite = iconSprites[selectedIconId];
                DisplayFeedback("Icon changed successfully.");
            }
            else
            {
                DisplayFeedback("Failed to change icon.");
            }
        });
    }

    private void PopulateIconDropdown()
    {
        iconDropdown.ClearOptions(); // ลบตัวเลือกเก่าออก
        List<string> options = new List<string>();

        for (int i = 0; i < iconSprites.Length; i++)
        {
            options.Add($"Icon {i + 1}"); // เพิ่มตัวเลือกตามจำนวนไอคอน
        }

        iconDropdown.AddOptions(options); // เพิ่มตัวเลือกใหม่ลงใน dropdown
    }

    public void DisplayFeedback(string message)
    {
        feedbackText.text = message;
    }
}

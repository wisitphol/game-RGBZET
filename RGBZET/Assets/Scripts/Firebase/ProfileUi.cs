using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

public class ProfileUI : MonoBehaviour
{
    [Header("Profile Info")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text matchText;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text loseText;
    [SerializeField] private TMP_Text drawText;
    [SerializeField] private TMP_Text wintournamentText;

    [Header("Buttons")]
    [SerializeField] private Button changeNameButton;
    [SerializeField] private Button backButton;

    [Header("Icon Selection")]
    [SerializeField] private Image currentPlayerIcon;
    [SerializeField] private GameObject iconSelectionPanel;
    [SerializeField] private Sprite[] iconSprites;

    [Header("Change Name Popup")]
    [SerializeField] private GameObject changeNamePopup;
    [SerializeField] private TMP_InputField newUsernameInput;
    [SerializeField] private Button saveNameButton;
    [SerializeField] private Button cancelNameButton;

    [Header("Notification")]
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private TMP_Text notificationText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private DatabaseReference userRef;
    private string currentUsername;
    private Button[] iconButtons;
    private const int MAX_USERNAME_LENGTH = 10;

    void Start()
    {
        if (AuthManager.Instance.IsUserLoggedIn())
        {
            string userId = AuthManager.Instance.GetCurrentUserId();
            userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);
            LoadUserData();
            SetupIconSelection();
        }

        SetupButtons();
        InitializePopups();
        SetupInputField();
    }

    void SetupInputField()
    {
        // จำกัดจำนวนตัวอักษรใน InputField
        newUsernameInput.characterLimit = MAX_USERNAME_LENGTH;
        
        // เพิ่ม event listener สำหรับการเปลี่ยนแปลงข้อความ
        newUsernameInput.onValueChanged.AddListener(OnUsernameInputChanged);
    }

    void OnUsernameInputChanged(string newValue)
    {
        if (newValue.Length > MAX_USERNAME_LENGTH)
        {
            newUsernameInput.text = newValue.Substring(0, MAX_USERNAME_LENGTH);
            ShowNotification($"Username cannot exceed {MAX_USERNAME_LENGTH} characters");
        }
    }

    void SetupIconSelection()
    {
        iconButtons = new Button[iconSprites.Length];
        
        // ปรับขนาดไอคอนที่นี่
        float iconSize = 150f; // เพิ่มขนาดเป็น 150x150
        float spacing = 20f;   // เพิ่มระยะห่างระหว่างไอคอน
        
        for (int i = 0; i < iconSprites.Length; i++)
        {
            GameObject buttonObj = new GameObject($"IconButton_{i}");
            buttonObj.transform.SetParent(iconSelectionPanel.transform, false);
            
            Button button = buttonObj.AddComponent<Button>();
            iconButtons[i] = button;
            
            // เพิ่ม Background สำหรับปุ่ม
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(1, 1, 1, 0.1f); // สีพื้นหลังโปร่งใส
            
            // สร้าง GameObject แยกสำหรับรูปไอคอน
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);
            
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = iconSprites[i];
            iconImage.preserveAspect = true;
            
            // ตั้งค่า RectTransform ของปุ่ม
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(iconSize, iconSize);
            
            // ตั้งค่า RectTransform ของไอคอน
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(1, 1);
            iconRect.sizeDelta = new Vector2(-20, -20); // ขอบ padding 10 pixel รอบไอคอน
            iconRect.anchoredPosition = Vector2.zero;
            
            int iconIndex = i;
            button.onClick.AddListener(() => SoundOnClick(() => OnIconSelected(iconIndex)));
            
            // เพิ่ม Hover Effect
            Button btnComponent = buttonObj.GetComponent<Button>();
            ColorBlock colors = btnComponent.colors;
            colors.normalColor = new Color(1, 1, 1, 0.1f);
            colors.highlightedColor = new Color(1, 1, 1, 0.3f);
            colors.pressedColor = new Color(1, 1, 1, 0.5f);
            colors.selectedColor = new Color(1, 1, 1, 0.3f);
            btnComponent.colors = colors;
        }

        // ตั้งค่า GridLayoutGroup
        GridLayoutGroup grid = iconSelectionPanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(iconSize, iconSize);
        grid.spacing = new Vector2(spacing, spacing);
        grid.padding = new RectOffset(20, 20, 20, 20); // เพิ่ม padding รอบ panel
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
    }

    // เพิ่มเมธอดใหม่สำหรับ Visual Feedback เมื่อเลือกไอคอน
    void OnIconSelected(int iconIndex)
    {
        ShowNotification("Saving icon...");
        currentPlayerIcon.sprite = iconSprites[iconIndex];
        
        // Visual feedback animation
        StartCoroutine(ScaleIconAnimation(iconButtons[iconIndex].gameObject));
        
        userRef.Child("icon").SetValueAsync(iconIndex).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                ShowNotification("Icon updated");
            }
            else
            {
                ShowNotification("Icon save failed");
            }
        });
    }

    // เพิ่ม Animation เมื่อเลือกไอคอน
    private IEnumerator ScaleIconAnimation(GameObject icon)
    {
        RectTransform rect = icon.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;
        float animationTime = 0.2f;
        
        // Scale up
        float elapsed = 0;
        while (elapsed < animationTime / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationTime / 2);
            rect.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, progress);
            yield return null;
        }
        
        // Scale back
        elapsed = 0;
        while (elapsed < animationTime / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationTime / 2);
            rect.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, progress);
            yield return null;
        }
        
        rect.localScale = originalScale;
    }

    void InitializePopups()
    {
        changeNamePopup.SetActive(false);
        notificationPopup.SetActive(false);

        saveNameButton.onClick.AddListener(() => SoundOnClick(SaveNewUsername));
        cancelNameButton.onClick.AddListener(() => SoundOnClick(() => changeNamePopup.SetActive(false)));
    }

    void SetupButtons()
    {
        backButton.onClick.AddListener(() => SoundOnClick(() => SceneManager.LoadScene("Menu")));
        changeNameButton.onClick.AddListener(() => SoundOnClick(ShowChangeNamePopup));
    }

    void ShowChangeNamePopup()
    {
        newUsernameInput.text = currentUsername;
        changeNamePopup.SetActive(true);
    }

    async void SaveNewUsername()
    {
        string newUsername = newUsernameInput.text.Trim();
        
        if (string.IsNullOrEmpty(newUsername))
        {
            ShowNotification("Username empty");
            return;
        }

        if (newUsername.Length > MAX_USERNAME_LENGTH)
        {
            ShowNotification($"Username cannot exceed {MAX_USERNAME_LENGTH} characters");
            return;
        }

        if (newUsername == currentUsername)
        {
            changeNamePopup.SetActive(false);
            return;
        }

        ShowNotification("Checking...");
        bool isAvailable = await CheckUsernameAvailability(newUsername);
        
        if (!isAvailable)
        {
            ShowNotification("Username taken");
            return;
        }

        UpdateUsername(newUsername);
    }

    void LoadUserData()
    {
        ShowNotification("Loading data...");
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                ShowNotification("Load failed");
                return;
            }

            if (!task.Result.Exists)
            {
                ShowNotification("No data found");
                return;
            }

            DataSnapshot snapshot = task.Result;
            currentUsername = snapshot.Child("username").Value.ToString();
            usernameText.text = currentUsername;

            UpdateStats(snapshot);
            UpdateIcon(snapshot);
            ShowNotification("Data loaded");
        });
    }

    void UpdateStats(DataSnapshot snapshot)
    {
        matchText.text = $"Matches: {GetSnapshotValue(snapshot, "gamescount", "0")}";
        winText.text = $"Win: {GetSnapshotValue(snapshot, "gameswin", "0")}";
        loseText.text = $"Lose: {GetSnapshotValue(snapshot, "gameslose", "0")}";
        drawText.text = $"Draw: {GetSnapshotValue(snapshot, "gamesdraw", "0")}";
        wintournamentText.text = $"WinTournament: {GetSnapshotValue(snapshot, "gameswintournament", "0")}";
    }

    void UpdateIcon(DataSnapshot snapshot)
    {
        if (snapshot.Child("icon").Exists)
        {
            int iconId = int.Parse(snapshot.Child("icon").Value.ToString());
            currentPlayerIcon.sprite = iconSprites[iconId];
        }
    }

    string GetSnapshotValue(DataSnapshot snapshot, string key, string defaultValue)
    {
        return snapshot.Child(key).Value?.ToString() ?? defaultValue;
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
        ShowNotification("Updating...");
        AuthManager.Instance.UpdateUserProfile(newUsername, (success) =>
        {
            if (success)
            {
                userRef.Child("username").SetValueAsync(newUsername).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        currentUsername = newUsername;
                        usernameText.text = newUsername;
                        changeNamePopup.SetActive(false);
                        newUsernameInput.text = "";
                        ShowNotification("Username updated");
                    }
                    else
                    {
                        ShowNotification("Update failed");
                    }
                });
            }
            else
            {
                ShowNotification("Update failed");
            }
        });
    }

    void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationPopup.SetActive(true);
        StartCoroutine(HideNotificationAfterDelay(3f));
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPopup.SetActive(false);
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

    void OnDestroy()
    {
        if (iconButtons != null)
        {
            foreach (var button in iconButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (changeNameButton != null) changeNameButton.onClick.RemoveAllListeners();
        if (saveNameButton != null) saveNameButton.onClick.RemoveAllListeners();
        if (cancelNameButton != null) cancelNameButton.onClick.RemoveAllListeners();
        if (newUsernameInput != null) newUsernameInput.onValueChanged.RemoveAllListeners();
    }
}
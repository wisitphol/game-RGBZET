using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LogoSceneController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image logoImage;
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private TextMeshProUGUI tapToContinueText;

    [Header("Animation Settings")]
    [SerializeField] private float logoFadeInDuration = 1f;
    [SerializeField] private float logoScaleDuration = 1f;
    [SerializeField] private float logoRotateDuration = 1f;
    [SerializeField] private float textFadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private void Start()
    {
        // เริ่มต้นซ่อนทุกอย่าง
        logoImage.color = new Color(1, 1, 1, 0);
        tapToContinueText.alpha = 0;
        fadePanel.alpha = 1;

        // เริ่ม animation sequence
        StartCoroutine(PlayLogoAnimation());
    }

    private IEnumerator PlayLogoAnimation()
    {
        // Fade out หน้าจอดำ
        yield return StartCoroutine(FadeCanvasGroup(fadePanel, 1, 0, 0.5f));

        // Fade in โลโก้
        yield return StartCoroutine(FadeImage(logoImage, 0, 1, logoFadeInDuration));

        // Scale animation
        yield return StartCoroutine(ScaleLogo());

        // Rotate animation
        yield return StartCoroutine(RotateLogo());

        // แสดงข้อความ tap to continue
        yield return StartCoroutine(FadeText(tapToContinueText, 0, 1, textFadeInDuration));

        // เปิดให้กดได้
        EnableTapToContinue();
    }

    private IEnumerator ScaleLogo()
    {
        float elapsed = 0;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 1.2f;
        
        while (elapsed < logoScaleDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / logoScaleDuration;
            
            // ใช้ animation curve แบบ smooth
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            logoImage.transform.localScale = Vector3.Lerp(startScale, endScale, smoothProgress);
            
            yield return null;
        }
        
        // Scale กลับ
        elapsed = 0;
        while (elapsed < logoScaleDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / logoScaleDuration;
            
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            logoImage.transform.localScale = Vector3.Lerp(endScale, startScale, smoothProgress);
            
            yield return null;
        }
    }

    private IEnumerator RotateLogo()
    {
        float elapsed = 0;
        Quaternion startRotation = Quaternion.identity;
        Quaternion endRotation = Quaternion.Euler(0, 0, 360);
        
        while (elapsed < logoRotateDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / logoRotateDuration;
            
            logoImage.transform.rotation = Quaternion.Lerp(startRotation, endRotation, progress);
            
            yield return null;
        }
        
        logoImage.transform.rotation = startRotation;
    }

    private IEnumerator FadeImage(Image image, float start, float end, float duration)
    {
        float elapsed = 0;
        Color startColor = image.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, end);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            image.color = Color.Lerp(startColor, endColor, progress);
            
            yield return null;
        }
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            text.alpha = Mathf.Lerp(start, end, progress);
            
            yield return null;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            group.alpha = Mathf.Lerp(start, end, progress);
            
            yield return null;
        }
    }

    private void EnableTapToContinue()
    {
        // เปิดให้กดได้
        enabled = true;
    }

    private void Update()
    {
        // เช็คว่ามีการแตะหน้าจอไหม
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            enabled = false; // ปิดไม่ให้กดซ้ำ
            StartCoroutine(TransitionToNextScene());
        }
    }

    private IEnumerator TransitionToNextScene()
    {
        // Fade out ทุกอย่าง
        yield return StartCoroutine(FadeImage(logoImage, 1, 0, fadeOutDuration));
        yield return StartCoroutine(FadeText(tapToContinueText, 1, 0, fadeOutDuration));
        yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0, 1, fadeOutDuration));

        // โหลดฉากถัดไป
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
}
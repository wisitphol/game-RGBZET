using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.1f;
    public float clickScale = 0.9f;
    public float animationDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine currentAnimation;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ScaleAnimation(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ScaleAnimation(originalScale));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ScaleAnimation(originalScale * clickScale));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ScaleAnimation(originalScale * hoverScale));
    }

    private IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < animationDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class GradientBackground : MonoBehaviour
{
    public Color topColor = new Color(0.4f, 0.6f, 1f);
    public Color bottomColor = new Color(0.8f, 0.9f, 1f);
    
    void Start()
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            Texture2D texture = new Texture2D(1, 2);
            texture.SetPixels(new Color[] { topColor, bottomColor });
            texture.Apply();
            image.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f));
        }
    }
}
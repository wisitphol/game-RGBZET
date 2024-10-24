using UnityEngine;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Camera mainCamera;
    
    [Header("Background Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.2f);
    [SerializeField] private Color gradientTopColor = new Color(0.2f, 0.2f, 0.4f);
    
    [Header("Particle Settings")]
    [SerializeField] private int particleCount = 50;
    [SerializeField] private float minParticleSize = 0.1f;
    [SerializeField] private float maxParticleSize = 0.5f;
    [SerializeField] private float minParticleSpeed = 0.5f;
    [SerializeField] private float maxParticleSpeed = 2f;
    [SerializeField] private float minParticleAlpha = 0.2f;
    [SerializeField] private float maxParticleAlpha = 0.5f;
    
    [Header("Spawn Area")]
    [SerializeField] private float spawnWidth = 10f;
    [SerializeField] private float spawnHeight = 5f;
    
    private List<ParticleObject> particles;
    private Material gradientMaterial;

    private class ParticleObject
    {
        public GameObject gameObject;
        public Vector3 velocity;
        public float size;
        public SpriteRenderer spriteRenderer;
    }

    void Awake()
    {
        mainCamera = Camera.main;
        particles = new List<ParticleObject>();
        SetupBackground();
        CreateParticles();
    }

    void SetupBackground()
    {
        // Create gradient background
        GameObject backgroundQuad = CreateBackgroundQuad();
        CreateGradientMaterial(backgroundQuad);
        
        // Set camera
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = backgroundColor;
    }

    GameObject CreateBackgroundQuad()
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = transform;
        quad.transform.localScale = new Vector3(spawnWidth, spawnHeight, 1);
        quad.transform.position = new Vector3(0, 0, 1);
        return quad;
    }

    void CreateGradientMaterial(GameObject backgroundQuad)
    {
        gradientMaterial = new Material(Shader.Find("Unlit/Transparent"));
        Texture2D gradientTexture = new Texture2D(1, 2);
        gradientTexture.SetPixel(0, 0, backgroundColor);
        gradientTexture.SetPixel(0, 1, gradientTopColor);
        gradientTexture.Apply();
        
        gradientMaterial.mainTexture = gradientTexture;
        backgroundQuad.GetComponent<MeshRenderer>().material = gradientMaterial;
    }

    void CreateParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            CreateParticle();
        }
    }

    void CreateParticle()
    {
        GameObject obj = Instantiate(particlePrefab, GetRandomPosition(), Quaternion.identity, transform);
        float size = Random.Range(minParticleSize, maxParticleSize);
        obj.transform.localScale = new Vector3(size, size, 1);

        ParticleObject particle = new ParticleObject
        {
            gameObject = obj,
            velocity = GetRandomVelocity(),
            size = size,
            spriteRenderer = obj.GetComponent<SpriteRenderer>()
        };

        // Set random transparency
        Color particleColor = particle.spriteRenderer.color;
        particleColor.a = Random.Range(minParticleAlpha, maxParticleAlpha);
        particle.spriteRenderer.color = particleColor;

        particles.Add(particle);
    }

    Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-spawnWidth/2, spawnWidth/2),
            Random.Range(-spawnHeight/2, spawnHeight/2),
            0
        );
    }

    Vector3 GetRandomVelocity()
    {
        float speed = Random.Range(minParticleSpeed, maxParticleSpeed);
        float angle = Random.Range(0, 2 * Mathf.PI);
        return new Vector3(
            Mathf.Cos(angle) * speed,
            Mathf.Sin(angle) * speed,
            0
        );
    }

    void Update()
    {
        UpdateParticles();
    }

    void UpdateParticles()
    {
        foreach (var particle in particles)
        {
            // Move particle
            particle.gameObject.transform.position += particle.velocity * Time.deltaTime;

            // Check if out of bounds
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(particle.gameObject.transform.position);
            
            if (IsOutOfBounds(viewportPoint))
            {
                // Reset position when particle goes off screen
                particle.gameObject.transform.position = GetRandomPosition();
                
                // Optionally change velocity
                particle.velocity = GetRandomVelocity();
            }
        }
    }

    bool IsOutOfBounds(Vector3 viewportPoint)
    {
        return viewportPoint.x < -0.1f || viewportPoint.x > 1.1f || 
               viewportPoint.y < -0.1f || viewportPoint.y > 1.1f;
    }

    void OnDestroy()
    {
        if (gradientMaterial != null)
        {
            Destroy(gradientMaterial.mainTexture);
            Destroy(gradientMaterial);
        }

        foreach (var particle in particles)
        {
            if (particle.gameObject != null)
            {
                Destroy(particle.gameObject);
            }
        }
    }
}
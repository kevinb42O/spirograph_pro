using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIGlowEffect : MonoBehaviour
{
    private Image glowImage;
    
    [Header("Glow Settings")]
    public Color glowColor = new Color(0.3f, 0.8f, 1f, 0.5f);
    public float glowIntensity = 1.5f;
    public float pulseSpeed = 2f;
    public bool enablePulse = true;
    
    [Header("Size")]
    public float glowSizeMultiplier = 1.2f;
    
    private Material glowMaterial;
    private float pulseTime = 0f;
    
    private void Awake()
    {
        glowImage = GetComponent<Image>();
        
        // Create a material instance for this glow effect
        if (glowImage.material != null)
        {
            glowMaterial = new Material(glowImage.material);
            glowImage.material = glowMaterial;
        }
        
        // Set initial glow properties
        glowImage.color = glowColor;
        transform.localScale *= glowSizeMultiplier;
    }
    
    private void Update()
    {
        if (enablePulse)
        {
            pulseTime += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTime) + 1f) * 0.5f; // 0 to 1
            
            Color currentColor = glowColor;
            currentColor.a = glowColor.a * (0.5f + pulse * 0.5f);
            
            if (glowImage != null)
            {
                glowImage.color = currentColor;
            }
        }
    }
    
    public void SetGlowColor(Color color)
    {
        glowColor = color;
        if (glowImage != null)
        {
            glowImage.color = glowColor;
        }
    }
    
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
        Color currentColor = glowColor;
        currentColor.a = intensity;
        if (glowImage != null)
        {
            glowImage.color = currentColor;
        }
    }
}
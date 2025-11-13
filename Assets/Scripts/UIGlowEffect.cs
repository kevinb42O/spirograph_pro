using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIGlowEffect : MonoBehaviour
{
    private Image glowImage;
    
    [Header("Glow Settings")]
    public Color glowColor = UIConstants.BlueGlow;
    public float glowIntensity = 1.5f;
    public float pulseSpeed = UIConstants.PulseSpeed;
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
            
            // Apply smooth easing to pulse for more organic feel
            float smoothPulse = UIConstants.SmoothEase(pulse);
            
            Color currentColor = glowColor;
            currentColor.a = glowColor.a * (0.5f + smoothPulse * 0.5f);
            
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
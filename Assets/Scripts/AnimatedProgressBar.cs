using UnityEngine;
using UnityEngine.UI;

public class AnimatedProgressBar : MonoBehaviour
{
    private Image fillImage;
    private float currentFillAmount = 0f;
    private float targetFillAmount = 0f;
    
    [Header("Animation Settings")]
    public float animationSpeed = UIConstants.AnimationSpeed / 2f; // Slower for smoother progress bars
    public bool smoothAnimation = true;
    
    [Header("Color Gradient")]
    public Gradient fillColorGradient;
    
    [Header("Pulse Effect")]
    public bool enablePulse = true;
    public float pulseSpeed = UIConstants.PulseSpeed;
    public float pulseIntensity = 0.1f;
    
    private void Awake()
    {
        fillImage = GetComponent<Image>();
        
        // Initialize color gradient if not set
        if (fillColorGradient == null || fillColorGradient.colorKeys.Length == 0)
        {
            fillColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(new Color(1f, 0.2f, 0.2f), 0f);  // Red at 0%
            colorKeys[1] = new GradientColorKey(new Color(1f, 0.8f, 0.2f), 0.5f); // Yellow at 50%
            colorKeys[2] = new GradientColorKey(new Color(0.2f, 1f, 0.2f), 1f);   // Green at 100%
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            
            fillColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    private void Update()
    {
        if (fillImage != null)
        {
            // Animate fill amount with smooth easing
            if (smoothAnimation)
            {
                float t = Time.deltaTime * animationSpeed;
                float easedT = UIConstants.SmoothEase(Mathf.Clamp01(t));
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, easedT);
            }
            else
            {
                currentFillAmount = targetFillAmount;
            }
            
            // Add pulse effect with smooth easing
            float displayFillAmount = currentFillAmount;
            if (enablePulse && currentFillAmount > 0f && currentFillAmount < 1f)
            {
                float rawPulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                float smoothPulse = UIConstants.SmoothEase((rawPulse + pulseIntensity) / (2f * pulseIntensity)) * 2f * pulseIntensity - pulseIntensity;
                displayFillAmount += smoothPulse;
            }
            
            fillImage.fillAmount = Mathf.Clamp01(displayFillAmount);
            
            // Update color based on progress with smooth gradient
            fillImage.color = fillColorGradient.Evaluate(currentFillAmount);
        }
    }
    
    public void SetProgress(float progress)
    {
        targetFillAmount = Mathf.Clamp01(progress);
    }
    
    public void SetProgressImmediate(float progress)
    {
        targetFillAmount = Mathf.Clamp01(progress);
        currentFillAmount = targetFillAmount;
        if (fillImage != null)
        {
            fillImage.fillAmount = currentFillAmount;
            fillImage.color = fillColorGradient.Evaluate(currentFillAmount);
        }
    }
}
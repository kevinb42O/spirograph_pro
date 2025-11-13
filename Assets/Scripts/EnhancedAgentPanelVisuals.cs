using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Adds stunning visual enhancements to the Agent Panel including:
/// - Animated background gradient
/// - Border glow effects
/// - Particle system for ambient effects
/// - Smooth transitions and animations
/// </summary>
public class EnhancedAgentPanelVisuals : MonoBehaviour
{
    [Header("Background Animation")]
    public bool animateBackground = true;
    public float backgroundAnimationSpeed = 0.3f;
    public Color backgroundColor1 = UIConstants.DarkSpaceGlass;
    public Color backgroundColor2 = UIConstants.DeepSpaceGlass;
    
    [Header("Border Glow")]
    public bool enableBorderGlow = true;
    public float borderGlowSpeed = UIConstants.PulseSpeed * 0.75f;
    public float borderMinIntensity = 0.4f;
    public float borderMaxIntensity = 0.8f;
    public Color borderGlowColor = UIConstants.CyanGlow;
    
    [Header("Particle Effects")]
    public bool enableParticles = true;
    public int particleCount = 30;
    public float particleSpeed = 20f;
    public Color particleColor = UIConstants.BlueGlow;
    
    [Header("Panel Scale Animation")]
    public bool enableScaleAnimation = true;
    public float scaleAnimationDuration = UIConstants.TransitionSlow;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Image backgroundImage;
    private Outline borderOutline;
    private GameObject particleContainer;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float animationTime = 0f;
    
    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        borderOutline = GetComponent<Outline>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        rectTransform = GetComponent<RectTransform>();
        
        SetupParticleEffects();
    }
    
    private void OnEnable()
    {
        if (enableScaleAnimation)
        {
            StartCoroutine(PlayShowAnimation());
        }
    }
    
    private void Update()
    {
        animationTime += Time.deltaTime;
        
        if (animateBackground && backgroundImage != null)
        {
            AnimateBackground();
        }
        
        if (enableBorderGlow && borderOutline != null)
        {
            AnimateBorderGlow();
        }
    }
    
    private void AnimateBackground()
    {
        float rawT = (Mathf.Sin(animationTime * backgroundAnimationSpeed) + 1f) * 0.5f;
        float smoothT = UIConstants.SmoothEase(rawT);
        backgroundImage.color = Color.Lerp(backgroundColor1, backgroundColor2, smoothT);
    }
    
    private void AnimateBorderGlow()
    {
        float rawT = (Mathf.Sin(animationTime * borderGlowSpeed) + 1f) * 0.5f;
        float smoothT = UIConstants.SmoothEase(rawT);
        float intensity = Mathf.Lerp(borderMinIntensity, borderMaxIntensity, smoothT);
        
        Color glowColor = borderGlowColor;
        glowColor.a = intensity;
        borderOutline.effectColor = glowColor;
    }
    
    private void SetupParticleEffects()
    {
        if (!enableParticles) return;
        
        // Create particle container
        particleContainer = new GameObject("AmbientParticles");
        particleContainer.transform.SetParent(transform, false);
        
        RectTransform particleRect = particleContainer.AddComponent<RectTransform>();
        particleRect.anchorMin = Vector2.zero;
        particleRect.anchorMax = Vector2.one;
        particleRect.sizeDelta = Vector2.zero;
        particleRect.anchoredPosition = Vector2.zero;
        
        // Create individual particle elements
        for (int i = 0; i < particleCount; i++)
        {
            CreateFloatingParticle(particleContainer);
        }
    }
    
    private void CreateFloatingParticle(GameObject parent)
    {
        GameObject particle = new GameObject($"Particle_{Random.Range(0, 10000)}");
        particle.transform.SetParent(parent.transform, false);
        
        RectTransform particleRect = particle.AddComponent<RectTransform>();
        particleRect.sizeDelta = new Vector2(Random.Range(2f, 5f), Random.Range(2f, 5f));
        particleRect.anchoredPosition = new Vector2(
            Random.Range(-200f, 200f),
            Random.Range(-350f, 350f)
        );
        
        Image particleImage = particle.AddComponent<Image>();
        particleImage.color = particleColor;
        
        FloatingParticleBehavior behavior = particle.AddComponent<FloatingParticleBehavior>();
        behavior.speed = particleSpeed;
        behavior.startPosition = particleRect.anchoredPosition;
    }
    
    private IEnumerator PlayShowAnimation()
    {
        if (rectTransform == null || canvasGroup == null) yield break;
        
        // Start from scaled down and faded
        Vector3 originalScale = rectTransform.localScale;
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = scaleCurve.Evaluate(elapsed / scaleAnimationDuration);
            
            rectTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            canvasGroup.alpha = t;
            
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }
}

/// <summary>
/// Controls individual floating particle behavior
/// </summary>
public class FloatingParticleBehavior : MonoBehaviour
{
    public float speed = 20f;
    public Vector2 startPosition;
    
    private RectTransform rectTransform;
    private Image image;
    private float lifetime = 0f;
    private float maxLifetime;
    private Vector2 direction;
    
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        maxLifetime = Random.Range(3f, 6f);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    
    private void Update()
    {
        if (rectTransform == null) return;
        
        lifetime += Time.deltaTime;
        
        // Move particle
        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;
        
        // Fade out over lifetime
        if (image != null)
        {
            Color color = image.color;
            color.a = Mathf.Lerp(0.3f, 0f, lifetime / maxLifetime);
            image.color = color;
        }
        
        // Reset when lifetime expires
        if (lifetime >= maxLifetime)
        {
            ResetParticle();
        }
    }
    
    private void ResetParticle()
    {
        lifetime = 0f;
        rectTransform.anchoredPosition = startPosition;
        maxLifetime = Random.Range(3f, 6f);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}
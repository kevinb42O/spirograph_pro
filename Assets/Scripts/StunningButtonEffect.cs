using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StunningButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Image buttonImage;
    private Vector3 originalScale;
    private Color originalColor;
    
    [Header("Visual Effects")]
    public float hoverScaleMultiplier = 1.1f;
    public float pressScaleMultiplier = 0.95f;
    public Color hoverTintColor = new Color(1.2f, 1.2f, 1.2f, 1f);
    public float transitionSpeed = 10f;
    
    [Header("Glow Effect")]
    public bool enableGlow = true;
    public float glowIntensity = 2f;
    
    private Vector3 targetScale;
    private Color targetColor;
    private bool isHovering = false;
    
    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        targetScale = originalScale;
        targetColor = originalColor;
    }
    
    private void Update()
    {
        // Smooth transitions
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        
        if (buttonImage != null)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * transitionSpeed);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = originalScale * hoverScaleMultiplier;
        targetColor = originalColor * hoverTintColor;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
        targetColor = originalColor;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressScaleMultiplier;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isHovering)
        {
            targetScale = originalScale * hoverScaleMultiplier;
        }
        else
        {
            targetScale = originalScale;
        }
    }
}
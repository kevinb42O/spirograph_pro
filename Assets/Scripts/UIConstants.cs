using UnityEngine;

/// <summary>
/// Centralized UI constants for visual consistency across the entire application.
/// This ensures Adobe-level design consistency with glassmorphic cosmic theme.
/// </summary>
public static class UIConstants
{
    // ============================================================================
    // COLOR PALETTE - Cosmic Glassmorphism Theme
    // ============================================================================
    
    /// <summary>Deep space background - primary panel color</summary>
    public static readonly Color DeepSpaceGlass = new Color(0.02f, 0.02f, 0.08f, 0.75f);
    
    /// <summary>Darker variant for nested panels</summary>
    public static readonly Color DarkSpaceGlass = new Color(0.01f, 0.02f, 0.12f, 0.92f);
    
    /// <summary>Section backgrounds</summary>
    public static readonly Color SectionBackground = new Color(0.03f, 0.03f, 0.1f, 0.7f);
    
    /// <summary>Control element backgrounds (sliders, inputs)</summary>
    public static readonly Color ControlBackground = new Color(0.08f, 0.12f, 0.22f, 0.7f);
    
    /// <summary>Button backgrounds</summary>
    public static readonly Color ButtonBackground = new Color(0.08f, 0.12f, 0.22f, 0.7f);
    
    /// <summary>Primary cyan glow color</summary>
    public static readonly Color CyanGlow = new Color(0.4f, 0.7f, 1f, 0.6f);
    
    /// <summary>Secondary blue glow</summary>
    public static readonly Color BlueGlow = new Color(0.3f, 0.6f, 1f, 0.5f);
    
    /// <summary>Bright cyan for highlights</summary>
    public static readonly Color BrightCyan = new Color(0.7f, 0.85f, 1f, 0.9f);
    
    /// <summary>Soft cyan-white for text</summary>
    public static readonly Color SoftCyanWhite = new Color(0.8f, 0.9f, 1f, 0.95f);
    
    /// <summary>Bright white for titles</summary>
    public static readonly Color BrightWhite = new Color(0.85f, 0.95f, 1f, 1f);
    
    /// <summary>Muted text color</summary>
    public static readonly Color MutedText = new Color(0.6f, 0.75f, 0.9f, 0.9f);
    
    /// <summary>Success/Active green</summary>
    public static readonly Color SuccessGreen = new Color(0.3f, 0.8f, 0.4f, 1f);
    
    /// <summary>Warning/Paused yellow</summary>
    public static readonly Color WarningYellow = new Color(0.9f, 0.7f, 0.3f, 1f);
    
    /// <summary>Error/Remove red</summary>
    public static readonly Color ErrorRed = new Color(0.8f, 0.3f, 0.3f, 1f);
    
    /// <summary>Purple accent</summary>
    public static readonly Color PurpleAccent = new Color(0.5f, 0.3f, 0.6f, 0.9f);
    
    // ============================================================================
    // SPACING & LAYOUT
    // ============================================================================
    
    /// <summary>Extra small spacing (2px)</summary>
    public const float SpacingXS = 2f;
    
    /// <summary>Small spacing (5px)</summary>
    public const float SpacingSmall = 5f;
    
    /// <summary>Medium spacing (10px) - Default for most elements</summary>
    public const float SpacingMedium = 10f;
    
    /// <summary>Large spacing (15px)</summary>
    public const float SpacingLarge = 15f;
    
    /// <summary>Extra large spacing (20px)</summary>
    public const float SpacingXL = 20f;
    
    /// <summary>Panel edge padding</summary>
    public const float PanelPadding = 15f;
    
    /// <summary>Section spacing between major sections</summary>
    public const float SectionSpacing = 10f;
    
    // ============================================================================
    // TYPOGRAPHY
    // ============================================================================
    
    /// <summary>Main panel title size</summary>
    public const int FontSizeTitle = 18;
    
    /// <summary>Section header size</summary>
    public const int FontSizeHeader = 14;
    
    /// <summary>Standard body text</summary>
    public const int FontSizeBody = 12;
    
    /// <summary>Small text (labels, stats)</summary>
    public const int FontSizeSmall = 10;
    
    /// <summary>Tiny text (9px minimum for readability)</summary>
    public const int FontSizeTiny = 9;
    
    // ============================================================================
    // COMPONENT SIZES
    // ============================================================================
    
    /// <summary>Standard button height</summary>
    public const float ButtonHeight = 40f;
    
    /// <summary>Compact button height</summary>
    public const float ButtonHeightCompact = 32f;
    
    /// <summary>Small button height (minimum touch target)</summary>
    public const float ButtonHeightSmall = 28f;
    
    /// <summary>Slider height</summary>
    public const float SliderHeight = 50f;
    
    /// <summary>Slider track height</summary>
    public const float SliderTrackHeight = 8f;
    
    /// <summary>Slider handle size</summary>
    public const float SliderHandleSize = 20f;
    
    /// <summary>Minimum touch target size (accessibility)</summary>
    public const float MinTouchTarget = 44f;
    
    // ============================================================================
    // ANIMATION TIMING
    // ============================================================================
    
    /// <summary>Very fast transition (100ms)</summary>
    public const float TransitionVeryFast = 0.1f;
    
    /// <summary>Fast transition (200ms)</summary>
    public const float TransitionFast = 0.2f;
    
    /// <summary>Normal transition (300ms)</summary>
    public const float TransitionNormal = 0.3f;
    
    /// <summary>Slow transition (500ms)</summary>
    public const float TransitionSlow = 0.5f;
    
    /// <summary>Button hover scale multiplier</summary>
    public const float ButtonHoverScale = 1.08f;
    
    /// <summary>Button press scale multiplier</summary>
    public const float ButtonPressScale = 0.95f;
    
    /// <summary>Default animation speed for components</summary>
    public const float AnimationSpeed = 10f;
    
    /// <summary>Pulse animation speed</summary>
    public const float PulseSpeed = 2f;
    
    // ============================================================================
    // VISUAL EFFECTS
    // ============================================================================
    
    /// <summary>Standard outline distance</summary>
    public static readonly Vector2 OutlineDistance = new Vector2(2f, -2f);
    
    /// <summary>Large outline distance for emphasis</summary>
    public static readonly Vector2 OutlineDistanceLarge = new Vector2(3f, -3f);
    
    /// <summary>Shadow distance</summary>
    public static readonly Vector2 ShadowDistance = new Vector2(1f, -1f);
    
    /// <summary>Glow effect distance (0,0 for radial)</summary>
    public static readonly Vector2 GlowDistance = Vector2.zero;
    
    /// <summary>Standard corner radius for UI elements</summary>
    public const float CornerRadius = 4f;
    
    // ============================================================================
    // PERFORMANCE SETTINGS
    // ============================================================================
    
    /// <summary>UI update frequency (Hz) for real-time stats</summary>
    public const float UIUpdateFrequency = 10f;
    
    /// <summary>UI update interval in seconds</summary>
    public const float UIUpdateInterval = 0.1f;
    
    /// <summary>Canvas scaler reference resolution width</summary>
    public const float CanvasReferenceWidth = 1920f;
    
    /// <summary>Canvas scaler reference resolution height</summary>
    public const float CanvasReferenceHeight = 1080f;
    
    // ============================================================================
    // HELPER METHODS
    // ============================================================================
    
    /// <summary>
    /// Creates a smooth easing curve for animations (ease in-out cubic)
    /// </summary>
    public static float SmoothEase(float t)
    {
        return t < 0.5f 
            ? 4f * t * t * t 
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
    
    /// <summary>
    /// Creates an elastic ease out effect
    /// </summary>
    public static float ElasticEaseOut(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;
        
        return t == 0f
            ? 0f
            : t == 1f
            ? 1f
            : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }
    
    /// <summary>
    /// Validates color contrast ratio for accessibility (WCAG AA requires 4.5:1 for normal text)
    /// </summary>
    public static float GetContrastRatio(Color fg, Color bg)
    {
        float l1 = GetRelativeLuminance(fg);
        float l2 = GetRelativeLuminance(bg);
        
        float lighter = Mathf.Max(l1, l2);
        float darker = Mathf.Min(l1, l2);
        
        return (lighter + 0.05f) / (darker + 0.05f);
    }
    
    /// <summary>
    /// Calculate relative luminance for WCAG contrast calculations
    /// </summary>
    private static float GetRelativeLuminance(Color color)
    {
        float r = color.r <= 0.03928f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
        float g = color.g <= 0.03928f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
        float b = color.b <= 0.03928f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);
        
        return 0.2126f * r + 0.7152f * g + 0.0722f * b;
    }
}

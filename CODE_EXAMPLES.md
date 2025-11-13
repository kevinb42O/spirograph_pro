# 💡 Spirograph Pro - Code Examples & Patterns

This document provides practical examples of how to use the Spirograph Pro architecture.

## 📦 Using Configuration System

### Accessing Configuration

```csharp
using SpirographPro.Configuration;

public class MyComponent : MonoBehaviour
{
    void Start()
    {
        // Access singleton instance
        SpirographConfiguration config = SpirographConfiguration.Instance;
        
        // Use configuration values
        float maxSpeed = config.maxSpeed;
        int maxAgents = config.maxAgentCount;
        Color defaultColor = config.defaultAgentColor;
        
        // All constants validated and guaranteed to be in valid ranges
    }
}
```

### Using Constants

```csharp
using SpirographPro.Configuration;

public class MyUIComponent : MonoBehaviour
{
    void CreateUI()
    {
        // Use named constants instead of magic numbers
        float panelWidth = SpirographConstants.CONTROL_PANEL_WIDTH; // 340f
        float buttonHeight = SpirographConstants.BUTTON_HEIGHT; // 40f
        float padding = SpirographConstants.UI_PADDING; // 10f
        
        // Better than: float panelWidth = 340f;
    }
    
    void FormatValues()
    {
        float value = 123.456f;
        
        // Use format constants
        string formatted1 = value.ToString(SpirographConstants.FORMAT_F1); // "123.5"
        string formatted2 = value.ToString(SpirographConstants.FORMAT_F2); // "123.46"
        
        // Better than: value.ToString("F1")
    }
}
```

## 🎨 Using Color Utilities

### Get Rainbow Colors

```csharp
using UnityEngine;
using SpirographPro.Utils;

public class ColorExample : MonoBehaviour
{
    void AssignRainbowColors()
    {
        int agentCount = 8;
        
        for (int i = 0; i < agentCount; i++)
        {
            // Distribute agents across rainbow spectrum
            float hue = (float)i / agentCount;
            Color agentColor = ColorUtility.GetRainbowColor(hue);
            
            // Use the color...
        }
    }
}
```

### Get Agent Colors by Mode

```csharp
using UnityEngine;
using SpirographPro.Utils;

public class AgentColorAssignment : MonoBehaviour
{
    void AssignColors(AgentColorMode mode)
    {
        int agentIndex = 0;
        int totalAgents = 10;
        Color masterColor = Color.cyan;
        
        // Get color based on mode
        Color agentColor = ColorUtility.GetAgentColor(
            mode, 
            agentIndex, 
            totalAgents, 
            masterColor
        );
        
        // Master mode: returns masterColor
        // Rainbow mode: returns color from spectrum
        // Individual mode: returns color from palette
        // Custom mode: returns masterColor (can be overridden)
    }
}
```

### Color Manipulation

```csharp
using UnityEngine;
using SpirographPro.Utils;

public class ColorManipulation : MonoBehaviour
{
    void ManipulateColors()
    {
        Color baseColor = Color.red;
        
        // Lighten color
        Color lighter = ColorUtility.Lighten(baseColor, 0.3f);
        
        // Darken color
        Color darker = ColorUtility.Darken(baseColor, 0.3f);
        
        // Change alpha
        Color transparent = ColorUtility.WithAlpha(baseColor, 0.5f);
        
        // Create emissive version
        Color glowing = ColorUtility.CreateEmissiveColor(baseColor, 2f);
        
        // Multi-color interpolation (gradient)
        Color[] gradient = { Color.red, Color.yellow, Color.green };
        Color interpolated = ColorUtility.MultiColorLerp(0.5f, gradient); // Yellow-green
    }
}
```

## 🎯 Implementing Interfaces

### Creating a Custom Agent

```csharp
using UnityEngine;
using SpirographPro.Core;

public class MyCustomAgent : MonoBehaviour, IAgent
{
    // Interface properties
    public int AgentIndex { get; set; }
    public string AgentName { get; set; }
    public AgentStatus Status { get; private set; }
    public Vector3 CurrentPosition { get; private set; }
    public Color AgentColor { get; set; }
    
    // Interface methods
    public void StartDrawing()
    {
        Status = AgentStatus.Active;
        // Start your drawing logic
    }
    
    public void Pause()
    {
        if (Status == AgentStatus.Active)
        {
            Status = AgentStatus.Paused;
        }
    }
    
    public void Resume()
    {
        if (Status == AgentStatus.Paused)
        {
            Status = AgentStatus.Active;
        }
    }
    
    public void ResetAgent()
    {
        Status = AgentStatus.Idle;
        CurrentPosition = Vector3.zero;
    }
    
    public void SetColor(Color newColor)
    {
        AgentColor = newColor;
        // Update visual elements with new color
    }
}
```

### Creating a Colorable Component

```csharp
using UnityEngine;
using SpirographPro.Core;
using System.Collections;

public class ColorableObject : MonoBehaviour, IColorable
{
    private Color currentColor;
    private Color targetColor;
    private Material material;
    
    public Color PrimaryColor 
    { 
        get => currentColor;
        set => UpdateColor(value, false);
    }
    
    void Awake()
    {
        material = GetComponent<Renderer>().material;
        currentColor = material.color;
    }
    
    public void UpdateColor(Color newColor, bool smooth = false)
    {
        if (smooth)
        {
            targetColor = newColor;
            StartCoroutine(SmoothColorTransition());
        }
        else
        {
            currentColor = newColor;
            material.color = newColor;
        }
    }
    
    public void ResetToDefaultColor()
    {
        UpdateColor(Color.white, false);
    }
    
    private IEnumerator SmoothColorTransition()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = currentColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentColor = Color.Lerp(startColor, targetColor, elapsed / duration);
            material.color = currentColor;
            yield return null;
        }
        
        currentColor = targetColor;
    }
}
```

## 🎨 Creating UI Components

### Extending BaseUIComponent

```csharp
using UnityEngine;
using UnityEngine.UI;
using SpirographPro.UI;
using SpirographPro.Configuration;

public class MyCustomUI : BaseUIComponent
{
    void CreateCustomPanel()
    {
        // Create panel with consistent styling
        GameObject panel = new GameObject("MyPanel");
        panel.transform.SetParent(transform, false);
        
        // Apply glassmorphic styling
        ApplyPanelStyling(panel, new Color(0.02f, 0.02f, 0.08f, 0.75f));
        
        // Create button using base method
        Button myButton = CreateModernButton(
            parent: panel.transform,
            name: "MyButton",
            position: new Vector2(10, -10),
            size: new Vector2(200, SpirographConstants.BUTTON_HEIGHT),
            text: "Click Me",
            color: null // Uses default color
        );
        
        myButton.onClick.AddListener(OnButtonClick);
        
        // Create slider using base method
        var (slider, label, value) = CreateModernSlider(
            parent: panel.transform,
            name: "MySlider",
            position: new Vector2(10, -60),
            minValue: 0f,
            maxValue: 100f,
            defaultValue: 50f,
            labelText: "My Setting",
            valueText: "50.0"
        );
        
        slider.onValueChanged.AddListener((val) => {
            value.text = val.ToString(SpirographConstants.FORMAT_F1);
            OnSliderChanged(val);
        });
    }
    
    void OnButtonClick() 
    { 
        Debug.Log("Button clicked!");
    }
    
    void OnSliderChanged(float value) 
    { 
        Debug.Log($"Slider value: {value}");
    }
}
```

### Creating Consistent UI Elements

```csharp
using UnityEngine;
using UnityEngine.UI;
using SpirographPro.Configuration;

public class UIFactory : MonoBehaviour
{
    // Create a section header
    public GameObject CreateSectionHeader(Transform parent, string title)
    {
        GameObject header = new GameObject($"{title}Header");
        header.transform.SetParent(parent, false);
        
        RectTransform rect = header.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 35);
        
        Text text = header.AddComponent<Text>();
        text.text = $"⚡ {title}";
        text.font = Resources.GetBuiltinResource<Font>(SpirographConstants.LEGACY_FONT);
        text.fontSize = 14;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        text.alignment = TextAnchor.MiddleLeft;
        
        return header;
    }
    
    // Create a toggle with consistent styling
    public Toggle CreateStyledToggle(Transform parent, string labelText)
    {
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(parent, false);
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(toggleObj.transform, false);
        
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.15f, 0.25f, 0.6f);
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(bg.transform, false);
        
        Text check = checkmark.AddComponent<Text>();
        check.text = "✓";
        check.font = Resources.GetBuiltinResource<Font>(SpirographConstants.LEGACY_FONT);
        check.fontSize = 22;
        check.color = Config.uiAccentColor;
        check.alignment = TextAnchor.MiddleCenter;
        
        // Toggle component
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.targetGraphic = bgImage;
        toggle.graphic = check;
        
        return toggle;
    }
}
```

## 🔄 Event System Usage

### Publishing Events

```csharp
using UnityEngine;

public class EventPublisher : MonoBehaviour
{
    // Define event delegates
    public delegate void AgentEventHandler(int agentIndex);
    public event AgentEventHandler OnAgentSelected;
    
    public delegate void ProgressEventHandler(float progress);
    public event ProgressEventHandler OnProgressChanged;
    
    // Fire events
    public void SelectAgent(int index)
    {
        Debug.Log($"Selecting agent {index}");
        
        // Notify all subscribers
        OnAgentSelected?.Invoke(index);
    }
    
    public void UpdateProgress(float progress)
    {
        OnProgressChanged?.Invoke(progress);
    }
}
```

### Subscribing to Events

```csharp
using UnityEngine;

public class EventSubscriber : MonoBehaviour
{
    private EventPublisher publisher;
    
    void Start()
    {
        publisher = FindFirstObjectByType<EventPublisher>();
        
        if (publisher != null)
        {
            // Subscribe to events
            publisher.OnAgentSelected += HandleAgentSelected;
            publisher.OnProgressChanged += HandleProgressChanged;
        }
    }
    
    void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        if (publisher != null)
        {
            publisher.OnAgentSelected -= HandleAgentSelected;
            publisher.OnProgressChanged -= HandleProgressChanged;
        }
    }
    
    void HandleAgentSelected(int index)
    {
        Debug.Log($"Agent {index} was selected");
        // React to selection
    }
    
    void HandleProgressChanged(float progress)
    {
        Debug.Log($"Progress: {progress:P0}");
        // Update UI
    }
}
```

## ⚡ Performance Optimization

### Caching Components

```csharp
using UnityEngine;

public class OptimizedComponent : MonoBehaviour
{
    // Cache references in Awake/Start
    private Renderer cachedRenderer;
    private Transform cachedTransform;
    private Rigidbody cachedRigidbody;
    
    void Awake()
    {
        // Cache once during initialization
        cachedRenderer = GetComponent<Renderer>();
        cachedTransform = transform; // Built-in caching for transform
        cachedRigidbody = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        // Use cached references - much faster than GetComponent every frame
        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = Color.Lerp(
                cachedRenderer.material.color, 
                Color.red, 
                Time.deltaTime
            );
        }
        
        // Use cached transform
        cachedTransform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }
}
```

### Optimized Update Loops

```csharp
using UnityEngine;
using SpirographPro.Configuration;

public class OptimizedUpdates : MonoBehaviour
{
    private float updateInterval;
    private float timeSinceLastUpdate;
    
    void Start()
    {
        // Update at configured frequency (e.g., 10 Hz instead of 60 Hz)
        updateInterval = 1f / SpirographConfiguration.Instance.uiUpdateFrequency;
        timeSinceLastUpdate = 0f;
    }
    
    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            PerformExpensiveUpdate();
        }
    }
    
    void PerformExpensiveUpdate()
    {
        // Only called at configured frequency, not every frame
        // E.g., update UI stats, calculate complex values, etc.
    }
}
```

## 🧪 Testing Examples

### Unit Test for ColorUtility

```csharp
using NUnit.Framework;
using UnityEngine;
using SpirographPro.Utils;

public class ColorUtilityTests
{
    [Test]
    public void GetRainbowColor_ReturnsCorrectHue()
    {
        // Arrange
        float hue = 0.5f; // Cyan

        // Act
        Color result = ColorUtility.GetRainbowColor(hue);
        
        // Assert
        Color expected = Color.HSVToRGB(0.5f, 1f, 1f);
        Assert.AreEqual(expected, result);
    }
    
    [Test]
    public void Lighten_IncreasesColorBrightness()
    {
        // Arrange
        Color dark = new Color(0.2f, 0.2f, 0.2f);
        
        // Act
        Color result = ColorUtility.Lighten(dark, 0.5f);
        
        // Assert
        Assert.Greater(result.r, dark.r);
        Assert.Greater(result.g, dark.g);
        Assert.Greater(result.b, dark.b);
    }
    
    [Test]
    public void WithAlpha_ClampsToValidRange()
    {
        // Arrange
        Color color = Color.red;
        
        // Act
        Color result = ColorUtility.WithAlpha(color, 1.5f); // Invalid alpha
        
        // Assert
        Assert.AreEqual(1f, result.a); // Should be clamped to 1
    }
}
```

### Integration Test

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class AgentIntegrationTests
{
    [UnityTest]
    public IEnumerator Agent_StartsDrawing_StatusChangesToActive()
    {
        // Arrange
        GameObject agentObj = new GameObject();
        PathAgent agent = agentObj.AddComponent<PathAgent>();
        agent.status = PathAgent.AgentStatus.Idle;
        
        // Act
        agent.StartDrawing();
        yield return null; // Wait one frame
        
        // Assert
        Assert.AreEqual(PathAgent.AgentStatus.Active, agent.status);
        
        // Cleanup
        Object.Destroy(agentObj);
    }
}
```

## 🎓 Best Practices Summary

1. **Use Configuration System**: Access constants via `SpirographConstants` and config via `SpirographConfiguration.Instance`
2. **Use ColorUtility**: All color operations should go through `ColorUtility` to avoid duplication
3. **Extend BaseUIComponent**: Create UI components by extending `BaseUIComponent` for consistency
4. **Implement Interfaces**: Use `IAgent`, `IPathFollower`, `IColorable` for loose coupling
5. **Cache Components**: Cache `GetComponent` calls in Awake/Start
6. **Use Events**: Prefer event-based communication over direct references
7. **Document Code**: Add XML comments to all public APIs
8. **Optimize Updates**: Use configurable update frequencies for expensive operations
9. **Test Thoroughly**: Write unit tests for utility functions and integration tests for workflows

## 📚 Additional Resources

- See `ARCHITECTURE.md` for system design details
- See inline XML documentation in code files
- Check configuration tooltips in Unity Inspector
- Review existing implementations for examples

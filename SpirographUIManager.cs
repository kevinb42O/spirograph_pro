using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Generator for Spirograph controls with modern 2025+ glassmorphism space theme
/// USAGE:
/// 1. Add this component to any GameObject in your scene
/// 2. In the Inspector, check the "generateUI" checkbox
/// 3. The UI will be created automatically with smooth animations and cosmic aesthetics
/// 4. Hook up the sliders/buttons to SpirographRoller, RotateParent, and CameraController manually
/// </summary>
public class SpirographUIManager : MonoBehaviour
{
    [Header("UI Elements - Auto-populated after generation")]
    public Slider speedSlider;
    public Slider cyclesSlider;
    public Slider rotationSpeedSlider;
    public Slider objectRotationSpeedSlider;
    public Slider penDistanceSlider;
    public Slider lineWidthSlider;
    public Slider lineBrightnessSlider;
    public Button pauseButton;
    public Button resetButton;
    public Button lookAtButton;
    public Button smoothFollowButton;
    public Button autoOrbitButton;
    public Button toggleVisualsButton;
    public Button lineEffectsButton;
    public Button hideUIButton;
    
    [Header("Color Picker Elements")]
    public Slider hueSlider;
    public Slider saturationSlider;
    public Slider valueSlider;
    public GameObject colorPreview;
    public Button[] colorPresetButtons;
    
    [Header("Section Toggles")]
    public Button motionSectionToggle;
    public Button visualsSectionToggle;
    public Button colorSectionToggle;
    public Button environmentSectionToggle;
    
    private GameObject motionSection;
    private GameObject visualsSection;
    private GameObject colorSection;
    private GameObject environmentSection;
    
    [Header("Skybox Dropdown")]
    public Dropdown skyboxDropdown;
    
    [Header("UI State")]
    private GameObject controlPanel;
    private bool isUIVisible = true;
    
    [Header("Generate UI")]
    [Tooltip("Check this box to generate UI (will auto-uncheck after generation)")]
    public bool generateUI = false;
    
    void OnValidate()
    {
        if (generateUI)
        {
            generateUI = false;
            #if UNITY_EDITOR
            GenerateCompleteUI();
            #endif
        }
    }
    
    /// <summary>
    /// Call this method from code or Unity events to generate UI at runtime
    /// </summary>
    public void GenerateUIAtRuntime()
    {
        GenerateCompleteUI();
    }
    
    void Start()
    {
        // Find the control panel if it exists
        if (controlPanel == null)
        {
            GameObject canvas = GameObject.Find("SpirographCanvas");
            if (canvas != null)
            {
                controlPanel = canvas.transform.Find("ControlPanel")?.gameObject;
            }
        }
        
        // Find and setup the hide button
        if (hideUIButton == null)
        {
            GameObject canvas = GameObject.Find("SpirographCanvas");
            if (canvas != null)
            {
                Transform buttonTransform = canvas.transform.Find("HideUIButton");
                if (buttonTransform != null)
                {
                    hideUIButton = buttonTransform.GetComponent<Button>();
                }
            }
        }
        
        if (hideUIButton != null)
        {
            hideUIButton.onClick.AddListener(ToggleUI);
        }
        
        // Auto-connect all UI elements to their respective scripts
        ConnectUIElements();
    }
    
    /// <summary>
    /// Automatically finds and connects all UI elements to SpirographRoller, CameraController, etc.
    /// This makes the UI "just work" without manual setup in the Inspector.
    /// </summary>
    void ConnectUIElements()
    {
        // Find the main scripts
        SpirographRoller roller = FindObjectOfType<SpirographRoller>();
        CameraController cameraController = FindObjectOfType<CameraController>();
        RotateParent rotateParent = FindObjectOfType<RotateParent>();
        SkyboxManager skyboxManager = FindObjectOfType<SkyboxManager>();
        
        // Find UI elements if not already assigned
        FindUIElements();
        
        // Connect SpirographRoller controls
        if (roller != null)
        {
            ConnectSpirographControls(roller);
            ConnectColorControls(roller);
            ConnectVisualControls(roller);
        }
        else
        {
            Debug.LogWarning("SpirographUIManager: SpirographRoller not found in scene. UI controls won't function.");
        }
        
        // Connect Camera controls
        if (cameraController != null)
        {
            ConnectCameraControls(cameraController);
        }
        else
        {
            Debug.LogWarning("SpirographUIManager: CameraController not found. Camera buttons won't function.");
        }
        
        // Connect Rotation controls
        if (rotateParent != null && objectRotationSpeedSlider != null)
        {
            objectRotationSpeedSlider.minValue = 0f;
            objectRotationSpeedSlider.maxValue = 100f;
            objectRotationSpeedSlider.value = rotateParent.rotationSpeed;
            objectRotationSpeedSlider.onValueChanged.AddListener((value) => {
                rotateParent.rotationSpeed = value;
                UpdateSliderLabel(objectRotationSpeedSlider, value.ToString("F1"));
            });
        }
        
        // Connect Skybox dropdown
        if (skyboxManager != null && skyboxDropdown != null)
        {
            // Already connected in CreateModernDropdown, but verify
            int currentIndex = skyboxManager.GetCurrentSkyboxIndex();
            if (currentIndex >= 0)
            {
                skyboxDropdown.value = currentIndex;
            }
        }
        
        Debug.Log("✓ UI Manager: All controls connected and ready!");
    }
    
    void FindUIElements()
    {
        // Find sliders if not assigned
        if (speedSlider == null)
            speedSlider = GameObject.Find("SpeedSlider")?.GetComponent<Slider>();
        if (cyclesSlider == null)
            cyclesSlider = GameObject.Find("CyclesSlider")?.GetComponent<Slider>();
        if (rotationSpeedSlider == null)
            rotationSpeedSlider = GameObject.Find("RotationSpeedSlider")?.GetComponent<Slider>();
        if (objectRotationSpeedSlider == null)
            objectRotationSpeedSlider = GameObject.Find("ObjectRotationSpeedSlider")?.GetComponent<Slider>();
        if (penDistanceSlider == null)
            penDistanceSlider = GameObject.Find("PenDistanceSlider")?.GetComponent<Slider>();
        if (lineWidthSlider == null)
            lineWidthSlider = GameObject.Find("LineWidthSlider")?.GetComponent<Slider>();
        if (lineBrightnessSlider == null)
            lineBrightnessSlider = GameObject.Find("LineBrightnessSlider")?.GetComponent<Slider>();
        if (hueSlider == null)
            hueSlider = GameObject.Find("HueSlider")?.GetComponent<Slider>();
        if (saturationSlider == null)
            saturationSlider = GameObject.Find("SaturationSlider")?.GetComponent<Slider>();
        if (valueSlider == null)
            valueSlider = GameObject.Find("ValueSlider")?.GetComponent<Slider>();
        
        // Find buttons if not assigned
        if (pauseButton == null)
            pauseButton = GameObject.Find("PauseButton")?.GetComponent<Button>();
        if (resetButton == null)
            resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
        if (toggleVisualsButton == null)
            toggleVisualsButton = GameObject.Find("ToggleVisualsButton")?.GetComponent<Button>();
        if (lineEffectsButton == null)
            lineEffectsButton = GameObject.Find("LineEffectsButton")?.GetComponent<Button>();
        if (lookAtButton == null)
            lookAtButton = GameObject.Find("LookAtButton")?.GetComponent<Button>();
        if (smoothFollowButton == null)
            smoothFollowButton = GameObject.Find("SmoothFollowButton")?.GetComponent<Button>();
        if (autoOrbitButton == null)
            autoOrbitButton = GameObject.Find("AutoOrbitButton")?.GetComponent<Button>();
        
        // Find color preview
        if (colorPreview == null)
            colorPreview = GameObject.Find("ColorPreview");
        
        // Find skybox dropdown
        if (skyboxDropdown == null)
            skyboxDropdown = GameObject.Find("SkyboxDropdown")?.GetComponent<Dropdown>();
    }
    
    void ConnectSpirographControls(SpirographRoller roller)
    {
        // Speed Slider
        if (speedSlider != null)
        {
            speedSlider.minValue = 0f;
            speedSlider.maxValue = 250f;
            speedSlider.value = roller.speed;
            speedSlider.onValueChanged.AddListener((value) => {
                roller.speed = value;
                UpdateSliderLabel(speedSlider, value.ToString("F1"));
            });
            UpdateSliderLabel(speedSlider, roller.speed.ToString("F1"));
        }
        
        // Cycles Slider
        if (cyclesSlider != null)
        {
            cyclesSlider.minValue = 1f;
            cyclesSlider.maxValue = 500f;
            cyclesSlider.wholeNumbers = true;
            cyclesSlider.value = roller.cycles;
            cyclesSlider.onValueChanged.AddListener((value) => {
                roller.cycles = (int)value;
                UpdateSliderLabel(cyclesSlider, ((int)value).ToString());
            });
            UpdateSliderLabel(cyclesSlider, roller.cycles.ToString());
        }
        
        // Rotation Speed Slider
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.minValue = 0f;
            rotationSpeedSlider.maxValue = 1f;
            rotationSpeedSlider.value = roller.rotationSpeed;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                roller.rotationSpeed = value;
                UpdateSliderLabel(rotationSpeedSlider, value.ToString("F2"));
            });
            UpdateSliderLabel(rotationSpeedSlider, roller.rotationSpeed.ToString("F2"));
        }
        
        // Pen Distance Slider
        if (penDistanceSlider != null)
        {
            penDistanceSlider.minValue = 0f;
            penDistanceSlider.maxValue = 5f;
            penDistanceSlider.value = roller.penDistance;
            penDistanceSlider.onValueChanged.AddListener((value) => {
                roller.penDistance = value;
                UpdateSliderLabel(penDistanceSlider, value.ToString("F2") + "x");
            });
            UpdateSliderLabel(penDistanceSlider, roller.penDistance.ToString("F2") + "x");
        }
        
        // Line Width Slider
        if (lineWidthSlider != null)
        {
            lineWidthSlider.minValue = 0.01f;
            lineWidthSlider.maxValue = 2f;
            lineWidthSlider.value = roller.lineWidth;
            lineWidthSlider.onValueChanged.AddListener((value) => {
                roller.lineWidth = value;
                UpdateSliderLabel(lineWidthSlider, value.ToString("F2"));
            });
            UpdateSliderLabel(lineWidthSlider, roller.lineWidth.ToString("F2"));
        }
        
        // Line Brightness Slider
        if (lineBrightnessSlider != null)
        {
            lineBrightnessSlider.minValue = 0f;
            lineBrightnessSlider.maxValue = 1f;
            lineBrightnessSlider.value = roller.lineBrightness;
            lineBrightnessSlider.onValueChanged.AddListener((value) => {
                roller.lineBrightness = value;
                UpdateSliderLabel(lineBrightnessSlider, value.ToString("F2"));
            });
            UpdateSliderLabel(lineBrightnessSlider, roller.lineBrightness.ToString("F2"));
        }
        
        // Pause Button
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(() => {
                roller.TogglePause();
                Text buttonText = pauseButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = roller.IsPaused() ? "▶ PLAY" : "⏸ PAUSE";
                }
            });
        }
        
        // Reset Button
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() => {
                roller.ResetPath();
            });
        }
    }
    
    void ConnectColorControls(SpirographRoller roller)
    {
        // HSV Sliders with live preview
        if (hueSlider != null && saturationSlider != null && valueSlider != null)
        {
            // Initialize from current color
            float h, s, v;
            Color.RGBToHSV(roller.currentLineColor, out h, out s, out v);
            hueSlider.value = h;
            saturationSlider.value = s;
            valueSlider.value = v;
            
            System.Action updateColor = () => {
                Color newColor = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
                roller.ChangeLineColor(newColor);
                
                // Update preview
                if (colorPreview != null)
                {
                    Image preview = colorPreview.GetComponent<Image>();
                    if (preview != null) preview.color = newColor;
                }
            };
            
            hueSlider.onValueChanged.AddListener((value) => {
                updateColor();
                UpdateSliderLabel(hueSlider, value.ToString("F2"));
            });
            
            saturationSlider.onValueChanged.AddListener((value) => {
                updateColor();
                UpdateSliderLabel(saturationSlider, value.ToString("F2"));
            });
            
            valueSlider.onValueChanged.AddListener((value) => {
                updateColor();
                UpdateSliderLabel(valueSlider, value.ToString("F2"));
            });
            
            // Initialize labels
            UpdateSliderLabel(hueSlider, h.ToString("F2"));
            UpdateSliderLabel(saturationSlider, s.ToString("F2"));
            UpdateSliderLabel(valueSlider, v.ToString("F2"));
        }
        
        // Color preset buttons already connected in CreateColorPresetButton
    }
    
    void ConnectVisualControls(SpirographRoller roller)
    {
        // Toggle Visuals Button
        if (toggleVisualsButton != null)
        {
            toggleVisualsButton.onClick.AddListener(() => {
                roller.ToggleVisibility();
                Text buttonText = toggleVisualsButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = roller.AreVisualsVisible() ? "👁 HIDE" : "👁 SHOW";
                }
            });
        }
        
        // Line Effects Button (cycle through effects)
        if (lineEffectsButton != null)
        {
            lineEffectsButton.onClick.AddListener(() => {
                roller.CycleLineEffect();
                Text buttonText = lineEffectsButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "✨ LINE FX: " + roller.GetCurrentEffectName();
                }
            });
            
            // Initialize text
            Text buttonText = lineEffectsButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "✨ LINE FX: " + roller.GetCurrentEffectName();
            }
        }
    }
    
    void ConnectCameraControls(CameraController cameraController)
    {
        // Look At (Free Fly) Button
        if (lookAtButton != null)
        {
            lookAtButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(0); // LookAt mode
            });
        }
        
        // Smooth Follow Button
        if (smoothFollowButton != null)
        {
            smoothFollowButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(1); // SmoothFollow mode
            });
        }
        
        // Auto Orbit Button
        if (autoOrbitButton != null)
        {
            autoOrbitButton.onClick.AddListener(() => {
                cameraController.ToggleAutoOrbit();
                Text buttonText = autoOrbitButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = cameraController.IsAutoOrbitEnabled() ? "🎬 STOP ORBIT" : "🎬 AUTO ORBIT";
                }
            });
        }
    }
    
    void UpdateSliderLabel(Slider slider, string value)
    {
        if (slider == null) return;
        Text label = slider.transform.Find("ValueLabel")?.GetComponent<Text>();
        if (label != null)
        {
            label.text = value;
        }
    }
    
    /// <summary>
    /// Apply a preset configuration to the spirograph
    /// </summary>
    void ApplyPreset(int presetIndex)
    {
        SpirographRoller roller = FindObjectOfType<SpirographRoller>();
        if (roller == null)
        {
            Debug.LogWarning("Cannot apply preset: SpirographRoller not found");
            return;
        }
        
        // Define preset configurations
        switch (presetIndex)
        {
            case 0: // Classic
                roller.speed = 50f;
                roller.cycles = 50;
                roller.rotationSpeed = 0.5f;
                roller.penDistance = 0.3f;
                roller.lineWidth = 0.3f;
                roller.ChangeLineColor(Color.cyan);
                roller.lineEffectMode = SpirographRoller.LineEffectMode.Normal;
                Debug.Log("⭐ Applied Classic preset");
                break;
                
            case 1: // Rosette
                roller.speed = 80f;
                roller.cycles = 120;
                roller.rotationSpeed = 0.7f;
                roller.penDistance = 0.5f;
                roller.lineWidth = 0.4f;
                roller.ChangeLineColor(new Color(1f, 0.4f, 0.7f)); // Pink
                roller.lineEffectMode = SpirographRoller.LineEffectMode.Glow;
                Debug.Log("⭐ Applied Rosette preset");
                break;
                
            case 2: // Flower
                roller.speed = 60f;
                roller.cycles = 80;
                roller.rotationSpeed = 0.8f;
                roller.penDistance = 0.7f;
                roller.lineWidth = 0.5f;
                roller.ChangeLineColor(new Color(1f, 0.8f, 0f)); // Gold
                roller.lineEffectMode = SpirographRoller.LineEffectMode.Rainbow;
                Debug.Log("⭐ Applied Flower preset");
                break;
                
            case 3: // Star
                roller.speed = 100f;
                roller.cycles = 200;
                roller.rotationSpeed = 0.3f;
                roller.penDistance = 0.2f;
                roller.lineWidth = 0.2f;
                roller.ChangeLineColor(Color.white);
                roller.lineEffectMode = SpirographRoller.LineEffectMode.Neon;
                Debug.Log("⭐ Applied Star preset");
                break;
                
            case 4: // Spiral
                roller.speed = 40f;
                roller.cycles = 300;
                roller.rotationSpeed = 0.9f;
                roller.penDistance = 1.2f;
                roller.lineWidth = 0.25f;
                roller.ChangeLineColor(new Color(0.5f, 0f, 1f)); // Purple
                roller.lineEffectMode = SpirographRoller.LineEffectMode.Pulse;
                Debug.Log("⭐ Applied Spiral preset");
                break;
                
            case 5: // Chaos
                roller.speed = 150f;
                roller.cycles = 400;
                roller.rotationSpeed = 0.15f;
                roller.penDistance = 0.8f;
                roller.lineWidth = 0.15f;
                roller.ChangeLineColor(Color.red);
                roller.lineEffectMode = SpirographRoller.LineEffectMode.Hologram;
                Debug.Log("⭐ Applied Chaos preset");
                break;
        }
        
        // Update UI sliders to reflect new values
        UpdateSlidersFromRoller(roller);
    }
    
    void UpdateSlidersFromRoller(SpirographRoller roller)
    {
        if (speedSlider != null)
        {
            speedSlider.value = roller.speed;
            UpdateSliderLabel(speedSlider, roller.speed.ToString("F1"));
        }
        
        if (cyclesSlider != null)
        {
            cyclesSlider.value = roller.cycles;
            UpdateSliderLabel(cyclesSlider, roller.cycles.ToString());
        }
        
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.value = roller.rotationSpeed;
            UpdateSliderLabel(rotationSpeedSlider, roller.rotationSpeed.ToString("F2"));
        }
        
        if (penDistanceSlider != null)
        {
            penDistanceSlider.value = roller.penDistance;
            UpdateSliderLabel(penDistanceSlider, roller.penDistance.ToString("F2") + "x");
        }
        
        if (lineWidthSlider != null)
        {
            lineWidthSlider.value = roller.lineWidth;
            UpdateSliderLabel(lineWidthSlider, roller.lineWidth.ToString("F2"));
        }
        
        // Update color sliders and preview
        float h, s, v;
        Color.RGBToHSV(roller.currentLineColor, out h, out s, out v);
        
        if (hueSlider != null)
        {
            hueSlider.value = h;
            UpdateSliderLabel(hueSlider, h.ToString("F2"));
        }
        
        if (saturationSlider != null)
        {
            saturationSlider.value = s;
            UpdateSliderLabel(saturationSlider, s.ToString("F2"));
        }
        
        if (valueSlider != null)
        {
            valueSlider.value = v;
            UpdateSliderLabel(valueSlider, v.ToString("F2"));
        }
        
        if (colorPreview != null)
        {
            Image preview = colorPreview.GetComponent<Image>();
            if (preview != null) preview.color = roller.currentLineColor;
        }
        
        // Update line effects button text
        if (lineEffectsButton != null)
        {
            Text buttonText = lineEffectsButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "✨ LINE FX: " + roller.GetCurrentEffectName();
            }
        }
    }
    
    void SaveCurrentConfiguration()
    {
        SpirographRoller roller = FindObjectOfType<SpirographRoller>();
        if (roller == null)
        {
            Debug.LogWarning("Cannot save: SpirographRoller not found");
            return;
        }
        
        // Save to PlayerPrefs (simple persistence)
        PlayerPrefs.SetFloat("Spirograph_Speed", roller.speed);
        PlayerPrefs.SetInt("Spirograph_Cycles", roller.cycles);
        PlayerPrefs.SetFloat("Spirograph_RotationSpeed", roller.rotationSpeed);
        PlayerPrefs.SetFloat("Spirograph_PenDistance", roller.penDistance);
        PlayerPrefs.SetFloat("Spirograph_LineWidth", roller.lineWidth);
        PlayerPrefs.SetFloat("Spirograph_LineBrightness", roller.lineBrightness);
        
        // Save color as HSV
        float h, s, v;
        Color.RGBToHSV(roller.currentLineColor, out h, out s, out v);
        PlayerPrefs.SetFloat("Spirograph_ColorH", h);
        PlayerPrefs.SetFloat("Spirograph_ColorS", s);
        PlayerPrefs.SetFloat("Spirograph_ColorV", v);
        
        PlayerPrefs.SetInt("Spirograph_LineEffect", (int)roller.lineEffectMode);
        PlayerPrefs.Save();
        
        Debug.Log("💾 Configuration saved successfully!");
    }
    
    void LoadSavedConfiguration()
    {
        SpirographRoller roller = FindObjectOfType<SpirographRoller>();
        if (roller == null)
        {
            Debug.LogWarning("Cannot load: SpirographRoller not found");
            return;
        }
        
        if (!PlayerPrefs.HasKey("Spirograph_Speed"))
        {
            Debug.LogWarning("📂 No saved configuration found");
            return;
        }
        
        // Load from PlayerPrefs
        roller.speed = PlayerPrefs.GetFloat("Spirograph_Speed", 50f);
        roller.cycles = PlayerPrefs.GetInt("Spirograph_Cycles", 50);
        roller.rotationSpeed = PlayerPrefs.GetFloat("Spirograph_RotationSpeed", 0.5f);
        roller.penDistance = PlayerPrefs.GetFloat("Spirograph_PenDistance", 0.3f);
        roller.lineWidth = PlayerPrefs.GetFloat("Spirograph_LineWidth", 0.3f);
        roller.lineBrightness = PlayerPrefs.GetFloat("Spirograph_LineBrightness", 1f);
        
        // Load color
        float h = PlayerPrefs.GetFloat("Spirograph_ColorH", 0.5f);
        float s = PlayerPrefs.GetFloat("Spirograph_ColorS", 0.8f);
        float v = PlayerPrefs.GetFloat("Spirograph_ColorV", 1f);
        roller.ChangeLineColor(Color.HSVToRGB(h, s, v));
        
        roller.lineEffectMode = (SpirographRoller.LineEffectMode)PlayerPrefs.GetInt("Spirograph_LineEffect", 0);
        
        // Update UI
        UpdateSlidersFromRoller(roller);
        
        Debug.Log("📂 Configuration loaded successfully!");
    }
    
    void Update()
    {
        // Toggle UI visibility with Enter key
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ToggleUI();
        }
    }
    
    void ToggleUI()
    {
        isUIVisible = !isUIVisible;
        
        if (controlPanel != null)
        {
            StartCoroutine(AnimateUIToggle(controlPanel, isUIVisible));
        }
        
        // Update button text with icon
        if (hideUIButton != null)
        {
            Text buttonText = hideUIButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = isUIVisible ? "⊗ HIDE" : "⊕ SHOW";
            }
        }
        
        Debug.Log($"UI {(isUIVisible ? "Shown" : "Hidden")} (Press Enter to toggle)");
    }
    
    IEnumerator AnimateUIToggle(GameObject panel, bool show)
    {
        // IMPORTANT: Must activate panel BEFORE animating if showing
        if (show && !panel.activeSelf)
        {
            panel.SetActive(true);
        }
        
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        // If showing, start from hidden state
        if (show)
        {
            canvasGroup.alpha = 0f;
            panel.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        }
        
        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;
        
        Vector3 startScale = panel.transform.localScale;
        Vector3 targetScale = show ? Vector3.one : new Vector3(0.95f, 0.95f, 1f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth easing
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothT);
            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        panel.transform.localScale = targetScale;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;
        
        // Only deactivate if hiding
        if (!show)
        {
            panel.SetActive(false);
        }
    }
    
    [ContextMenu("Generate Complete UI")]
    void GenerateCompleteUI()
    {
        // Create EventSystem
        if (EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            Debug.Log("✓ Created EventSystem with InputSystemUIInputModule");
        }
        
        // Create Canvas
        GameObject canvasObj = new GameObject("SpirographCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Hide UI Button (Top-Right Corner - ALWAYS VISIBLE) - Modern glassmorphic style
        hideUIButton = CreateModernButton(canvas.transform, "HideUIButton", new Vector2(-15, -15), new Vector2(90, 40), "⊗ HIDE");
        RectTransform hideButtonRect = hideUIButton.GetComponent<RectTransform>();
        hideButtonRect.anchorMin = new Vector2(1, 1); // Top-right anchor
        hideButtonRect.anchorMax = new Vector2(1, 1);
        hideButtonRect.pivot = new Vector2(1, 1);
        
        // Glassmorphic style for hide button
        Image hideButtonImage = hideUIButton.GetComponent<Image>();
        hideButtonImage.color = new Color(0.05f, 0.05f, 0.15f, 0.7f); // Deep space glass
        
        // Add subtle glow outline
        Outline hideOutline = hideUIButton.gameObject.AddComponent<Outline>();
        hideOutline.effectColor = new Color(0.4f, 0.6f, 1f, 0.5f); // Cyan glow
        hideOutline.effectDistance = new Vector2(1, -1);
        
        Text hideButtonText = hideUIButton.GetComponentInChildren<Text>();
        hideButtonText.fontSize = 14;
        hideButtonText.fontStyle = FontStyle.Bold;
        hideButtonText.color = new Color(0.8f, 0.9f, 1f, 0.95f); // Soft cyan-white
        
        // Create Panel Background - Modern Glassmorphism with cosmic theme - NOW SCROLLABLE!
        GameObject panel = new GameObject("ControlPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 0.5f);
        panelRect.anchoredPosition = new Vector2(15, 0);
        panelRect.sizeDelta = new Vector2(340, -80); // Full height minus padding (80 = 15 top + 15 bottom + 50 for title)
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.02f, 0.08f, 0.75f);
        
        // Add Canvas Group for smooth transitions
        CanvasGroup panelGroup = panel.AddComponent<CanvasGroup>();
        panelGroup.alpha = 1f;
        
        // Add subtle outer glow
        Shadow panelGlow = panel.AddComponent<Shadow>();
        panelGlow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        panelGlow.effectDistance = new Vector2(0, 0);
        panelGlow.useGraphicAlpha = true;
        
        // Add border accent
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.3f, 0.5f, 0.9f, 0.25f);
        panelOutline.effectDistance = new Vector2(2, -2);
        
        // Add ScrollRect for scrolling
        ScrollRect panelScroll = panel.AddComponent<ScrollRect>();
        panelScroll.horizontal = false;
        panelScroll.vertical = true;
        panelScroll.scrollSensitivity = 20f;
        panelScroll.movementType = ScrollRect.MovementType.Clamped;
        panelScroll.inertia = true;
        panelScroll.decelerationRate = 0.135f;
        
        // Create Viewport for ScrollRect
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(panel.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.pivot = new Vector2(0, 1);
        
        // Add mask to viewport
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear; // Invisible but required for mask
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        panelScroll.viewport = viewportRect;
        
        // Create Content container (this will hold all our UI elements)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 2000); // Large height, will be adjusted later
        
        panelScroll.content = contentRect;
        
        // Store reference to control panel
        controlPanel = panel;
        
        // Now parent all UI elements to 'content' instead of 'panel'
        Transform uiParent = content.transform;
        
        float yPos = -20; // More top padding
        
        // Add panel title
        GameObject titleObj = new GameObject("PanelTitle");
        titleObj.transform.SetParent(uiParent, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 35);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "✦ SPIROGRAPH CONTROLS";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.7f, 0.85f, 1f, 0.9f); // Bright cyan
        titleText.alignment = TextAnchor.MiddleCenter;
        
        // Add subtle glow to title
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0.3f, 0.6f, 1f, 0.5f);
        titleShadow.effectDistance = new Vector2(0, 0);
        
        yPos -= 55;
        
        // ============================================================
        // MOTION SECTION
        // ============================================================
        motionSectionToggle = CreateSectionHeader(uiParent, "MotionHeader", new Vector2(15, yPos), "⚡ MOTION & SPEED", true);
        yPos -= 45;
        
        motionSection = CreateSection(uiParent, "MotionSection", new Vector2(15, yPos));
        float motionYPos = -10;
        
        // Speed Slider
        Text speedText;
        speedSlider = CreateModernSlider(motionSection.transform, "SpeedSlider", new Vector2(0, motionYPos), 0f, 250f, 
            0f, out speedText, "Travel Speed", "0.0");
        motionYPos -= 70;
        
        // Cycles Slider
        Text cyclesText;
        cyclesSlider = CreateModernSlider(motionSection.transform, "CyclesSlider", new Vector2(0, motionYPos), 1f, 500f, 
            10f, out cyclesText, "Cycles", "10");
        cyclesSlider.wholeNumbers = true;
        motionYPos -= 70;
        
        // Rotation Speed Slider
        Text rotationSpeedText;
        rotationSpeedSlider = CreateModernSlider(motionSection.transform, "RotationSpeedSlider", new Vector2(0, motionYPos), 0f, 1f, 
            0.5f, out rotationSpeedText, "Rotation Speed", "0.5");
        motionYPos -= 70;
        
        // Object Rotation Speed Slider
        Text objectRotationSpeedText;
        objectRotationSpeedSlider = CreateModernSlider(motionSection.transform, "ObjectRotationSpeedSlider", new Vector2(0, motionYPos), 0f, 100f, 
            1f, out objectRotationSpeedText, "Object Rotation", "1.0");
        motionYPos -= 70;
        
        // Pen Distance Slider
        Text penDistanceText;
        penDistanceSlider = CreateModernSlider(motionSection.transform, "PenDistanceSlider", new Vector2(0, motionYPos), 0f, 5f, 
            0.3f, out penDistanceText, "Rotor Radius", "0.30x");
        motionYPos -= 75;
        
        // Set motion section height
        RectTransform motionRect = motionSection.GetComponent<RectTransform>();
        motionRect.sizeDelta = new Vector2(290, Mathf.Abs(motionYPos) + 10);
        yPos += motionYPos - 20;
        
        // ============================================================
        // VISUALS SECTION
        // ============================================================
        visualsSectionToggle = CreateSectionHeader(uiParent, "VisualsHeader", new Vector2(15, yPos), "🎨 VISUAL EFFECTS", true);
        yPos -= 45;
        
        visualsSection = CreateSection(uiParent, "VisualsSection", new Vector2(15, yPos));
        float visualsYPos = -10;
        
        // Line Width Slider
        Text lineWidthText;
        lineWidthSlider = CreateModernSlider(visualsSection.transform, "LineWidthSlider", new Vector2(0, visualsYPos), 0.01f, 2f, 
            0.3f, out lineWidthText, "Line Width", "0.30");
        visualsYPos -= 70;
        
        // Line Brightness Slider
        Text lineBrightnessText;
        lineBrightnessSlider = CreateModernSlider(visualsSection.transform, "LineBrightnessSlider", new Vector2(0, visualsYPos), 0f, 1f, 
            1f, out lineBrightnessText, "Line Brightness", "1.00");
        visualsYPos -= 75;
        
        // Pause/Reset Buttons
        pauseButton = CreateModernButton(visualsSection.transform, "PauseButton", new Vector2(0, visualsYPos), new Vector2(140, 38), "⏸ PAUSE");
        resetButton = CreateModernButton(visualsSection.transform, "ResetButton", new Vector2(150, visualsYPos), new Vector2(140, 38), "↻ RESET");
        visualsYPos -= 50;
        
        // Toggle Visuals Button
        toggleVisualsButton = CreateModernButton(visualsSection.transform, "ToggleVisualsButton", new Vector2(0, visualsYPos), new Vector2(290, 38), "👁 SHOW/HIDE");
        visualsYPos -= 50;
        
        // Line Effects Cycle Button
        lineEffectsButton = CreateModernButton(visualsSection.transform, "LineEffectsButton", new Vector2(0, visualsYPos), new Vector2(290, 38), "✨ LINE FX: Normal");
        Image effectsImg = lineEffectsButton.GetComponent<Image>();
        effectsImg.color = new Color(0.15f, 0.08f, 0.20f, 0.8f);
        visualsYPos -= 55;
        
        // Set visuals section height
        RectTransform visualsRect = visualsSection.GetComponent<RectTransform>();
        visualsRect.sizeDelta = new Vector2(290, Mathf.Abs(visualsYPos) + 10);
        yPos += visualsYPos - 20;
        
        // ============================================================
        // COLOR PICKER SECTION
        // ============================================================
        colorSectionToggle = CreateSectionHeader(uiParent, "ColorHeader", new Vector2(15, yPos), "🌈 COLOR PICKER", true);
        yPos -= 45;
        
        colorSection = CreateSection(uiParent, "ColorSection", new Vector2(15, yPos));
        float colorYPos = -10;
        
        // Color Preview Box
        GameObject previewObj = new GameObject("ColorPreview");
        previewObj.transform.SetParent(colorSection.transform, false);
        RectTransform previewRect = previewObj.AddComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0, 1);
        previewRect.anchorMax = new Vector2(0, 1);
        previewRect.pivot = new Vector2(0, 1);
        previewRect.anchoredPosition = new Vector2(0, colorYPos);
        previewRect.sizeDelta = new Vector2(290, 50);
        Image previewImage = previewObj.AddComponent<Image>();
        previewImage.color = Color.cyan;
        Outline previewOutline = previewObj.AddComponent<Outline>();
        previewOutline.effectColor = new Color(1f, 1f, 1f, 0.5f);
        previewOutline.effectDistance = new Vector2(2, -2);
        colorPreview = previewObj;
        colorYPos -= 60;
        
        // HSV Sliders
        Text hueText;
        hueSlider = CreateModernSlider(colorSection.transform, "HueSlider", new Vector2(0, colorYPos), 0f, 1f, 
            0.5f, out hueText, "Hue", "0.50");
        colorYPos -= 70;
        
        Text satText;
        saturationSlider = CreateModernSlider(colorSection.transform, "SaturationSlider", new Vector2(0, colorYPos), 0f, 1f, 
            0.8f, out satText, "Saturation", "0.80");
        colorYPos -= 70;
        
        Text valText;
        valueSlider = CreateModernSlider(colorSection.transform, "ValueSlider", new Vector2(0, colorYPos), 0f, 1f, 
            1f, out valText, "Brightness", "1.00");
        colorYPos -= 75;
        
        // Color Presets (2 rows of 6)
        AddColorPresetLabel(colorSection.transform, new Vector2(0, colorYPos), "Quick Colors:");
        colorYPos -= 25;
        
        colorPresetButtons = new Button[16];
        Color[] presetColors = new Color[] {
            Color.white, Color.black, Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green,
            Color.cyan, Color.blue, new Color(0.5f, 0f, 1f), Color.magenta,
            new Color(1f, 0.4f, 0.7f), new Color(0.5f, 0.3f, 0.1f), new Color(0.8f, 0.8f, 0.8f),
            new Color(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.3f, 0.3f), new Color(1f, 0.8f, 0f)
        };
        
        for (int i = 0; i < 16; i++)
        {
            int row = i / 8;
            int col = i % 8;
            float xPos = col * 36f;
            float yPosPreset = colorYPos - (row * 32f);
            colorPresetButtons[i] = CreateColorPresetButton(colorSection.transform, $"Preset{i}", 
                new Vector2(xPos, yPosPreset), presetColors[i]);
        }
        colorYPos -= 72;
        
        // Set color section height
        RectTransform colorRect = colorSection.GetComponent<RectTransform>();
        colorRect.sizeDelta = new Vector2(290, Mathf.Abs(colorYPos) + 10);
        yPos += colorYPos - 20;
        
        // ============================================================
        // ENVIRONMENT SECTION
        // ============================================================
        environmentSectionToggle = CreateSectionHeader(uiParent, "EnvironmentHeader", new Vector2(15, yPos), "🌌 ENVIRONMENT", false);
        yPos -= 45;
        
        environmentSection = CreateSection(uiParent, "EnvironmentSection", new Vector2(15, yPos));
        environmentSection.SetActive(false); // Collapsed by default
        float envYPos = -10;
        
        // Skybox Label
        GameObject skyboxLabelObj = new GameObject("SkyboxLabel");
        skyboxLabelObj.transform.SetParent(environmentSection.transform, false);
        RectTransform skyboxLabelRect = skyboxLabelObj.AddComponent<RectTransform>();
        skyboxLabelRect.anchorMin = new Vector2(0, 1);
        skyboxLabelRect.anchorMax = new Vector2(0, 1);
        skyboxLabelRect.pivot = new Vector2(0, 1);
        skyboxLabelRect.anchoredPosition = new Vector2(0, envYPos);
        skyboxLabelRect.sizeDelta = new Vector2(290, 20);
        Text skyboxLabel = skyboxLabelObj.AddComponent<Text>();
        skyboxLabel.text = "Skybox Theme:";
        skyboxLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        skyboxLabel.fontSize = 12;
        skyboxLabel.fontStyle = FontStyle.Bold;
        skyboxLabel.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        skyboxLabel.alignment = TextAnchor.MiddleLeft;
        envYPos -= 28;
        
        // Skybox Dropdown
        skyboxDropdown = CreateModernDropdown(environmentSection.transform, "SkyboxDropdown", new Vector2(0, envYPos));
        envYPos -= 70;
        
        // Set environment section height
        RectTransform envRect = environmentSection.GetComponent<RectTransform>();
        envRect.sizeDelta = new Vector2(290, Mathf.Abs(envYPos) + 10);
        yPos += envYPos - 20;
        
        // ============================================================
        // CAMERA SECTION
        // ============================================================
        Button cameraSectionToggle = CreateSectionHeader(uiParent, "CameraHeader", new Vector2(15, yPos), "📷 CAMERA CONTROLS", false);
        yPos -= 45;
        
        GameObject cameraSection = CreateSection(uiParent, "CameraSection", new Vector2(15, yPos));
        cameraSection.SetActive(false); // Collapsed by default
        float cameraYPos = -10;
        
        // Camera Mode Buttons
        lookAtButton = CreateModernButton(cameraSection.transform, "LookAtButton", new Vector2(0, cameraYPos), new Vector2(140, 38), "✈ Free Fly");
        smoothFollowButton = CreateModernButton(cameraSection.transform, "SmoothFollowButton", new Vector2(150, cameraYPos), new Vector2(140, 38), "◎ Follow");
        cameraYPos -= 50;
        
        autoOrbitButton = CreateModernButton(cameraSection.transform, "AutoOrbitButton", new Vector2(0, cameraYPos), new Vector2(290, 38), "🎬 AUTO ORBIT");
        Image orbitImg = autoOrbitButton.GetComponent<Image>();
        orbitImg.color = new Color(0.15f, 0.05f, 0.25f, 0.8f);
        cameraYPos -= 50;
        
        // Orbit Preset Buttons
        CreateModernLabel(cameraSection.transform, "OrbitPresetsLabel", new Vector2(0, cameraYPos), new Vector2(290, 20), "Orbit Presets:");
        cameraYPos -= 25;
        
        Button topViewButton = CreateModernButton(cameraSection.transform, "TopViewButton", new Vector2(0, cameraYPos), new Vector2(68, 32), "⬇ Top");
        Button sideViewButton = CreateModernButton(cameraSection.transform, "SideViewButton", new Vector2(74, cameraYPos), new Vector2(68, 32), "↔ Side");
        Button frontViewButton = CreateModernButton(cameraSection.transform, "FrontViewButton", new Vector2(148, cameraYPos), new Vector2(68, 32), "→ Front");
        Button isoViewButton = CreateModernButton(cameraSection.transform, "IsoViewButton", new Vector2(222, cameraYPos), new Vector2(68, 32), "◇ Iso");
        
        // Style preset buttons with slightly different color
        Color presetColor = new Color(0.1f, 0.15f, 0.25f, 0.7f);
        topViewButton.GetComponent<Image>().color = presetColor;
        sideViewButton.GetComponent<Image>().color = presetColor;
        frontViewButton.GetComponent<Image>().color = presetColor;
        isoViewButton.GetComponent<Image>().color = presetColor;
        cameraYPos -= 45;
        
        // Set camera section height
        RectTransform cameraRect = cameraSection.GetComponent<RectTransform>();
        cameraRect.sizeDelta = new Vector2(290, Mathf.Abs(cameraYPos) + 10);
        yPos += cameraYPos - 20;
        
        // ============================================================
        // PRESETS SECTION - Save/Load Favorite Configurations
        // ============================================================
        Button presetsSectionToggle = CreateSectionHeader(uiParent, "PresetsHeader", new Vector2(15, yPos), "⭐ PATTERN PRESETS", false);
        yPos -= 45;
        
        GameObject presetsSection = CreateSection(uiParent, "PresetsSection", new Vector2(15, yPos));
        presetsSection.SetActive(false); // Collapsed by default
        float presetsYPos = -10;
        
        // Preset buttons (3 columns, 2 rows = 6 presets)
        string[] presetNames = new string[] {
            "Classic", "Rosette", "Flower", "Star", "Spiral", "Chaos"
        };
        
        for (int i = 0; i < 6; i++)
        {
            int row = i / 3;
            int col = i % 3;
            float xPos = col * 95f;
            float yPosPreset = presetsYPos - (row * 50f);
            
            Button presetBtn = CreateModernButton(presetsSection.transform, $"Preset{i}Button", 
                new Vector2(xPos, yPosPreset), new Vector2(90, 42), presetNames[i]);
            
            // Store index for closure
            int presetIndex = i;
            presetBtn.onClick.AddListener(() => {
                ApplyPreset(presetIndex);
            });
        }
        presetsYPos -= 110;
        
        // Save/Load buttons
        Button savePresetButton = CreateModernButton(presetsSection.transform, "SavePresetButton", 
            new Vector2(0, presetsYPos), new Vector2(140, 38), "💾 SAVE");
        Button loadPresetButton = CreateModernButton(presetsSection.transform, "LoadPresetButton", 
            new Vector2(150, presetsYPos), new Vector2(140, 38), "📂 LOAD");
        
        Image saveBtnImg = savePresetButton.GetComponent<Image>();
        saveBtnImg.color = new Color(0.1f, 0.25f, 0.15f, 0.8f); // Green tint
        
        Image loadBtnImg = loadPresetButton.GetComponent<Image>();
        loadBtnImg.color = new Color(0.15f, 0.15f, 0.25f, 0.8f); // Blue tint
        
        savePresetButton.onClick.AddListener(() => {
            SaveCurrentConfiguration();
        });
        
        loadPresetButton.onClick.AddListener(() => {
            LoadSavedConfiguration();
        });
        
        presetsYPos -= 55;
        
        // Set presets section height
        RectTransform presetsRect = presetsSection.GetComponent<RectTransform>();
        presetsRect.sizeDelta = new Vector2(290, Mathf.Abs(presetsYPos) + 10);
        yPos += presetsYPos - 20;
        
        // Setup section toggle functionality
        SetupSectionToggle(motionSectionToggle, motionSection);
        SetupSectionToggle(visualsSectionToggle, visualsSection);
        SetupSectionToggle(colorSectionToggle, colorSection);
        SetupSectionToggle(environmentSectionToggle, environmentSection);
        SetupSectionToggle(cameraSectionToggle, cameraSection);
        SetupSectionToggle(presetsSectionToggle, presetsSection);
        
        // Setup HSV sliders to update preview
        if (hueSlider != null && saturationSlider != null && valueSlider != null && colorPreview != null)
        {
            System.Action updatePreview = () => {
                Color newColor = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
                Image preview = colorPreview.GetComponent<Image>();
                if (preview != null) preview.color = newColor;
            };
            
            hueSlider.onValueChanged.AddListener((val) => {
                updatePreview();
                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            saturationSlider.onValueChanged.AddListener((val) => {
                updatePreview();
                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            valueSlider.onValueChanged.AddListener((val) => {
                updatePreview();
                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
        }
        
        // Instructions - Modern style
        GameObject instructions = new GameObject("Instructions");
        instructions.transform.SetParent(uiParent, false);
        RectTransform instructRect = instructions.AddComponent<RectTransform>();
        instructRect.anchorMin = new Vector2(0, 1);
        instructRect.anchorMax = new Vector2(0, 1);
        instructRect.pivot = new Vector2(0, 1);
        instructRect.anchoredPosition = new Vector2(15, yPos);
        instructRect.sizeDelta = new Vector2(290, 110);
        
        // Add subtle background for instructions
        Image instructBg = instructions.AddComponent<Image>();
        instructBg.color = new Color(0.05f, 0.05f, 0.15f, 0.4f);
        
        GameObject instructTextObj = new GameObject("InstructionsText");
        instructTextObj.transform.SetParent(instructions.transform, false);
        RectTransform instructTextRect = instructTextObj.AddComponent<RectTransform>();
        instructTextRect.anchorMin = Vector2.zero;
        instructTextRect.anchorMax = Vector2.one;
        instructTextRect.offsetMin = new Vector2(8, 8);
        instructTextRect.offsetMax = new Vector2(-8, -8);
        
        Text instructText = instructTextObj.AddComponent<Text>();
        instructText.text = "💡 QUICK START GUIDE\n━━━━━━━━━━━━━━━━━━━━━━━\n" +
                           "▼/▶ Click headers to expand/collapse\n" +
                           "⌨ WASD/ZQSD: Move • Shift: Sprint\n" +
                           "🖱 Right Click: Look • Scroll: Zoom\n" +
                           "🎨 Click color swatches for instant colors\n" +
                           "⏎ Press ENTER to hide/show this panel\n" +
                           "F1: Keyboard shortcuts • F2: Stats";
        instructText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructText.fontSize = 10;
        instructText.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        instructText.alignment = TextAnchor.UpperLeft;
        instructText.lineSpacing = 1.2f;
        
        // Set final content height based on all UI elements
        yPos -= 125; // Account for instructions height
        contentRect.sizeDelta = new Vector2(0, Mathf.Abs(yPos) + 50);
        
        // Add hint text for Hide UI button - Modern style
        GameObject hintText = new GameObject("HideUIHint");
        hintText.transform.SetParent(canvas.transform, false);
        RectTransform hintRect = hintText.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(1, 1);
        hintRect.anchorMax = new Vector2(1, 1);
        hintRect.pivot = new Vector2(1, 1);
        hintRect.anchoredPosition = new Vector2(-15, -60);
        hintRect.sizeDelta = new Vector2(180, 22);
        Text hint = hintText.AddComponent<Text>();
        hint.text = "⏎ ENTER to toggle";
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 10;
        hint.color = new Color(0.5f, 0.7f, 1f, 0.5f);
        hint.alignment = TextAnchor.MiddleRight;
        hint.fontStyle = FontStyle.Italic;
        
        // Add glow effect
        Shadow hintGlow = hintText.AddComponent<Shadow>();
        hintGlow.effectColor = new Color(0.3f, 0.5f, 1f, 0.3f);
        hintGlow.effectDistance = new Vector2(0, 0);
        
        // Create Performance/Status Display (Bottom-Right) - Optional, can be toggled
        GameObject statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(canvas.transform, false);
        RectTransform statusRect = statusPanel.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(1, 0);
        statusRect.anchorMax = new Vector2(1, 0);
        statusRect.pivot = new Vector2(1, 0);
        statusRect.anchoredPosition = new Vector2(-15, 15);
        statusRect.sizeDelta = new Vector2(200, 120);
        
        Image statusBg = statusPanel.AddComponent<Image>();
        statusBg.color = new Color(0.02f, 0.02f, 0.08f, 0.75f);
        
        Outline statusOutline = statusPanel.AddComponent<Outline>();
        statusOutline.effectColor = new Color(0.3f, 0.5f, 0.9f, 0.25f);
        statusOutline.effectDistance = new Vector2(1, -1);
        
        // Status text container
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(statusPanel.transform, false);
        RectTransform statusTextRect = statusTextObj.AddComponent<RectTransform>();
        statusTextRect.anchorMin = Vector2.zero;
        statusTextRect.anchorMax = Vector2.one;
        statusTextRect.offsetMin = new Vector2(10, 10);
        statusTextRect.offsetMax = new Vector2(-10, -10);
        
        Text statusText = statusTextObj.AddComponent<Text>();
        statusText.text = "⚡ SPIROGRAPH PRO\n━━━━━━━━━━━━━━━━\n📊 FPS: --\n⚙️ Speed: --\n🎨 Effect: --\n📷 Camera: --";
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 10;
        statusText.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        statusText.alignment = TextAnchor.UpperLeft;
        statusText.lineSpacing = 1.2f;
        
        // Initially hide status panel (can be toggled with a hotkey later)
        statusPanel.SetActive(false);
        
        // Add toggle button for status panel
        Button toggleStatusButton = CreateModernButton(canvas.transform, "ToggleStatusButton", new Vector2(-15, 15), new Vector2(35, 35), "📊");
        RectTransform toggleStatusRect = toggleStatusButton.GetComponent<RectTransform>();
        toggleStatusRect.anchorMin = new Vector2(1, 0);
        toggleStatusRect.anchorMax = new Vector2(1, 0);
        toggleStatusRect.pivot = new Vector2(1, 0);
        
        Image toggleStatusImg = toggleStatusButton.GetComponent<Image>();
        toggleStatusImg.color = new Color(0.05f, 0.05f, 0.15f, 0.7f);
        
        toggleStatusButton.onClick.AddListener(() => {
            statusPanel.SetActive(!statusPanel.activeSelf);
        });
        
        // Add keyboard shortcuts panel (initially hidden, toggle with F1)
        GameObject shortcutsPanel = new GameObject("ShortcutsPanel");
        shortcutsPanel.transform.SetParent(canvas.transform, false);
        RectTransform shortcutsRect = shortcutsPanel.AddComponent<RectTransform>();
        shortcutsRect.anchorMin = new Vector2(0.5f, 0.5f);
        shortcutsRect.anchorMax = new Vector2(0.5f, 0.5f);
        shortcutsRect.pivot = new Vector2(0.5f, 0.5f);
        shortcutsRect.anchoredPosition = Vector2.zero;
        shortcutsRect.sizeDelta = new Vector2(500, 400);
        
        Image shortcutsBg = shortcutsPanel.AddComponent<Image>();
        shortcutsBg.color = new Color(0.02f, 0.02f, 0.08f, 0.95f);
        
        Outline shortcutsOutline = shortcutsPanel.AddComponent<Outline>();
        shortcutsOutline.effectColor = new Color(0.4f, 0.6f, 1f, 0.5f);
        shortcutsOutline.effectDistance = new Vector2(2, -2);
        
        // Shortcuts title
        GameObject shortcutsTitleObj = new GameObject("Title");
        shortcutsTitleObj.transform.SetParent(shortcutsPanel.transform, false);
        RectTransform shortcutsTitleRect = shortcutsTitleObj.AddComponent<RectTransform>();
        shortcutsTitleRect.anchorMin = new Vector2(0, 1);
        shortcutsTitleRect.anchorMax = new Vector2(1, 1);
        shortcutsTitleRect.pivot = new Vector2(0.5f, 1);
        shortcutsTitleRect.anchoredPosition = new Vector2(0, -15);
        shortcutsTitleRect.sizeDelta = new Vector2(-30, 40);
        
        Text shortcutsTitleText = shortcutsTitleObj.AddComponent<Text>();
        shortcutsTitleText.text = "⌨ KEYBOARD SHORTCUTS";
        shortcutsTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        shortcutsTitleText.fontSize = 20;
        shortcutsTitleText.fontStyle = FontStyle.Bold;
        shortcutsTitleText.color = new Color(0.7f, 0.85f, 1f, 0.95f);
        shortcutsTitleText.alignment = TextAnchor.MiddleCenter;
        
        // Shortcuts content
        GameObject shortcutsContentObj = new GameObject("Content");
        shortcutsContentObj.transform.SetParent(shortcutsPanel.transform, false);
        RectTransform shortcutsContentRect = shortcutsContentObj.AddComponent<RectTransform>();
        shortcutsContentRect.anchorMin = new Vector2(0, 0);
        shortcutsContentRect.anchorMax = new Vector2(1, 1);
        shortcutsContentRect.offsetMin = new Vector2(20, 50);
        shortcutsContentRect.offsetMax = new Vector2(-20, -60);
        
        Text shortcutsText = shortcutsContentObj.AddComponent<Text>();
        shortcutsText.text = "🎮 CAMERA CONTROLS\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "WASD / ZQSD ........... Move Camera\n" +
                            "Shift ........................... Sprint Mode\n" +
                            "Right Click + Drag ... Rotate View\n" +
                            "Scroll Wheel ............. Zoom In/Out\n\n" +
                            "🎨 UI CONTROLS\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "Enter ......................... Toggle UI Panel\n" +
                            "F1 .............................. Show/Hide This Help\n" +
                            "F2 .............................. Toggle Performance Stats\n" +
                            "Esc ............................. Close Dialogs\n\n" +
                            "✨ QUICK TIPS\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "• Click section headers to collapse/expand\n" +
                            "• Color swatches give instant color changes\n" +
                            "• All sliders update in real-time\n" +
                            "• Right panel scrolls for more controls";
        shortcutsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        shortcutsText.fontSize = 12;
        shortcutsText.color = new Color(0.65f, 0.8f, 0.95f, 0.9f);
        shortcutsText.alignment = TextAnchor.UpperLeft;
        shortcutsText.lineSpacing = 1.3f;
        
        // Close button for shortcuts panel
        Button closeShortcutsButton = CreateModernButton(shortcutsPanel.transform, "CloseButton", new Vector2(0, -10), new Vector2(100, 35), "✕ CLOSE");
        RectTransform closeShortcutsRect = closeShortcutsButton.GetComponent<RectTransform>();
        closeShortcutsRect.anchorMin = new Vector2(0.5f, 0);
        closeShortcutsRect.anchorMax = new Vector2(0.5f, 0);
        closeShortcutsRect.pivot = new Vector2(0.5f, 0);
        
        closeShortcutsButton.onClick.AddListener(() => {
            shortcutsPanel.SetActive(false);
        });
        
        // Initially hide shortcuts panel
        shortcutsPanel.SetActive(false);
        
        // Store references for runtime access
        this.gameObject.AddComponent<PerformanceMonitor>().Initialize(statusText);
        this.gameObject.AddComponent<ShortcutsManager>().Initialize(shortcutsPanel, statusPanel);
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.Selection.activeGameObject = canvasObj;
        #endif
        
        Debug.Log("✓ Generated complete UI! Canvas selected in Hierarchy.");
        Debug.Log("✓ HIDE UI BUTTON: Top-right corner (always visible) - Press ENTER to toggle!");
        Debug.Log("✓ PERFORMANCE STATS: Bottom-right toggle button or F2 key");
        Debug.Log("✓ KEYBOARD SHORTCUTS: Press F1 to view all controls");
        Debug.Log("✓ You can now customize colors, sizes, and positions.");
        Debug.Log("✓ UI automatically connects to SpirographRoller, RotateParent, and CameraController.");
    }
    
    Slider CreateModernSlider(Transform parent, string name, Vector2 position, float min, float max, float value, out Text valueLabel, string labelText, string valueText)
    {
        // Container
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(0, 1);
        sliderRect.pivot = new Vector2(0, 1);
        sliderRect.anchoredPosition = position;
        sliderRect.sizeDelta = new Vector2(290, 24);
        
        // Label (left aligned)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.anchoredPosition = new Vector2(0, 26);
        labelRect.sizeDelta = new Vector2(180, 20);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 12;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.7f, 0.85f, 1f, 0.9f); // Cyan tint
        label.alignment = TextAnchor.MiddleLeft;
        
        // Value Label (right aligned)
        GameObject valueLabelObj = new GameObject("ValueLabel");
        valueLabelObj.transform.SetParent(sliderObj.transform, false);
        RectTransform valueLabelRect = valueLabelObj.AddComponent<RectTransform>();
        valueLabelRect.anchorMin = new Vector2(1, 1);
        valueLabelRect.anchorMax = new Vector2(1, 1);
        valueLabelRect.pivot = new Vector2(1, 1);
        valueLabelRect.anchoredPosition = new Vector2(0, 26);
        valueLabelRect.sizeDelta = new Vector2(100, 20);
        valueLabel = valueLabelObj.AddComponent<Text>();
        valueLabel.text = valueText;
        valueLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueLabel.fontSize = 11;
        valueLabel.color = new Color(0.5f, 0.7f, 1f, 0.8f); // Softer cyan
        valueLabel.alignment = TextAnchor.MiddleRight;
        
        // Background (glassmorphic)
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.15f, 0.25f, 0.4f); // Deep space glass
        
        // Add subtle outline
        Outline bgOutline = bg.AddComponent<Outline>();
        bgOutline.effectColor = new Color(0.2f, 0.4f, 0.7f, 0.3f);
        bgOutline.effectDistance = new Vector2(1, -1);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(4, 4);
        fillAreaRect.offsetMax = new Vector2(-16, -4);
        
        // Fill (gradient cyan-blue)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 1f, 0.6f); // Bright cyan
        
        // Add glow to fill
        Shadow fillGlow = fill.AddComponent<Shadow>();
        fillGlow.effectColor = new Color(0.4f, 0.7f, 1f, 0.5f);
        fillGlow.effectDistance = new Vector2(0, 0);
        
        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(8, 0);
        handleAreaRect.offsetMax = new Vector2(-8, 0);
        
        // Handle (modern circular design)
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(16, 16);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.9f, 0.95f, 1f, 1f); // Bright white-cyan
        
        // Add handle glow
        Shadow handleGlow = handle.AddComponent<Shadow>();
        handleGlow.effectColor = new Color(0.4f, 0.7f, 1f, 0.8f);
        handleGlow.effectDistance = new Vector2(0, 0);
        
        // Slider
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        
        return slider;
    }
    
    Button CreateSectionHeader(Transform parent, string name, Vector2 position, string headerText, bool startExpanded)
    {
        GameObject headerObj = new GameObject(name);
        headerObj.transform.SetParent(parent, false);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(0, 1);
        headerRect.pivot = new Vector2(0, 1);
        headerRect.anchoredPosition = position;
        headerRect.sizeDelta = new Vector2(290, 35);
        
        Image headerImage = headerObj.AddComponent<Image>();
        headerImage.color = new Color(0.12f, 0.15f, 0.25f, 0.9f);
        
        Outline headerOutline = headerObj.AddComponent<Outline>();
        headerOutline.effectColor = new Color(0.4f, 0.6f, 1f, 0.4f);
        headerOutline.effectDistance = new Vector2(1, -1);
        
        Button button = headerObj.AddComponent<Button>();
        button.targetGraphic = headerImage;
        
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 1f, 1f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(headerObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        Text text = textObj.AddComponent<Text>();
        text.text = (startExpanded ? "▼ " : "▶ ") + headerText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 13;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.8f, 0.95f, 1f, 1f);
        text.alignment = TextAnchor.MiddleLeft;
        
        RectTransform textRectPadding = text.GetComponent<RectTransform>();
        textRectPadding.offsetMin = new Vector2(10, 0);
        textRectPadding.offsetMax = new Vector2(-10, 0);
        
        return button;
    }
    
    GameObject CreateSection(Transform parent, string name, Vector2 position)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);
        RectTransform sectionRect = section.AddComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(0, 1);
        sectionRect.anchorMax = new Vector2(0, 1);
        sectionRect.pivot = new Vector2(0, 1);
        sectionRect.anchoredPosition = position;
        sectionRect.sizeDelta = new Vector2(290, 100); // Temp, will be adjusted
        
        Image sectionBg = section.AddComponent<Image>();
        sectionBg.color = new Color(0.03f, 0.03f, 0.1f, 0.5f);
        
        return section;
    }
    
    void SetupSectionToggle(Button toggleButton, GameObject section)
    {
        if (toggleButton == null || section == null) return;
        
        toggleButton.onClick.AddListener(() => {
            section.SetActive(!section.activeSelf);
            Text buttonText = toggleButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                string[] parts = buttonText.text.Split(' ');
                if (parts.Length > 1)
                {
                    string arrow = section.activeSelf ? "▼" : "▶";
                    buttonText.text = arrow + " " + string.Join(" ", parts, 1, parts.Length - 1);
                }
            }
        });
    }
    
    void AddColorPresetLabel(Transform parent, Vector2 position, string labelText)
    {
        GameObject labelObj = new GameObject("PresetLabel");
        labelObj.transform.SetParent(parent, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.anchoredPosition = position;
        labelRect.sizeDelta = new Vector2(290, 20);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 11;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        label.alignment = TextAnchor.MiddleLeft;
    }
    
    Button CreateColorPresetButton(Transform parent, string name, Vector2 position, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.pivot = new Vector2(0, 1);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(32, 28);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        
        Outline buttonOutline = buttonObj.AddComponent<Outline>();
        buttonOutline.effectColor = new Color(1f, 1f, 1f, 0.5f);
        buttonOutline.effectDistance = new Vector2(1, -1);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        
        // Connect to SpirographRoller
        button.onClick.AddListener(() => {
            SpirographRoller roller = FindObjectOfType<SpirographRoller>();
            if (roller != null)
            {
                roller.ChangeLineColor(color);
                
                // Update HSV sliders
                float h, s, v;
                Color.RGBToHSV(color, out h, out s, out v);
                if (hueSlider != null) hueSlider.value = h;
                if (saturationSlider != null) saturationSlider.value = s;
                if (valueSlider != null) valueSlider.value = v;
                
                // Update preview
                if (colorPreview != null)
                {
                    Image preview = colorPreview.GetComponent<Image>();
                    if (preview != null) preview.color = color;
                }
            }
        });
        
        return button;
    }
    
    Dropdown CreateModernDropdown(Transform parent, string name, Vector2 position)
    {
        // Main Dropdown GameObject
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0, 1);
        dropdownRect.anchorMax = new Vector2(0, 1);
        dropdownRect.pivot = new Vector2(0, 1);
        dropdownRect.anchoredPosition = position;
        dropdownRect.sizeDelta = new Vector2(290, 38);
        
        // Background
        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.08f, 0.12f, 0.22f, 0.7f);
        dropdownBg.raycastTarget = true;
        
        Outline dropdownOutline = dropdownObj.AddComponent<Outline>();
        dropdownOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        dropdownOutline.effectDistance = new Vector2(1, -1);
        
        // Dropdown Component
        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownBg;
        
        // Label (shows current selection)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 2);
        labelRect.offsetMax = new Vector2(-30, -2);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "Starfield";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 13;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        labelText.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = labelText;
        
        // Arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0);
        arrowRect.anchorMax = new Vector2(1, 1);
        arrowRect.offsetMin = new Vector2(-25, 0);
        arrowRect.offsetMax = new Vector2(-5, 0);
        Text arrowText = arrowObj.AddComponent<Text>();
        arrowText.text = "▼";
        arrowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowText.fontSize = 12;
        arrowText.color = new Color(0.6f, 0.8f, 1f, 0.8f);
        arrowText.alignment = TextAnchor.MiddleCenter;
        
        // Template (the dropdown list that appears)
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);
        
        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
        templateBg.raycastTarget = true;
        
        // Add Canvas component to template to render it on top of ScrollRect mask
        Canvas templateCanvas = templateObj.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 1000; // Render on top of everything
        templateObj.AddComponent<GraphicRaycaster>();
        
        Outline templateOutline = templateObj.AddComponent<Outline>();
        templateOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        templateOutline.effectDistance = new Vector2(2, -2);
        
        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10;
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = Color.white;
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scrollRect.viewport = viewportRect;
        
        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 150);
        
        scrollRect.content = contentRect;
        
        // Item (template for each option)
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(0, 30);
        
        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        itemToggle.isOn = false;
        itemToggle.interactable = true;
        
        Image itemBg = itemObj.AddComponent<Image>();
        itemBg.color = new Color(0.1f, 0.15f, 0.25f, 1f);
        itemBg.raycastTarget = true;
        itemToggle.targetGraphic = itemBg;
        
        ColorBlock toggleColors = itemToggle.colors;
        toggleColors.normalColor = new Color(1f, 1f, 1f, 1f);
        toggleColors.highlightedColor = new Color(0.8f, 0.95f, 1f, 1f);
        toggleColors.pressedColor = new Color(0.6f, 0.8f, 1f, 1f);
        toggleColors.selectedColor = new Color(0.4f, 0.7f, 1f, 1f);
        itemToggle.colors = toggleColors;
        
        // Item Label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(10, 1);
        itemLabelRect.offsetMax = new Vector2(-10, -1);
        Text itemLabelText = itemLabelObj.AddComponent<Text>();
        itemLabelText.text = "Option";
        itemLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemLabelText.fontSize = 12;
        itemLabelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        itemLabelText.alignment = TextAnchor.MiddleLeft;
        dropdown.itemText = itemLabelText;
        
        dropdown.template = templateRect;
        templateObj.SetActive(false);
        
        // Populate with skybox names and connect to SkyboxManager
        SkyboxManager skyboxManager = FindObjectOfType<SkyboxManager>();
        if (skyboxManager == null)
        {
            // Create SkyboxManager if it doesn't exist
            GameObject managerObj = new GameObject("SkyboxManager");
            skyboxManager = managerObj.AddComponent<SkyboxManager>();
        }
        
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(skyboxManager.GetAllSkyboxNames()));
        
        // Set current value
        int currentIndex = skyboxManager.GetCurrentSkyboxIndex();
        if (currentIndex >= 0)
        {
            dropdown.value = currentIndex;
        }
        
        // Connect listener
        dropdown.onValueChanged.AddListener((index) => {
            skyboxManager.SetSkybox(index);
        });
        
        return dropdown;
    }
    
    Button CreateModernButton(Transform parent, string name, Vector2 position, Vector2 size, string buttonText)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.pivot = new Vector2(0, 1);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = size;
        
        // Glassmorphic background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.08f, 0.12f, 0.22f, 0.7f); // Deep space glass
        
        // Add subtle outline
        Outline buttonOutline = buttonObj.AddComponent<Outline>();
        buttonOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f); // Cyan glow
        buttonOutline.effectDistance = new Vector2(1, -1);
        
        // Add hover glow effect
        Shadow buttonGlow = buttonObj.AddComponent<Shadow>();
        buttonGlow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        buttonGlow.effectDistance = new Vector2(0, 0);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Setup hover colors with enhanced feedback
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f); // Bright on hover
        colors.pressedColor = new Color(0.6f, 0.8f, 1f, 1f); // Cyan on press
        colors.selectedColor = new Color(0.85f, 0.95f, 1f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.colorMultiplier = 1.2f;
        colors.fadeDuration = 0.15f;
        button.colors = colors;
        
        // Add enhanced button animator for visual feedback
        ButtonAnimator animator = buttonObj.AddComponent<ButtonAnimator>();
        animator.button = button;
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 12;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.85f, 0.95f, 1f, 0.95f); // Bright cyan-white
        text.alignment = TextAnchor.MiddleCenter;
        
        // Add text shadow for depth
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.5f);
        textShadow.effectDistance = new Vector2(1, -1);
        
        return button;
    }
}

/// <summary>
/// Monitors and displays performance metrics in real-time
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    private Text statusText;
    private float deltaTime = 0.0f;
    private float updateInterval = 0.5f; // Update twice per second
    private float timeSinceLastUpdate = 0f;
    
    public void Initialize(Text text)
    {
        statusText = text;
    }
    
    void Update()
    {
        if (statusText == null) return;
        
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        timeSinceLastUpdate += Time.unscaledDeltaTime;
        
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            UpdateStats();
        }
    }
    
    void UpdateStats()
    {
        float fps = 1.0f / deltaTime;
        SpirographRoller roller = FindObjectOfType<SpirographRoller>();
        CameraController camera = FindObjectOfType<CameraController>();
        
        string fpsColor = fps >= 60 ? "✅" : fps >= 30 ? "⚠️" : "❌";
        string speedValue = roller != null ? roller.speed.ToString("F1") : "--";
        string effectValue = roller != null ? roller.GetCurrentEffectName() : "--";
        string cameraMode = camera != null && camera.IsAutoOrbitEnabled() ? "Auto Orbit" : "Manual";
        
        statusText.text = $"⚡ SPIROGRAPH PRO\n━━━━━━━━━━━━━━━━\n{fpsColor} FPS: {fps:F0}\n⚙️ Speed: {speedValue}\n🎨 Effect: {effectValue}\n📷 Camera: {cameraMode}";
    }
}

/// <summary>
/// Manages keyboard shortcuts for UI panels
/// </summary>
public class ShortcutsManager : MonoBehaviour
{
    private GameObject shortcutsPanel;
    private GameObject statusPanel;
    
    public void Initialize(GameObject shortcuts, GameObject status)
    {
        shortcutsPanel = shortcuts;
        statusPanel = status;
    }
    
    void Update()
    {
        // F1 - Toggle keyboard shortcuts help
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (shortcutsPanel != null)
            {
                shortcutsPanel.SetActive(!shortcutsPanel.activeSelf);
                Debug.Log(shortcutsPanel.activeSelf ? "📖 Shortcuts panel shown" : "📖 Shortcuts panel hidden");
            }
        }
        
        // F2 - Toggle performance stats
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (statusPanel != null)
            {
                statusPanel.SetActive(!statusPanel.activeSelf);
                Debug.Log(statusPanel.activeSelf ? "📊 Performance stats shown" : "📊 Performance stats hidden");
            }
        }
        
        // Escape - Close all panels
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (shortcutsPanel != null && shortcutsPanel.activeSelf)
            {
                shortcutsPanel.SetActive(false);
                Debug.Log("📖 Shortcuts panel closed");
            }
        }
    }
}

/// <summary>
/// Adds smooth scale animation to buttons on hover and click
/// Makes the UI feel more responsive and professional
/// </summary>
public class ButtonAnimator : MonoBehaviour
{
    public Button button;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;
    private bool isPressed = false;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        targetScale = originalScale;
    }
    
    void Update()
    {
        if (button == null || rectTransform == null) return;
        
        // Check if mouse is over button (simple raycast check)
        bool wasHovering = isHovering;
        isHovering = UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() &&
                    button.IsInteractable();
        
        // Determine target scale based on state
        if (isPressed)
        {
            targetScale = originalScale * 0.95f; // Pressed: slightly smaller
        }
        else if (isHovering)
        {
            targetScale = originalScale * 1.05f; // Hover: slightly larger
        }
        else
        {
            targetScale = originalScale; // Normal
        }
        
        // Smooth interpolation to target scale
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * 12f);
    }
    
    void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonPressed);
        }
    }
    
    void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonPressed);
        }
    }
    
    void OnButtonPressed()
    {
        // Quick pulse animation on click
        StartCoroutine(PulseAnimation());
    }
    
    System.Collections.IEnumerator PulseAnimation()
    {
        isPressed = true;
        yield return new WaitForSecondsRealtime(0.1f);
        isPressed = false;
    }
}

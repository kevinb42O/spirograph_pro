using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    [Header("Active Rotor Tracking")]
    [Tooltip("The currently active rotor that UI controls")]
    public SpirographRoller activeRoller = null;
    
    [Header("Per-Agent Control")]
    [Tooltip("The currently selected agent that the ENTIRE UI controls")]
    public PathAgent selectedAgent = null;
    
    [Tooltip("Are we currently in per-agent control mode?")]
    public bool perAgentControlMode = false;
    
    [Header("UI Elements - Auto-populated after generation")]
    public Slider speedSlider;
    public Text speedText;
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
    public Button cameraSectionToggle;
    
    [Header("Section GameObjects")]
    public GameObject motionSection;
    public GameObject visualsSection;
    public GameObject colorSection;
    public GameObject environmentSection;
    public GameObject cameraSection;
    
    [Header("Skybox Dropdown")]
    public Dropdown skyboxDropdown;
    
    [Header("Pattern Generator")]
    public Dropdown patternDropdown;
    public Button generatePatternButton;
    public Button clearPatternsButton;
    
    [Header("Multi-Agent System")]
    public Toggle multiAgentToggle;
    public Slider agentCountSlider;
    public Text agentCountText;
    public Dropdown agentColorModeDropdown;
    public Dropdown agentSpawnModeDropdown;
    public MultiAgentManager multiAgentManager;
    public SharedPathState sharedPathState;
    public AgentPanelUI agentPanelUI;
    
    [Header("UI State")]
    private GameObject controlPanel;
    private bool isUIVisible = true;
    private Text panelTitleText; // Reference to panel title for dynamic updates
    private ScrollRect controlPanelScrollRect; // Reference to scroll rect for auto-scrolling
    
    [Header("2025 Modern UI - Context System")]
    private GameObject contextBanner; // Large prominent banner showing WHO is being controlled
    private Text contextBannerText; // "CONTROLLING: MASTER ROTOR" or "CONTROLLING: AGENT #3"
    private Image contextBannerBackground; // Glassmorphic background
    private CanvasGroup contextBannerGroup; // For smooth fade transitions
    private Image contextBannerAccent; // Color accent bar matching agent color
    private bool isTransitioningContext = false; // Prevent double transitions
    
    [Header("Generate UI")]
    [Tooltip("Check this box to generate UI (will auto-uncheck after generation)")]
    public bool generateUI = false;
    
    void OnValidate()
    {
        if (generateUI)
        {
            generateUI = false;
            #if UNITY_EDITOR
            // Delay execution to next editor update to ensure proper serialization
            UnityEditor.EditorApplication.delayCall += () => {
                if (this != null)
                {
                    GenerateCompleteUI();
                }
            };
            #endif
        }
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
        
        // Reconnect all UI elements at runtime (listeners from Edit mode don't persist)
        ReconnectUIElements();
        
        // Connect Pattern Generator
        ConnectPatternGenerator();
        
        // Connect Multi-Agent System
        ConnectMultiAgentSystem();
        
        // Subscribe to PatternSpawner rotor change events
        SubscribeToPatternSpawner();
        
        // Set initial active rotor
        if (activeRoller == null)
        {
            activeRoller = FindFirstObjectByType<SpirographRoller>();
            if (activeRoller != null)
            {
                Debug.Log($"✓ Initial active rotor set: {activeRoller.gameObject.name}");
            }
        }
        
        // Subscribe to MultiAgentManager selection events
        if (multiAgentManager != null)
        {
            multiAgentManager.OnAgentSelected += OnAgentSelectedForControl;
            Debug.Log("✓ UI Manager subscribed to agent selection events");
        }
        
        // Cache ScrollRect reference for auto-scroll functionality
        if (controlPanel != null)
        {
            controlPanelScrollRect = controlPanel.GetComponent<ScrollRect>();
            if (controlPanelScrollRect != null)
            {
                Debug.Log("✓ Control panel ScrollRect cached for auto-scroll");
            }
        }
    }
    
    /// <summary>
    /// Subscribe to PatternSpawner events to track active rotor changes
    /// </summary>
    void SubscribeToPatternSpawner()
    {
        PatternSpawner spawner = FindFirstObjectByType<PatternSpawner>();
        if (spawner != null)
        {
            spawner.OnActiveRotorChanged += OnActiveRotorChanged;
            Debug.Log("✓ UI Manager subscribed to PatternSpawner rotor change events");
        }
    }
    
    /// <summary>
    /// ★★★ ULTIMATE PER-AGENT CONTROL ★★★
    /// Called when an agent is selected in the Agent Panel UI.
    /// This binds the ENTIRE main UI control panel to control THAT specific agent.
    /// This is the ULTIMATE control system - users can adjust every single parameter per agent!
    /// </summary>
    void OnAgentSelectedForControl(PathAgent agent)
    {
        if (agent == null)
        {
            Debug.LogWarning("[UIManager] Agent selection is null, disabling per-agent control");
            ExitPerAgentControl();
            return;
        }
        
        Debug.Log($"★★★ ULTIMATE PER-AGENT CONTROL ACTIVATED for Agent {agent.agentIndex} ★★★");
        
        // Enter per-agent control mode
        perAgentControlMode = true;
        selectedAgent = agent;
        
        // Enable individual settings mode on the agent
        agent.useIndividualSettings = true;
        
        // Bind ALL UI controls to this agent's properties
        BindUIToSelectedAgent(agent);
        
        // Update modern context banner
        StartCoroutine(TransitionToAgentContext(agent));
        
        Debug.Log($"✓ All UI controls now bound to Agent {agent.agentIndex} - {agent.agentName}");
        Debug.Log($"✓ User has FULL CONTROL over this agent's motion, visuals, and color!");
    }
    
    /// <summary>
    /// Bind ALL UI sliders, buttons, and controls to the selected agent
    /// This makes the entire main UI control THIS specific agent
    /// </summary>
    void BindUIToSelectedAgent(PathAgent agent)
    {
        if (agent == null) return;
        
        // ========== SPEED SLIDER ==========
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.value = agent.agentSpeed; // Set to agent's current value
            speedSlider.onValueChanged.AddListener((value) => {
                agent.agentSpeed = value;
                agent.useIndividualSettings = true; // Ensure agent uses its own settings
                if (speedText != null)
                {
                    speedText.text = "Travel Speed: " + value.ToString("F1");
                }
                Debug.Log($"Agent {agent.agentIndex} speed: {value:F1}");
            });
            if (speedText != null)
            {
                speedText.text = "Travel Speed: " + agent.agentSpeed.ToString("F1");
            }
        }
        
        // ========== ROTATION SPEED SLIDER ==========
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.onValueChanged.RemoveAllListeners();
            rotationSpeedSlider.value = agent.agentRotationSpeed;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                agent.agentRotationSpeed = value;
                agent.useIndividualSettings = true;
                Debug.Log($"Agent {agent.agentIndex} rotation speed: {value:F2}");
            });
        }
        
        // ========== CYCLES SLIDER ==========
        if (cyclesSlider != null)
        {
            cyclesSlider.onValueChanged.RemoveAllListeners();
            cyclesSlider.value = agent.agentCycles;
            cyclesSlider.onValueChanged.AddListener((value) => {
                agent.agentCycles = (int)value;
                agent.useIndividualSettings = true;
                Debug.Log($"Agent {agent.agentIndex} cycles: {(int)value}");
            });
        }
        
        // ========== PEN DISTANCE SLIDER ==========
        if (penDistanceSlider != null)
        {
            penDistanceSlider.onValueChanged.RemoveAllListeners();
            penDistanceSlider.value = agent.agentPenDistance;
            penDistanceSlider.onValueChanged.AddListener((value) => {
                agent.agentPenDistance = value;
                agent.useIndividualSettings = true;
                Debug.Log($"Agent {agent.agentIndex} pen distance: {value:F2}x");
            });
        }
        
        // ========== LINE WIDTH SLIDER ==========
        if (lineWidthSlider != null)
        {
            lineWidthSlider.onValueChanged.RemoveAllListeners();
            lineWidthSlider.value = agent.agentLineWidth;
            lineWidthSlider.onValueChanged.AddListener((value) => {
                agent.agentLineWidth = value;
                agent.UpdateLineWidth(value);
                agent.useIndividualSettings = true;
                Debug.Log($"Agent {agent.agentIndex} line width: {value:F2}");
            });
        }
        
        // ========== LINE BRIGHTNESS SLIDER ==========
        if (lineBrightnessSlider != null)
        {
            lineBrightnessSlider.onValueChanged.RemoveAllListeners();
            lineBrightnessSlider.value = agent.agentLineBrightness;
            lineBrightnessSlider.onValueChanged.AddListener((value) => {
                agent.agentLineBrightness = value;
                agent.useIndividualSettings = true;
                Debug.Log($"Agent {agent.agentIndex} line brightness: {value:F2}");
            });
        }
        
        // ========== COLOR SLIDERS (HSV) ==========
        if (hueSlider != null && saturationSlider != null && valueSlider != null && colorPreview != null)
        {
            // Extract current HSV from agent color
            float h, s, v;
            Color.RGBToHSV(agent.agentColor, out h, out s, out v);
            
            hueSlider.onValueChanged.RemoveAllListeners();
            saturationSlider.onValueChanged.RemoveAllListeners();
            valueSlider.onValueChanged.RemoveAllListeners();
            
            hueSlider.value = h;
            saturationSlider.value = s;
            valueSlider.value = v;
            
            System.Action updateAgentColor = () => {
                float hueVal = hueSlider.value;
                float satVal = saturationSlider.value;
                float valVal = valueSlider.value;
                Color newColor = Color.HSVToRGB(hueVal, satVal, valVal);
                
                // Update preview
                Image preview = colorPreview.GetComponent<Image>();
                if (preview != null) preview.color = newColor;
                
                // Change agent color
                agent.SetColor(newColor);
                Debug.Log($"Agent {agent.agentIndex} color changed");
            };
            
            hueSlider.onValueChanged.AddListener((val) => {
                updateAgentColor();
                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            saturationSlider.onValueChanged.AddListener((val) => {
                updateAgentColor();
                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            valueSlider.onValueChanged.AddListener((val) => {
                updateAgentColor();
                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            // Update preview immediately
            Image previewImg = colorPreview.GetComponent<Image>();
            if (previewImg != null) previewImg.color = agent.agentColor;
        }
        
        // ========== PAUSE/RESUME BUTTON ==========
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(() => {
                if (agent.status == PathAgent.AgentStatus.Active)
                {
                    agent.Pause();
                }
                else if (agent.status == PathAgent.AgentStatus.Paused)
                {
                    agent.Resume();
                }
                Text btnText = pauseButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = agent.isPaused ? "▶ RESUME" : "⏸ PAUSE";
                }
                Debug.Log($"Agent {agent.agentIndex} {(agent.isPaused ? "Paused" : "Resumed")}");
            });
            
            // Update button text based on current state
            Text pauseText = pauseButton.GetComponentInChildren<Text>();
            if (pauseText != null)
            {
                pauseText.text = agent.isPaused ? "▶ RESUME" : "⏸ PAUSE";
            }
        }
        
        // ========== RESET BUTTON ==========
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() => {
                agent.ResetAgent();
                Debug.Log($"Agent {agent.agentIndex} reset to starting position");
            });
        }
        
        Debug.Log($"✓✓✓ ULTIMATE CONTROL: All {11} UI controls bound to Agent {agent.agentIndex}!");
        Debug.Log($"  → Speed, Rotation, Cycles, Pen Distance, Line Width, Brightness, HSV Color");
        Debug.Log($"  → Pause/Resume, Reset - EVERYTHING is now controlled per-agent!");
        
        // Update panel title to show we're in per-agent control mode
        UpdatePanelTitle();
    }
    
    /// <summary>
    /// Update the control panel title to reflect current control mode
    /// </summary>
    void UpdatePanelTitle()
    {
        if (panelTitleText == null)
        {
            // Try to find the title through the control panel hierarchy
            if (controlPanel != null)
            {
                // Navigate: ControlPanel -> Viewport -> Content -> PanelTitle
                Transform viewport = controlPanel.transform.Find("Viewport");
                if (viewport != null)
                {
                    Transform content = viewport.Find("Content");
                    if (content != null)
                    {
                        Transform titleTransform = content.Find("PanelTitle");
                        if (titleTransform != null)
                        {
                            panelTitleText = titleTransform.GetComponent<Text>();
                            Debug.Log("✓ Found PanelTitle text component");
                        }
                    }
                }
            }
            
            // Fallback: try global search
            if (panelTitleText == null)
            {
                GameObject panelTitleObj = GameObject.Find("PanelTitle");
                if (panelTitleObj != null)
                {
                    panelTitleText = panelTitleObj.GetComponent<Text>();
                }
            }
        }
        
        // Update both legacy title and modern context banner
        if (panelTitleText != null)
        {
            if (perAgentControlMode && selectedAgent != null)
            {
                // Show we're controlling a specific agent
                panelTitleText.text = $"✦ AGENT {selectedAgent.agentIndex} CONTROL ✦";
                panelTitleText.color = selectedAgent.agentColor; // Use agent's color!
                Debug.Log($"★ Panel title updated: Controlling Agent {selectedAgent.agentIndex}");
            }
            else
            {
                // Normal mode - controlling master/rotor
                panelTitleText.text = "✦ SPIROGRAPH CONTROLS";
                panelTitleText.color = new Color(0.7f, 0.85f, 1f, 0.9f); // Default cyan
            }
        }
        
        // Update modern context banner
        UpdateContextBanner();
    }
    
    /// <summary>
    /// Scroll the control panel to the top (Motion Controls section)
    /// This provides instant visual feedback when selecting an agent
    /// </summary>
    void ScrollToTop()
    {
        if (controlPanelScrollRect == null)
        {
            // Try to find the ScrollRect if not yet cached
            if (controlPanel != null)
            {
                controlPanelScrollRect = controlPanel.GetComponent<ScrollRect>();
            }
        }
        
        if (controlPanelScrollRect != null)
        {
            // Smoothly animate scroll to top
            StartCoroutine(AnimateScrollToTop());
            Debug.Log("★ Auto-scrolled to Motion Controls - ready for per-agent control!");
        }
        else
        {
            Debug.LogWarning("[UIManager] Could not find ScrollRect for auto-scroll");
        }
    }
    
    /// <summary>
    /// Animate smooth scroll to top of control panel
    /// </summary>
    System.Collections.IEnumerator AnimateScrollToTop()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        float startValue = controlPanelScrollRect.verticalNormalizedPosition;
        float targetValue = 1f; // 1 = top, 0 = bottom
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth easing
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            controlPanelScrollRect.verticalNormalizedPosition = Mathf.Lerp(startValue, targetValue, smoothT);
            yield return null;
        }
        
        controlPanelScrollRect.verticalNormalizedPosition = targetValue;
    }
    
    /// <summary>
    /// Exit per-agent control mode and return to master/rotor control
    /// EDGE CASE SAFE: Handles agent deletion, null agents, and invalid states
    /// </summary>
    public void ExitPerAgentControl()
    {
        if (!perAgentControlMode && selectedAgent == null) return; // Already in master mode
        
        Debug.Log("Exiting per-agent control mode, returning to master control");
        
        // Safely disable individual settings
        if (selectedAgent != null)
        {
            try {
                selectedAgent.useIndividualSettings = false; // Agent will use master settings again
            }
            catch (System.Exception e) {
                Debug.LogWarning($"Could not disable individual settings on agent: {e.Message}");
            }
        }
        
        perAgentControlMode = false;
        selectedAgent = null;
        
        // Smooth transition to master context
        StartCoroutine(TransitionToMasterContext());
        
        // Rebind UI to active rotor or master settings
        if (activeRoller != null)
        {
            ConnectSlidersToActiveRotor();
        }
        else
        {
            // Fallback: try to find any active rotor
            activeRoller = FindFirstObjectByType<SpirographRoller>();
            if (activeRoller != null)
            {
                ConnectSlidersToActiveRotor();
            }
        }
        
        // Update title back to normal
        UpdatePanelTitle();
        
        Debug.Log("✓ Returned to master control mode");
    }
    
    /// <summary>
    /// Called when the active rotor changes (new pattern spawned)
    /// </summary>
    void OnActiveRotorChanged(SpirographRoller newActiveRotor, SpirographRoller oldRotor)
    {
        Debug.Log($"[UI Manager] Active rotor changed! Old: {oldRotor?.gameObject.name ?? "None"}, New: {newActiveRotor?.gameObject.name ?? "None"}");
        
        // Update active rotor reference
        activeRoller = newActiveRotor;
        
        // Reconnect UI sliders to new active rotor
        if (activeRoller != null)
        {
            ConnectSlidersToActiveRotor();
            Debug.Log($"✓ UI now controlling new rotor: {activeRoller.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Connect all UI sliders to the currently active rotor
    /// </summary>
    void ConnectSlidersToActiveRotor()
    {
        if (activeRoller == null) return;
        
        // Speed Slider
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null)
                {
                    activeRoller.speed = value;
                    if (speedText != null)
                    {
                        speedText.text = "Travel Speed: " + value.ToString("F1");
                    }
                    Debug.Log($"Speed slider changed: {value} -> activeRoller.speed is now {activeRoller.speed}");
                }
            });
            speedSlider.value = activeRoller.speed; // Set value AFTER adding listener
            if (speedText != null)
            {
                speedText.text = "Travel Speed: " + activeRoller.speed.ToString("F1");
            }
            Debug.Log($"✓ Speed slider connected to active rotor (current speed: {activeRoller.speed})");
        }
        
        // Rotation Speed Slider
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.onValueChanged.RemoveAllListeners();
            rotationSpeedSlider.value = activeRoller.rotationSpeed;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null) activeRoller.rotationSpeed = value;
            });
        }
        
        // Pen Distance Slider
        if (penDistanceSlider != null)
        {
            penDistanceSlider.onValueChanged.RemoveAllListeners();
            penDistanceSlider.value = activeRoller.penDistance;
            penDistanceSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null) activeRoller.penDistance = value;
            });
        }
        
        // Cycles Slider
        if (cyclesSlider != null)
        {
            cyclesSlider.onValueChanged.RemoveAllListeners();
            cyclesSlider.value = activeRoller.cycles;
            cyclesSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null) activeRoller.cycles = (int)value;
            });
        }
        
        Debug.Log("✓ All UI sliders connected to active rotor");
    }
    
    void ReconnectUIElements()
    {
        // Reconnect section toggles
        if (motionSectionToggle != null && motionSection != null)
            SetupSectionToggle(motionSectionToggle, motionSection);
        if (visualsSectionToggle != null && visualsSection != null)
            SetupSectionToggle(visualsSectionToggle, visualsSection);
        if (colorSectionToggle != null && colorSection != null)
            SetupSectionToggle(colorSectionToggle, colorSection);
        if (environmentSectionToggle != null && environmentSection != null)
            SetupSectionToggle(environmentSectionToggle, environmentSection);
        if (cameraSectionToggle != null && cameraSection != null)
            SetupSectionToggle(cameraSectionToggle, cameraSection);
        
        // Reconnect camera buttons
        CameraController cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            if (lookAtButton != null)
            {
                lookAtButton.onClick.RemoveAllListeners();
                lookAtButton.onClick.AddListener(() => {
                    cameraController.SetCameraMode(CameraController.CameraMode.FreeFly);
                    Debug.Log("Camera Mode: Free Fly");
                });
            }
            
            if (smoothFollowButton != null)
            {
                smoothFollowButton.onClick.RemoveAllListeners();
                smoothFollowButton.onClick.AddListener(() => {
                    cameraController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                    Debug.Log("Camera Mode: Smooth Follow");
                });
            }
            
            if (autoOrbitButton != null)
            {
                autoOrbitButton.onClick.RemoveAllListeners();
                autoOrbitButton.onClick.AddListener(() => {
                    cameraController.SetCameraMode(CameraController.CameraMode.AutoOrbit);
                    Debug.Log("Camera Mode: Auto Orbit");
                });
            }
        }
        
        // Reconnect HSV sliders
        if (hueSlider != null && saturationSlider != null && valueSlider != null && colorPreview != null)
        {
            hueSlider.onValueChanged.RemoveAllListeners();
            saturationSlider.onValueChanged.RemoveAllListeners();
            valueSlider.onValueChanged.RemoveAllListeners();
            
            System.Action updateColorAndPreview = () => {
                float h = hueSlider.value;
                float s = saturationSlider.value;
                float v = valueSlider.value;
                Color newColor = Color.HSVToRGB(h, s, v);
                
                // Update preview
                Image preview = colorPreview.GetComponent<Image>();
                if (preview != null) preview.color = newColor;
                
                // Change line color on ACTIVE rotor - creates a NEW trail preserving the old one
                if (activeRoller != null)
                {
                    activeRoller.ChangeLineColor(newColor);
                }
            };
            
            hueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            saturationSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            valueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
        }
        
        // Reconnect color   preset buttons
        if (colorPresetButtons != null && colorPresetButtons.Length > 0)
        {
            Color[] presetColors = new Color[] {
                Color.white, Color.black, Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green,
                Color.cyan, Color.blue, new Color(0.5f, 0f, 1f), Color.magenta,
                new Color(1f, 0.4f, 0.7f), new Color(0.5f, 0.3f, 0.1f), new Color(0.8f, 0.8f, 0.8f),
                new Color(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.3f, 0.3f), new Color(1f, 0.8f, 0f)
            };
            
            for (int i = 0; i < colorPresetButtons.Length && i < presetColors.Length; i++)
            {
                if (colorPresetButtons[i] != null)
                {
                    Button btn = colorPresetButtons[i];
                    Color color = presetColors[i];
                    
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"🎨 Preset button clicked! Color: {color}");
                        
                        SpirographRoller roller = FindFirstObjectByType<SpirographRoller>();
                        if (roller == null)
                        {
                            Debug.LogError("❌ SpirographRoller not found!");
                            return;
                        }
                        
                        try
                        {
                            // Update HSV sliders
                            float h, s, v;
                            Color.RGBToHSV(color, out h, out s, out v);
                            
                            if (hueSlider != null) 
                            {
                                hueSlider.SetValueWithoutNotify(h);
                                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                                if (text != null) text.text = h.ToString("F2");
                            }
                            if (saturationSlider != null) 
                            {
                                saturationSlider.SetValueWithoutNotify(s);
                                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                                if (text != null) text.text = s.ToString("F2");
                            }
                            if (valueSlider != null) 
                            {
                                valueSlider.SetValueWithoutNotify(v);
                                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                                if (text != null) text.text = v.ToString("F2");
                            }
                            
                            // Change color
                            roller.ChangeLineColor(color);
                            
                            // Update preview
                            if (colorPreview != null)
                            {
                                Image preview = colorPreview.GetComponent<Image>();
                                if (preview != null) preview.color = color;
                            }
                            
                            Debug.Log($"✅ Color changed to {color}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"❌ Error: {e.Message}");
                        }
                    });
                }
            }
            
            Debug.Log($"✓ Reconnected {colorPresetButtons.Length} color preset buttons!");
        }
        
        // Reconnect skybox dropdown
        if (skyboxDropdown != null)
        {
            skyboxDropdown.onValueChanged.RemoveAllListeners();
            
            SkyboxManager skyboxManager = FindFirstObjectByType<SkyboxManager>();
            if (skyboxManager == null)
            {
                // Create SkyboxManager if it doesn't exist
                GameObject managerObj = new GameObject("SkyboxManager");
                skyboxManager = managerObj.AddComponent<SkyboxManager>();
                Debug.Log("✓ Created SkyboxManager");
            }
            
            // Connect listener - changes skybox IMMEDIATELY when dropdown value changes
            skyboxDropdown.onValueChanged.AddListener((index) => {
                Debug.Log($"🌌 Skybox dropdown changed to index: {index}");
                skyboxManager.SetSkybox(index);
            });
            
            Debug.Log("✓ Skybox dropdown listener reconnected - will change skybox immediately!");
        }
        else
        {
            Debug.LogWarning("⚠ No color preset buttons found to reconnect!");
        }
        
        // Reconnect remaining sliders to active rotor
        if (activeRoller != null)
        {
            // Line Width Slider
            if (lineWidthSlider != null)
            {
                lineWidthSlider.onValueChanged.RemoveAllListeners();
                lineWidthSlider.onValueChanged.AddListener((value) => {
                    if (activeRoller != null)
                    {
                        activeRoller.lineWidth = value;
                    }
                });
            }
            
            // Line Brightness Slider
            if (lineBrightnessSlider != null)
            {
                lineBrightnessSlider.onValueChanged.RemoveAllListeners();
                lineBrightnessSlider.onValueChanged.AddListener((value) => {
                    if (activeRoller != null)
                    {
                        activeRoller.lineBrightness = value;
                    }
                });
            }
            
            // Pen Distance Slider (already in ConnectSlidersToActiveRotor)
            // Cycles Slider (already in ConnectSlidersToActiveRotor)
            
            // Object Rotation Speed Slider
            if (objectRotationSpeedSlider != null)
            {
                objectRotationSpeedSlider.onValueChanged.RemoveAllListeners();
                objectRotationSpeedSlider.onValueChanged.AddListener((value) => {
                    RotateParent rotateParent = FindFirstObjectByType<RotateParent>();
                    if (rotateParent != null)
                    {
                        // Use reflection to set the private rotationSpeedMultiplier field
                        var field = typeof(RotateParent).GetField("rotationSpeedMultiplier", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null)
                        {
                            field.SetValue(rotateParent, value);
                        }
                        
                        // Update the slider text if it exists
                        Text sliderText = objectRotationSpeedSlider.GetComponentInChildren<Text>();
                        if (sliderText != null)
                        {
                            sliderText.text = "Object Rotation Speed: " + value.ToString("F1");
                        }
                    }
                });
            }
        }
        
        // Reconnect pause button
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    activeRoller.TogglePause();
                    Text btnText = pauseButton.GetComponentInChildren<Text>();
                    if (btnText != null)
                    {
                        btnText.text = activeRoller.IsPaused() ? "▶ RESUME" : "⏸ PAUSE";
                    }
                    Debug.Log($"Rotor {(activeRoller.IsPaused() ? "Paused" : "Resumed")}");
                }
            });
        }
        
        // Reconnect reset button
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    activeRoller.ResetSpirograph();
                    Debug.Log("Rotor position reset");
                }
            });
        }
        
        // Reconnect toggle visuals button
        if (toggleVisualsButton != null)
        {
            toggleVisualsButton.onClick.RemoveAllListeners();
            toggleVisualsButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    activeRoller.ToggleVisuals();
                    Debug.Log("Rotor visuals toggled");
                }
            });
        }
        
        // Reconnect line effects button
        if (lineEffectsButton != null)
        {
            lineEffectsButton.onClick.RemoveAllListeners();
            lineEffectsButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    // Cycle through line effects using the lineEffectMode enum
                    int currentMode = (int)activeRoller.lineEffectMode;
                    int nextMode = (currentMode + 1) % 8; // 8 effect types: None, Glow, Rainbow, etc.
                    activeRoller.lineEffectMode = (SpirographRoller.LineEffectMode)nextMode;
                    Debug.Log($"Line effect changed to: {activeRoller.lineEffectMode}");
                }
            });
        }
        
        Debug.Log("✓ UI elements reconnected at runtime!");
    }
    
    void Update()
    {
        // Toggle UI visibility with Enter key using new Input System
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
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
        
        float duration = UIConstants.TransitionNormal;
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;
        
        Vector3 startScale = panel.transform.localScale;
        Vector3 targetScale = show ? Vector3.one : new Vector3(0.95f, 0.95f, 1f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Use UIConstants smooth easing for better feel
            float smoothT = UIConstants.SmoothEase(t);
            
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
        scaler.referenceResolution = new Vector2(UIConstants.CanvasReferenceWidth, UIConstants.CanvasReferenceHeight);
        scaler.matchWidthOrHeight = 0.5f; // Balance between width and height matching
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Hide UI Button (Top-Right Corner - ALWAYS VISIBLE) - Modern glassmorphic style
        hideUIButton = CreateModernButton(canvas.transform, "HideUIButton", new Vector2(-UIConstants.PanelPadding, -UIConstants.PanelPadding), new Vector2(90, UIConstants.ButtonHeight), "⊗ HIDE");
        RectTransform hideButtonRect = hideUIButton.GetComponent<RectTransform>();
        hideButtonRect.anchorMin = new Vector2(1, 1); // Top-right anchor
        hideButtonRect.anchorMax = new Vector2(1, 1);
        hideButtonRect.pivot = new Vector2(1, 1);
        
        // Glassmorphic style for hide button using UIConstants
        Image hideButtonImage = hideUIButton.GetComponent<Image>();
        hideButtonImage.color = UIConstants.SectionBackground;
        
        // Add subtle glow outline using UIConstants
        Outline hideOutline = hideUIButton.gameObject.AddComponent<Outline>();
        hideOutline.effectColor = UIConstants.CyanGlow;
        hideOutline.effectDistance = UIConstants.ShadowDistance;
        
        Text hideButtonText = hideUIButton.GetComponentInChildren<Text>();
        hideButtonText.fontSize = UIConstants.FontSizeHeader;
        hideButtonText.fontStyle = FontStyle.Bold;
        hideButtonText.color = UIConstants.SoftCyanWhite;
        
        // Create Panel Background - Modern Glassmorphism with cosmic theme using UIConstants
        GameObject panel = new GameObject("ControlPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 0.5f);
        panelRect.anchoredPosition = new Vector2(UIConstants.PanelPadding, 0);
        panelRect.sizeDelta = new Vector2(340, -80); // Full height minus padding
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = UIConstants.DeepSpaceGlass;
        
        // Add Canvas Group for smooth transitions
        CanvasGroup panelGroup = panel.AddComponent<CanvasGroup>();
        panelGroup.alpha = 1f;
        
        // Add subtle outer glow using UIConstants
        Shadow panelGlow = panel.AddComponent<Shadow>();
        panelGlow.effectColor = UIConstants.BlueGlow;
        panelGlow.effectDistance = UIConstants.GlowDistance;
        panelGlow.useGraphicAlpha = true;
        
        // Add border accent using UIConstants
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = UIConstants.CyanGlow;
        panelOutline.effectDistance = UIConstants.OutlineDistance;
        
        // Add ScrollRect for scrolling
        ScrollRect panelScroll = panel.AddComponent<ScrollRect>();
        panelScroll.horizontal = false;
        panelScroll.vertical = true;
        panelScroll.scrollSensitivity = 20f;
        panelScroll.movementType = ScrollRect.MovementType.Clamped;
        panelScroll.inertia = true;
        panelScroll.decelerationRate = 0.135f;
        
        // Create Viewport for ScrollRect (fills the panel with padding)
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(panel.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.offsetMin = new Vector2(10, 10); // Padding
        viewportRect.offsetMax = new Vector2(-10, -10); // Padding
        
        // Add mask to viewport
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.white; // Fully opaque white (mask hides it with showMaskGraphic=false)
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false; // Don't render the image, but use it for masking
        
        panelScroll.viewport = viewportRect;
        
        // Create Content container (this will hold all our UI elements)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = new Vector2(0, 0);
        contentRect.sizeDelta = new Vector2(-20, 2000); // Width accounts for padding, height will be adjusted
        
        // Add VerticalLayoutGroup to automatically stack elements and collapse gaps
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = false;
        contentLayout.spacing = UIConstants.SectionSpacing;
        contentLayout.padding = new RectOffset(0, 0, (int)UIConstants.SpacingXL, (int)UIConstants.SpacingXL);
        
        // Add ContentSizeFitter to auto-adjust height based on content
        ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
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
        // Let VerticalLayoutGroup control positioning
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 35);
        
        // Add LayoutElement for title
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 35;
        titleLayout.flexibleHeight = 0;
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "✦ SPIROGRAPH CONTROLS";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = UIConstants.FontSizeTitle;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = UIConstants.BrightCyan;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        // Add subtle glow to title using UIConstants
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = UIConstants.BlueGlow;
        titleShadow.effectDistance = UIConstants.GlowDistance;
        
        yPos -= 55;
        
        // ============================================================
        // MOTION SECTION
        // ============================================================
        motionSectionToggle = CreateSectionHeader(uiParent, "MotionHeader", new Vector2(15, yPos), "⚡ MOTION & SPEED", true);
        yPos -= 45;
        
        motionSection = CreateSection(uiParent, "MotionSection", new Vector2(15, yPos));
        float motionYPos = -10;
        
        // Speed Slider
        speedSlider = CreateModernSlider(motionSection.transform, "SpeedSlider", new Vector2(0, motionYPos), 0f, 750f, 
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
        float motionHeight = Mathf.Abs(motionYPos) + 10;
        motionRect.sizeDelta = new Vector2(0, motionHeight);
        LayoutElement motionLayout = motionSection.GetComponent<LayoutElement>();
        if (motionLayout != null) motionLayout.preferredHeight = motionHeight;
        
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
        float visualsHeight = Mathf.Abs(visualsYPos) + 10;
        visualsRect.sizeDelta = new Vector2(0, visualsHeight);
        LayoutElement visualsLayout = visualsSection.GetComponent<LayoutElement>();
        if (visualsLayout != null) visualsLayout.preferredHeight = visualsHeight;
        
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
        float colorHeight = Mathf.Abs(colorYPos) + 10;
        colorRect.sizeDelta = new Vector2(0, colorHeight);
        LayoutElement colorLayout = colorSection.GetComponent<LayoutElement>();
        if (colorLayout != null) colorLayout.preferredHeight = colorHeight;
        
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
        envYPos -= 80;
        
        // Pattern Generator Label
        GameObject patternLabelObj = new GameObject("PatternGeneratorLabel");
        patternLabelObj.transform.SetParent(environmentSection.transform, false);
        RectTransform patternLabelRect = patternLabelObj.AddComponent<RectTransform>();
        patternLabelRect.anchorMin = new Vector2(0, 1);
        patternLabelRect.anchorMax = new Vector2(0, 1);
        patternLabelRect.pivot = new Vector2(0, 1);
        patternLabelRect.anchoredPosition = new Vector2(0, envYPos);
        patternLabelRect.sizeDelta = new Vector2(290, 20);
        Text patternLabel = patternLabelObj.AddComponent<Text>();
        patternLabel.text = "Pattern Generator:";
        patternLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        patternLabel.fontSize = 12;
        patternLabel.fontStyle = FontStyle.Bold;
        patternLabel.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        patternLabel.alignment = TextAnchor.MiddleLeft;
        envYPos -= 28;
        
        // Pattern Dropdown
        patternDropdown = CreatePatternDropdown(environmentSection.transform, "PatternDropdown", new Vector2(0, envYPos));
        envYPos -= 50;
        
        // Generate and Clear Buttons
        generatePatternButton = CreateModernButton(environmentSection.transform, "GeneratePatternButton", new Vector2(0, envYPos), new Vector2(140, 38), "✦ GENERATE");
        Image generateImg = generatePatternButton.GetComponent<Image>();
        generateImg.color = new Color(0.15f, 0.25f, 0.08f, 0.8f); // Green tint
        
        clearPatternsButton = CreateModernButton(environmentSection.transform, "ClearPatternsButton", new Vector2(150, envYPos), new Vector2(140, 38), "✖ CLEAR ALL");
        Image clearImg = clearPatternsButton.GetComponent<Image>();
        clearImg.color = new Color(0.25f, 0.08f, 0.08f, 0.8f); // Red tint
        envYPos -= 65;
        
        // ============================================================
        // MULTI-AGENT SYSTEM CONTROLS
        // ============================================================
        
        // Multi-Agent Mode Section Label
        GameObject multiAgentLabelObj = new GameObject("MultiAgentLabel");
        multiAgentLabelObj.transform.SetParent(environmentSection.transform, false);
        RectTransform multiAgentLabelRect = multiAgentLabelObj.AddComponent<RectTransform>();
        multiAgentLabelRect.anchorMin = new Vector2(0, 1);
        multiAgentLabelRect.anchorMax = new Vector2(0, 1);
        multiAgentLabelRect.pivot = new Vector2(0, 1);
        multiAgentLabelRect.anchoredPosition = new Vector2(0, envYPos);
        multiAgentLabelRect.sizeDelta = new Vector2(290, 20);
        Text multiAgentLabel = multiAgentLabelObj.AddComponent<Text>();
        multiAgentLabel.text = "Multi-Agent System:";
        multiAgentLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        multiAgentLabel.fontSize = 12;
        multiAgentLabel.fontStyle = FontStyle.Bold;
        multiAgentLabel.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        multiAgentLabel.alignment = TextAnchor.MiddleLeft;
        envYPos -= 28;
        
        // Multi-Agent Toggle
        multiAgentToggle = CreateModernToggle(environmentSection.transform, "MultiAgentToggle", new Vector2(0, envYPos), "Enable Multi-Agent Mode");
        envYPos -= 45;
        
        // Agent Count Slider (initially hidden)
        GameObject agentCountContainer = new GameObject("AgentCountContainer");
        agentCountContainer.transform.SetParent(environmentSection.transform, false);
        RectTransform agentCountContainerRect = agentCountContainer.AddComponent<RectTransform>();
        agentCountContainerRect.anchorMin = new Vector2(0, 1);
        agentCountContainerRect.anchorMax = new Vector2(0, 1);
        agentCountContainerRect.pivot = new Vector2(0, 1);
        agentCountContainerRect.anchoredPosition = new Vector2(0, envYPos);
        agentCountContainerRect.sizeDelta = new Vector2(290, 70);
        agentCountContainer.SetActive(false); // Hidden until multi-agent enabled
        
        agentCountSlider = CreateModernSlider(agentCountContainer.transform, "AgentCountSlider", new Vector2(0, 0), 1f, 16f, 
            4f, out agentCountText, "Agent Count", "4");
        agentCountSlider.wholeNumbers = true;
        envYPos -= 75;
        
        // Agent Color Mode Dropdown (initially hidden)
        GameObject colorModeContainer = new GameObject("ColorModeContainer");
        colorModeContainer.transform.SetParent(environmentSection.transform, false);
        RectTransform colorModeContainerRect = colorModeContainer.AddComponent<RectTransform>();
        colorModeContainerRect.anchorMin = new Vector2(0, 1);
        colorModeContainerRect.anchorMax = new Vector2(0, 1);
        colorModeContainerRect.pivot = new Vector2(0, 1);
        colorModeContainerRect.anchoredPosition = new Vector2(0, envYPos);
        colorModeContainerRect.sizeDelta = new Vector2(290, 70);
        colorModeContainer.SetActive(false); // Hidden until multi-agent enabled
        
        GameObject colorModeLabelObj = new GameObject("ColorModeLabel");
        colorModeLabelObj.transform.SetParent(colorModeContainer.transform, false);
        RectTransform colorModeLabelRect = colorModeLabelObj.AddComponent<RectTransform>();
        colorModeLabelRect.anchorMin = new Vector2(0, 1);
        colorModeLabelRect.anchorMax = new Vector2(0, 1);
        colorModeLabelRect.pivot = new Vector2(0, 1);
        colorModeLabelRect.anchoredPosition = new Vector2(0, 0);
        colorModeLabelRect.sizeDelta = new Vector2(290, 20);
        Text colorModeLabel = colorModeLabelObj.AddComponent<Text>();
        colorModeLabel.text = "Agent Color Mode:";
        colorModeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        colorModeLabel.fontSize = 11;
        colorModeLabel.fontStyle = FontStyle.Bold;
        colorModeLabel.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        colorModeLabel.alignment = TextAnchor.MiddleLeft;
        
        agentColorModeDropdown = CreateCompactDropdown(colorModeContainer.transform, "AgentColorModeDropdown", new Vector2(0, -25), 
            new string[] { "Master", "Rainbow", "Individual", "Custom" });
        envYPos -= 75;
        
        // Agent Spawn Mode Dropdown (initially hidden)
        GameObject spawnModeContainer = new GameObject("SpawnModeContainer");
        spawnModeContainer.transform.SetParent(environmentSection.transform, false);
        RectTransform spawnModeContainerRect = spawnModeContainer.AddComponent<RectTransform>();
        spawnModeContainerRect.anchorMin = new Vector2(0, 1);
        spawnModeContainerRect.anchorMax = new Vector2(0, 1);
        spawnModeContainerRect.pivot = new Vector2(0, 1);
        spawnModeContainerRect.anchoredPosition = new Vector2(0, envYPos);
        spawnModeContainerRect.sizeDelta = new Vector2(290, 70);
        spawnModeContainer.SetActive(false); // Hidden until multi-agent enabled
        
        GameObject spawnModeLabelObj = new GameObject("SpawnModeLabel");
        spawnModeLabelObj.transform.SetParent(spawnModeContainer.transform, false);
        RectTransform spawnModeLabelRect = spawnModeLabelObj.AddComponent<RectTransform>();
        spawnModeLabelRect.anchorMin = new Vector2(0, 1);
        spawnModeLabelRect.anchorMax = new Vector2(0, 1);
        spawnModeLabelRect.pivot = new Vector2(0, 1);
        spawnModeLabelRect.anchoredPosition = new Vector2(0, 0);
        spawnModeLabelRect.sizeDelta = new Vector2(290, 20);
        Text spawnModeLabel = spawnModeLabelObj.AddComponent<Text>();
        spawnModeLabel.text = "Spawn Mode:";
        spawnModeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        spawnModeLabel.fontSize = 11;
        spawnModeLabel.fontStyle = FontStyle.Bold;
        spawnModeLabel.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        spawnModeLabel.alignment = TextAnchor.MiddleLeft;
        
        agentSpawnModeDropdown = CreateCompactDropdown(spawnModeContainer.transform, "AgentSpawnModeDropdown", new Vector2(0, -25), 
            new string[] { "Simultaneous", "Sequential", "Staggered", "Competitive" });
        envYPos -= 80;
        
        // Set environment section height
        RectTransform envRect = environmentSection.GetComponent<RectTransform>();
        float envHeight = Mathf.Abs(envYPos) + 10;
        envRect.sizeDelta = new Vector2(0, envHeight);
        LayoutElement envLayout = environmentSection.GetComponent<LayoutElement>();
        if (envLayout != null) envLayout.preferredHeight = envHeight;
        
        // ============================================================
        // CAMERA SECTION
        // ============================================================
        cameraSectionToggle = CreateSectionHeader(uiParent, "CameraHeader", new Vector2(15, yPos), "📷 CAMERA CONTROLS", false);
        yPos -= 45;
        
        cameraSection = CreateSection(uiParent, "CameraSection", new Vector2(15, yPos));
        cameraSection.SetActive(false); // Collapsed by default
        float cameraYPos = -10;
        
        // Camera Mode Buttons - IMPORTANT: These are assigned to public fields
        lookAtButton = CreateModernButton(cameraSection.transform, "LookAtButton", new Vector2(0, cameraYPos), new Vector2(140, 38), "✈ Free Fly");
        smoothFollowButton = CreateModernButton(cameraSection.transform, "SmoothFollowButton", new Vector2(150, cameraYPos), new Vector2(140, 38), "◎ Follow");
        cameraYPos -= 50;
        
        autoOrbitButton = CreateModernButton(cameraSection.transform, "AutoOrbitButton", new Vector2(0, cameraYPos), new Vector2(290, 38), "🎬 AUTO ORBIT");
        Image orbitImg = autoOrbitButton.GetComponent<Image>();
        orbitImg.color = new Color(0.15f, 0.05f, 0.25f, 0.8f);
        cameraYPos -= 55;
        
        #if UNITY_EDITOR
        // Force serialization of button references
        if (lookAtButton != null) UnityEditor.EditorUtility.SetDirty(lookAtButton.gameObject);
        if (smoothFollowButton != null) UnityEditor.EditorUtility.SetDirty(smoothFollowButton.gameObject);
        if (autoOrbitButton != null) UnityEditor.EditorUtility.SetDirty(autoOrbitButton.gameObject);
        #endif
        
        // Connect camera buttons to CameraController
        CameraController cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            lookAtButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(CameraController.CameraMode.FreeFly);
                Debug.Log("Camera Mode: Free Fly");
            });
            
            smoothFollowButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                Debug.Log("Camera Mode: Smooth Follow");
            });
            
            autoOrbitButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(CameraController.CameraMode.AutoOrbit);
                Debug.Log("Camera Mode: Auto Orbit");
            });
            
            Debug.Log("✓ Camera buttons connected to CameraController!");
        }
        else
        {
            Debug.LogWarning("⚠ CameraController not found in scene! Camera buttons will not function.");
        }
        
        // Set camera section height
        RectTransform cameraRect = cameraSection.GetComponent<RectTransform>();
        float cameraHeight = Mathf.Abs(cameraYPos) + 10;
        cameraRect.sizeDelta = new Vector2(0, cameraHeight);
        LayoutElement cameraLayout = cameraSection.GetComponent<LayoutElement>();
        if (cameraLayout != null) cameraLayout.preferredHeight = cameraHeight;
        
        // Setup section toggle functionality
        SetupSectionToggle(motionSectionToggle, motionSection);
        SetupSectionToggle(visualsSectionToggle, visualsSection);
        SetupSectionToggle(colorSectionToggle, colorSection);
        SetupSectionToggle(environmentSectionToggle, environmentSection);
        SetupSectionToggle(cameraSectionToggle, cameraSection);
        
        // Setup HSV sliders to update preview AND create new trail with new color
        if (hueSlider != null && saturationSlider != null && valueSlider != null && colorPreview != null)
        {
            System.Action updateColorAndPreview = () => {
                float h = hueSlider.value;
                float s = saturationSlider.value;
                float v = valueSlider.value;
                Color newColor = Color.HSVToRGB(h, s, v);
                
                // Update preview
                Image preview = colorPreview.GetComponent<Image>();
                if (preview != null) preview.color = newColor;
                
                // Change line color on ACTIVE rotor - creates a NEW trail preserving the old one
                if (activeRoller != null)
                {
                    activeRoller.ChangeLineColor(newColor);
                }
            };
            
            hueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            saturationSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            valueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
        }
        
        // Instructions - Modern style
        GameObject instructions = new GameObject("Instructions");
        instructions.transform.SetParent(uiParent, false);
        RectTransform instructRect = instructions.AddComponent<RectTransform>();
        // Let VerticalLayoutGroup control positioning
        instructRect.anchorMin = new Vector2(0, 1);
        instructRect.anchorMax = new Vector2(1, 1);
        instructRect.pivot = new Vector2(0.5f, 1);
        instructRect.sizeDelta = new Vector2(0, 110);
        
        // Add LayoutElement for instructions
        LayoutElement instructLayout = instructions.AddComponent<LayoutElement>();
        instructLayout.preferredHeight = 110;
        instructLayout.flexibleHeight = 0;
        
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
        instructText.text = "💡 Click section headers (▼/▶) to expand/collapse\n\n⌨ WASD/ZQSD: Move • Shift: Sprint\n🖱 Right Click: Look • Scroll: Zoom\n\n🎨 Click color swatches for instant colors\n🌌 Choose skybox from Environment section";
        instructText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructText.fontSize = 10;
        instructText.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        instructText.alignment = TextAnchor.UpperLeft;
        instructText.lineSpacing = 1.15f;
        
        // ContentSizeFitter will automatically adjust content height based on LayoutElements
        
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
        
        // ============================================================
        // AGENT PANEL (Bottom-Right)
        // ============================================================
        GameObject agentPanelUIObj = new GameObject("AgentPanelUI");
        agentPanelUIObj.transform.SetParent(canvas.transform, false);
        agentPanelUI = agentPanelUIObj.AddComponent<AgentPanelUI>();
        agentPanelUI.CreatePanel(canvas);
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.Selection.activeGameObject = canvasObj;
        #endif
        
        Debug.Log("✓ Generated complete UI! Canvas selected in Hierarchy.");
        Debug.Log("✓ HIDE UI BUTTON: Top-right corner (always visible) - Press ENTER to toggle!");
        Debug.Log("✓ Agent Panel: Bottom-right (shown when multi-agent mode enabled)");
        Debug.Log("✓ You can now customize colors, sizes, and positions.");
        Debug.Log("✓ Don't forget to assign 4 materials to SpirographRoller for the material buttons!");
        Debug.Log("✓ UI automatically connects to SpirographRoller, RotateParent, and CameraController.");
        
        // Log button assignments for verification
        Debug.Log($"✓ Camera Buttons Created: LookAt={lookAtButton != null}, SmoothFollow={smoothFollowButton != null}, AutoOrbit={autoOrbitButton != null}");
        if (lookAtButton != null) Debug.Log($"  → Look At Button: {lookAtButton.name}");
        if (smoothFollowButton != null) Debug.Log($"  → Smooth Follow Button: {smoothFollowButton.name}");
        if (autoOrbitButton != null) Debug.Log($"  → Auto Orbit Button: {autoOrbitButton.name}");
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
        sliderRect.sizeDelta = new Vector2(290, 12); // Half height (was 24)
        
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
        label.fontSize = UIConstants.FontSizeBody;
        label.fontStyle = FontStyle.Bold;
        label.color = UIConstants.BrightCyan;
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
        valueLabel.fontSize = UIConstants.FontSizeSmall;
        valueLabel.color = UIConstants.MutedText;
        valueLabel.alignment = TextAnchor.MiddleRight;
        
        // Background (glassmorphic)
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = UIConstants.ControlBackground;
        
        // Add subtle outline using UIConstants
        Outline bgOutline = bg.AddComponent<Outline>();
        bgOutline.effectColor = UIConstants.CyanGlow;
        bgOutline.effectDistance = UIConstants.ShadowDistance;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2, 2); // Smaller padding for thinner slider
        fillAreaRect.offsetMax = new Vector2(-10, -2);
        
        // Fill (gradient cyan-blue)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = UIConstants.BlueGlow;
        
        // Add glow to fill using UIConstants
        Shadow fillGlow = fill.AddComponent<Shadow>();
        fillGlow.effectColor = UIConstants.CyanGlow;
        fillGlow.effectDistance = UIConstants.GlowDistance;
        
        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(6, 0); // Adjusted for thinner slider
        handleAreaRect.offsetMax = new Vector2(-6, 0);
        
        // Handle (modern circular design)
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(10, 10); // Smaller handle (was 16x16)
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = UIConstants.BrightWhite;
        
        // Add handle glow using UIConstants
        Shadow handleGlow = handle.AddComponent<Shadow>();
        handleGlow.effectColor = UIConstants.CyanGlow;
        handleGlow.effectDistance = UIConstants.GlowDistance;
        
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
        // Let VerticalLayoutGroup control positioning
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 35);
        
        // Add Image FIRST (required for Button)
        Image headerImage = headerObj.AddComponent<Image>();
        headerImage.color = new Color(0.12f, 0.15f, 0.25f, 0.9f);
        headerImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        headerImage.type = Image.Type.Sliced;
        
        // Add Button and set target graphic
        Button button = headerObj.AddComponent<Button>();
        button.targetGraphic = headerImage;
        button.transition = Selectable.Transition.ColorTint;
        
        // Add LayoutElement so it works with VerticalLayoutGroup
        LayoutElement headerLayout = headerObj.AddComponent<LayoutElement>();
        headerLayout.preferredHeight = 35;
        headerLayout.flexibleHeight = 0;
        
        Outline headerOutline = headerObj.AddComponent<Outline>();
        headerOutline.effectColor = new Color(0.4f, 0.6f, 1f, 0.4f);
        headerOutline.effectDistance = new Vector2(1, -1);
        
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
        // Let VerticalLayoutGroup control positioning
        sectionRect.anchorMin = new Vector2(0, 1);
        sectionRect.anchorMax = new Vector2(1, 1);
        sectionRect.pivot = new Vector2(0.5f, 1);
        sectionRect.sizeDelta = new Vector2(0, 100); // Temp, will be adjusted
        
        Image sectionBg = section.AddComponent<Image>();
        sectionBg.color = new Color(0.03f, 0.03f, 0.1f, 0.5f);
        
        // Add LayoutElement so it works with VerticalLayoutGroup
        LayoutElement sectionLayout = section.AddComponent<LayoutElement>();
        sectionLayout.preferredHeight = 100; // Will be updated per section
        sectionLayout.flexibleHeight = 0;
        
        return section;
    }
    
    void SetupSectionToggle(Button toggleButton, GameObject section)
    {
        if (toggleButton == null || section == null) return;
        
        // Get references (height will be read dynamically later)
        RectTransform sectionRect = section.GetComponent<RectTransform>();
        LayoutElement sectionLayout = section.GetComponent<LayoutElement>();
        
        // Remove old listeners to prevent duplicates
        toggleButton.onClick.RemoveAllListeners();
        
        toggleButton.onClick.AddListener(() => {
            bool willBeActive = !section.activeSelf;
            
            // Get the actual height from the LayoutElement (this is the correct height)
            float fullHeight = sectionLayout != null ? sectionLayout.preferredHeight : sectionRect.sizeDelta.y;
            
            // If collapsing, we just set to 0
            // If expanding, we restore the full height
            if (sectionLayout != null)
            {
                // When expanding, make sure we have the correct height stored
                if (willBeActive && fullHeight <= 0)
                {
                    // Height wasn't stored properly, get it from sizeDelta
                    fullHeight = sectionRect.sizeDelta.y;
                    if (fullHeight <= 0) fullHeight = 100; // Fallback
                }
                
                sectionLayout.preferredHeight = willBeActive ? fullHeight : 0;
                // Mark layout as dirty to force recalculation
                LayoutRebuilder.MarkLayoutForRebuild(sectionLayout.GetComponent<RectTransform>());
            }
            
            // Then change active state
            section.SetActive(willBeActive);
            
            // Update arrow icon
            Text buttonText = toggleButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                string[] parts = buttonText.text.Split(' ');
                if (parts.Length > 1)
                {
                    string arrow = willBeActive ? "▼" : "▶";
                    buttonText.text = arrow + " " + string.Join(" ", parts, 1, parts.Length - 1);
                }
            }
            
            // Force complete layout rebuild (multiple passes for reliability)
            RectTransform contentRect = section.transform.parent.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            Canvas.ForceUpdateCanvases();
            
            // Second pass to ensure everything updates
            UnityEngine.Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            
            Debug.Log($"Section {section.name} toggled: {(willBeActive ? "EXPANDED" : "COLLAPSED")} - Height: {fullHeight}");
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
        
        // Connect to ACTIVE SpirographRoller
        button.onClick.AddListener(() => {
            Debug.Log($"🎨 Color preset button clicked! Color: {color}");
            
            if (activeRoller == null)
            {
                Debug.LogError("❌ Active SpirographRoller not found! Cannot change color.");
                return;
            }
            
            Debug.Log($"✓ Found active SpirographRoller, changing color...");
            
            try
            {
                // Update HSV sliders first
                float h, s, v;
                Color.RGBToHSV(color, out h, out s, out v);
                
                if (hueSlider != null) 
                {
                    hueSlider.SetValueWithoutNotify(h);
                    Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                    if (text != null) text.text = h.ToString("F2");
                }
                if (saturationSlider != null) 
                {
                    saturationSlider.SetValueWithoutNotify(s);
                    Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                    if (text != null) text.text = s.ToString("F2");
                }
                if (valueSlider != null) 
                {
                    valueSlider.SetValueWithoutNotify(v);
                    Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                    if (text != null) text.text = v.ToString("F2");
                }
                
                // Change line color on ACTIVE rotor - this creates a NEW trail with the new color
                // while preserving the old trail in its current color
                activeRoller.ChangeLineColor(color);
                
                // Update preview
                if (colorPreview != null)
                {
                    Image preview = colorPreview.GetComponent<Image>();
                    if (preview != null) preview.color = color;
                }
                
                Debug.Log($"✓ Color changed successfully to {color}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error changing color: {e.Message}\n{e.StackTrace}");
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
        
        // Template (the dropdown list that appears) - POSITIONED TO THE RIGHT
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(1, 0.5f); // Anchor to right middle of dropdown
        templateRect.anchorMax = new Vector2(1, 0.5f);
        templateRect.pivot = new Vector2(0, 0.5f); // Pivot on left middle so it extends right
        templateRect.anchoredPosition = new Vector2(10, 0); // 10 pixels to the right
        templateRect.sizeDelta = new Vector2(250, 200); // Fixed width, taller for better visibility
        
        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
        templateBg.raycastTarget = true;
        
        // Add Canvas component to template to break out of parent's clipping
        Canvas templateCanvas = templateObj.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 1000; // Render on top of everything
        
        // Add CanvasGroup to ensure it renders properly
        CanvasGroup templateGroup = templateObj.AddComponent<CanvasGroup>();
        templateGroup.blocksRaycasts = true;
        
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
        SkyboxManager skyboxManager = FindFirstObjectByType<SkyboxManager>();
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
        
        // Glassmorphic background using UIConstants
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = UIConstants.ButtonBackground;
        
        // Add subtle outline using UIConstants
        Outline buttonOutline = buttonObj.AddComponent<Outline>();
        buttonOutline.effectColor = UIConstants.CyanGlow;
        buttonOutline.effectDistance = UIConstants.ShadowDistance;
        
        // Add hover glow effect using UIConstants
        Shadow buttonGlow = buttonObj.AddComponent<Shadow>();
        buttonGlow.effectColor = UIConstants.BlueGlow;
        buttonGlow.effectDistance = UIConstants.GlowDistance;
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Setup hover colors with consistent styling
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(UIConstants.BrightCyan.r, UIConstants.BrightCyan.g, UIConstants.BrightCyan.b, 1f);
        colors.pressedColor = new Color(UIConstants.CyanGlow.r, UIConstants.CyanGlow.g, UIConstants.CyanGlow.b, 1f);
        colors.selectedColor = new Color(UIConstants.BrightCyan.r, UIConstants.BrightCyan.g, UIConstants.BrightCyan.b, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.colorMultiplier = 1.2f;
        colors.fadeDuration = UIConstants.TransitionFast;
        button.colors = colors;
        
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
        text.fontSize = UIConstants.FontSizeBody;
        text.fontStyle = FontStyle.Bold;
        text.color = UIConstants.SoftCyanWhite;
        text.alignment = TextAnchor.MiddleCenter;
        
        // Add text shadow for depth using UIConstants
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.5f);
        textShadow.effectDistance = UIConstants.ShadowDistance;
        
        return button;
    }
    
    /// <summary>
    /// Create dropdown populated with all GeometricPatternGenerator shape types
    /// </summary>
    Dropdown CreatePatternDropdown(Transform parent, string name, Vector2 position)
    {
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0, 1);
        dropdownRect.anchorMax = new Vector2(0, 1);
        dropdownRect.pivot = new Vector2(0, 1);
        dropdownRect.anchoredPosition = position;
        dropdownRect.sizeDelta = new Vector2(290, 38);
        
        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.08f, 0.12f, 0.22f, 0.7f);
        dropdownBg.raycastTarget = true;
        
        Outline dropdownOutline = dropdownObj.AddComponent<Outline>();
        dropdownOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        dropdownOutline.effectDistance = new Vector2(1, -1);
        
        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownBg;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 2);
        labelRect.offsetMax = new Vector2(-30, -2);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "StarBurst";
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
        
        // Template - POSITIONED TO THE RIGHT
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(1, 0.5f); // Anchor to right middle of dropdown
        templateRect.anchorMax = new Vector2(1, 0.5f);
        templateRect.pivot = new Vector2(0, 0.5f); // Pivot on left middle so it extends right
        templateRect.anchoredPosition = new Vector2(10, 0); // 10 pixels to the right
        templateRect.sizeDelta = new Vector2(250, 300); // Fixed width, extra tall for many options
        
        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
        templateBg.raycastTarget = true;
        
        // Add Canvas component to break out of parent's clipping
        Canvas templateCanvas = templateObj.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 1000; // Render on top of everything
        
        // Add CanvasGroup to ensure it renders properly
        CanvasGroup templateGroup = templateObj.AddComponent<CanvasGroup>();
        templateGroup.blocksRaycasts = true;
        
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
        contentRect.sizeDelta = new Vector2(0, 200);
        
        scrollRect.content = contentRect;
        
        // Item
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
        
        // Populate with ALL shape types from GeometricPatternGenerator
        dropdown.ClearOptions();
        List<string> shapeNames = new List<string>();
        foreach (GeometricPatternGenerator.ShapeType shape in System.Enum.GetValues(typeof(GeometricPatternGenerator.ShapeType)))
        {
            shapeNames.Add(shape.ToString());
        }
        dropdown.AddOptions(shapeNames);
        
        return dropdown;
    }
    
    /// <summary>
    /// Connect pattern generator buttons to PatternSpawner
    /// </summary>
    void ConnectPatternGenerator()
    {
        // Find or create PatternSpawner
        PatternSpawner spawner = FindFirstObjectByType<PatternSpawner>();
        if (spawner == null)
        {
            GameObject spawnerObj = new GameObject("PatternSpawner");
            spawner = spawnerObj.AddComponent<PatternSpawner>();
            Debug.Log("✓ Created PatternSpawner in scene");
        }
        
        // Connect Generate Button
        if (generatePatternButton != null && patternDropdown != null)
        {
            generatePatternButton.onClick.RemoveAllListeners();
            generatePatternButton.onClick.AddListener(() => {
                // Get selected shape from dropdown
                string selectedShape = patternDropdown.options[patternDropdown.value].text;
                GeometricPatternGenerator.ShapeType shapeType = (GeometricPatternGenerator.ShapeType)System.Enum.Parse(
                    typeof(GeometricPatternGenerator.ShapeType), selectedShape);
                
                // Spawn pattern
                GameObject newPattern = spawner.SpawnPattern(shapeType);
                Debug.Log($"✓ Generated {selectedShape} pattern at runtime!");
            });
        }
        
        // Connect Clear Button
        if (clearPatternsButton != null)
        {
            clearPatternsButton.onClick.RemoveAllListeners();
            clearPatternsButton.onClick.AddListener(() => {
                spawner.ClearAllPatterns();
                Debug.Log("✓ Cleared all generated patterns");
            });
        }
        
        Debug.Log("✓ Pattern generator UI connected!");
    }
    
    /// <summary>
    /// Create a modern toggle (checkbox) control
    /// </summary>
    Toggle CreateModernToggle(Transform parent, string name, Vector2 position, string labelText)
    {
        // Container
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0, 1);
        toggleRect.anchorMax = new Vector2(0, 1);
        toggleRect.pivot = new Vector2(0, 1);
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(290, 35);
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(30, 0);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.15f, 0.25f, 0.6f);
        
        Outline bgOutline = bgObj.AddComponent<Outline>();
        bgOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        bgOutline.effectDistance = new Vector2(1, -1);
        
        // Checkmark
        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(bgObj.transform, false);
        RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.sizeDelta = Vector2.zero;
        
        Text checkmark = checkmarkObj.AddComponent<Text>();
        checkmark.text = "✓";
        checkmark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        checkmark.fontSize = 22;
        checkmark.fontStyle = FontStyle.Bold;
        checkmark.color = new Color(0.3f, 0.8f, 1f, 1f);
        checkmark.alignment = TextAnchor.MiddleCenter;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(38, 0);
        labelRect.sizeDelta = new Vector2(-38, 0);
        
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 12;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        label.alignment = TextAnchor.MiddleLeft;
        
        // Toggle component
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkmark;
        toggle.isOn = false;
        
        ColorBlock colors = toggle.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 1f, 1f);
        colors.selectedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
        toggle.colors = colors;
        
        return toggle;
    }
    
    /// <summary>
    /// Create a compact dropdown (smaller than the default modern dropdown)
    /// </summary>
    Dropdown CreateCompactDropdown(Transform parent, string name, Vector2 position, string[] options)
    {
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0, 1);
        dropdownRect.anchorMax = new Vector2(0, 1);
        dropdownRect.pivot = new Vector2(0, 1);
        dropdownRect.anchoredPosition = position;
        dropdownRect.sizeDelta = new Vector2(290, 32);
        
        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.08f, 0.12f, 0.22f, 0.7f);
        dropdownBg.raycastTarget = true;
        
        Outline dropdownOutline = dropdownObj.AddComponent<Outline>();
        dropdownOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        dropdownOutline.effectDistance = new Vector2(1, -1);
        
        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownBg;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8, 2);
        labelRect.offsetMax = new Vector2(-25, -2);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = options[0];
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 11;
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
        arrowRect.offsetMin = new Vector2(-20, 0);
        arrowRect.offsetMax = new Vector2(-4, 0);
        Text arrowText = arrowObj.AddComponent<Text>();
        arrowText.text = "▼";
        arrowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowText.fontSize = 10;
        arrowText.color = new Color(0.6f, 0.8f, 1f, 0.8f);
        arrowText.alignment = TextAnchor.MiddleCenter;
        
        // Template (same as modern dropdown but positioned to the right)
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(1, 0.5f);
        templateRect.anchorMax = new Vector2(1, 0.5f);
        templateRect.pivot = new Vector2(0, 0.5f);
        templateRect.anchoredPosition = new Vector2(10, 0);
        templateRect.sizeDelta = new Vector2(200, 150);
        
        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
        templateBg.raycastTarget = true;
        
        Canvas templateCanvas = templateObj.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 1000;
        
        CanvasGroup templateGroup = templateObj.AddComponent<CanvasGroup>();
        templateGroup.blocksRaycasts = true;
        
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
        contentRect.sizeDelta = new Vector2(0, 100);
        
        scrollRect.content = contentRect;
        
        // Item
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(0, 25);
        
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
        itemLabelRect.offsetMin = new Vector2(8, 1);
        itemLabelRect.offsetMax = new Vector2(-8, -1);
        Text itemLabelText = itemLabelObj.AddComponent<Text>();
        itemLabelText.text = "Option";
        itemLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemLabelText.fontSize = 11;
        itemLabelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        itemLabelText.alignment = TextAnchor.MiddleLeft;
        dropdown.itemText = itemLabelText;
        
        dropdown.template = templateRect;
        templateObj.SetActive(false);
        
        // Populate options
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        
        return dropdown;
    }
    
    /// <summary>
    /// Connect multi-agent UI to MultiAgentManager
    /// </summary>
    void ConnectMultiAgentSystem()
    {
        // Find or create MultiAgentManager
        multiAgentManager = FindFirstObjectByType<MultiAgentManager>();
        if (multiAgentManager == null)
        {
            GameObject managerObj = new GameObject("MultiAgentManager");
            multiAgentManager = managerObj.AddComponent<MultiAgentManager>();
            Debug.Log("✓ Created MultiAgentManager");
        }
        
        // Find or create SharedPathState
        sharedPathState = FindFirstObjectByType<SharedPathState>();
        if (sharedPathState == null)
        {
            GameObject stateObj = new GameObject("SharedPathState");
            sharedPathState = stateObj.AddComponent<SharedPathState>();
            Debug.Log("✓ Created SharedPathState");
        }
        
        // Link manager to shared state
        multiAgentManager.sharedState = sharedPathState;
        
        // Find or create AgentPanelUI
        if (agentPanelUI == null)
        {
            agentPanelUI = FindFirstObjectByType<AgentPanelUI>();
        }
        
        // Link agent panel to manager
        if (agentPanelUI != null)
        {
            agentPanelUI.agentManager = multiAgentManager;
        }
        
        // Connect Multi-Agent Toggle
        if (multiAgentToggle != null)
        {
            multiAgentToggle.onValueChanged.RemoveAllListeners();
            multiAgentToggle.onValueChanged.AddListener((isOn) => {
                OnMultiAgentModeToggled(isOn);
            });
        }
        
        // Connect Agent Count Slider
        if (agentCountSlider != null)
        {
            agentCountSlider.onValueChanged.RemoveAllListeners();
            agentCountSlider.onValueChanged.AddListener((value) => {
                int count = (int)value;
                if (agentCountText != null)
                {
                    agentCountText.text = $"Agent Count: {count}";
                }
                if (multiAgentManager != null && multiAgentManager.isMultiAgentMode)
                {
                    multiAgentManager.SetAgentCount(count);
                }
            });
        }
        
        // Connect Color Mode Dropdown
        if (agentColorModeDropdown != null)
        {
            agentColorModeDropdown.onValueChanged.RemoveAllListeners();
            agentColorModeDropdown.onValueChanged.AddListener((index) => {
                if (multiAgentManager != null && multiAgentManager.isMultiAgentMode)
                {
                    multiAgentManager.SetColorMode((MultiAgentManager.AgentColorMode)index);
                    Debug.Log($"Agent color mode: {(MultiAgentManager.AgentColorMode)index}");
                }
            });
        }
        
        // Connect Spawn Mode Dropdown
        if (agentSpawnModeDropdown != null)
        {
            agentSpawnModeDropdown.onValueChanged.RemoveAllListeners();
            agentSpawnModeDropdown.onValueChanged.AddListener((index) => {
                if (multiAgentManager != null && multiAgentManager.isMultiAgentMode)
                {
                    multiAgentManager.SetSpawnMode((MultiAgentManager.AgentSpawnMode)index);
                    Debug.Log($"Agent spawn mode: {(MultiAgentManager.AgentSpawnMode)index}");
                }
            });
        }
        
        // When multi-agent mode is enabled, connect sliders to SharedPathState
        // This will be handled in OnMultiAgentModeToggled
        
        Debug.Log("✓ Multi-agent system UI connected!");
    }
    
    /// <summary>
    /// Handle multi-agent mode toggle
    /// </summary>
    void OnMultiAgentModeToggled(bool isEnabled)
    {
        Debug.Log($"Multi-Agent Mode: {(isEnabled ? "ENABLED" : "DISABLED")}");
        
        // Show/hide multi-agent controls
        GameObject agentCountContainer = GameObject.Find("AgentCountContainer");
        GameObject colorModeContainer = GameObject.Find("ColorModeContainer");
        GameObject spawnModeContainer = GameObject.Find("SpawnModeContainer");
        
        if (agentCountContainer != null) agentCountContainer.SetActive(isEnabled);
        if (colorModeContainer != null) colorModeContainer.SetActive(isEnabled);
        if (spawnModeContainer != null) spawnModeContainer.SetActive(isEnabled);
        
        if (isEnabled)
        {
            // Enable multi-agent mode
            if (multiAgentManager != null)
            {
                // Setup shared path state with current pattern's path
                PatternSpawner spawner = FindFirstObjectByType<PatternSpawner>();
                if (spawner != null && spawner.activeRoller != null && spawner.activeRoller.pathPoints != null)
                {
                    sharedPathState.pathPoints = spawner.activeRoller.pathPoints;
                    
                    // Copy current settings from active rotor to shared state
                    sharedPathState.masterSpeed = spawner.activeRoller.speed;
                    sharedPathState.masterRotationSpeed = spawner.activeRoller.rotationSpeed;
                    sharedPathState.masterPenDistance = spawner.activeRoller.penDistance;
                    sharedPathState.masterCycles = spawner.activeRoller.cycles;
                    sharedPathState.masterLineWidth = spawner.activeRoller.lineWidth;
                    sharedPathState.masterLineBrightness = spawner.activeRoller.lineBrightness;
                    sharedPathState.masterLineColor = spawner.activeRoller.currentLineColor;
                    
                    Debug.Log("✓ Copied active rotor settings to SharedPathState");
                }
                
                // Set agent count from slider
                if (agentCountSlider != null)
                {
                    multiAgentManager.agentCount = (int)agentCountSlider.value;
                }
                
                // Set color mode from dropdown
                if (agentColorModeDropdown != null)
                {
                    multiAgentManager.colorMode = (MultiAgentManager.AgentColorMode)agentColorModeDropdown.value;
                }
                
                // Set spawn mode from dropdown
                if (agentSpawnModeDropdown != null)
                {
                    multiAgentManager.spawnMode = (MultiAgentManager.AgentSpawnMode)agentSpawnModeDropdown.value;
                }
                
                multiAgentManager.EnableMultiAgentMode();
                
                // Subscribe to spawn completion to populate roster
                multiAgentManager.OnAgentsSpawned += () => {
                    if (agentPanelUI != null)
                    {
                        agentPanelUI.PopulateAgentList();
                        Debug.Log("✓ Agent roster populated after spawn");
                    }
                };
            }
            
            // Show agent panel
            if (agentPanelUI != null)
            {
                agentPanelUI.ShowPanel();
            }
            
            // Redirect sliders to control SharedPathState instead of activeRoller
            ConnectSlidersToSharedState();
        }
        else
        {
            // Exit per-agent control first if active
            if (perAgentControlMode)
            {
                ExitPerAgentControl();
            }
            
            // Disable multi-agent mode
            if (multiAgentManager != null)
            {
                multiAgentManager.DisableMultiAgentMode();
            }
            
            // Hide agent panel
            if (agentPanelUI != null)
            {
                agentPanelUI.HidePanel();
            }
            
            // Redirect sliders back to activeRoller
            ConnectSlidersToActiveRotor();
        }
    }
    
    /// <summary>
    /// Connect UI sliders to SharedPathState (for multi-agent mode)
    /// </summary>
    void ConnectSlidersToSharedState()
    {
        if (sharedPathState == null) return;
        
        // Speed Slider
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.value = sharedPathState.masterSpeed;
            speedSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterSpeed = value;
                if (speedText != null)
                {
                    speedText.text = "Travel Speed: " + value.ToString("F1") + " (ALL)";
                }
            });
            // Update text to show it's controlling all agents
            if (speedText != null)
            {
                speedText.text = "Travel Speed: " + sharedPathState.masterSpeed.ToString("F1") + " (ALL)";
            }
        }
        
        // Rotation Speed Slider
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.onValueChanged.RemoveAllListeners();
            rotationSpeedSlider.value = sharedPathState.masterRotationSpeed;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterRotationSpeed = value;
            });
        }
        
        // Pen Distance Slider
        if (penDistanceSlider != null)
        {
            penDistanceSlider.onValueChanged.RemoveAllListeners();
            penDistanceSlider.value = sharedPathState.masterPenDistance;
            penDistanceSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterPenDistance = value;
            });
        }
        
        // Cycles Slider
        if (cyclesSlider != null)
        {
            cyclesSlider.onValueChanged.RemoveAllListeners();
            cyclesSlider.value = sharedPathState.masterCycles;
            cyclesSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterCycles = (int)value;
            });
        }
        
        // Line Width Slider
        if (lineWidthSlider != null)
        {
            lineWidthSlider.onValueChanged.RemoveAllListeners();
            lineWidthSlider.value = sharedPathState.masterLineWidth;
            lineWidthSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterLineWidth = value;
                // Update all agents
                if (multiAgentManager != null)
                {
                    foreach (PathAgent agent in multiAgentManager.agents)
                    {
                        if (agent != null) agent.UpdateLineWidth(value);
                    }
                }
            });
        }
        
        // Line Brightness Slider
        if (lineBrightnessSlider != null)
        {
            lineBrightnessSlider.onValueChanged.RemoveAllListeners();
            lineBrightnessSlider.value = sharedPathState.masterLineBrightness;
            lineBrightnessSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterLineBrightness = value;
            });
        }
        
        Debug.Log("✓ Sliders now controlling SharedPathState (Multi-Agent Mode)");
    }
    
    // =============================================================================
    // MODERN 2025/2026 UI SYSTEM - CONTEXT BANNER & TRANSITIONS
    // =============================================================================
    
    /// <summary>
    /// Create the modern context banner at the top of the screen
    /// This prominently shows WHO is being controlled: "MASTER ROTOR" or "AGENT #3"
    /// </summary>
    void CreateContextBanner()
    {
        // Find the canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found for context banner!");
            return;
        }
        
        // Create banner container
        contextBanner = new GameObject("ContextBanner");
        contextBanner.transform.SetParent(canvas.transform, false);
        
        RectTransform bannerRect = contextBanner.AddComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.5f, 1f); // Top center
        bannerRect.anchorMax = new Vector2(0.5f, 1f);
        bannerRect.pivot = new Vector2(0.5f, 1f);
        bannerRect.anchoredPosition = new Vector2(0, -10); // 10px from top
        bannerRect.sizeDelta = new Vector2(500, 50); // Wide banner
        
        // Add CanvasGroup for smooth fade transitions
        contextBannerGroup = contextBanner.AddComponent<CanvasGroup>();
        contextBannerGroup.alpha = 0f; // Start invisible
        
        // Background (glassmorphic)
        contextBannerBackground = contextBanner.AddComponent<Image>();
        contextBannerBackground.color = new Color(0.05f, 0.1f, 0.2f, 0.85f); // Dark semi-transparent
        contextBannerBackground.raycastTarget = true; // Make clickable
        
        // Add Button component for browsing agents
        Button bannerButton = contextBanner.AddComponent<Button>();
        bannerButton.targetGraphic = contextBannerBackground;
        bannerButton.onClick.AddListener(OnContextBannerClick);
        
        // Button colors
        ColorBlock colors = bannerButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        bannerButton.colors = colors;
        
        // Add outline for depth
        Outline bannerOutline = contextBanner.AddComponent<Outline>();
        bannerOutline.effectColor = new Color(0.3f, 0.6f, 1f, 0.6f);
        bannerOutline.effectDistance = new Vector2(2, -2);
        
        // Add shadow for elevation
        Shadow bannerShadow = contextBanner.AddComponent<Shadow>();
        bannerShadow.effectColor = new Color(0, 0, 0, 0.5f);
        bannerShadow.effectDistance = new Vector2(0, -3);
        
        // Color accent bar on the left
        GameObject accentObj = new GameObject("ColorAccent");
        accentObj.transform.SetParent(contextBanner.transform, false);
        RectTransform accentRect = accentObj.AddComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0, 0);
        accentRect.anchorMax = new Vector2(0, 1);
        accentRect.pivot = new Vector2(0, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(6, 0); // 6px wide accent bar
        
        contextBannerAccent = accentObj.AddComponent<Image>();
        contextBannerAccent.color = new Color(0.4f, 0.7f, 1f, 1f); // Default cyan
        
        // Banner text
        GameObject textObj = new GameObject("BannerText");
        textObj.transform.SetParent(contextBanner.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 0); // Padding from accent bar
        textRect.offsetMax = new Vector2(-20, 0);
        
        contextBannerText = textObj.AddComponent<Text>();
        contextBannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        contextBannerText.fontSize = 18;
        contextBannerText.fontStyle = FontStyle.Bold;
        contextBannerText.color = new Color(0.9f, 0.95f, 1f, 1f);
        contextBannerText.alignment = TextAnchor.MiddleCenter;
        contextBannerText.text = "CONTROLLING: MASTER ROTOR";
        
        // Add pulsing effect component
        contextBanner.AddComponent<ContextBannerPulse>();
        
        Debug.Log("✓ Modern context banner created!");
    }
    
    /// <summary>
    /// Handle clicking the context banner to browse agents
    /// Cycles: Master → Agent #0 → Agent #1 → ... → Master
    /// </summary>
    void OnContextBannerClick()
    {
        if (multiAgentManager == null || multiAgentManager.agents.Count == 0)
        {
            // No agents - stay on master
            Debug.Log("No agents to browse");
            return;
        }
        
        if (!perAgentControlMode || selectedAgent == null)
        {
            // Currently on master → switch to first agent
            PathAgent firstAgent = multiAgentManager.agents[0];
            multiAgentManager.SelectAgent(0);
            Debug.Log("★ Context banner click: Master → Agent #0");
        }
        else
        {
            // Currently on an agent → go to next agent or back to master
            int currentIndex = selectedAgent.agentIndex;
            int nextIndex = currentIndex + 1;
            
            if (nextIndex >= multiAgentManager.agents.Count)
            {
                // Wrap back to master
                ExitPerAgentControl();
                Debug.Log("★ Context banner click: Agent #" + currentIndex + " → Master");
            }
            else
            {
                // Go to next agent
                multiAgentManager.SelectAgent(nextIndex);
                Debug.Log($"★ Context banner click: Agent #{currentIndex} → Agent #{nextIndex}");
            }
        }
    }
    
    /// <summary>
    /// Update context banner to reflect current control mode
    /// </summary>
    void UpdateContextBanner()
    {
        if (contextBanner == null)
        {
            CreateContextBanner();
        }
        
        if (contextBannerText == null || contextBannerAccent == null) return;
        
        if (perAgentControlMode && selectedAgent != null)
        {
            // Controlling specific agent
            contextBannerText.text = $"CONTROLLING: AGENT #{selectedAgent.agentIndex}";
            contextBannerAccent.color = selectedAgent.agentColor;
            
            // Make banner visible if hidden
            if (contextBannerGroup != null && contextBannerGroup.alpha < 0.5f)
            {
                StartCoroutine(FadeContextBanner(1f, 0.3f));
            }
        }
        else
        {
            // Controlling master rotor
            contextBannerText.text = "CONTROLLING: MASTER ROTOR";
            contextBannerAccent.color = new Color(0.4f, 0.7f, 1f, 1f); // Default cyan
            
            // Make banner visible if hidden
            if (contextBannerGroup != null && contextBannerGroup.alpha < 0.5f)
            {
                StartCoroutine(FadeContextBanner(1f, 0.3f));
            }
        }
    }
    
    /// <summary>
    /// Smooth transition when switching to agent control
    /// </summary>
    System.Collections.IEnumerator TransitionToAgentContext(PathAgent agent)
    {
        if (isTransitioningContext) yield break;
        isTransitioningContext = true;
        
        // Fade out banner using UIConstants timing
        if (contextBannerGroup != null)
        {
            yield return StartCoroutine(FadeContextBanner(0f, UIConstants.TransitionFast));
        }
        
        // Wait a moment
        yield return new WaitForSeconds(UIConstants.TransitionVeryFast / 2f);
        
        // Update content
        if (contextBannerText != null)
        {
            contextBannerText.text = $"CONTROLLING: AGENT #{agent.agentIndex}";
        }
        if (contextBannerAccent != null)
        {
            contextBannerAccent.color = agent.agentColor;
        }
        
        // Fade in banner with new content using UIConstants timing
        if (contextBannerGroup != null)
        {
            yield return StartCoroutine(FadeContextBanner(1f, UIConstants.TransitionNormal));
        }
        
        isTransitioningContext = false;
    }
    
    /// <summary>
    /// Smooth transition when returning to master control
    /// </summary>
    System.Collections.IEnumerator TransitionToMasterContext()
    {
        if (isTransitioningContext) yield break;
        isTransitioningContext = true;
        
        // Fade out banner using UIConstants timing
        if (contextBannerGroup != null)
        {
            yield return StartCoroutine(FadeContextBanner(0f, UIConstants.TransitionFast));
        }
        
        // Wait a moment
        yield return new WaitForSeconds(UIConstants.TransitionVeryFast / 2f);
        
        // Update content
        if (contextBannerText != null)
        {
            contextBannerText.text = "CONTROLLING: MASTER ROTOR";
        }
        if (contextBannerAccent != null)
        {
            contextBannerAccent.color = UIConstants.CyanGlow;
        }
        
        // Fade in banner with new content using UIConstants timing
        if (contextBannerGroup != null)
        {
            yield return StartCoroutine(FadeContextBanner(1f, UIConstants.TransitionNormal));
        }
        
        isTransitioningContext = false;
    }
    
    /// <summary>
    /// Fade context banner to target alpha
    /// </summary>
    System.Collections.IEnumerator FadeContextBanner(float targetAlpha, float duration)
    {
        if (contextBannerGroup == null) yield break;
        
        float startAlpha = contextBannerGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Use UIConstants smooth easing for better feel
            float smoothT = UIConstants.SmoothEase(t);
            contextBannerGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothT);
            yield return null;
        }
        
        contextBannerGroup.alpha = targetAlpha;
    }
}

/// <summary>
/// Pulsing effect for the context banner to draw attention
/// </summary>
public class ContextBannerPulse : MonoBehaviour
{
    private Outline outline;
    private float time = 0f;
    private Color baseColor = new Color(0.3f, 0.6f, 1f, 0.6f);
    
    void Start()
    {
        outline = GetComponent<Outline>();
    }
    
    void Update()
    {
        if (outline == null) return;
        
        time += Time.deltaTime * 1.5f;
        float pulse = (Mathf.Sin(time) + 1f) * 0.5f; // 0 to 1
        float alpha = Mathf.Lerp(0.4f, 0.8f, pulse);
        
        Color pulseColor = baseColor;
        pulseColor.a = alpha;
        outline.effectColor = pulseColor;
    }
}

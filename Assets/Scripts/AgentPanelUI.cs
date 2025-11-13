using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Bottom-right panel UI for managing and monitoring multiple agents.
/// Shows agent roster, stats, and provides controls for individual agents.
/// </summary>
public class AgentPanelUI : MonoBehaviour
{
    /// <summary>
    /// Debug method to force update panel (can be called from Inspector)
    /// </summary>
    [ContextMenu("Force Update Agent Panel")]
    public void ForceUpdatePanel()
    {
        Debug.Log("=== FORCE UPDATE AGENT PANEL ===");
        Debug.Log($"agentManager: {(agentManager != null ? "EXISTS" : "NULL")}");
        Debug.Log($"agentPanel: {(agentPanel != null ? agentPanel.name : "NULL")}");
        Debug.Log($"agentListContent: {(agentListContent != null ? agentListContent.name : "NULL")}");
        Debug.Log($"globalStatsText: {(globalStatsText != null ? "EXISTS" : "NULL")}");
        Debug.Log($"agentListScrollRect: {(agentListScrollRect != null ? "EXISTS" : "NULL")}");

        if (agentManager != null)
        {
            List<PathAgent> agents = agentManager.GetAllAgents();
            Debug.Log($"Agent count: {(agents != null ? agents.Count : 0)}");
            Debug.Log($"Multi-agent mode: {agentManager.isMultiAgentMode}");
        }

        if (agentListContent != null && agentManager != null)
        {
            PopulateAgentList();
        }
        else
        {
            Debug.LogError("Cannot populate - missing references!");
        }
    }

    [Header("Panel Configuration")]
    [Tooltip("Width of the panel")]
    public float panelWidth = 420f;

    [Tooltip("Height of the panel")]
    public float panelHeight = 700f;

    [Tooltip("Enable stunning visual effects")]
    public bool enableStunningEffects = true;

    [Tooltip("Animation speed multiplier")]
    public float animationSpeed = 1.2f;

    [Header("References")]
    public MultiAgentManager agentManager; // Auto-assigned by SpirographUIManager

    [Header("Runtime UI Elements (Auto-Created)")]
    [HideInInspector] public GameObject agentPanel;
    [HideInInspector] public GameObject agentListContent;
    [HideInInspector] public ScrollRect agentListScrollRect;
    [HideInInspector] public Text globalStatsText;

    [Header("Agent Cards")]
    private List<AgentCard> agentCards = new List<AgentCard>();

    // Agent card prefab (created at runtime)
    private GameObject agentCardPrefab;

    // Add Agent Button
    private Button addAgentButton;

    // Agent creation settings panel
    private GameObject agentCreationPanel;
    private Slider agentSpeedSlider;
    private Text agentSpeedValueText;
    private Button createAgentButton;
    private Button cancelCreateButton;

    // Current agent being configured
    private float newAgentSpeed = 1f;

    // Update frequency for stats (10 Hz = every 0.1s)
    private float updateInterval = 0.1f;
    private float timeSinceLastUpdate = 0f;

    /// <summary>
    /// Represents a single agent card in the roster
    /// </summary>
    private class AgentCard
    {
        public GameObject cardObject;
        public PathAgent agent;
        public Toggle selectToggle;
        public Button focusButton;
        public Button pauseResumeButton;
        public Text statusText;
        public Text progressText;
        public Text speedText;
        public Slider progressBar;
        public Image statusIcon;
        public Image cardBackground;

        // Status colors
        public static Color idleColor = new Color(0.3f, 0.3f, 0.4f, 0.8f);
        public static Color activeColor = new Color(0.1f, 0.3f, 0.2f, 0.8f);
        public static Color pausedColor = new Color(0.3f, 0.2f, 0.1f, 0.8f);
        public static Color completedColor = new Color(0.2f, 0.25f, 0.3f, 0.8f);
    }

    void Start()
    {
        // Find agent manager if not assigned
        if (agentManager == null)
        {
            agentManager = FindFirstObjectByType<MultiAgentManager>();
        }

        // Subscribe to agent manager events
        if (agentManager != null)
        {
            agentManager.OnAgentSelected += OnAgentSelected;
        }

        // Verify panel components exist
        if (agentPanel != null)
        {
            // Try to find missing components if they're null
            if (agentListContent == null)
            {
                Transform contentTransform = agentPanel.transform.Find("AgentList/Viewport/Content");
                if (contentTransform != null)
                {
                    agentListContent = contentTransform.gameObject;
                    Debug.Log("✓ Found agentListContent by search");
                }
            }

            if (globalStatsText == null)
            {
                Transform statsTransform = agentPanel.transform.Find("GlobalStats");
                if (statsTransform != null)
                {
                    globalStatsText = statsTransform.GetComponent<Text>();
                    Debug.Log("✓ Found globalStatsText by search");
                }
            }

            if (agentListScrollRect == null)
            {
                Transform scrollTransform = agentPanel.transform.Find("AgentList");
                if (scrollTransform != null)
                {
                    agentListScrollRect = scrollTransform.GetComponent<ScrollRect>();
                    Debug.Log("✓ Found agentListScrollRect by search");
                }
            }

            agentPanel.SetActive(false);
        }

        Debug.Log("✓ AgentPanelUI initialized - panel will be shown when multi-agent mode is enabled");
    }

    void Update()
    {
        // Update stats at 10 Hz instead of 60 Hz for performance
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            UpdateAllAgentCards();
            UpdateGlobalStats();
        }
    }

    /// <summary>
    /// Show the agent panel
    /// </summary>
    public void ShowPanel()
    {
        if (agentPanel == null)
        {
            Debug.LogWarning("[AgentPanelUI] agentPanel is null! Attempting to find or recreate...");

            // Try to find existing panel
            GameObject foundPanel = GameObject.Find("AgentPanel");
            if (foundPanel != null)
            {
                agentPanel = foundPanel;
                Debug.Log("✓ Found existing AgentPanel");

                // Re-link components
                Transform contentTransform = agentPanel.transform.Find("AgentList/Viewport/Content");
                if (contentTransform != null) agentListContent = contentTransform.gameObject;

                Transform statsTransform = agentPanel.transform.Find("GlobalStats");
                if (statsTransform != null) globalStatsText = statsTransform.GetComponent<Text>();

                Transform scrollTransform = agentPanel.transform.Find("AgentList");
                if (scrollTransform != null) agentListScrollRect = scrollTransform.GetComponent<ScrollRect>();
            }
            else
            {
                // Panel doesn't exist - try to create it
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    Debug.Log("Recreating AgentPanel...");
                    CreatePanel(canvas);
                }
                else
                {
                    Debug.LogError("Cannot create AgentPanel - no Canvas found!");
                    return;
                }
            }
        }

        if (agentPanel != null)
        {
            agentPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide the agent panel
    /// </summary>
    public void HidePanel()
    {
        if (agentPanel != null)
        {
            agentPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Create just the AgentList section (if it's missing)
    /// </summary>
    void CreateAgentListSection(GameObject panelObj)
    {
        Debug.Log("[AgentPanelUI] Creating AgentList section...");

        // Agent List Section (Scrollable)
        GameObject listObj = new GameObject("AgentList");
        listObj.transform.SetParent(panelObj.transform, false);
        RectTransform listRect = listObj.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0);
        listRect.anchorMax = new Vector2(1, 0);
        listRect.pivot = new Vector2(0.5f, 0);
        listRect.anchoredPosition = new Vector2(0, 60);
        listRect.sizeDelta = new Vector2(-20, 350);

        Image listBg = listObj.AddComponent<Image>();
        listBg.color = new Color(0.03f, 0.03f, 0.1f, 0.7f);

        agentListScrollRect = listObj.AddComponent<ScrollRect>();
        agentListScrollRect.horizontal = false;
        agentListScrollRect.vertical = true;
        agentListScrollRect.scrollSensitivity = 15f;
        agentListScrollRect.movementType = ScrollRect.MovementType.Clamped;
        agentListScrollRect.inertia = true;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(listObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);

        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = Color.white;
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        agentListScrollRect.viewport = viewportRect;

        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 400);

        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.spacing = 5f;
        contentLayout.padding = new RectOffset(5, 5, 5, 5);

        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        agentListScrollRect.content = contentRect;
        agentListContent = contentObj;

        Debug.Log("✓ AgentList section created successfully!");

        // Also create control buttons if they don't exist
        if (panelObj.transform.Find("GlobalControls") == null)
        {
            CreateGlobalControlButtons(panelObj);
        }
    }

    /// <summary>
    /// Create the agent panel UI
    /// </summary>
    public void CreatePanel(Canvas canvas)
    {
        // Create panel container
        GameObject panelObj = new GameObject("AgentPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(1, 0);
        panelRect.anchoredPosition = new Vector2(-15, 15);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // Panel background - STUNNING GLASSMORPHIC STYLE
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.01f, 0.02f, 0.12f, 0.92f); // Deep cosmic blue with high opacity

        // Add multiple outlines for depth effect
        Outline panelOutline = panelObj.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.4f, 0.7f, 1f, 0.6f); // Bright cyan glow
        panelOutline.effectDistance = new Vector2(3, -3);

        // Add inner glow
        Shadow panelGlow = panelObj.AddComponent<Shadow>();
        panelGlow.effectColor = new Color(0.3f, 0.6f, 1f, 0.35f); // Intense blue glow
        panelGlow.effectDistance = new Vector2(0, 0);

        // Add pulsing animation component
        if (enableStunningEffects)
        {
            PanelPulseEffect pulseEffect = panelObj.AddComponent<PanelPulseEffect>();
            pulseEffect.glowColor = new Color(0.3f, 0.6f, 1f, 0.5f);
            pulseEffect.pulseSpeed = animationSpeed;

            // Add enhanced visual effects
            EnhancedAgentPanelVisuals enhancedVisuals = panelObj.AddComponent<EnhancedAgentPanelVisuals>();
            enhancedVisuals.animateBackground = true;
            enhancedVisuals.enableBorderGlow = true;
            enhancedVisuals.enableParticles = true;
            enhancedVisuals.enableScaleAnimation = true;
            enhancedVisuals.backgroundAnimationSpeed = animationSpeed * 0.3f;
            enhancedVisuals.borderGlowSpeed = animationSpeed * 1.5f;
        }

        agentPanel = panelObj;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 35);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "✦ AGENT COMMAND CENTER ✦";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.8f, 0.95f, 1f, 1f); // Brighter, more vibrant
        titleText.alignment = TextAnchor.MiddleCenter;

        // Multiple shadow layers for depth
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0.4f, 0.8f, 1f, 0.8f); // Brighter glow
        titleShadow.effectDistance = new Vector2(0, 0);

        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.2f, 0.5f, 1f, 0.6f);
        titleOutline.effectDistance = new Vector2(2, -2);

        // Add pulsing text animation
        if (enableStunningEffects)
        {
            TextPulseEffect textPulse = titleObj.AddComponent<TextPulseEffect>();
            textPulse.minAlpha = 0.8f;
            textPulse.maxAlpha = 1f;
            textPulse.pulseSpeed = animationSpeed * 0.5f;
        }

        // Global Stats Section
        GameObject statsObj = new GameObject("GlobalStats");
        statsObj.transform.SetParent(panelObj.transform, false);
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 1);
        statsRect.anchorMax = new Vector2(1, 1);
        statsRect.pivot = new Vector2(0.5f, 1);
        statsRect.anchoredPosition = new Vector2(0, -50);
        statsRect.sizeDelta = new Vector2(-20, 60);

        Image statsBg = statsObj.AddComponent<Image>();
        statsBg.color = new Color(0.05f, 0.05f, 0.15f, 0.6f);

        globalStatsText = statsObj.AddComponent<Text>();
        globalStatsText.text = "Active: 0 | Paused: 0 | Completed: 0\nAvg Progress: 0%\nTotal Distance: 0m";
        globalStatsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        globalStatsText.fontSize = 10;
        globalStatsText.color = new Color(0.6f, 0.75f, 0.9f, 0.9f);
        globalStatsText.alignment = TextAnchor.UpperLeft;

        RectTransform globalStatsTextRect = globalStatsText.GetComponent<RectTransform>();
        globalStatsTextRect.anchorMin = Vector2.zero;
        globalStatsTextRect.anchorMax = Vector2.one;
        globalStatsTextRect.offsetMin = new Vector2(10, 5);
        globalStatsTextRect.offsetMax = new Vector2(-10, -5);

        Debug.Log("✓ AgentPanel: Created title and stats");

        // Agent List Section (Scrollable)
        GameObject listObj = new GameObject("AgentList");
        listObj.transform.SetParent(panelObj.transform, false);
        RectTransform listRect = listObj.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0);
        listRect.anchorMax = new Vector2(1, 0);
        listRect.pivot = new Vector2(0.5f, 0);
        listRect.anchoredPosition = new Vector2(0, 60);  // Above control buttons
        listRect.sizeDelta = new Vector2(-20, 350);  // Fixed height for scroll area

        Image listBg = listObj.AddComponent<Image>();
        listBg.color = new Color(0.03f, 0.03f, 0.1f, 0.7f);

        // Add ScrollRect
        agentListScrollRect = listObj.AddComponent<ScrollRect>();
        agentListScrollRect.horizontal = false;
        agentListScrollRect.vertical = true;
        agentListScrollRect.scrollSensitivity = 15f;
        agentListScrollRect.movementType = ScrollRect.MovementType.Clamped;
        agentListScrollRect.inertia = true;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(listObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);

        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = Color.white;
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        agentListScrollRect.viewport = viewportRect;

        // Content (holds agent cards)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 400);

        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.spacing = 5f;
        contentLayout.padding = new RectOffset(5, 5, 5, 5);

        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        agentListScrollRect.content = contentRect;
        agentListContent = contentObj;

        Debug.Log("✓ AgentPanel: Created scroll view");

        // Create Agent Button (primary action at top) - SINGLE SOURCE OF TRUTH
        CreateCreateAgentButton(panelObj);

        // Global Control Buttons (at bottom)
        CreateGlobalControlButtons(panelObj);

        Debug.Log("✓ AgentPanel: Created control buttons");

        // Initially hidden
        panelObj.SetActive(false);

        Debug.Log("✓ Agent Panel UI created");
    }

    /// <summary>
    /// Create the CREATE AGENT button - SINGLE SOURCE OF TRUTH for agent creation
    /// </summary>
    void CreateCreateAgentButton(GameObject panelObj)
    {
        GameObject createButtonObj = new GameObject("CreateAgentButton");
        createButtonObj.transform.SetParent(panelObj.transform, false);
        RectTransform createButtonRect = createButtonObj.AddComponent<RectTransform>();
        createButtonRect.anchorMin = new Vector2(0, 0);
        createButtonRect.anchorMax = new Vector2(1, 0);
        createButtonRect.pivot = new Vector2(0.5f, 0);
        createButtonRect.anchoredPosition = new Vector2(0, 420);
        createButtonRect.sizeDelta = new Vector2(-20, 50);

        Image createButtonBg = createButtonObj.AddComponent<Image>();
        createButtonBg.color = new Color(0.2f, 0.8f, 0.4f, 0.95f);

        Button createButton = createButtonObj.AddComponent<Button>();
        createButton.targetGraphic = createButtonBg;

        // Add stunning effects
        if (enableStunningEffects)
        {
            StunningButtonEffect stunningEffect = createButtonObj.AddComponent<StunningButtonEffect>();
            stunningEffect.hoverScaleMultiplier = 1.1f;
            stunningEffect.pressScaleMultiplier = 0.93f;
            stunningEffect.transitionSpeed = 10f;
        }

        // Add glow
        Outline createButtonOutline = createButtonObj.AddComponent<Outline>();
        createButtonOutline.effectColor = new Color(0.5f, 1f, 0.7f, 0.8f);
        createButtonOutline.effectDistance = new Vector2(3, -3);

        Shadow createButtonShadow = createButtonObj.AddComponent<Shadow>();
        createButtonShadow.effectColor = new Color(0.3f, 0.9f, 0.5f, 0.6f);
        createButtonShadow.effectDistance = new Vector2(0, 0);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(createButtonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text createButtonText = textObj.AddComponent<Text>();
        createButtonText.text = "+ CREATE AGENT";
        createButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        createButtonText.fontSize = 18;
        createButtonText.fontStyle = FontStyle.Bold;
        createButtonText.color = new Color(1f, 1f, 1f, 1f);
        createButtonText.alignment = TextAnchor.MiddleCenter;

        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.8f);
        textShadow.effectDistance = new Vector2(2, -2);

        // Connect button to create agent directly
        createButton.onClick.AddListener(OnCreateAgentClicked);

        Debug.Log("✓ AgentPanel: Created CREATE AGENT button - SINGLE SOURCE OF TRUTH");
    }

    /// <summary>
    /// Called when Create Agent button is clicked - SINGLE SOURCE OF TRUTH for agent creation
    /// </summary>
    void OnCreateAgentClicked()
    {
        if (agentManager == null)
        {
            Debug.LogError("[AgentPanelUI] AgentManager is null!");
            return;
        }

        // Get current agent count
        int currentCount = agentManager.agents.Count;

        // Check limit
        if (currentCount >= 16)
        {
            Debug.LogWarning("[AgentPanelUI] Maximum of 16 agents reached!");
            return;
        }

        // Create the agent using MultiAgentManager's CreateAgent method
        PathAgent newAgent = agentManager.CreateAgent(currentCount);

        if (newAgent != null)
        {
            Debug.Log($"✓ Created Agent {currentCount} via CREATE AGENT button");

            // Repopulate the agent list to show the new agent
            PopulateAgentList();
        }
        else
        {
            Debug.LogError("[AgentPanelUI] Failed to create agent!");
        }
    }

    /// <summary>
    /// Create Agent Creation Settings Panel
    /// </summary>
    void CreateAgentCreationPanel(GameObject panelObj)
    {
        agentCreationPanel = new GameObject("AgentCreationPanel");
        agentCreationPanel.transform.SetParent(panelObj.transform, false);
        RectTransform creationRect = agentCreationPanel.AddComponent<RectTransform>();
        creationRect.anchorMin = new Vector2(0, 0);
        creationRect.anchorMax = new Vector2(1, 1);
        creationRect.pivot = new Vector2(0.5f, 0.5f);
        creationRect.anchoredPosition = Vector2.zero;
        creationRect.sizeDelta = Vector2.zero;

        // Semi-transparent background
        Image creationBg = agentCreationPanel.AddComponent<Image>();
        creationBg.color = new Color(0.01f, 0.02f, 0.12f, 0.95f);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(agentCreationPanel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(350, 40);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Configure Helper Agent";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.8f, 0.95f, 1f, 1f);
        titleText.alignment = TextAnchor.MiddleCenter;

        // Speed Slider
        GameObject speedSliderObj = new GameObject("SpeedSlider");
        speedSliderObj.transform.SetParent(agentCreationPanel.transform, false);
        RectTransform speedSliderRect = speedSliderObj.AddComponent<RectTransform>();
        speedSliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        speedSliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        speedSliderRect.pivot = new Vector2(0.5f, 0.5f);
        speedSliderRect.anchoredPosition = new Vector2(0, 50);
        speedSliderRect.sizeDelta = new Vector2(340, 50);

        // Speed Label
        GameObject speedLabelObj = new GameObject("SpeedLabel");
        speedLabelObj.transform.SetParent(speedSliderObj.transform, false);
        RectTransform speedLabelRect = speedLabelObj.AddComponent<RectTransform>();
        speedLabelRect.anchorMin = new Vector2(0, 1);
        speedLabelRect.anchorMax = new Vector2(0, 1);
        speedLabelRect.pivot = new Vector2(0, 1);
        speedLabelRect.anchoredPosition = new Vector2(0, 0);
        speedLabelRect.sizeDelta = new Vector2(200, 25);

        Text speedLabel = speedLabelObj.AddComponent<Text>();
        speedLabel.text = "Agent Speed:";
        speedLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        speedLabel.fontSize = 14;
        speedLabel.fontStyle = FontStyle.Bold;
        speedLabel.color = new Color(0.7f, 0.85f, 1f, 1f);
        speedLabel.alignment = TextAnchor.MiddleLeft;

        // Speed Value Label
        GameObject speedValueObj = new GameObject("SpeedValue");
        speedValueObj.transform.SetParent(speedSliderObj.transform, false);
        RectTransform speedValueRect = speedValueObj.AddComponent<RectTransform>();
        speedValueRect.anchorMin = new Vector2(1, 1);
        speedValueRect.anchorMax = new Vector2(1, 1);
        speedValueRect.pivot = new Vector2(1, 1);
        speedValueRect.anchoredPosition = new Vector2(0, 0);
        speedValueRect.sizeDelta = new Vector2(100, 25);

        agentSpeedValueText = speedValueObj.AddComponent<Text>();
        agentSpeedValueText.text = "1.0x";
        agentSpeedValueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        agentSpeedValueText.fontSize = 14;
        agentSpeedValueText.fontStyle = FontStyle.Bold;
        agentSpeedValueText.color = new Color(0.4f, 0.9f, 0.5f, 1f);
        agentSpeedValueText.alignment = TextAnchor.MiddleRight;

        // Slider background
        GameObject sliderBgObj = new GameObject("Background");
        sliderBgObj.transform.SetParent(speedSliderObj.transform, false);
        RectTransform sliderBgRect = sliderBgObj.AddComponent<RectTransform>();
        sliderBgRect.anchorMin = new Vector2(0, 0);
        sliderBgRect.anchorMax = new Vector2(1, 0);
        sliderBgRect.pivot = new Vector2(0.5f, 0);
        sliderBgRect.anchoredPosition = new Vector2(0, 0);
        sliderBgRect.sizeDelta = new Vector2(0, 8);

        Image sliderBg = sliderBgObj.AddComponent<Image>();
        sliderBg.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);

        // Fill Area
        GameObject fillAreaObj = new GameObject("FillArea");
        fillAreaObj.transform.SetParent(speedSliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0);
        fillAreaRect.anchorMax = new Vector2(1, 0);
        fillAreaRect.pivot = new Vector2(0, 0);
        fillAreaRect.anchoredPosition = new Vector2(0, 0);
        fillAreaRect.sizeDelta = new Vector2(-4, 8);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.sizeDelta = Vector2.zero;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.8f, 0.4f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        // Handle
        GameObject handleAreaObj = new GameObject("HandleArea");
        handleAreaObj.transform.SetParent(speedSliderObj.transform, false);
        RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);
        handleAreaRect.sizeDelta = new Vector2(-4, 0);

        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);

        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = new Color(0.4f, 1f, 0.5f, 1f);

        // Slider component
        agentSpeedSlider = speedSliderObj.AddComponent<Slider>();
        agentSpeedSlider.fillRect = fillRect;
        agentSpeedSlider.handleRect = handleRect;
        agentSpeedSlider.minValue = 0.1f;
        agentSpeedSlider.maxValue = 3f;
        agentSpeedSlider.value = 1f;
        agentSpeedSlider.onValueChanged.AddListener(OnAgentSpeedChanged);

        // Create and Cancel buttons
        GameObject buttonRowObj = new GameObject("ButtonRow");
        buttonRowObj.transform.SetParent(agentCreationPanel.transform, false);
        RectTransform buttonRowRect = buttonRowObj.AddComponent<RectTransform>();
        buttonRowRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRowRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRowRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRowRect.anchoredPosition = new Vector2(0, -50);
        buttonRowRect.sizeDelta = new Vector2(340, 50);

        HorizontalLayoutGroup buttonLayout = buttonRowObj.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 15f;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;
        buttonLayout.childForceExpandHeight = true;

        // Create Button
        createAgentButton = CreateControlButton(buttonRowObj, "CREATE AGENT", new Color(0.2f, 0.7f, 0.4f, 0.9f));
        createAgentButton.onClick.AddListener(CreateNewAgent);

        // Cancel Button
        cancelCreateButton = CreateControlButton(buttonRowObj, "CANCEL", new Color(0.6f, 0.3f, 0.3f, 0.9f));
        cancelCreateButton.onClick.AddListener(HideAgentCreationPanel);

        // Initially hidden
        agentCreationPanel.SetActive(false);

        Debug.Log("✓ AgentPanel: Created Agent Creation Panel");
    }

    /// <summary>
    /// Show the agent creation panel
    /// </summary>
    void ShowAgentCreationPanel()
    {
        if (agentCreationPanel != null)
        {
            newAgentSpeed = 1f;
            agentSpeedSlider.value = 1f;
            agentSpeedValueText.text = "1.0x";
            agentCreationPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide the agent creation panel
    /// </summary>
    void HideAgentCreationPanel()
    {
        if (agentCreationPanel != null)
        {
            agentCreationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Handle agent speed slider change
    /// </summary>
    void OnAgentSpeedChanged(float value)
    {
        newAgentSpeed = value;
        if (agentSpeedValueText != null)
        {
            agentSpeedValueText.text = $"{value:F1}x";
        }
    }

    /// <summary>
    /// Create a new agent with configured settings
    /// </summary>
    void CreateNewAgent()
    {
        if (agentManager == null)
        {
            Debug.LogError("[AgentPanelUI] AgentManager is null!");
            return;
        }

        // Get current agent count
        int currentCount = agentManager.agents.Count;

        // Check limit
        if (currentCount >= 16)
        {
            Debug.LogWarning("[AgentPanelUI] Maximum of 16 agents reached!");
            HideAgentCreationPanel();
            return;
        }

        // Create the agent using MultiAgentManager's CreateAgent method
        PathAgent newAgent = agentManager.CreateAgent(currentCount);

        if (newAgent != null)
        {
            // Set the configured speed
            newAgent.speedMultiplier = newAgentSpeed;

            Debug.Log($"[AgentPanelUI] Created Agent {currentCount} with speed {newAgentSpeed:F1}x");

            // Repopulate the agent list to show the new agent
            PopulateAgentList();
        }

        // Hide the creation panel
        HideAgentCreationPanel();
    }

    /// <summary>
    /// Create global control buttons for all agents
    /// </summary>
    void CreateGlobalControlButtons(GameObject panelObj)
    {
        // Control button container
        GameObject buttonRowObj = new GameObject("GlobalControls");
        buttonRowObj.transform.SetParent(panelObj.transform, false);
        RectTransform buttonRowRect = buttonRowObj.AddComponent<RectTransform>();
        buttonRowRect.anchorMin = new Vector2(0, 0);
        buttonRowRect.anchorMax = new Vector2(1, 0);
        buttonRowRect.pivot = new Vector2(0.5f, 0);
        buttonRowRect.anchoredPosition = new Vector2(0, 10);
        buttonRowRect.sizeDelta = new Vector2(-20, 40);

        HorizontalLayoutGroup buttonLayout = buttonRowObj.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 8f;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;
        buttonLayout.childForceExpandHeight = true;

        // START ALL button
        Button startAllBtn = CreateControlButton(buttonRowObj, "START ALL", new Color(0.2f, 0.6f, 0.3f, 0.9f));
        startAllBtn.onClick.AddListener(() =>
        {
            if (agentManager != null)
            {
                agentManager.StartAllAgents();
                Debug.Log("✓ All agents started");
            }
        });

        // PAUSE ALL button
        Button pauseAllBtn = CreateControlButton(buttonRowObj, "PAUSE ALL", new Color(0.6f, 0.4f, 0.2f, 0.9f));
        pauseAllBtn.onClick.AddListener(() =>
        {
            if (agentManager != null)
            {
                agentManager.PauseAllAgents();
                Debug.Log("⏸ All agents paused");
            }
        });

        // RESET ALL button
        Button resetAllBtn = CreateControlButton(buttonRowObj, "RESET ALL", new Color(0.5f, 0.3f, 0.6f, 0.9f));
        resetAllBtn.onClick.AddListener(() =>
        {
            if (agentManager != null)
            {
                agentManager.ResetAllAgents();
                Debug.Log("↻ All agents reset");
            }
        });
    }

    /// <summary>
    /// Helper to create a control button
    /// </summary>
    Button CreateControlButton(GameObject parent, string text, Color color)
    {
        GameObject btnObj = new GameObject(text.Replace(" ", ""));
        btnObj.transform.SetParent(parent.transform, false);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = color;

        Button btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f); // Brighter highlight
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Darker press
        colors.fadeDuration = 0.1f; // Faster transition
        btn.colors = colors;

        // Add stunning outline
        Outline btnOutline = btnObj.AddComponent<Outline>();
        btnOutline.effectColor = new Color(color.r * 1.5f, color.g * 1.5f, color.b * 1.5f, 0.6f);
        btnOutline.effectDistance = new Vector2(2, -2);

        // Add glow shadow
        Shadow btnShadow = btnObj.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(color.r, color.g, color.b, 0.4f);
        btnShadow.effectDistance = new Vector2(0, 0);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text btnText = textObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 12; // Slightly larger
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = new Color(1f, 1f, 1f, 0.95f); // Bright white
        btnText.alignment = TextAnchor.MiddleCenter;

        // Add text shadow for depth
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.6f);
        textShadow.effectDistance = new Vector2(1, -1);

        // Add button animator for stunning press effect
        if (enableStunningEffects)
        {
            StunningButtonEffect stunningEffect = btnObj.AddComponent<StunningButtonEffect>();
            stunningEffect.hoverScaleMultiplier = 1.1f;
            stunningEffect.pressScaleMultiplier = 0.92f;
            stunningEffect.transitionSpeed = animationSpeed * 6f;
        }

        return btn;
    }

    /// <summary>
    /// Populate the agent list with cards for all agents
    /// </summary>
    public void PopulateAgentList()
    {
        if (agentManager == null)
        {
            // Silently return during initialization - this is normal
            return;
        }

        if (agentListContent == null)
        {
            // Try to auto-fix before logging error
            if (agentPanel != null)
            {
                Transform contentTransform = agentPanel.transform.Find("AgentList/Viewport/Content");
                if (contentTransform != null)
                {
                    agentListContent = contentTransform.gameObject;
                }
            }
            
            // If still null, just return silently during initialization
            if (agentListContent == null)
            {
                return;
            }
        }



        // Clear existing cards
        ClearAgentList();

        // Create card for each agent
        List<PathAgent> agents = agentManager.GetAllAgents();

        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[AgentPanelUI] No agents to display");
            return;
        }

        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i] != null)
            {
                CreateAgentCard(agents[i], i);
            }
        }

        Debug.Log($"✓ Created {agentCards.Count} agent cards");
    }

    /// <summary>
    /// Clear all agent cards
    /// </summary>
    void ClearAgentList()
    {
        foreach (AgentCard card in agentCards)
        {
            if (card.cardObject != null)
            {
                Destroy(card.cardObject);
            }
        }
        agentCards.Clear();
    }

    /// <summary>
    /// Create a card for a single agent
    /// </summary>
    void CreateAgentCard(PathAgent agent, int index)
    {
        // Card container
        GameObject cardObj = new GameObject($"AgentCard_{index}");
        cardObj.transform.SetParent(agentListContent.transform, false);
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(0, 90);

        LayoutElement cardLayout = cardObj.AddComponent<LayoutElement>();
        cardLayout.preferredHeight = 90;
        cardLayout.flexibleHeight = 0;

        // Card background - STUNNING GLASSMORPHIC CARD
        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.color = AgentCard.idleColor;

        // Multiple outline layers for 3D depth
        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.4f, 0.7f, 1f, 0.5f); // Brighter cyan outline
        cardOutline.effectDistance = new Vector2(2, -2);

        Shadow cardShadow = cardObj.AddComponent<Shadow>();
        cardShadow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        cardShadow.effectDistance = new Vector2(3, -3);

        // Add hover effect component
        if (enableStunningEffects)
        {
            AgentCardHoverEffect hoverEffect = cardObj.AddComponent<AgentCardHoverEffect>();
            hoverEffect.normalColor = AgentCard.idleColor;
            hoverEffect.hoverColor = new Color(AgentCard.idleColor.r * 1.2f, AgentCard.idleColor.g * 1.2f, AgentCard.idleColor.b * 1.2f, 0.95f);
            hoverEffect.outline = cardOutline;
        }

        // Agent name and status (top row)
        GameObject topRowObj = new GameObject("TopRow");
        topRowObj.transform.SetParent(cardObj.transform, false);
        RectTransform topRowRect = topRowObj.AddComponent<RectTransform>();
        topRowRect.anchorMin = new Vector2(0, 1);
        topRowRect.anchorMax = new Vector2(1, 1);
        topRowRect.pivot = new Vector2(0, 1);
        topRowRect.anchoredPosition = new Vector2(8, -5);
        topRowRect.sizeDelta = new Vector2(-16, 20);

        Text nameText = topRowObj.AddComponent<Text>();
        nameText.text = $"Agent {index}";
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 12;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = agent.agentColor;
        nameText.alignment = TextAnchor.MiddleLeft;

        // Status icon and text (next to name)
        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(topRowObj.transform, false);
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(1, 0);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(1, 0.5f);
        statusRect.anchoredPosition = Vector2.zero;
        statusRect.sizeDelta = new Vector2(80, 0);

        Text statusText = statusObj.AddComponent<Text>();
        statusText.text = "● Idle";
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 10;
        statusText.color = new Color(0.6f, 0.6f, 0.7f, 1f);
        statusText.alignment = TextAnchor.MiddleRight;

        // Progress bar
        GameObject progressBarObj = new GameObject("ProgressBar");
        progressBarObj.transform.SetParent(cardObj.transform, false);
        RectTransform progressBarRect = progressBarObj.AddComponent<RectTransform>();
        progressBarRect.anchorMin = new Vector2(0, 1);
        progressBarRect.anchorMax = new Vector2(1, 1);
        progressBarRect.pivot = new Vector2(0, 1);
        progressBarRect.anchoredPosition = new Vector2(8, -30);
        progressBarRect.sizeDelta = new Vector2(-16, 8);

        Image progressBg = progressBarObj.AddComponent<Image>();
        progressBg.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);

        GameObject fillAreaObj = new GameObject("FillArea");
        fillAreaObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = agent.agentColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        Slider progressBar = progressBarObj.AddComponent<Slider>();
        progressBar.fillRect = fillRect;
        progressBar.interactable = false;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;

        // Add animated progress bar effect
        if (enableStunningEffects)
        {
            ProgressBarAnimator progressAnimator = progressBarObj.AddComponent<ProgressBarAnimator>();
            progressAnimator.progressBar = progressBar;
            progressAnimator.animationSpeed = animationSpeed * 4f;
            progressAnimator.useColorGradient = true;
            progressAnimator.startColor = new Color(0.8f, 0.3f, 0.3f, 1f); // Red for starting
            progressAnimator.midColor = new Color(0.9f, 0.8f, 0.3f, 1f); // Yellow for mid
            progressAnimator.endColor = new Color(0.3f, 0.9f, 0.4f, 1f); // Green for completing

            // Add advanced animated progress bar for even smoother animations
            AnimatedProgressBar animatedBar = fillObj.AddComponent<AnimatedProgressBar>();
            animatedBar.animationSpeed = animationSpeed * 5f;
            animatedBar.smoothAnimation = true;
            animatedBar.enablePulse = true;
            animatedBar.pulseSpeed = animationSpeed * 2f;
        }

        // Stats text (progress and speed)
        GameObject statsTextObj = new GameObject("StatsText");
        statsTextObj.transform.SetParent(cardObj.transform, false);
        RectTransform statsTextRect = statsTextObj.AddComponent<RectTransform>();
        statsTextRect.anchorMin = new Vector2(0, 1);
        statsTextRect.anchorMax = new Vector2(1, 1);
        statsTextRect.pivot = new Vector2(0, 1);
        statsTextRect.anchoredPosition = new Vector2(8, -42);
        statsTextRect.sizeDelta = new Vector2(-16, 15);

        Text progressText = statsTextObj.AddComponent<Text>();
        progressText.text = $"Progress: 0% | Speed: {agent.speedMultiplier:F1}x";
        progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        progressText.fontSize = 9;
        progressText.color = new Color(0.5f, 0.65f, 0.8f, 0.9f);
        progressText.alignment = TextAnchor.MiddleLeft;

        // Buttons (bottom row) - now includes Remove button
        // Idle/Focus button (starts agent or focuses camera)
        GameObject focusButtonObj = new GameObject("IdleButton");
        focusButtonObj.transform.SetParent(cardObj.transform, false);
        RectTransform focusButtonRect = focusButtonObj.AddComponent<RectTransform>();
        focusButtonRect.anchorMin = new Vector2(0, 0);
        focusButtonRect.anchorMax = new Vector2(0, 0);
        focusButtonRect.pivot = new Vector2(0, 0);
        focusButtonRect.anchoredPosition = new Vector2(8, 5);
        focusButtonRect.sizeDelta = new Vector2(45, 25);

        Image focusButtonImage = focusButtonObj.AddComponent<Image>();
        focusButtonImage.color = new Color(0.1f, 0.2f, 0.3f, 0.8f);

        Button focusButton = focusButtonObj.AddComponent<Button>();
        focusButton.targetGraphic = focusButtonImage;

        // Add stunning button effect
        if (enableStunningEffects)
        {
            StunningButtonEffect focusEffect = focusButtonObj.AddComponent<StunningButtonEffect>();
            focusEffect.hoverScaleMultiplier = 1.08f;
            focusEffect.pressScaleMultiplier = 0.95f;
            focusEffect.transitionSpeed = 8f;
        }

        GameObject focusTextObj = new GameObject("Text");
        focusTextObj.transform.SetParent(focusButtonObj.transform, false);
        RectTransform focusTextRect = focusTextObj.AddComponent<RectTransform>();
        focusTextRect.anchorMin = Vector2.zero;
        focusTextRect.anchorMax = Vector2.one;
        focusTextRect.sizeDelta = Vector2.zero;

        Text focusText = focusTextObj.AddComponent<Text>();
        focusText.text = "Idle";
        focusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        focusText.fontSize = 9;
        focusText.fontStyle = FontStyle.Bold;
        focusText.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        focusText.alignment = TextAnchor.MiddleCenter;

        // Pause/Resume button
        GameObject pauseButtonObj = new GameObject("PauseButton");
        pauseButtonObj.transform.SetParent(cardObj.transform, false);
        RectTransform pauseButtonRect = pauseButtonObj.AddComponent<RectTransform>();
        pauseButtonRect.anchorMin = new Vector2(0, 0);
        pauseButtonRect.anchorMax = new Vector2(0, 0);
        pauseButtonRect.pivot = new Vector2(0, 0);
        pauseButtonRect.anchoredPosition = new Vector2(58, 5);
        pauseButtonRect.sizeDelta = new Vector2(60, 25);

        Image pauseButtonImage = pauseButtonObj.AddComponent<Image>();
        pauseButtonImage.color = new Color(0.15f, 0.2f, 0.1f, 0.8f);

        Button pauseButton = pauseButtonObj.AddComponent<Button>();
        pauseButton.targetGraphic = pauseButtonImage;

        // Add stunning button effect
        if (enableStunningEffects)
        {
            StunningButtonEffect pauseEffect = pauseButtonObj.AddComponent<StunningButtonEffect>();
            pauseEffect.hoverScaleMultiplier = 1.08f;
            pauseEffect.pressScaleMultiplier = 0.95f;
            pauseEffect.transitionSpeed = 8f;
        }

        GameObject pauseTextObj = new GameObject("Text");
        pauseTextObj.transform.SetParent(pauseButtonObj.transform, false);
        RectTransform pauseTextRect = pauseTextObj.AddComponent<RectTransform>();
        pauseTextRect.anchorMin = Vector2.zero;
        pauseTextRect.anchorMax = Vector2.one;
        pauseTextRect.sizeDelta = Vector2.zero;

        Text pauseText = pauseTextObj.AddComponent<Text>();
        pauseText.text = "Pause";
        pauseText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pauseText.fontSize = 9;
        pauseText.fontStyle = FontStyle.Bold;
        pauseText.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        pauseText.alignment = TextAnchor.MiddleCenter;

        // Remove button
        GameObject removeButtonObj = new GameObject("RemoveButton");
        removeButtonObj.transform.SetParent(cardObj.transform, false);
        RectTransform removeButtonRect = removeButtonObj.AddComponent<RectTransform>();
        removeButtonRect.anchorMin = new Vector2(0, 0);
        removeButtonRect.anchorMax = new Vector2(0, 0);
        removeButtonRect.pivot = new Vector2(0, 0);
        removeButtonRect.anchoredPosition = new Vector2(123, 5);
        removeButtonRect.sizeDelta = new Vector2(60, 25);

        Image removeButtonImage = removeButtonObj.AddComponent<Image>();
        removeButtonImage.color = new Color(0.4f, 0.1f, 0.1f, 0.8f);

        Button removeButton = removeButtonObj.AddComponent<Button>();
        removeButton.targetGraphic = removeButtonImage;

        // Add stunning button effect
        if (enableStunningEffects)
        {
            StunningButtonEffect removeEffect = removeButtonObj.AddComponent<StunningButtonEffect>();
            removeEffect.hoverScaleMultiplier = 1.08f;
            removeEffect.pressScaleMultiplier = 0.95f;
            removeEffect.transitionSpeed = 8f;
            removeEffect.hoverTintColor = new Color(1.3f, 1.1f, 1.1f, 1f); // Slight red tint on hover
        }

        GameObject removeTextObj = new GameObject("Text");
        removeTextObj.transform.SetParent(removeButtonObj.transform, false);
        RectTransform removeTextRect = removeTextObj.AddComponent<RectTransform>();
        removeTextRect.anchorMin = Vector2.zero;
        removeTextRect.anchorMax = Vector2.one;
        removeTextRect.sizeDelta = Vector2.zero;

        Text removeText = removeTextObj.AddComponent<Text>();
        removeText.text = "Remove";
        removeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        removeText.fontSize = 9;
        removeText.fontStyle = FontStyle.Bold;
        removeText.color = new Color(1f, 0.7f, 0.7f, 0.9f);
        removeText.alignment = TextAnchor.MiddleCenter;

        // Select toggle (radio button)
        GameObject selectToggleObj = new GameObject("SelectToggle");
        selectToggleObj.transform.SetParent(cardObj.transform, false);
        RectTransform selectToggleRect = selectToggleObj.AddComponent<RectTransform>();
        selectToggleRect.anchorMin = new Vector2(1, 0.5f);
        selectToggleRect.anchorMax = new Vector2(1, 0.5f);
        selectToggleRect.pivot = new Vector2(1, 0.5f);
        selectToggleRect.anchoredPosition = new Vector2(-8, 0);
        selectToggleRect.sizeDelta = new Vector2(20, 20);

        Image selectBg = selectToggleObj.AddComponent<Image>();
        selectBg.color = new Color(0.1f, 0.15f, 0.25f, 0.8f);

        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(selectToggleObj.transform, false);
        RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.sizeDelta = Vector2.zero;

        Text checkmark = checkmarkObj.AddComponent<Text>();
        checkmark.text = "●";
        checkmark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        checkmark.fontSize = 16;
        checkmark.color = agent.agentColor;
        checkmark.alignment = TextAnchor.MiddleCenter;

        Toggle selectToggle = selectToggleObj.AddComponent<Toggle>();
        selectToggle.targetGraphic = selectBg;
        selectToggle.graphic = checkmark;
        selectToggle.isOn = false;

        // Create AgentCard data structure
        AgentCard card = new AgentCard();
        card.cardObject = cardObj;
        card.agent = agent;
        card.selectToggle = selectToggle;
        card.focusButton = focusButton;
        card.pauseResumeButton = pauseButton;
        card.statusText = statusText;
        card.progressText = progressText;
        card.speedText = null; // Not used separately
        card.progressBar = progressBar;
        card.statusIcon = null; // Not used separately
        card.cardBackground = cardBg;

        // Connect buttons
        int agentIndex = index; // Capture for closure
        focusButton.onClick.AddListener(() => OnIdleButtonClicked(agentIndex, focusText));
        pauseButton.onClick.AddListener(() => OnPauseResumeButtonClicked(agentIndex, pauseText));
        removeButton.onClick.AddListener(() => OnRemoveButtonClicked(agentIndex));
        selectToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn) OnAgentCardSelected(agentIndex);
        });

        agentCards.Add(card);
    }

    /// <summary>
    /// Update all agent cards (called at 10 Hz)
    /// </summary>
    void UpdateAllAgentCards()
    {
        if (agentManager == null || !agentManager.isMultiAgentMode) return;

        if (agentCards.Count == 0)
        {
            // Try to repopulate if we have agents but no cards
            List<PathAgent> agents = agentManager.GetAllAgents();
            if (agents != null && agents.Count > 0 && agentListContent != null)
            {
                Debug.LogWarning("[AgentPanelUI] Cards missing but agents exist - repopulating...");
                PopulateAgentList();
            }
            return;
        }

        for (int i = 0; i < agentCards.Count; i++)
        {
            if (agentCards[i].agent != null)
            {
                UpdateAgentCard(agentCards[i]);
            }
        }
    }

    /// <summary>
    /// Update a single agent card's display
    /// </summary>
    void UpdateAgentCard(AgentCard card)
    {
        PathAgent agent = card.agent;

        // Performance optimization: Only update if card is visible in viewport
        if (!IsCardVisible(card))
        {
            return; // Skip update for cards outside viewport
        }

        // Update status text and card color (only if changed to reduce UI updates)
        PathAgent.AgentStatus currentStatus = agent.status;
        string statusText = "";
        Color statusColor = Color.white;
        Color cardColor = AgentCard.idleColor;

        switch (currentStatus)
        {
            case PathAgent.AgentStatus.Idle:
                statusText = "● Idle";
                statusColor = new Color(0.5f, 0.5f, 0.6f, 1f);
                cardColor = AgentCard.idleColor;
                break;
            case PathAgent.AgentStatus.Active:
                statusText = "▶ Active";
                statusColor = new Color(0.3f, 0.8f, 0.4f, 1f);
                cardColor = AgentCard.activeColor;
                break;
            case PathAgent.AgentStatus.Paused:
                statusText = "⏸ Paused";
                statusColor = new Color(0.9f, 0.7f, 0.3f, 1f);
                cardColor = AgentCard.pausedColor;
                break;
            case PathAgent.AgentStatus.Completed:
                statusText = "✓ Complete";
                statusColor = new Color(0.4f, 0.6f, 0.9f, 1f);
                cardColor = AgentCard.completedColor;
                break;
        }

        // Only update if changed (dirty flag optimization)
        if (card.statusText.text != statusText)
        {
            card.statusText.text = statusText;
            card.statusText.color = statusColor;
            card.cardBackground.color = cardColor;
        }

        // Update progress bar and text
        float cycleProgress = agent.currentCycle;
        float positionProgress = 0f;
        if (agentManager.sharedState != null && agent.totalPathLength > 0)
        {
            positionProgress = agent.currentDistance / agent.totalPathLength;
        }
        float totalProgress = (cycleProgress + positionProgress) / Mathf.Max(1, agentManager.sharedState.masterCycles);

        card.progressBar.value = totalProgress;

        // Update stats text
        float speedMultiplier = agent.speedMultiplier;
        card.progressText.text = $"Progress: {(totalProgress * 100f):F1}% | Speed: {speedMultiplier:F1}x";
    }

    /// <summary>
    /// Check if card is visible in viewport (optimization)
    /// </summary>
    bool IsCardVisible(AgentCard card)
    {
        if (card.cardObject == null || agentListScrollRect == null) return true;

        // Simple check: if card has active GameObject, assume visible
        // More complex viewport culling could be added here if needed
        return card.cardObject.activeInHierarchy;
    }

    /// <summary>
    /// Update global statistics display
    /// </summary>
    void UpdateGlobalStats()
    {
        if (agentManager == null || globalStatsText == null) return;

        globalStatsText.text = $"Active: {agentManager.activeAgentCount} | Paused: {agentManager.pausedAgentCount} | Completed: {agentManager.completedAgentCount}\n" +
                               $"Avg Progress: {(agentManager.averageProgress * 100f):F1}%\n" +
                               $"Total Distance: {agentManager.totalDistanceCovered:F1}m";
    }

    /// <summary>
    /// Handle Idle button click (starts agent or focuses camera)
    /// </summary>
    void OnIdleButtonClicked(int agentIndex, Text buttonText)
    {
        if (agentManager == null) return;

        PathAgent agent = agentManager.GetAgent(agentIndex);
        if (agent != null)
        {
            if (agent.status == PathAgent.AgentStatus.Idle)
            {
                agent.StartDrawing();
                buttonText.text = "Focus";
                Debug.Log($"Agent {agentIndex} started drawing");
            }
            else
            {
                // Focus camera on agent
                CameraController cameraController = FindFirstObjectByType<CameraController>();
                if (cameraController != null)
                {
                    cameraController.target = agent.transform;
                    cameraController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                    Debug.Log($"Camera focused on Agent {agentIndex}");
                }
            }
        }
    }

    /// <summary>
    /// Handle pause/resume button click
    /// </summary>
    void OnPauseResumeButtonClicked(int agentIndex, Text buttonText)
    {
        if (agentManager == null) return;

        PathAgent agent = agentManager.GetAgent(agentIndex);
        if (agent != null)
        {
            if (agent.status == PathAgent.AgentStatus.Active)
            {
                agentManager.PauseAgent(agentIndex);
                buttonText.text = "Resume";
            }
            else if (agent.status == PathAgent.AgentStatus.Paused)
            {
                agentManager.ResumeAgent(agentIndex);
                buttonText.text = "Pause";
            }
        }
    }

    /// <summary>
    /// Handle remove button click
    /// </summary>
    void OnRemoveButtonClicked(int agentIndex)
    {
        if (agentManager == null) return;

        PathAgent agent = agentManager.GetAgent(agentIndex);
        if (agent != null)
        {
            // Remove agent from manager
            agentManager.RemoveAgent(agentIndex);

            // Repopulate the agent list
            PopulateAgentList();

            Debug.Log($"Agent {agentIndex} removed");
        }
    }

    /// <summary>
    /// Handle agent card selection (radio button)
    /// </summary>
    void OnAgentCardSelected(int agentIndex)
    {
        if (agentManager == null) return;

        // Check if this agent is currently selected
        PathAgent clickedAgent = agentManager.GetAgent(agentIndex);
        bool isCurrentlySelected = agentCards[agentIndex].selectToggle.isOn;
        bool wasAlreadySelected = (agentManager.selectedAgent == clickedAgent);

        // If clicking the already-selected agent, deselect it
        if (isCurrentlySelected && wasAlreadySelected)
        {
            // Deselect this agent
            agentCards[agentIndex].selectToggle.SetIsOnWithoutNotify(false);
            agentManager.selectedAgent = null;

            // Un-highlight trail
            if (clickedAgent != null)
            {
                clickedAgent.HighlightTrail(false);
            }

            // Notify UI manager to exit per-agent control mode
            SpirographUIManager uiManager = FindFirstObjectByType<SpirographUIManager>();
            if (uiManager != null)
            {
                uiManager.ExitPerAgentControl();
            }

            Debug.Log($"★ Agent {agentIndex} deselected - returned to master control mode");
            return;
        }

        // If selecting a new agent (or first selection)
        if (isCurrentlySelected)
        {
            // Deselect all other toggles (radio button behavior)
            for (int i = 0; i < agentCards.Count; i++)
            {
                if (i != agentIndex && agentCards[i].selectToggle != null)
                {
                    agentCards[i].selectToggle.SetIsOnWithoutNotify(false);
                    // Un-highlight other trails
                    if (agentCards[i].agent != null)
                    {
                        agentCards[i].agent.HighlightTrail(false);
                    }
                }
            }

            // Select the agent in the manager (this fires OnAgentSelected event)
            agentManager.SelectAgent(agentIndex);

            // Highlight selected agent's trail
            if (clickedAgent != null)
            {
                clickedAgent.HighlightTrail(true);
            }

            // Focus camera on selected agent
            if (clickedAgent != null)
            {
                CameraController cameraController = FindFirstObjectByType<CameraController>();
                if (cameraController != null)
                {
                    cameraController.target = clickedAgent.transform;
                    cameraController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                    Debug.Log($"★ Camera focused on Agent {agentIndex}");
                }
            }

            Debug.Log($"★ Agent {agentIndex} selected for per-agent control");
        }
    }

    /// <summary>
    /// Handle agent selection from manager (update UI)
    /// </summary>
    void OnAgentSelected(PathAgent agent)
    {
        if (agent == null) return;

        // Update toggles to reflect selection and highlight trails
        for (int i = 0; i < agentCards.Count; i++)
        {
            if (agentCards[i].agent == agent)
            {
                agentCards[i].selectToggle.SetIsOnWithoutNotify(true);
                // Highlight selected agent's trail
                agentCards[i].agent.HighlightTrail(true);
                // Pulse animation for selected card
                StartCoroutine(PulseCardSelection(agentCards[i]));
            }
            else
            {
                agentCards[i].selectToggle.SetIsOnWithoutNotify(false);
                // Un-highlight other trails
                if (agentCards[i].agent != null)
                {
                    agentCards[i].agent.HighlightTrail(false);
                }
            }
        }
    }

    /// <summary>
    /// Pulse animation for selected agent card
    /// </summary>
    System.Collections.IEnumerator PulseCardSelection(AgentCard card)
    {
        if (card.cardObject == null) yield break;

        Outline outline = card.cardObject.GetComponent<Outline>();
        if (outline == null) yield break;

        Color originalColor = outline.effectColor;
        Color brightColor = new Color(0.5f, 0.8f, 1f, 0.8f);

        // Pulse once
        float duration = 0.3f;
        float elapsed = 0f;

        // Brighten
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            outline.effectColor = Color.Lerp(originalColor, brightColor, t);
            yield return null;
        }

        elapsed = 0f;

        // Dim back
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            outline.effectColor = Color.Lerp(brightColor, originalColor, t);
            yield return null;
        }

        outline.effectColor = originalColor;
    }
}

/// <summary>
/// Creates a pulsing glow effect on the agent panel
/// </summary>
public class PanelPulseEffect : MonoBehaviour
{
    public Color glowColor = new Color(0.3f, 0.6f, 1f, 0.5f);
    public float pulseSpeed = 1.2f;
    public float minIntensity = 0.3f;
    public float maxIntensity = 0.6f;

    private Shadow glowShadow;
    private float time = 0f;

    void Start()
    {
        glowShadow = GetComponent<Shadow>();
    }

    void Update()
    {
        if (glowShadow == null) return;

        time += Time.deltaTime * pulseSpeed;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(time) + 1f) * 0.5f);

        Color pulseColor = glowColor;
        pulseColor.a = intensity;
        glowShadow.effectColor = pulseColor;
    }
}

/// <summary>
/// Creates a subtle pulsing effect on text elements
/// </summary>
public class TextPulseEffect : MonoBehaviour
{
    public float minAlpha = 0.8f;
    public float maxAlpha = 1f;
    public float pulseSpeed = 0.6f;

    private Text text;
    private Color originalColor;
    private float time = 0f;

    void Start()
    {
        text = GetComponent<Text>();
        if (text != null)
        {
            originalColor = text.color;
        }
    }

    void Update()
    {
        if (text == null) return;

        time += Time.deltaTime * pulseSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(time) + 1f) * 0.5f);

        Color newColor = originalColor;
        newColor.a = alpha;
        text.color = newColor;
    }
}

/// <summary>
/// Creates a hover effect on agent cards with smooth transitions
/// </summary>
public class AgentCardHoverEffect : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public Color normalColor;
    public Color hoverColor;
    public Outline outline;
    public float transitionSpeed = 8f;

    private Image cardImage;
    private Color targetColor;
    private Color targetOutlineColor;
    private Color normalOutlineColor;
    private Color brightOutlineColor;
    private bool isHovering = false;

    void Start()
    {
        cardImage = GetComponent<Image>();
        targetColor = normalColor;

        if (outline != null)
        {
            normalOutlineColor = outline.effectColor;
            brightOutlineColor = new Color(normalOutlineColor.r * 1.5f, normalOutlineColor.g * 1.5f, normalOutlineColor.b * 1.5f, normalOutlineColor.a * 1.3f);
            targetOutlineColor = normalOutlineColor;
        }
    }

    void Update()
    {
        if (cardImage != null)
        {
            cardImage.color = Color.Lerp(cardImage.color, targetColor, Time.deltaTime * transitionSpeed);
        }

        if (outline != null)
        {
            outline.effectColor = Color.Lerp(outline.effectColor, targetOutlineColor, Time.deltaTime * transitionSpeed);
        }
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isHovering = true;
        targetColor = hoverColor;
        targetOutlineColor = brightOutlineColor;
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isHovering = false;
        targetColor = normalColor;
        targetOutlineColor = normalOutlineColor;
    }
}

/// <summary>
/// Creates animated progress bars with smooth filling
/// </summary>
public class ProgressBarAnimator : MonoBehaviour
{
    public Slider progressBar;
    public float animationSpeed = 5f;
    public bool useColorGradient = true;
    public Color startColor = Color.red;
    public Color midColor = Color.yellow;
    public Color endColor = Color.green;

    private Image fillImage;
    private float targetValue = 0f;

    void Start()
    {
        if (progressBar != null && progressBar.fillRect != null)
        {
            fillImage = progressBar.fillRect.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (progressBar == null) return;

        // Smooth animation
        float currentValue = progressBar.value;
        if (Mathf.Abs(currentValue - targetValue) > 0.001f)
        {
            progressBar.value = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * animationSpeed);
        }

        // Color gradient based on progress
        if (useColorGradient && fillImage != null)
        {
            float normalizedValue = progressBar.value / progressBar.maxValue;
            Color targetColor;

            if (normalizedValue < 0.5f)
            {
                targetColor = Color.Lerp(startColor, midColor, normalizedValue * 2f);
            }
            else
            {
                targetColor = Color.Lerp(midColor, endColor, (normalizedValue - 0.5f) * 2f);
            }

            fillImage.color = targetColor;
        }
    }

    public void SetProgress(float value)
    {
        targetValue = Mathf.Clamp(value, progressBar.minValue, progressBar.maxValue);
    }
}

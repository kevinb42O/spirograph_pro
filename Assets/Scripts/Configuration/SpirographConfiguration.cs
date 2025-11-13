using UnityEngine;

namespace SpirographPro.Configuration
{
    /// <summary>
    /// Centralized configuration for Spirograph system.
    /// Contains constants and default values to avoid magic numbers throughout the codebase.
    /// </summary>
    [CreateAssetMenu(fileName = "SpirographConfiguration", menuName = "Spirograph Pro/Configuration", order = 1)]
    public class SpirographConfiguration : ScriptableObject
    {
        [Header("Agent Limits")]
        [Tooltip("Minimum number of agents allowed")]
        public int minAgentCount = 1;
        
        [Tooltip("Maximum number of agents allowed")]
        public int maxAgentCount = 16;
        
        [Header("Motion Parameters")]
        [Tooltip("Minimum speed value")]
        public float minSpeed = 0f;
        
        [Tooltip("Maximum speed value")]
        public float maxSpeed = 750f;
        
        [Tooltip("Default speed for new agents")]
        public float defaultSpeed = 100f;
        
        [Tooltip("Minimum rotation speed")]
        public float minRotationSpeed = 0f;
        
        [Tooltip("Maximum rotation speed")]
        public float maxRotationSpeed = 1f;
        
        [Tooltip("Default rotation speed")]
        public float defaultRotationSpeed = 0.5f;
        
        [Tooltip("Minimum number of cycles")]
        public int minCycles = 1;
        
        [Tooltip("Maximum number of cycles")]
        public int maxCycles = 500;
        
        [Tooltip("Default number of cycles")]
        public int defaultCycles = 10;
        
        [Tooltip("Minimum pen distance multiplier")]
        public float minPenDistance = 0f;
        
        [Tooltip("Maximum pen distance multiplier")]
        public float maxPenDistance = 5f;
        
        [Tooltip("Default pen distance multiplier")]
        public float defaultPenDistance = 0.3f;
        
        [Header("Visual Parameters")]
        [Tooltip("Minimum line width")]
        public float minLineWidth = 0.01f;
        
        [Tooltip("Maximum line width")]
        public float maxLineWidth = 2f;
        
        [Tooltip("Default line width")]
        public float defaultLineWidth = 0.3f;
        
        [Tooltip("Minimum line brightness")]
        public float minLineBrightness = 0f;
        
        [Tooltip("Maximum line brightness")]
        public float maxLineBrightness = 1f;
        
        [Tooltip("Default line brightness")]
        public float defaultLineBrightness = 1f;
        
        [Header("UI Configuration")]
        [Tooltip("Main control panel width")]
        public float controlPanelWidth = 340f;
        
        [Tooltip("Agent panel width")]
        public float agentPanelWidth = 420f;
        
        [Tooltip("Agent panel height")]
        public float agentPanelHeight = 700f;
        
        [Tooltip("UI update frequency in Hz (10 = every 0.1s)")]
        public float uiUpdateFrequency = 10f;
        
        [Header("Performance")]
        [Tooltip("Stats update interval in seconds")]
        public float statsUpdateInterval = 0.1f;
        
        [Tooltip("Enable visual effects and animations")]
        public bool enableVisualEffects = true;
        
        [Tooltip("Animation speed multiplier")]
        public float animationSpeedMultiplier = 1.2f;
        
        [Header("Default Colors")]
        [Tooltip("Default agent color")]
        public Color defaultAgentColor = Color.cyan;
        
        [Tooltip("Default line color")]
        public Color defaultLineColor = Color.cyan;
        
        [Tooltip("UI accent color")]
        public Color uiAccentColor = new Color(0.4f, 0.7f, 1f, 1f);
        
        /// <summary>
        /// Singleton instance for easy access.
        /// </summary>
        private static SpirographConfiguration _instance;
        
        /// <summary>
        /// Get the configuration instance. Creates a default one if none exists.
        /// </summary>
        public static SpirographConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SpirographConfiguration>("SpirographConfiguration");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<SpirographConfiguration>();
                        Debug.LogWarning("[SpirographConfiguration] No configuration asset found in Resources. Using default values.");
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Validate configuration values on load.
        /// </summary>
        private void OnValidate()
        {
            // Ensure min/max values are sensible
            maxAgentCount = Mathf.Max(minAgentCount, maxAgentCount);
            maxSpeed = Mathf.Max(minSpeed, maxSpeed);
            maxRotationSpeed = Mathf.Max(minRotationSpeed, maxRotationSpeed);
            maxCycles = Mathf.Max(minCycles, maxCycles);
            maxPenDistance = Mathf.Max(minPenDistance, maxPenDistance);
            maxLineWidth = Mathf.Max(minLineWidth, maxLineWidth);
            maxLineBrightness = Mathf.Max(minLineBrightness, maxLineBrightness);
            
            // Clamp defaults to valid ranges
            defaultSpeed = Mathf.Clamp(defaultSpeed, minSpeed, maxSpeed);
            defaultRotationSpeed = Mathf.Clamp(defaultRotationSpeed, minRotationSpeed, maxRotationSpeed);
            defaultCycles = Mathf.Clamp(defaultCycles, minCycles, maxCycles);
            defaultPenDistance = Mathf.Clamp(defaultPenDistance, minPenDistance, maxPenDistance);
            defaultLineWidth = Mathf.Clamp(defaultLineWidth, minLineWidth, maxLineWidth);
            defaultLineBrightness = Mathf.Clamp(defaultLineBrightness, minLineBrightness, maxLineBrightness);
            
            // Ensure update frequency is reasonable
            uiUpdateFrequency = Mathf.Max(1f, uiUpdateFrequency);
            statsUpdateInterval = 1f / uiUpdateFrequency;
            
            animationSpeedMultiplier = Mathf.Max(0.1f, animationSpeedMultiplier);
        }
    }
}

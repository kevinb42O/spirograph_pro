namespace SpirographPro.Configuration
{
    /// <summary>
    /// Static constants used throughout the Spirograph system.
    /// Centralizes magic numbers to improve code maintainability.
    /// </summary>
    public static class SpirographConstants
    {
        #region Agent Constants
        
        /// <summary>Minimum number of agents.</summary>
        public const int MIN_AGENT_COUNT = 1;
        
        /// <summary>Maximum number of agents.</summary>
        public const int MAX_AGENT_COUNT = 16;
        
        /// <summary>Default rotor radius fallback value.</summary>
        public const float DEFAULT_ROTOR_RADIUS = 0.5f;
        
        /// <summary>Default agent sphere scale.</summary>
        public const float DEFAULT_AGENT_SCALE = 0.4f;
        
        /// <summary>Pen dot visual scale.</summary>
        public const float PEN_DOT_SCALE = 0.12f;
        
        /// <summary>Radius line width.</summary>
        public const float RADIUS_LINE_WIDTH = 0.03f;
        
        #endregion
        
        #region Trail Constants
        
        /// <summary>Default trail time (persistence).</summary>
        public const float DEFAULT_TRAIL_TIME = 1000f;
        
        /// <summary>Minimum vertex distance for trail renderer.</summary>
        public const float TRAIL_MIN_VERTEX_DISTANCE = 0.01f;
        
        /// <summary>Number of corner vertices for trail.</summary>
        public const int TRAIL_CORNER_VERTICES = 5;
        
        /// <summary>Number of cap vertices for trail.</summary>
        public const int TRAIL_CAP_VERTICES = 5;
        
        #endregion
        
        #region UI Constants
        
        /// <summary>Control panel width.</summary>
        public const float CONTROL_PANEL_WIDTH = 340f;
        
        /// <summary>Agent panel width.</summary>
        public const float AGENT_PANEL_WIDTH = 420f;
        
        /// <summary>Agent panel height.</summary>
        public const float AGENT_PANEL_HEIGHT = 700f;
        
        /// <summary>UI padding standard size.</summary>
        public const float UI_PADDING = 10f;
        
        /// <summary>UI margin between elements.</summary>
        public const float UI_MARGIN = 5f;
        
        /// <summary>Default button height.</summary>
        public const float BUTTON_HEIGHT = 40f;
        
        /// <summary>Default slider height.</summary>
        public const float SLIDER_HEIGHT = 70f;
        
        /// <summary>Agent card height in list.</summary>
        public const float AGENT_CARD_HEIGHT = 90f;
        
        /// <summary>Update frequency for UI stats (Hz).</summary>
        public const float UI_UPDATE_FREQUENCY = 10f;
        
        /// <summary>UI animation duration (seconds).</summary>
        public const float UI_ANIMATION_DURATION = 0.3f;
        
        #endregion
        
        #region Animation Constants
        
        /// <summary>Default animation speed multiplier.</summary>
        public const float DEFAULT_ANIMATION_SPEED = 1.2f;
        
        /// <summary>Spawn animation duration.</summary>
        public const float SPAWN_ANIMATION_DURATION = 0.3f;
        
        /// <summary>Pulse effect speed.</summary>
        public const float PULSE_EFFECT_SPEED = 1.5f;
        
        /// <summary>Hover scale multiplier.</summary>
        public const float HOVER_SCALE_MULTIPLIER = 1.1f;
        
        /// <summary>Press scale multiplier.</summary>
        public const float PRESS_SCALE_MULTIPLIER = 0.93f;
        
        #endregion
        
        #region Color Constants
        
        /// <summary>Emission intensity for glowing materials.</summary>
        public const float EMISSION_INTENSITY_LOW = 0.5f;
        
        /// <summary>Emission intensity for standard glow.</summary>
        public const float EMISSION_INTENSITY_MEDIUM = 1.5f;
        
        /// <summary>Emission intensity for bright glow.</summary>
        public const float EMISSION_INTENSITY_HIGH = 3f;
        
        /// <summary>Trail emission multiplier.</summary>
        public const float TRAIL_EMISSION_MULTIPLIER = 0.5f;
        
        /// <summary>Pen dot emission multiplier.</summary>
        public const float PEN_DOT_EMISSION_MULTIPLIER = 3f;
        
        #endregion
        
        #region Performance Constants
        
        /// <summary>Stats update interval (seconds).</summary>
        public const float STATS_UPDATE_INTERVAL = 0.1f;
        
        /// <summary>Scroll sensitivity for scroll views.</summary>
        public const float SCROLL_SENSITIVITY = 15f;
        
        /// <summary>Deceleration rate for scrolling.</summary>
        public const float SCROLL_DECELERATION = 0.135f;
        
        #endregion
        
        #region Layer Names
        
        /// <summary>UI layer name.</summary>
        public const string UI_LAYER = "UI";
        
        /// <summary>Default layer name.</summary>
        public const string DEFAULT_LAYER = "Default";
        
        #endregion
        
        #region Resource Paths
        
        /// <summary>Path to configuration asset in Resources.</summary>
        public const string CONFIG_RESOURCE_PATH = "SpirographConfiguration";
        
        /// <summary>Standard shader name.</summary>
        public const string STANDARD_SHADER = "Standard";
        
        /// <summary>Unlit shader name for particles.</summary>
        public const string PARTICLES_UNLIT_SHADER = "Particles/Standard Unlit";
        
        /// <summary>Legacy runtime font.</summary>
        public const string LEGACY_FONT = "LegacyRuntime.ttf";
        
        #endregion
        
        #region String Formats
        
        /// <summary>Float format for 1 decimal place.</summary>
        public const string FORMAT_F1 = "F1";
        
        /// <summary>Float format for 2 decimal places.</summary>
        public const string FORMAT_F2 = "F2";
        
        /// <summary>Percentage format.</summary>
        public const string FORMAT_P0 = "P0";
        
        #endregion
    }
}

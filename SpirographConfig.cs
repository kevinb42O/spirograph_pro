using UnityEngine;

namespace SpirographPro.Configuration
{
    /// <summary>
    /// Central configuration constants for the Spirograph system.
    /// Extracted from magic numbers throughout the codebase for maintainability.
    /// </summary>
    public static class SpirographConfig
    {
        // Performance
        public const int TRIG_LOOKUP_SIZE = 3600; // 0.1 degree precision
        public const int OBJECT_POOL_INITIAL_SIZE = 1000;
        public const int MAX_INTERPOLATION_POINTS = 50;
        public const float DEFAULT_CURVATURE_DELTA = 0.02f;
        
        // Trail Quality
        public const float DEFAULT_MAX_TRAIL_SEGMENT_LENGTH = 0.05f;
        public const float MIN_ADAPTIVE_SEGMENT_LENGTH = 0.2f; // Multiplier of max segment length
        public const float CURVATURE_THRESHOLD = 5f;
        public const int DEFAULT_ANTIALIASING_QUALITY = 3;
        public const int MIN_ANTIALIASING_QUALITY = 1;
        public const int MAX_ANTIALIASING_QUALITY = 5;
        
        // Visual Constants
        public const float DEFAULT_LINE_WIDTH = 0.3f;
        public const float DEFAULT_PEN_GLOW_INTENSITY = 5f;
        public const float DEFAULT_RADIUS_LINE_WIDTH = 0.05f;
        public const float PEN_DOT_SIZE = 0.15f;
        
        // Camera
        public const float DEFAULT_CAMERA_FOV = 60f;
        public const float CAMERA_DISTANCE_SCALE_DIVISOR = 20f;
        public const float CAMERA_SCALE_MIN = 0.5f;
        public const float CAMERA_SCALE_MAX = 2f;
        
        // UI
        public const float UI_ANIMATION_DURATION = 0.3f;
        public const float UI_SCALE_HIDDEN = 0.95f;
        public const float PERFORMANCE_UPDATE_INTERVAL = 0.5f; // Update twice per second
        
        // Pattern Generation
        public const int DEFAULT_NUM_SAMPLES_FOR_CURVATURE = 200;
        public const float DEFAULT_SPLINE_DELTA = 0.01f; // 1% spacing for Catmull-Rom
        
        // Speed Limits
        public const float MAX_SPEED = 250f;
        public const float MAX_CYCLES = 500f;
        public const float MAX_PEN_DISTANCE = 5f;
        public const float MAX_LINE_WIDTH = 2f;
        public const float MIN_LINE_WIDTH = 0.01f;
    }
}

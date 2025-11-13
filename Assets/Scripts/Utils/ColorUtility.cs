using UnityEngine;

namespace SpirographPro.Utils
{
    /// <summary>
    /// Utility class for color calculations and conversions.
    /// Consolidates common color operations to avoid duplication.
    /// </summary>
    public static class ColorUtility
    {
        /// <summary>
        /// Get a color from the rainbow spectrum based on hue value.
        /// </summary>
        /// <param name="hue">Hue value (0-1).</param>
        /// <param name="saturation">Saturation value (0-1). Default is 1.</param>
        /// <param name="value">Brightness value (0-1). Default is 1.</param>
        /// <returns>Color from the rainbow spectrum.</returns>
        public static Color GetRainbowColor(float hue, float saturation = 1f, float value = 1f)
        {
            return Color.HSVToRGB(hue, saturation, value);
        }
        
        /// <summary>
        /// Get an agent color based on the specified color mode.
        /// </summary>
        /// <param name="colorMode">The color distribution mode.</param>
        /// <param name="agentIndex">Index of the agent.</param>
        /// <param name="totalAgents">Total number of agents.</param>
        /// <param name="masterColor">Master/default color to use.</param>
        /// <returns>Color for the specified agent.</returns>
        public static Color GetAgentColor(AgentColorMode colorMode, int agentIndex, int totalAgents, Color masterColor)
        {
            switch (colorMode)
            {
                case AgentColorMode.Master:
                    return masterColor;
                    
                case AgentColorMode.Rainbow:
                    float hue = (float)agentIndex / Mathf.Max(1, totalAgents);
                    return GetRainbowColor(hue);
                    
                case AgentColorMode.Individual:
                    return GetPredefinedColor(agentIndex);
                    
                case AgentColorMode.Custom:
                default:
                    return masterColor;
            }
        }
        
        /// <summary>
        /// Get a predefined color from a palette.
        /// </summary>
        /// <param name="index">Index in the palette.</param>
        /// <returns>Predefined color.</returns>
        public static Color GetPredefinedColor(int index)
        {
            Color[] palette = new Color[]
            {
                Color.cyan,
                Color.magenta,
                Color.yellow,
                Color.red,
                Color.green,
                Color.blue,
                new Color(1f, 0.5f, 0f), // Orange
                new Color(0.5f, 0f, 1f), // Purple
                new Color(1f, 0.75f, 0.8f), // Pink
                new Color(0f, 1f, 0.5f), // Spring green
                new Color(1f, 1f, 0f), // Bright yellow
                new Color(0f, 0.5f, 1f), // Sky blue
                new Color(1f, 0f, 0.5f), // Hot pink
                new Color(0.5f, 1f, 0f), // Lime
                new Color(0.5f, 0f, 0.5f), // Dark purple
                Color.white
            };
            
            return palette[index % palette.Length];
        }
        
        /// <summary>
        /// Create an emissive color for glowing effects.
        /// </summary>
        /// <param name="baseColor">Base color.</param>
        /// <param name="intensity">Emission intensity multiplier.</param>
        /// <returns>Emissive color.</returns>
        public static Color CreateEmissiveColor(Color baseColor, float intensity = 2f)
        {
            return baseColor * intensity;
        }
        
        /// <summary>
        /// Lighten a color by a specified amount.
        /// </summary>
        /// <param name="color">Source color.</param>
        /// <param name="amount">Amount to lighten (0-1).</param>
        /// <returns>Lightened color.</returns>
        public static Color Lighten(Color color, float amount)
        {
            amount = Mathf.Clamp01(amount);
            return Color.Lerp(color, Color.white, amount);
        }
        
        /// <summary>
        /// Darken a color by a specified amount.
        /// </summary>
        /// <param name="color">Source color.</param>
        /// <param name="amount">Amount to darken (0-1).</param>
        /// <returns>Darkened color.</returns>
        public static Color Darken(Color color, float amount)
        {
            amount = Mathf.Clamp01(amount);
            return Color.Lerp(color, Color.black, amount);
        }
        
        /// <summary>
        /// Get a color with modified alpha.
        /// </summary>
        /// <param name="color">Source color.</param>
        /// <param name="alpha">New alpha value (0-1).</param>
        /// <returns>Color with new alpha.</returns>
        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
        
        /// <summary>
        /// Interpolate between multiple colors based on a normalized value.
        /// </summary>
        /// <param name="value">Normalized value (0-1).</param>
        /// <param name="colors">Array of colors to interpolate between.</param>
        /// <returns>Interpolated color.</returns>
        public static Color MultiColorLerp(float value, params Color[] colors)
        {
            if (colors == null || colors.Length == 0)
                return Color.white;
            
            if (colors.Length == 1)
                return colors[0];
            
            value = Mathf.Clamp01(value);
            float scaledValue = value * (colors.Length - 1);
            int index = Mathf.FloorToInt(scaledValue);
            float t = scaledValue - index;
            
            if (index >= colors.Length - 1)
                return colors[colors.Length - 1];
            
            return Color.Lerp(colors[index], colors[index + 1], t);
        }
    }
    
    /// <summary>
    /// Color modes for agent coloring.
    /// </summary>
    public enum AgentColorMode
    {
        /// <summary>All agents use the master color.</summary>
        Master,
        
        /// <summary>Agents are distributed across the rainbow spectrum.</summary>
        Rainbow,
        
        /// <summary>Each agent gets a predefined color from a palette.</summary>
        Individual,
        
        /// <summary>Agents can have custom colors set individually.</summary>
        Custom
    }
}

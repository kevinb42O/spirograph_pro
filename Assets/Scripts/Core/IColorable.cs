using UnityEngine;

namespace SpirographPro.Core
{
    /// <summary>
    /// Interface for objects that support color customization.
    /// </summary>
    public interface IColorable
    {
        /// <summary>
        /// Get or set the primary color of this object.
        /// </summary>
        Color PrimaryColor { get; set; }
        
        /// <summary>
        /// Update the color with optional transition.
        /// </summary>
        /// <param name="newColor">The target color.</param>
        /// <param name="smooth">Whether to smoothly transition to the new color.</param>
        void UpdateColor(Color newColor, bool smooth = false);
        
        /// <summary>
        /// Reset to default color.
        /// </summary>
        void ResetToDefaultColor();
    }
}

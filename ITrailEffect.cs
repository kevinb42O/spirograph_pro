using UnityEngine;

namespace SpirographPro.Effects
{
    /// <summary>
    /// Interface for trail visual effects.
    /// Allows for extensible effect system following the Strategy pattern.
    /// </summary>
    public interface ITrailEffect
    {
        /// <summary>
        /// Applies the visual effect to the specified trail renderer.
        /// </summary>
        /// <param name="trail">The TrailRenderer to apply the effect to</param>
        /// <param name="effectTimer">Current effect timer for animated effects</param>
        /// <param name="intensity">Effect intensity multiplier</param>
        /// <param name="speed">Effect animation speed</param>
        /// <param name="lineWidth">Base line width</param>
        /// <param name="brightness">Line brightness/alpha value</param>
        void ApplyEffect(TrailRenderer trail, float effectTimer, float intensity, float speed, float lineWidth, float brightness);
        
        /// <summary>
        /// Gets the human-readable name of this effect.
        /// </summary>
        string GetEffectName();
    }
}

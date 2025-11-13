using UnityEngine;

namespace SpirographPro.Core
{
    /// <summary>
    /// Interface for objects that follow a predefined path.
    /// </summary>
    public interface IPathFollower
    {
        /// <summary>
        /// Current distance traveled along the path.
        /// </summary>
        float CurrentDistance { get; }
        
        /// <summary>
        /// Total length of the path being followed.
        /// </summary>
        float TotalPathLength { get; }
        
        /// <summary>
        /// Current cycle/lap number.
        /// </summary>
        int CurrentCycle { get; }
        
        /// <summary>
        /// Speed multiplier for movement along the path.
        /// </summary>
        float SpeedMultiplier { get; set; }
        
        /// <summary>
        /// Get a point on the path at the specified distance.
        /// </summary>
        /// <param name="distance">Distance along the path.</param>
        /// <returns>World position at that distance.</returns>
        Vector3 GetPointOnPath(float distance);
        
        /// <summary>
        /// Update the path being followed.
        /// </summary>
        /// <param name="pathPoints">Array of transforms defining the path.</param>
        void UpdatePath(Transform[] pathPoints);
    }
}

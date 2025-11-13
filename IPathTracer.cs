using UnityEngine;

namespace SpirographPro.Tracing
{
    /// <summary>
    /// Interface for path tracing algorithms.
    /// Allows different strategies for connecting path points (Convex Hull, Nearest Neighbor, etc.)
    /// Follows the Strategy pattern for algorithm selection.
    /// </summary>
    public interface IPathTracer
    {
        /// <summary>
        /// Traces a path through the given points and returns them in traversal order.
        /// </summary>
        /// <param name="points">The points to trace through</param>
        /// <returns>Array of transforms in traversal order</returns>
        Transform[] TracePath(System.Collections.Generic.List<Transform> points);
        
        /// <summary>
        /// Gets the human-readable name of this tracing algorithm.
        /// </summary>
        string GetAlgorithmName();
    }
}

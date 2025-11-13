using UnityEngine;

namespace SpirographPro.Core
{
    /// <summary>
    /// Interface for agents that can draw patterns.
    /// Defines the contract for agent behavior and control.
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Unique identifier for this agent.
        /// </summary>
        int AgentIndex { get; set; }
        
        /// <summary>
        /// Human-readable name for this agent.
        /// </summary>
        string AgentName { get; set; }
        
        /// <summary>
        /// Current status of the agent.
        /// </summary>
        AgentStatus Status { get; }
        
        /// <summary>
        /// Current position in world space.
        /// </summary>
        Vector3 CurrentPosition { get; }
        
        /// <summary>
        /// Color used for this agent's visual elements.
        /// </summary>
        Color AgentColor { get; set; }
        
        /// <summary>
        /// Start the agent's drawing process.
        /// </summary>
        void StartDrawing();
        
        /// <summary>
        /// Pause the agent's drawing.
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume the agent's drawing after pause.
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Reset the agent to its starting state.
        /// </summary>
        void ResetAgent();
        
        /// <summary>
        /// Update the agent's color.
        /// </summary>
        /// <param name="newColor">The new color to apply.</param>
        void SetColor(Color newColor);
    }
    
    /// <summary>
    /// Possible states for an agent.
    /// </summary>
    public enum AgentStatus
    {
        /// <summary>Agent has not started yet.</summary>
        Idle,
        
        /// <summary>Agent is actively drawing.</summary>
        Active,
        
        /// <summary>Agent is temporarily paused.</summary>
        Paused,
        
        /// <summary>Agent has completed its task.</summary>
        Completed
    }
}

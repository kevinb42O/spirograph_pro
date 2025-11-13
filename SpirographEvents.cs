using UnityEngine;

namespace SpirographPro.Events
{
    /// <summary>
    /// Central event system for Spirograph components.
    /// Implements Observer pattern to reduce coupling between systems.
    /// Replaces direct method calls and SendMessage with type-safe events.
    /// </summary>
    public static class SpirographEvents
    {
        // Path events
        public delegate void PathChangedHandler();
        public static event PathChangedHandler OnPathChanged;
        
        public delegate void PathResetHandler();
        public static event PathResetHandler OnPathReset;
        
        // Color events
        public delegate void ColorChangedHandler(Color newColor);
        public static event ColorChangedHandler OnColorChanged;
        
        // Playback events
        public delegate void PlaybackStateChangedHandler(bool isPaused);
        public static event PlaybackStateChangedHandler OnPlaybackStateChanged;
        
        public delegate void CycleCompletedHandler(int cycleNumber);
        public static event CycleCompletedHandler OnCycleCompleted;
        
        // Effect events
        public delegate void EffectChangedHandler(string effectName);
        public static event EffectChangedHandler OnEffectChanged;
        
        // Visibility events
        public delegate void VisibilityChangedHandler(bool isVisible);
        public static event VisibilityChangedHandler OnVisibilityChanged;
        
        /// <summary>
        /// Notifies listeners that the path has changed and needs recaching.
        /// </summary>
        public static void NotifyPathChanged()
        {
            OnPathChanged?.Invoke();
        }
        
        /// <summary>
        /// Notifies listeners that the path has been reset.
        /// </summary>
        public static void NotifyPathReset()
        {
            OnPathReset?.Invoke();
        }
        
        /// <summary>
        /// Notifies listeners that the line color has changed.
        /// </summary>
        public static void NotifyColorChanged(Color newColor)
        {
            OnColorChanged?.Invoke(newColor);
        }
        
        /// <summary>
        /// Notifies listeners that playback state (paused/playing) has changed.
        /// </summary>
        public static void NotifyPlaybackStateChanged(bool isPaused)
        {
            OnPlaybackStateChanged?.Invoke(isPaused);
        }
        
        /// <summary>
        /// Notifies listeners that a cycle has been completed.
        /// </summary>
        public static void NotifyCycleCompleted(int cycleNumber)
        {
            OnCycleCompleted?.Invoke(cycleNumber);
        }
        
        /// <summary>
        /// Notifies listeners that the visual effect has changed.
        /// </summary>
        public static void NotifyEffectChanged(string effectName)
        {
            OnEffectChanged?.Invoke(effectName);
        }
        
        /// <summary>
        /// Notifies listeners that visibility has been toggled.
        /// </summary>
        public static void NotifyVisibilityChanged(bool isVisible)
        {
            OnVisibilityChanged?.Invoke(isVisible);
        }
        
        /// <summary>
        /// Clears all event subscriptions. Call this when changing scenes.
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPathChanged = null;
            OnPathReset = null;
            OnColorChanged = null;
            OnPlaybackStateChanged = null;
            OnCycleCompleted = null;
            OnEffectChanged = null;
            OnVisibilityChanged = null;
        }
    }
}

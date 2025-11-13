using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Camera path system with waypoints and smooth interpolation
/// Supports time-based and trigger-based movement with Bezier curve paths
/// </summary>
public class CameraPath : MonoBehaviour
{
    [System.Serializable]
    public class Waypoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fov = 60f;
        public float arrivalTime = 1f; // Time to reach this waypoint from previous
        public Vector3 controlPoint1; // Bezier control point 1 (for smooth curves)
        public Vector3 controlPoint2; // Bezier control point 2
        public bool useControlPoints = false; // Enable Bezier curve
        
        public Waypoint(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
            // Auto-generate control points for smooth curves
            controlPoint1 = pos;
            controlPoint2 = pos;
        }
    }
    
    [Header("Waypoints")]
    public List<Waypoint> waypoints = new List<Waypoint>();
    
    [Header("Playback Settings")]
    public bool autoPlay = false;
    public bool loop = false;
    public float playbackSpeed = 1f;
    
    [Header("Interpolation")]
    public AnimationCurve smoothingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private int currentWaypointIndex = 0;
    private float currentTime = 0f;
    private bool isPlaying = false;
    private Camera targetCamera;
    
    void Start()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }
        
        if (autoPlay && waypoints.Count > 1)
        {
            Play();
        }
    }
    
    void Update()
    {
        if (isPlaying && waypoints.Count > 1)
        {
            UpdatePathMovement();
        }
    }
    
    /// <summary>
    /// Start playing the camera path
    /// </summary>
    public void Play()
    {
        if (waypoints.Count < 2)
        {
            Debug.LogWarning("CameraPath: Need at least 2 waypoints to play");
            return;
        }
        
        isPlaying = true;
        currentWaypointIndex = 0;
        currentTime = 0f;
        Debug.Log("CameraPath: Started playing path");
    }
    
    /// <summary>
    /// Stop playing the camera path
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        Debug.Log("CameraPath: Stopped playing path");
    }
    
    /// <summary>
    /// Pause the camera path
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }
    
    /// <summary>
    /// Resume the camera path
    /// </summary>
    public void Resume()
    {
        isPlaying = true;
    }
    
    /// <summary>
    /// Add a waypoint at the current camera position
    /// </summary>
    public void AddWaypointAtCurrentPosition()
    {
        if (targetCamera == null) return;
        
        Waypoint newWaypoint = new Waypoint(
            targetCamera.transform.position,
            targetCamera.transform.rotation
        );
        newWaypoint.fov = targetCamera.fieldOfView;
        
        // Auto-generate control points for smooth curves
        if (waypoints.Count > 0)
        {
            Waypoint lastWaypoint = waypoints[waypoints.Count - 1];
            Vector3 direction = newWaypoint.position - lastWaypoint.position;
            float distance = direction.magnitude;
            
            // Place control points at 1/3 and 2/3 of the distance
            lastWaypoint.controlPoint2 = lastWaypoint.position + direction.normalized * (distance * 0.33f);
            newWaypoint.controlPoint1 = newWaypoint.position - direction.normalized * (distance * 0.33f);
        }
        
        waypoints.Add(newWaypoint);
        Debug.Log($"CameraPath: Added waypoint {waypoints.Count} at {newWaypoint.position}");
    }
    
    /// <summary>
    /// Remove the last waypoint
    /// </summary>
    public void RemoveLastWaypoint()
    {
        if (waypoints.Count > 0)
        {
            waypoints.RemoveAt(waypoints.Count - 1);
            Debug.Log($"CameraPath: Removed last waypoint. Count: {waypoints.Count}");
        }
    }
    
    /// <summary>
    /// Clear all waypoints
    /// </summary>
    public void ClearWaypoints()
    {
        waypoints.Clear();
        currentWaypointIndex = 0;
        currentTime = 0f;
        Debug.Log("CameraPath: Cleared all waypoints");
    }
    
    /// <summary>
    /// Update camera movement along the path
    /// </summary>
    void UpdatePathMovement()
    {
        if (currentWaypointIndex >= waypoints.Count - 1)
        {
            // Reached end of path
            if (loop)
            {
                currentWaypointIndex = 0;
                currentTime = 0f;
            }
            else
            {
                Stop();
            }
            return;
        }
        
        Waypoint startWaypoint = waypoints[currentWaypointIndex];
        Waypoint endWaypoint = waypoints[currentWaypointIndex + 1];
        
        // Update time
        currentTime += Time.deltaTime * playbackSpeed;
        float t = Mathf.Clamp01(currentTime / endWaypoint.arrivalTime);
        
        // Apply smoothing curve
        float smoothT = smoothingCurve.Evaluate(t);
        
        // Interpolate position
        Vector3 newPosition;
        if (endWaypoint.useControlPoints)
        {
            // Bezier curve interpolation
            newPosition = CalculateBezierPoint(
                smoothT,
                startWaypoint.position,
                startWaypoint.controlPoint2,
                endWaypoint.controlPoint1,
                endWaypoint.position
            );
        }
        else
        {
            // Linear interpolation
            newPosition = Vector3.Lerp(startWaypoint.position, endWaypoint.position, smoothT);
        }
        
        // Interpolate rotation
        Quaternion newRotation = Quaternion.Slerp(startWaypoint.rotation, endWaypoint.rotation, smoothT);
        
        // Interpolate FOV
        float newFOV = Mathf.Lerp(startWaypoint.fov, endWaypoint.fov, smoothT);
        
        // Apply to camera
        if (targetCamera != null)
        {
            targetCamera.transform.position = newPosition;
            targetCamera.transform.rotation = newRotation;
            targetCamera.fieldOfView = newFOV;
        }
        
        // Move to next waypoint
        if (t >= 1f)
        {
            currentWaypointIndex++;
            currentTime = 0f;
            
            if (currentWaypointIndex >= waypoints.Count - 1 && !loop)
            {
                Stop();
            }
        }
    }
    
    /// <summary>
    /// Calculate a point on a cubic Bezier curve
    /// </summary>
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;
        
        return point;
    }
    
    /// <summary>
    /// Jump to a specific waypoint instantly
    /// </summary>
    public void JumpToWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Count)
        {
            Debug.LogWarning($"CameraPath: Waypoint index {index} out of range");
            return;
        }
        
        Waypoint waypoint = waypoints[index];
        if (targetCamera != null)
        {
            targetCamera.transform.position = waypoint.position;
            targetCamera.transform.rotation = waypoint.rotation;
            targetCamera.fieldOfView = waypoint.fov;
        }
        
        currentWaypointIndex = index;
        currentTime = 0f;
        Debug.Log($"CameraPath: Jumped to waypoint {index}");
    }
    
    /// <summary>
    /// Get the total duration of the path
    /// </summary>
    public float GetTotalDuration()
    {
        float total = 0f;
        foreach (var waypoint in waypoints)
        {
            total += waypoint.arrivalTime;
        }
        return total;
    }
    
    /// <summary>
    /// Draw gizmos for visualization in editor
    /// </summary>
    void OnDrawGizmos()
    {
        if (waypoints.Count < 2) return;
        
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            Waypoint waypoint = waypoints[i];
            
            // Draw waypoint sphere
            Gizmos.DrawWireSphere(waypoint.position, 0.3f);
            
            // Draw line to next waypoint
            if (i < waypoints.Count - 1)
            {
                Waypoint nextWaypoint = waypoints[i + 1];
                
                if (nextWaypoint.useControlPoints)
                {
                    // Draw Bezier curve
                    DrawBezierGizmo(
                        waypoint.position,
                        waypoint.controlPoint2,
                        nextWaypoint.controlPoint1,
                        nextWaypoint.position
                    );
                    
                    // Draw control points
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(waypoint.position, waypoint.controlPoint2);
                    Gizmos.DrawLine(nextWaypoint.position, nextWaypoint.controlPoint1);
                    Gizmos.DrawWireSphere(waypoint.controlPoint2, 0.15f);
                    Gizmos.DrawWireSphere(nextWaypoint.controlPoint1, 0.15f);
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    // Draw straight line
                    Gizmos.DrawLine(waypoint.position, nextWaypoint.position);
                }
            }
            
            // Draw direction indicator
            Gizmos.color = Color.red;
            Vector3 forward = waypoint.rotation * Vector3.forward;
            Gizmos.DrawRay(waypoint.position, forward * 0.5f);
            Gizmos.color = Color.yellow;
        }
    }
    
    /// <summary>
    /// Draw a Bezier curve with gizmos
    /// </summary>
    void DrawBezierGizmo(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 prevPoint = p0;
        int segments = 20;
        
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = CalculateBezierPoint(t, p0, p1, p2, p3);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}

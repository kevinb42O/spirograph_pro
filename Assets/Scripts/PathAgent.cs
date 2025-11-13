using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Individual agent that follows a shared path state.
/// Multiple agents can draw simultaneously, each maintaining their own position.
/// This is similar to SpirographRoller but reads from SharedPathState instead of direct control.
/// </summary>
public class PathAgent : MonoBehaviour
{
    [Header("Agent Identity")]
    public int agentIndex = 0;
    public string agentName = "Agent 0";
    
    [Header("Shared State Reference")]
    [Tooltip("The shared state this agent reads from")]
    public SharedPathState sharedState;
    
    [Header("Agent-Specific Settings")]
    [Tooltip("Starting position on the path (0-1, where 0 is start, 1 is end)")]
    [Range(0f, 1f)]
    public float startPositionPercent = 0f;
    
    [Tooltip("Speed multiplier for competitive mode (1.0 = normal)")]
    [Range(0.5f, 2f)]
    public float speedMultiplier = 1f;
    
    [Tooltip("Individual pause state (doesn't affect other agents)")]
    public bool isPaused = false;
    
    [Header("Per-Agent Motion Control")]
    [Tooltip("Individual agent speed (0-750)")]
    [Range(0f, 750f)]
    public float agentSpeed = 0f;
    
    [Tooltip("Individual rotation speed (0-1)")]
    [Range(0f, 1f)]
    public float agentRotationSpeed = 0.5f;
    
    [Tooltip("Individual cycles (1-500)")]
    [Range(1, 500)]
    public int agentCycles = 10;
    
    [Tooltip("Individual pen distance/rotor radius (0-5x)")]
    [Range(0f, 5f)]
    public float agentPenDistance = 0.3f;
    
    [Header("Per-Agent Visual Control")]
    [Tooltip("Individual line width")]
    [Range(0.01f, 2f)]
    public float agentLineWidth = 0.3f;
    
    [Tooltip("Individual line brightness")]
    [Range(0f, 1f)]
    public float agentLineBrightness = 1f;
    
    [Tooltip("Whether this agent uses per-agent settings (true) or master settings (false)")]
    public bool useIndividualSettings = false;
    
    [Header("Agent State")]
    public AgentStatus status = AgentStatus.Idle;
    public float currentDistance = 0f;
    public int currentCycle = 0;
    public Vector3 currentPosition = Vector3.zero;
    
    [Header("Agent Color")]
    public Color agentColor = Color.cyan;
    
    [Header("Progress Tracking")]
    [Tooltip("Segment this agent is responsible for (0-1 range)")]
    public float segmentStart = 0f;
    public float segmentEnd = 1f;
    
    [Tooltip("Progress within assigned segment (0-1)")]
    public float segmentProgress = 0f;
    
    [Tooltip("Total distance traveled by this agent")]
    public float totalDistanceTraveled = 0f;
    
    [Tooltip("Time elapsed since agent started")]
    public float elapsedTime = 0f;
    
    public enum AgentStatus
    {
        Idle,       // Not started yet
        Active,     // Currently drawing
        Paused,     // Temporarily stopped
        Completed   // Finished all cycles
    }
    
    // Internal state
    private List<Vector3> staticPathCache = new List<Vector3>();
    public float totalPathLength = 0f; // Made public for UI access
    private float currentAngle = 0f;
    private Vector3 previousPosition = Vector3.zero;
    private GameObject penObject;
    private GameObject penDotVisual;
    private LineRenderer radiusLine;
    private TrailRenderer trailRenderer;
    private float baseRotorRadius;
    private Vector3 startWorldPosition;
    
    // Performance: Cached components and values to avoid GetComponent calls
    private Material trailMaterial;
    private Material penDotMaterial;
    private Material radiusLineMaterial;
    
    // Performance: Pre-allocated for line width updates
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    
    // Performance: Cached segment lengths for faster path lookup
    private List<float> segmentLengths = new List<float>();
    private List<float> cumulativeLengths = new List<float>();
    
    // Performance: LOD system for trail quality based on camera distance
    [Header("Performance - LOD System")]
    [Tooltip("Enable LOD (Level of Detail) system for trails")]
    public bool enableTrailLOD = true;
    [Tooltip("Distance thresholds for LOD levels (near, medium, far)")]
    public float[] lodDistances = new float[] { 10f, 25f, 50f };
    [Tooltip("Trail time multipliers for each LOD level (1.0 = full quality)")]
    public float[] lodTrailTimes = new float[] { 1.0f, 0.6f, 0.3f };
    [Tooltip("Min vertex distance multipliers for each LOD level")]
    public float[] lodMinVertexDistances = new float[] { 0.01f, 0.05f, 0.15f };
    private int currentLODLevel = 0;
    private float lodUpdateTimer = 0f;
    private const float LOD_UPDATE_INTERVAL = 0.5f; // Update LOD every 0.5s
    
    void Start()
    {
        // Validate shared state
        if (sharedState == null)
        {
            Debug.LogError($"[PathAgent] Agent {agentIndex}: No SharedPathState assigned!");
            enabled = false;
            return;
        }
        
        if (sharedState.pathPoints == null || sharedState.pathPoints.Length == 0)
        {
            Debug.LogError($"[PathAgent] Agent {agentIndex}: No path points in SharedPathState!");
            enabled = false;
            return;
        }
        
        // Cache the path
        CacheStaticPath();
        
        // Calculate total path length
        CalculateTotalPathLength();
        
        // Set starting position based on percentage
        currentDistance = startPositionPercent * totalPathLength;
        
        // Get rotor radius
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            baseRotorRadius = renderer.bounds.extents.x;
        }
        else
        {
            baseRotorRadius = 0.5f; // Default fallback
        }
        
        // Create pen object
        CreatePenObject();
        
        // Create visual indicators
        CreatePenDotVisual();
        CreateRadiusLine();
        
        // Create trail renderer
        CreateTrailRenderer();
        
        // Set initial position
        startWorldPosition = GetPointOnPath(currentDistance);
        transform.position = startWorldPosition;
        previousPosition = startWorldPosition;
        currentPosition = startWorldPosition;
        
        // Set initial status
        status = AgentStatus.Idle;
        
        Debug.Log($"[PathAgent] Agent {agentIndex} initialized at {startPositionPercent:P0} of path");
    }
    
    void Update()
    {
        // Null checks for safety
        if (sharedState == null)
        {
            Debug.LogWarning($"[PathAgent] Agent {agentIndex}: SharedPathState is null!");
            return;
        }
        
        // Re-cache if path cache is empty or path changed
        if (staticPathCache.Count == 0)
        {
            Debug.LogWarning($"[PathAgent] Agent {agentIndex}: Path cache is empty! Recaching...");
            CacheStaticPath();
            CalculateTotalPathLength();
            
            if (staticPathCache.Count == 0)
            {
                Debug.LogError($"[PathAgent] Agent {agentIndex}: Still no path points after recache!");
                return;
            }
        }
        
        // Check if we should be drawing
        if (status == AgentStatus.Idle || status == AgentStatus.Completed)
        {
            return;
        }
        
        if (isPaused)
        {
            if (status == AgentStatus.Active)
            {
                status = AgentStatus.Paused;
            }
            return;
        }
        
        // Update elapsed time
        elapsedTime += Time.deltaTime;
        
        // Performance: Update LOD based on camera distance (low frequency)
        if (enableTrailLOD)
        {
            lodUpdateTimer += Time.deltaTime;
            if (lodUpdateTimer >= LOD_UPDATE_INTERVAL)
            {
                lodUpdateTimer = 0f;
                UpdateTrailLOD();
            }
        }
        
        // Read motion parameters - use per-agent settings if this agent has individual control
        float speed, rotationSpeed, penDistance;
        int targetCycles;
        
        if (useIndividualSettings)
        {
            // Use this agent's individual settings
            speed = agentSpeed;
            rotationSpeed = agentRotationSpeed;
            penDistance = agentPenDistance;
            targetCycles = agentCycles;
        }
        else
        {
            // Use master settings from shared state
            speed = sharedState.masterSpeed * speedMultiplier;
            rotationSpeed = sharedState.masterRotationSpeed;
            penDistance = sharedState.masterPenDistance;
            targetCycles = sharedState.masterCycles;
        }
        
        // Calculate movement for this frame
        float frameDistance = speed * Time.deltaTime;
        currentDistance += frameDistance;
        totalDistanceTraveled += frameDistance;
        
        // Handle cycle completion
        if (currentDistance >= totalPathLength)
        {
            currentCycle++;
            currentDistance = currentDistance % totalPathLength;
            
            if (currentCycle >= targetCycles)
            {
                status = AgentStatus.Completed;
                currentDistance = 0f;
                Debug.Log($"[PathAgent] Agent {agentIndex} completed {targetCycles} cycles!");
                return;
            }
        }
        
        // Update segment progress
        float segmentLength = (segmentEnd - segmentStart) * totalPathLength;
        float distanceInSegment = currentDistance - (segmentStart * totalPathLength);
        if (segmentLength > 0)
        {
            segmentProgress = Mathf.Clamp01(distanceInSegment / segmentLength);
        }
        
        // Get new position on path
        Vector3 newPosition = GetPointOnPath(currentDistance);
        
        // Update rotation
        float angleIncrement = rotationSpeed * Time.deltaTime * 360f;
        currentAngle -= angleIncrement;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        
        // Move to new position
        transform.position = newPosition;
        currentPosition = newPosition;
        
        // Update pen offset
        UpdatePenOffset(penDistance);
        
        // Update trail position
        if (trailRenderer != null)
        {
            trailRenderer.transform.position = penObject.transform.position;
        }
        
        // Update visual indicators
        UpdateRadiusLine();
        
        previousPosition = newPosition;
    }
    
    void CacheStaticPath()
    {
        staticPathCache.Clear();
        foreach (Transform point in sharedState.pathPoints)
        {
            if (point != null)
            {
                staticPathCache.Add(point.position);
            }
        }
    }
    
    void CalculateTotalPathLength()
    {
        // Performance: Pre-calculate and cache all segment lengths and cumulative distances
        segmentLengths.Clear();
        cumulativeLengths.Clear();
        totalPathLength = 0f;
        
        for (int i = 1; i < staticPathCache.Count; i++)
        {
            float segmentLength = Vector3.Distance(staticPathCache[i - 1], staticPathCache[i]);
            segmentLengths.Add(segmentLength);
            totalPathLength += segmentLength;
            cumulativeLengths.Add(totalPathLength);
        }
        
        // Add closing segment
        if (staticPathCache.Count > 0)
        {
            float closingLength = Vector3.Distance(staticPathCache[staticPathCache.Count - 1], staticPathCache[0]);
            segmentLengths.Add(closingLength);
            totalPathLength += closingLength;
            cumulativeLengths.Add(totalPathLength);
        }
    }
    
    Vector3 GetPointOnPath(float distance)
    {
        if (staticPathCache.Count == 0) return Vector3.zero;
        
        // Performance: Use cached segment lengths instead of recalculating distances
        // Binary search for the right segment (O(log n) instead of O(n))
        int segmentIndex = 0;
        if (cumulativeLengths.Count > 0)
        {
            // Find segment using binary search
            int left = 0;
            int right = cumulativeLengths.Count - 1;
            
            while (left < right)
            {
                int mid = (left + right) / 2;
                if (cumulativeLengths[mid] < distance)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid;
                }
            }
            segmentIndex = left;
        }
        
        // Calculate position within segment
        float prevCumulativeLength = segmentIndex > 0 ? cumulativeLengths[segmentIndex - 1] : 0f;
        float segmentLength = segmentIndex < segmentLengths.Count ? segmentLengths[segmentIndex] : 0f;
        
        if (segmentLength > 0f)
        {
            float t = (distance - prevCumulativeLength) / segmentLength;
            
            // Get start and end points for this segment
            if (segmentIndex < staticPathCache.Count - 1)
            {
                return Vector3.Lerp(staticPathCache[segmentIndex], staticPathCache[segmentIndex + 1], t);
            }
            else if (segmentIndex == staticPathCache.Count - 1)
            {
                // Wrap-around segment
                return Vector3.Lerp(staticPathCache[staticPathCache.Count - 1], staticPathCache[0], t);
            }
        }
        
        return staticPathCache.Count > 0 ? staticPathCache[0] : Vector3.zero;
    }
    
    void CreatePenObject()
    {
        penObject = new GameObject("Pen");
        penObject.transform.SetParent(transform);
        penObject.transform.localPosition = Vector3.zero;
    }
    
    void UpdatePenOffset(float penDistance)
    {
        if (penObject != null)
        {
            float actualDistance = penDistance * baseRotorRadius;
            penObject.transform.localPosition = new Vector3(actualDistance, 0, 0);
        }
    }
    
    void CreatePenDotVisual()
    {
        if (penObject == null) return;
        
        penDotVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        penDotVisual.name = $"PenDot_Agent{agentIndex}";
        penDotVisual.transform.SetParent(penObject.transform);
        penDotVisual.transform.localPosition = Vector3.zero;
        penDotVisual.transform.localScale = Vector3.one * 0.12f;
        
        // Remove collider
        Collider collider = penDotVisual.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        
        // Create glowing material
        Renderer renderer = penDotVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = agentColor;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor(EmissionColorID, agentColor * 3f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            renderer.material = mat;
            penDotMaterial = mat; // Performance: Cache material reference
        }
    }
    
    void CreateRadiusLine()
    {
        GameObject lineObj = new GameObject($"RadiusLine_Agent{agentIndex}");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        
        radiusLine = lineObj.AddComponent<LineRenderer>();
        radiusLine.positionCount = 2;
        radiusLine.startWidth = 0.03f;
        radiusLine.endWidth = 0.03f;
        radiusLine.useWorldSpace = false;
        
        Material lineMat = new Material(Shader.Find("Standard"));
        lineMat.color = agentColor;
        lineMat.SetFloat("_Metallic", 0f);
        lineMat.SetFloat("_Glossiness", 0.8f);
        lineMat.EnableKeyword("_EMISSION");
        lineMat.SetColor(EmissionColorID, agentColor * 1.5f);
        lineMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        radiusLine.material = lineMat;
        radiusLineMaterial = lineMat; // Performance: Cache material reference
    }
    
    void UpdateRadiusLine()
    {
        if (radiusLine != null && penObject != null)
        {
            radiusLine.SetPosition(0, Vector3.zero);
            radiusLine.SetPosition(1, penObject.transform.localPosition);
        }
    }
    
    void CreateTrailRenderer()
    {
        GameObject trailObj = new GameObject($"Trail_Agent{agentIndex}");
        trailObj.transform.position = penObject != null ? penObject.transform.position : transform.position;
        
        trailRenderer = trailObj.AddComponent<TrailRenderer>();
        trailRenderer.time = 1000f;
        trailRenderer.startWidth = sharedState.masterLineWidth;
        trailRenderer.endWidth = sharedState.masterLineWidth;
        trailRenderer.minVertexDistance = 0.01f;
        trailRenderer.numCornerVertices = 5;
        trailRenderer.numCapVertices = 5;
        
        // Create material
        Material trailMat = new Material(Shader.Find("Particles/Standard Unlit"));
        trailMat.color = agentColor;
        if (trailMat.HasProperty(EmissionColorID))
        {
            trailMat.EnableKeyword("_EMISSION");
            trailMat.SetColor(EmissionColorID, agentColor * 0.5f);
        }
        trailRenderer.material = trailMat;
        trailMaterial = trailMat; // Performance: Cache material reference
    }
    
    /// <summary>
    /// Start this agent's drawing
    /// </summary>
    public void StartDrawing()
    {
        if (status == AgentStatus.Idle || status == AgentStatus.Paused)
        {
            status = AgentStatus.Active;
            isPaused = false;
            Debug.Log($"[PathAgent] Agent {agentIndex} started drawing");
        }
    }
    
    /// <summary>
    /// Pause this agent
    /// </summary>
    public void Pause()
    {
        if (status == AgentStatus.Active)
        {
            isPaused = true;
            status = AgentStatus.Paused;
            Debug.Log($"[PathAgent] Agent {agentIndex} paused");
        }
    }
    
    /// <summary>
    /// Resume this agent
    /// </summary>
    public void Resume()
    {
        if (status == AgentStatus.Paused)
        {
            isPaused = false;
            status = AgentStatus.Active;
            Debug.Log($"[PathAgent] Agent {agentIndex} resumed");
        }
    }
    
    /// <summary>
    /// Reset this agent to starting position
    /// </summary>
    public void ResetAgent()
    {
        currentDistance = startPositionPercent * totalPathLength;
        currentCycle = 0;
        currentAngle = 0f;
        elapsedTime = 0f;
        totalDistanceTraveled = 0f;
        segmentProgress = 0f;
        status = AgentStatus.Idle;
        isPaused = false;
        
        transform.position = GetPointOnPath(currentDistance);
        transform.rotation = Quaternion.identity;
        
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
        
        Debug.Log($"[PathAgent] Agent {agentIndex} reset");
    }
    
    /// <summary>
    /// Update agent color (also updates visual elements)
    /// Performance: Uses cached material references and shader property IDs
    /// </summary>
    public void SetColor(Color newColor)
    {
        agentColor = newColor;
        
        // Update pen dot - Performance: Use cached material
        if (penDotMaterial != null)
        {
            penDotMaterial.color = newColor;
            penDotMaterial.SetColor(EmissionColorID, newColor * 3f);
        }
        
        // Update radius line - Performance: Use cached material
        if (radiusLineMaterial != null)
        {
            radiusLineMaterial.color = newColor;
            radiusLineMaterial.SetColor(EmissionColorID, newColor * 1.5f);
        }
        
        // Update trail - Performance: Use cached material
        if (trailMaterial != null)
        {
            trailMaterial.color = newColor;
            if (trailMaterial.HasProperty(EmissionColorID))
            {
                trailMaterial.SetColor(EmissionColorID, newColor * 0.5f);
            }
        }
    }
    
    /// <summary>
    /// Update trail width from shared state
    /// Performance: Direct property assignment, no material updates
    /// </summary>
    public void UpdateLineWidth(float width)
    {
        if (trailRenderer != null)
        {
            trailRenderer.startWidth = width;
            trailRenderer.endWidth = width;
        }
    }
    
    /// <summary>
    /// Update trail LOD based on camera distance
    /// Performance: Reduces trail quality for distant agents
    /// </summary>
    void UpdateTrailLOD()
    {
        if (trailRenderer == null) return;
        
        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        // Calculate distance from camera to this agent
        float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
        
        // Determine LOD level based on distance
        int newLODLevel = 0;
        for (int i = 0; i < lodDistances.Length; i++)
        {
            if (distance > lodDistances[i])
            {
                newLODLevel = i + 1;
            }
            else
            {
                break;
            }
        }
        
        // Clamp to available LOD levels
        newLODLevel = Mathf.Min(newLODLevel, lodTrailTimes.Length - 1);
        
        // Only update if LOD level changed
        if (newLODLevel != currentLODLevel)
        {
            currentLODLevel = newLODLevel;
            
            // Apply LOD settings to trail renderer
            if (currentLODLevel < lodTrailTimes.Length)
            {
                trailRenderer.time = 1000f * lodTrailTimes[currentLODLevel];
            }
            
            if (currentLODLevel < lodMinVertexDistances.Length)
            {
                trailRenderer.minVertexDistance = lodMinVertexDistances[currentLODLevel];
            }
            
            // Optionally reduce corner/cap vertices for far LOD levels
            if (currentLODLevel >= 2)
            {
                trailRenderer.numCornerVertices = 2;
                trailRenderer.numCapVertices = 2;
            }
            else
            {
                trailRenderer.numCornerVertices = 5;
                trailRenderer.numCapVertices = 5;
            }
        }
    }
    
    /// <summary>
    /// Highlight this agent's trail (pulse effect)
    /// Performance: Uses cached material and shader property IDs
    /// </summary>
    public void HighlightTrail(bool highlight)
    {
        if (trailMaterial != null)
        {
            float baseWidth = sharedState != null ? sharedState.masterLineWidth : 0.3f;
            
            if (highlight)
            {
                // Increase emission for highlight effect
                if (trailMaterial.HasProperty(EmissionColorID))
                {
                    trailMaterial.SetColor(EmissionColorID, agentColor * 2f);
                }
                // Slightly increase width
                if (trailRenderer != null)
                {
                    trailRenderer.startWidth = baseWidth * 1.5f;
                    trailRenderer.endWidth = baseWidth * 1.5f;
                }
            }
            else
            {
                // Reset to normal
                if (trailMaterial.HasProperty(EmissionColorID))
                {
                    trailMaterial.SetColor(EmissionColorID, agentColor * 0.5f);
                }
                // Reset width
                if (trailRenderer != null)
                {
                    trailRenderer.startWidth = baseWidth;
                    trailRenderer.endWidth = baseWidth;
                }
            }
        }
    }
}

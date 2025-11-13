using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpirographRoller : MonoBehaviour
{
    public Transform[] pathPoints;
    [Range(0f, 750f)]
    public float speed = 0f;
    [Range(0f, 1f)]
    public float rotationSpeed = 0.5f; // Independent rotation speed multiplier (0-1)
    public int cycles = 10;
    
    [Header("Rotor Control")]
    [Tooltip("Distance of pen from rotor center (0.0 to 5x rotor size)")]
    [Range(0f, 5f)]
    public float penDistance = 0.3f;
    
    [Header("Line Width Control")]
    [Tooltip("Width of the traced line")]
    [Range(0.01f, 2f)]
    public float lineWidth = 0.3f;
    
    [Header("Line Brightness Control")]
    [Tooltip("Brightness/Alpha of all traced lines (0-1)")]
    [Range(0f, 1f)]
    public float lineBrightness = 1f;
    
    [Header("Line Visual Effects")]
    [Tooltip("Click 'LINE FX' button in UI to cycle through effects, or change in inspector")]
    public LineEffectMode lineEffectMode = LineEffectMode.Normal;
    [Tooltip("Intensity of the current effect (higher = more pronounced)")]
    [Range(0f, 10f)]
    public float effectIntensity = 3f;
    [Tooltip("Speed of animated effects (pulse, rainbow, hologram)")]
    [Range(0f, 5f)]
    public float effectSpeed = 1f;
    
    public enum LineEffectMode
    {
        Normal,        // Standard material rendering
        Glow,          // Intense emission/bloom effect
        Rainbow,       // Animated color spectrum shift
        Pulse,         // Breathing brightness animation
        Wireframe,     // Thin cyan technical lines
        Neon,          // Ultra-bright pulsing cyberpunk
        FadeTrail,     // Gradient fade from bright to transparent
        Hologram       // Flickering scan-line holographic
    }
    
    [Header("Visual Indicators")]
    [Tooltip("Glow intensity for the pen point (0-10)")]
    [Range(0f, 10f)]
    public float penGlowIntensity = 5f;
    [Tooltip("Show radius line from rotor center to pen")]
    public bool showRadiusLine = true;
    [Tooltip("Color of the radius line")]
    public Color radiusLineColor = Color.cyan;
    
    public Vector3 penOffset = new Vector3(0.3f, 0, 0);
    
    [Header("Path Reference Frame")]
    [Tooltip("Use world space for path (ignores parent rotation). Enable this if parent rotates.")]
    public bool useWorldSpacePath = true;
    
    [Header("Trail Renderer (Optional)")]
    [Tooltip("Assign a TrailRenderer prefab/material, or leave empty to create default")]
    public TrailRenderer trailRendererPrefab;
    public Material trailMaterial;
    
    [Header("Line Color System")]
    [Tooltip("Current line color (HSV controlled via UI or inspector)")]
    public Color currentLineColor = Color.cyan;
    
    [Header("Performance & Smoothing")]
    [Tooltip("Adaptive smoothing based on speed (dynamic quality)")]
    public bool adaptiveQuality = true;
    [Tooltip("Sample delta for curvature calculation")]
    public float curvatureDelta = 0.02f;
    
    [Header("Trail Quality")]
    [Tooltip("Maximum distance between trail points (smaller = smoother lines at high speed)")]
    public float maxTrailSegmentLength = 0.05f;
    [Tooltip("Interpolate trail positions for smooth lines at any speed")]
    public bool highQualityTrail = true;
    
    [Header("Visibility")]
    [Tooltip("GameObject to show/hide (the visual object being traced). If empty, will hide/show renderers on this object.")]
    public GameObject visualObject;
    
    private bool isPaused = false;
    private bool visualsVisible = true;
    
    private List<Vector3> path = new List<Vector3>();
    private List<Vector3> staticPathCache = new List<Vector3>(); // World-space snapshot of path
    private float totalLength, distance;
    private int cycle;
    private float radius;
    private float currentAngle = 0f;
    private Vector3 previousPosition;
    private Vector3 currentRotationAxis;
    private GameObject penObject;
    private GameObject penDotVisual;
    private LineRenderer radiusLine; // Line from rotor center to pen
    private TrailRenderer trailRenderer;
    private List<TrailRenderer> oldTrails = new List<TrailRenderer>();
    private Vector3 startPosition;
    private float effectTimer = 0f; // For animated effects
    private Material baseMaterial; // Store original material for effect switching
    private float currentHue = 0.5f;
    private float currentSaturation = 0.8f;
    private float currentValue = 1f;
    private Slider speedSlider, rotationSpeedSlider, cyclesSlider, penDistanceSlider, lineWidthSlider, lineBrightnessSlider;
    private Text speedText, rotationSpeedText, cyclesText, penDistanceText, lineWidthText, lineBrightnessText;
    private Button pauseButton;
    private Text pauseButtonText;
    private float baseRotorRadius; // Store the base rotor size at start
    
    // Performance: Cached shader property IDs to avoid string lookups
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int MetallicID = Shader.PropertyToID("_Metallic");
    private static readonly int GlossinessID = Shader.PropertyToID("_Glossiness");
    
    void Start()
    {
        // CRITICAL: Validate path points exist
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("SpirographRoller: No path points assigned! Disabling component.");
            enabled = false;
            return;
        }
        
        // Cache the path in world space at initialization
        // This creates a static reference frame independent of parent rotation
        try
        {
            CacheStaticPath();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SpirographRoller: Error caching path: {e.Message}");
            enabled = false;
            return;
        }
        
        // Validate cache was successful
        if (staticPathCache == null || staticPathCache.Count == 0)
        {
            Debug.LogError("SpirographRoller: Path cache is empty after caching! Disabling component.");
            enabled = false;
            return;
        }
        
        // Calculate initial total length from cached positions
        totalLength = 0;
        for (int i = 1; i < staticPathCache.Count; i++)
        {
            float segmentLength = Vector3.Distance(staticPathCache[i-1], staticPathCache[i]);
            // Validate segment length
            if (float.IsNaN(segmentLength) || float.IsInfinity(segmentLength))
            {
                Debug.LogWarning($"SpirographRoller: Invalid segment length at index {i}, skipping");
                continue;
            }
            totalLength += segmentLength;
        }
        // Add distance from last point back to first (closed loop)
        if (staticPathCache.Count > 0)
        {
            float closingLength = Vector3.Distance(staticPathCache[staticPathCache.Count-1], staticPathCache[0]);
            if (!float.IsNaN(closingLength) && !float.IsInfinity(closingLength))
            {
                totalLength += closingLength;
            }
        }
        
        // Validate total length
        if (totalLength <= 0f || float.IsNaN(totalLength) || float.IsInfinity(totalLength))
        {
            Debug.LogError($"SpirographRoller: Invalid total path length: {totalLength}. Disabling component.");
            enabled = false;
            return;
        }
        
        radius = GetComponent<Renderer>().bounds.extents.x;
        baseRotorRadius = radius; // Store the base size for scaling calculations
        
        penObject = new GameObject("Pen");
        penObject.transform.SetParent(transform);
        UpdatePenOffset(); // Calculate initial pen position based on penDistance
        
        // Create visual dot at pen point with glow
        CreatePenDotVisual();
        
        // Create radius line from rotor center to pen
        CreateRadiusLine();
        
        // Create a separate GameObject for the trail that is NOT parented
        // This allows it to record true 3D world space positions
        GameObject trailObject = new GameObject("Trail");
        trailObject.transform.position = penObject.transform.position;
        
        // Use assigned TrailRenderer or create default
        if (trailRendererPrefab != null)
        {
            trailRenderer = Instantiate(trailRendererPrefab, trailObject.transform);
            trailRenderer.transform.localPosition = Vector3.zero;
        }
        else
        {
            trailRenderer = trailObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 1000f;
            trailRenderer.startWidth = lineWidth;
            trailRenderer.endWidth = lineWidth;
            trailRenderer.minVertexDistance = 0.01f;
            trailRenderer.numCornerVertices = 5;
            trailRenderer.numCapVertices = 5;
            
            // Apply custom material if provided
            if (trailMaterial != null)
            {
                trailRenderer.material = trailMaterial;
            }
        }
        
        startPosition = GetPoint(0);
        transform.position = startPosition;
        previousPosition = transform.position;
        currentRotationAxis = Vector3.right; // Initialize with a default axis
        
        ConnectToUI();
    }
    
    void OnValidate()
    {
        // Update pen offset and line width in editor when values change
        if (Application.isPlaying && penObject != null)
        {
            UpdatePenOffset();
            UpdateLineWidth();
            UpdateLineBrightness();
            UpdatePenGlow();
            UpdateRadiusLineColor();
            
            // Apply current effect to all trails
            if (trailRenderer != null)
                ApplyLineEffect(trailRenderer);
            foreach (var trail in oldTrails)
                if (trail != null)
                    ApplyLineEffect(trail);
        }
    }
    
    void Update()
    {
        if (isPaused || cycle >= cycles) return;
        
        // CRITICAL: Validate totalLength to prevent division by zero
        if (totalLength <= 0f || float.IsNaN(totalLength) || float.IsInfinity(totalLength))
        {
            Debug.LogError("SpirographRoller: Invalid totalLength in Update, disabling");
            enabled = false;
            return;
        }
        
        // Validate staticPathCache
        if (staticPathCache == null || staticPathCache.Count == 0)
        {
            Debug.LogError("SpirographRoller: Path cache is null or empty in Update, disabling");
            enabled = false;
            return;
        }
        
        // Calculate how far we need to travel this frame
        float frameDistance = speed * Time.deltaTime;
        
        // Validate frameDistance
        if (float.IsNaN(frameDistance) || float.IsInfinity(frameDistance))
        {
            Debug.LogWarning("SpirographRoller: Invalid frameDistance calculated, skipping frame");
            return;
        }
        
        float startDistance = distance;
        
        // Move the distance for this frame
        distance += frameDistance;
        
        // Handle cycle completion with safe modulo
        if (distance >= totalLength) 
        { 
            cycle++; 
            // Safe modulo operation
            if (totalLength > 0f)
            {
                distance = distance % totalLength; // Wrap around smoothly
            }
            else
            {
                distance = 0f;
            }
            
            if (cycle >= cycles)
            {
                distance = 0;
                return;
            }
        }
        
        // ===== HIGH-QUALITY TRAIL INTERPOLATION =====
        // At high speeds, we need to inject intermediate points for smooth trails
        if (highQualityTrail && frameDistance > maxTrailSegmentLength)
        {
            // Calculate how many intermediate points we need
            int numSegments = Mathf.CeilToInt(frameDistance / maxTrailSegmentLength);
            float segmentDistance = frameDistance / numSegments;
            
            // Interpolate through intermediate positions
            for (int i = 0; i < numSegments; i++)
            {
                float interpDistance = startDistance + (segmentDistance * (i + 1));
                if (interpDistance > totalLength)
                    interpDistance = interpDistance % totalLength;
                
                Vector3 interpPosition = GetPoint(interpDistance);
                
                // Calculate rotation for this intermediate point
                UpdateRotationForPosition(interpPosition, segmentDistance);
                
                // Move to intermediate position (trail renderer captures this)
                transform.position = interpPosition;
                
                // Update trail position to follow the pen in world space
                if (trailRenderer != null)
                    trailRenderer.transform.position = penObject.transform.position;
            }
        }
        else
        {
            // Normal single-step update
            Vector3 newPosition = GetPoint(distance);
            UpdateRotationForPosition(newPosition, frameDistance);
            transform.position = newPosition;
        }
        
        // Update trail position to follow the pen in world space
        if (trailRenderer != null)
            trailRenderer.transform.position = penObject.transform.position;
        
        // Update radius line every frame (visual feedback for pen position)
        UpdateRadiusLine();
        
        // Update line effects (animated effects)
        UpdateLineEffects();
        
        previousPosition = transform.position;
    }
    
    void UpdateRotationForPosition(Vector3 newPosition, float distanceMoved)
    {
        // ===== SIMPLE FIXED-AXIS ROTATION =====
        // Rotor spins around ONE fixed axis only - no path adaptation, no flipping
        // Just pure 2D rotation while moving through 3D space
        
        // ===== ROTATION - 100% INDEPENDENT AND FIXED =====
        // Rotation is ONLY controlled by rotationSpeed slider
        // Rotates around a FIXED axis (Z-axis / forward) - never changes
        float angleIncrement = rotationSpeed * Time.deltaTime * 360f; // degrees per second
        currentAngle -= angleIncrement;
        
        // Apply rotation around FIXED Z-axis only
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }
    
    void CreatePenDotVisual()
    {
        // Create a small sphere to visualize the pen point
        penDotVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        penDotVisual.name = "PenDotVisual";
        penDotVisual.transform.SetParent(penObject.transform);
        penDotVisual.transform.localPosition = Vector3.zero;
        penDotVisual.transform.localScale = Vector3.one * 0.15f; // Slightly larger for visibility
        
        // Remove collider (we don't need physics)
        Collider dotCollider = penDotVisual.GetComponent<Collider>();
        if (dotCollider != null)
            Destroy(dotCollider);
        
        // Make it GLOW with intense emission
        Renderer dotRenderer = penDotVisual.GetComponent<Renderer>();
        if (dotRenderer != null)
        {
            Material dotMat = new Material(Shader.Find("Standard"));
            dotMat.color = Color.yellow;
            dotMat.SetFloat(MetallicID, 0f);
            dotMat.SetFloat(GlossinessID, 1f);
            dotMat.EnableKeyword("_EMISSION");
            // Configurable glow intensity - Performance: Use cached property ID
            dotMat.SetColor(EmissionColorID, new Color(1f, 0.9f, 0f, 1f) * penGlowIntensity);
            dotMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            dotRenderer.material = dotMat;
        }
    }
    
    public void UpdatePenGlow()
    {
        // Update glow intensity in realtime
        if (penDotVisual != null)
        {
            Renderer dotRenderer = penDotVisual.GetComponent<Renderer>();
            if (dotRenderer != null && dotRenderer.material != null)
            {
                // Performance: Use cached property ID
                dotRenderer.material.SetColor(EmissionColorID, new Color(1f, 0.9f, 0f, 1f) * penGlowIntensity);
            }
        }
    }
    
    void CreateRadiusLine()
    {
        // Create a line from rotor center to pen point
        GameObject lineObj = new GameObject("RadiusLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        
        radiusLine = lineObj.AddComponent<LineRenderer>();
        radiusLine.positionCount = 2;
        radiusLine.startWidth = 0.05f;
        radiusLine.endWidth = 0.05f;
        radiusLine.useWorldSpace = false; // Use local space relative to rotor
        
        // Create glowing material for the line with configurable color
        Material lineMat = new Material(Shader.Find("Standard"));
        lineMat.color = radiusLineColor;
        lineMat.SetFloat(MetallicID, 0f);
        lineMat.SetFloat(GlossinessID, 0.8f);
        lineMat.EnableKeyword("_EMISSION");
        // Performance: Use cached property ID
        lineMat.SetColor(EmissionColorID, radiusLineColor * 2f);
        lineMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        radiusLine.material = lineMat;
        
        // Set initial line positions
        UpdateRadiusLine();
    }
    
    void UpdateRadiusLine()
    {
        if (radiusLine != null && penObject != null)
        {
            // Show/hide based on setting
            radiusLine.enabled = showRadiusLine;
            
            if (showRadiusLine)
            {
                // Line from rotor center (0,0,0 in local space) to pen position
                radiusLine.SetPosition(0, Vector3.zero);
                radiusLine.SetPosition(1, penObject.transform.localPosition);
            }
        }
    }
    
    public void UpdateRadiusLineColor()
    {
        // Update radius line color in realtime
        if (radiusLine != null && radiusLine.material != null)
        {
            radiusLine.material.color = radiusLineColor;
            // Performance: Use cached property ID
            radiusLine.material.SetColor(EmissionColorID, radiusLineColor * 2f);
        }
    }
    
    void CacheStaticPath()
    {
        // Create a static snapshot of the path in world space
        // CRITICAL: Initialize cache if null
        if (staticPathCache == null)
        {
            staticPathCache = new List<Vector3>();
        }
        
        staticPathCache.Clear();
        
        // CRITICAL: Validate pathPoints
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("SpirographRoller: Cannot cache path - pathPoints is null or empty");
            return;
        }
        
        int validPoints = 0;
        foreach (Transform point in pathPoints)
        {
            if (point != null)
            {
                staticPathCache.Add(point.position);
                validPoints++;
            }
            else
            {
                Debug.LogWarning("SpirographRoller: Null path point encountered during caching");
            }
        }
        
        if (validPoints == 0)
        {
            Debug.LogError("SpirographRoller: No valid path points found during caching");
        }
        else
        {
            Debug.Log($"SpirographRoller: Cached {validPoints} path points");
        }
    }
    
    Vector3 GetPoint(float d)
    {
        // CRITICAL: Validate distance parameter
        if (float.IsNaN(d) || float.IsInfinity(d))
        {
            Debug.LogWarning($"SpirographRoller: Invalid distance parameter: {d}");
            d = 0f;
        }
        
        if (useWorldSpacePath)
        {
            // Use the cached static path (unaffected by parent rotation)
            // This is the mathematically correct approach for spirographs
            
            // CRITICAL: Validate cache
            if (staticPathCache == null || staticPathCache.Count == 0)
            {
                Debug.LogWarning("SpirographRoller: Path cache is null or empty in GetPoint");
                return Vector3.zero;
            }
            
            float a = 0;
            for (int i = 1; i < staticPathCache.Count; i++)
            {
                // CRITICAL: Validate array indices
                if (i < 0 || i >= staticPathCache.Count || i - 1 < 0)
                {
                    Debug.LogError($"SpirographRoller: Invalid index in GetPoint loop: {i}");
                    continue;
                }
                
                float l = Vector3.Distance(staticPathCache[i-1], staticPathCache[i]);
                
                // Validate segment length
                if (float.IsNaN(l) || float.IsInfinity(l) || l < 0f)
                {
                    Debug.LogWarning($"SpirographRoller: Invalid segment length at {i}: {l}");
                    continue;
                }
                
                if (a + l >= d)
                {
                    // CRITICAL: Prevent division by zero
                    if (l > 0f)
                    {
                        float t = (d - a) / l;
                        t = Mathf.Clamp01(t); // Safety clamp
                        return Vector3.Lerp(staticPathCache[i-1], staticPathCache[i], t);
                    }
                    else
                    {
                        return staticPathCache[i-1];
                    }
                }
                a += l;
            }
            
            // Handle wrap-around from last to first point
            if (staticPathCache.Count > 0)
            {
                int lastIndex = staticPathCache.Count - 1;
                float l = Vector3.Distance(staticPathCache[lastIndex], staticPathCache[0]);
                
                if (!float.IsNaN(l) && !float.IsInfinity(l) && l >= 0f && a + l >= d)
                {
                    if (l > 0f)
                    {
                        float t = (d - a) / l;
                        t = Mathf.Clamp01(t);
                        return Vector3.Lerp(staticPathCache[lastIndex], staticPathCache[0], t);
                    }
                    else
                    {
                        return staticPathCache[lastIndex];
                    }
                }
            }
            
            return staticPathCache.Count > 0 ? staticPathCache[0] : Vector3.zero;
        }
        else
        {
            // Use current Transform positions (dynamically updated by parent rotation)
            // CRITICAL: Validate pathPoints
            if (pathPoints == null || pathPoints.Length == 0)
            {
                Debug.LogWarning("SpirographRoller: pathPoints is null or empty");
                return Vector3.zero;
            }
            
            float a = 0;
            for (int i = 1; i < pathPoints.Length; i++)
            {
                // CRITICAL: Null check transforms
                if (pathPoints[i] == null || pathPoints[i-1] == null)
                {
                    Debug.LogWarning($"SpirographRoller: Null path point at index {i}");
                    continue;
                }
                
                float l = Vector3.Distance(pathPoints[i-1].position, pathPoints[i].position);
                
                if (float.IsNaN(l) || float.IsInfinity(l) || l < 0f)
                {
                    Debug.LogWarning($"SpirographRoller: Invalid dynamic segment length at {i}: {l}");
                    continue;
                }
                
                if (a + l >= d)
                {
                    if (l > 0f)
                    {
                        float t = (d - a) / l;
                        t = Mathf.Clamp01(t);
                        return Vector3.Lerp(pathPoints[i-1].position, pathPoints[i].position, t);
                    }
                    else
                    {
                        return pathPoints[i-1].position;
                    }
                }
                a += l;
            }
            
            // Handle wrap-around from last to first point
            if (pathPoints.Length > 0 && pathPoints[0] != null && pathPoints[pathPoints.Length-1] != null)
            {
                float l = Vector3.Distance(pathPoints[pathPoints.Length-1].position, pathPoints[0].position);
                if (!float.IsNaN(l) && !float.IsInfinity(l) && l >= 0f && a + l >= d)
                {
                    if (l > 0f)
                    {
                        float t = (d - a) / l;
                        t = Mathf.Clamp01(t);
                        return Vector3.Lerp(pathPoints[pathPoints.Length-1].position, pathPoints[0].position, t);
                    }
                    else
                    {
                        return pathPoints[pathPoints.Length-1].position;
                    }
                }
            }
            
            return pathPoints.Length > 0 && pathPoints[0] != null ? pathPoints[0].position : Vector3.zero;
        }
    }
    
    void UpdatePenOffset()
    {
        // Calculate pen offset based on penDistance (scaled by base rotor radius)
        float actualDistance = penDistance * baseRotorRadius;
        penOffset = new Vector3(actualDistance, 0, 0);
        if (penObject != null)
        {
            penObject.transform.localPosition = penOffset;
            UpdateRadiusLine(); // Update the visual line
        }
    }
    
    void UpdateLineWidth()
    {
        // Update trail renderer width
        if (trailRenderer != null)
        {
            trailRenderer.startWidth = lineWidth;
            trailRenderer.endWidth = lineWidth;
        }
    }
    
    void UpdateLineBrightness()
    {
        // Update brightness/alpha of current trail - Performance: Use cached property ID
        if (trailRenderer != null && trailRenderer.material != null)
        {
            Color color = trailRenderer.material.color;
            color.a = lineBrightness;
            trailRenderer.material.color = color;
            
            // Also update emission if present
            if (trailRenderer.material.HasProperty(EmissionColorID))
            {
                Color emission = trailRenderer.material.GetColor(EmissionColorID);
                emission.a = lineBrightness;
                trailRenderer.material.SetColor(EmissionColorID, emission);
            }
        }
        
        // Update brightness of all old trails - Performance: Use cached property ID
        foreach (TrailRenderer oldTrail in oldTrails)
        {
            if (oldTrail != null && oldTrail.material != null)
            {
                Color color = oldTrail.material.color;
                color.a = lineBrightness;
                oldTrail.material.color = color;
                
                // Also update emission if present
                if (oldTrail.material.HasProperty(EmissionColorID))
                {
                    Color emission = oldTrail.material.GetColor(EmissionColorID);
                    emission.a = lineBrightness;
                    oldTrail.material.SetColor(EmissionColorID, emission);
                }
            }
        }
    }
    
    void UpdateLineEffects()
    {
        effectTimer += Time.deltaTime * effectSpeed;
        
        // Apply effect to current trail
        if (trailRenderer != null)
        {
            ApplyLineEffect(trailRenderer);
        }
        
        // Apply effect to all old trails
        foreach (TrailRenderer oldTrail in oldTrails)
        {
            if (oldTrail != null)
            {
                ApplyLineEffect(oldTrail);
            }
        }
    }
    
    void ApplyLineEffect(TrailRenderer trail)
    {
        if (trail == null || trail.material == null) return;
        
        Material mat = trail.material;
        
        switch (lineEffectMode)
        {
            case LineEffectMode.Normal:
                // Standard rendering - reset any modifications
                // Performance: Use cached property ID
                if (mat.HasProperty(EmissionColorID))
                {
                    Color baseColor = mat.color;
                    baseColor.a = lineBrightness;
                    mat.SetColor(EmissionColorID, baseColor * 0.5f);
                }
                trail.startWidth = lineWidth;
                trail.endWidth = lineWidth;
                break;
                
            case LineEffectMode.Glow:
                // Intense emission for bloom effect - Performance: Use cached property ID
                if (mat.HasProperty(EmissionColorID))
                {
                    Color glowColor = mat.color;
                    glowColor.a = lineBrightness;
                    mat.SetColor(EmissionColorID, glowColor * effectIntensity);
                }
                break;
                
            case LineEffectMode.Rainbow:
                // Cycle through rainbow colors - Performance: Use cached property ID
                float hue = (effectTimer * 0.2f) % 1f;
                Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f);
                rainbowColor.a = lineBrightness;
                mat.color = rainbowColor;
                if (mat.HasProperty(EmissionColorID))
                {
                    mat.SetColor(EmissionColorID, rainbowColor * effectIntensity);
                }
                break;
                
            case LineEffectMode.Pulse:
                // Pulsing brightness - Performance: Use cached property ID
                float pulse = (Mathf.Sin(effectTimer * 2f) * 0.5f + 0.5f); // 0 to 1
                float pulseBrightness = Mathf.Lerp(0.3f, 1f, pulse) * lineBrightness;
                Color pulseColor = mat.color;
                pulseColor.a = pulseBrightness;
                mat.color = pulseColor;
                if (mat.HasProperty(EmissionColorID))
                {
                    mat.SetColor(EmissionColorID, pulseColor * effectIntensity * pulse);
                }
                break;
                
            case LineEffectMode.Wireframe:
                // Thin, technical lines with subtle glow - Performance: Use cached property ID
                trail.startWidth = lineWidth * 0.3f;
                trail.endWidth = lineWidth * 0.3f;
                if (mat.HasProperty(EmissionColorID))
                {
                    Color wireColor = new Color(0.3f, 0.8f, 1f, lineBrightness); // Cyan
                    mat.color = wireColor;
                    mat.SetColor(EmissionColorID, wireColor * effectIntensity * 0.5f);
                }
                break;
                
            case LineEffectMode.Neon:
                // Ultra-bright cyberpunk look - Performance: Use cached property ID
                Color neonColor = mat.color;
                neonColor.a = lineBrightness;
                mat.color = neonColor;
                if (mat.HasProperty(EmissionColorID))
                {
                    // Very intense emission with slight pulsing
                    float neonPulse = Mathf.Sin(effectTimer * 3f) * 0.2f + 1f;
                    mat.SetColor(EmissionColorID, neonColor * effectIntensity * 2f * neonPulse);
                }
                trail.startWidth = lineWidth * 1.2f;
                trail.endWidth = lineWidth * 1.2f;
                break;
                
            case LineEffectMode.FadeTrail:
                // Gradually fade out over trail length
                trail.startWidth = lineWidth;
                trail.endWidth = lineWidth * 0.1f;
                // Unity's TrailRenderer already supports this via gradient
                Gradient fadeGradient = new Gradient();
                Color startColor = mat.color;
                startColor.a = lineBrightness;
                Color endColor = startColor;
                endColor.a = 0f;
                fadeGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(lineBrightness, 0f), new GradientAlphaKey(0f, 1f) }
                );
                trail.colorGradient = fadeGradient;
                break;
                
            case LineEffectMode.Hologram:
                // Scan-line holographic effect with flicker - Performance: Use cached property ID
                float scanLine = (effectTimer * 2f) % 1f;
                float flicker = Mathf.PerlinNoise(effectTimer * 10f, 0f) * 0.3f + 0.7f;
                Color holoColor = new Color(0.2f, 0.8f, 1f, lineBrightness * flicker);
                mat.color = holoColor;
                if (mat.HasProperty(EmissionColorID))
                {
                    mat.SetColor(EmissionColorID, holoColor * effectIntensity * flicker);
                }
                // Add scan-line effect via width modulation
                float widthMod = Mathf.Abs(Mathf.Sin(effectTimer * 5f)) * 0.5f + 0.5f;
                trail.startWidth = lineWidth * widthMod;
                trail.endWidth = lineWidth * widthMod * 0.8f;
                break;
        }
    }
    
    public void CycleLineEffect()
    {
        // Cycle to next effect mode
        int currentIndex = (int)lineEffectMode;
        int maxIndex = System.Enum.GetValues(typeof(LineEffectMode)).Length;
        currentIndex = (currentIndex + 1) % maxIndex;
        lineEffectMode = (LineEffectMode)currentIndex;
        
        effectTimer = 0f; // Reset timer for new effect
        
        Debug.Log($"Line Effect: {lineEffectMode}");
    }
    
    void ConnectToUI()
    {
        // Look for UI elements created by SpirographUIManager
        speedSlider = GameObject.Find("SpeedSlider")?.GetComponent<Slider>();
        rotationSpeedSlider = GameObject.Find("RotationSpeedSlider")?.GetComponent<Slider>();
        cyclesSlider = GameObject.Find("CyclesSlider")?.GetComponent<Slider>();
        penDistanceSlider = GameObject.Find("PenDistanceSlider")?.GetComponent<Slider>();
        lineWidthSlider = GameObject.Find("LineWidthSlider")?.GetComponent<Slider>();
        lineBrightnessSlider = GameObject.Find("LineBrightnessSlider")?.GetComponent<Slider>();
        pauseButton = GameObject.Find("PauseButton")?.GetComponent<Button>();
        Button resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
        Button recalculatePathButton = GameObject.Find("RecalculatePathButton")?.GetComponent<Button>();
        
        if (speedSlider != null)
        {
            speedText = speedSlider.GetComponentInChildren<Text>();
            speedSlider.minValue = 0f;
            speedSlider.maxValue = 750f;
            speedSlider.value = speed;
            speedSlider.onValueChanged.AddListener((value) => {
                speed = Mathf.Clamp(value, 0f, 750f);
                if (speedText != null) speedText.text = "Travel Speed: " + speed.ToString("F1");
            });
        }
        
        if (rotationSpeedSlider != null)
        {
            rotationSpeedText = rotationSpeedSlider.GetComponentInChildren<Text>();
            rotationSpeedSlider.minValue = 0f;
            rotationSpeedSlider.maxValue = 1f;
            rotationSpeedSlider.value = rotationSpeed;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                rotationSpeed = Mathf.Clamp(value, 0f, 1f);
                if (rotationSpeedText != null) rotationSpeedText.text = "Rotation Speed: " + rotationSpeed.ToString("F2");
            });
        }
        
        if (cyclesSlider != null)
        {
            cyclesText = cyclesSlider.GetComponentInChildren<Text>();
            cyclesSlider.value = cycles;
            cyclesSlider.onValueChanged.AddListener((value) => {
                cycles = (int)value;
                if (cyclesText != null) cyclesText.text = "Cycles: " + value.ToString();
            });
        }
        
        if (penDistanceSlider != null)
        {
            penDistanceText = penDistanceSlider.GetComponentInChildren<Text>();
            penDistanceSlider.minValue = 0f;
            penDistanceSlider.maxValue = 5f;
            penDistanceSlider.value = penDistance;
            penDistanceSlider.onValueChanged.AddListener((value) => {
                penDistance = value;
                UpdatePenOffset();
                if (penDistanceText != null) penDistanceText.text = "Rotor Radius: " + value.ToString("F2") + "x";
            });
        }
        
        if (lineWidthSlider != null)
        {
            lineWidthText = lineWidthSlider.GetComponentInChildren<Text>();
            lineWidthSlider.minValue = 0.01f;
            lineWidthSlider.maxValue = 2f;
            lineWidthSlider.value = lineWidth;
            lineWidthSlider.onValueChanged.AddListener((value) => {
                lineWidth = value;
                UpdateLineWidth();
                if (lineWidthText != null) lineWidthText.text = "Line Width: " + value.ToString("F2");
            });
        }
        
        if (lineBrightnessSlider != null)
        {
            lineBrightnessText = lineBrightnessSlider.GetComponentInChildren<Text>();
            lineBrightnessSlider.minValue = 0f;
            lineBrightnessSlider.maxValue = 1f;
            lineBrightnessSlider.value = lineBrightness;
            lineBrightnessSlider.onValueChanged.AddListener((value) => {
                lineBrightness = value;
                UpdateLineBrightness();
                if (lineBrightnessText != null) lineBrightnessText.text = "Line Brightness: " + value.ToString("F2");
            });
        }
        
        if (pauseButton != null)
        {
            pauseButtonText = pauseButton.GetComponentInChildren<Text>();
            pauseButton.onClick.AddListener(TogglePause);
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetSpirograph);
        }
        
        if (recalculatePathButton != null)
        {
            recalculatePathButton.onClick.AddListener(RecalculatePath);
        }
        
        // Connect color picker sliders
        Slider hueSlider = GameObject.Find("HueSlider")?.GetComponent<Slider>();
        Slider saturationSlider = GameObject.Find("SaturationSlider")?.GetComponent<Slider>();
        Slider valueSlider = GameObject.Find("ValueSlider")?.GetComponent<Slider>();
        
        if (hueSlider != null)
        {
            hueSlider.value = currentHue;
            hueSlider.onValueChanged.AddListener((value) => {
                currentHue = value;
                UpdateLineColorFromHSV(currentHue, currentSaturation, currentValue);
            });
        }
        
        if (saturationSlider != null)
        {
            saturationSlider.value = currentSaturation;
            saturationSlider.onValueChanged.AddListener((value) => {
                currentSaturation = value;
                UpdateLineColorFromHSV(currentHue, currentSaturation, currentValue);
            });
        }
        
        if (valueSlider != null)
        {
            valueSlider.value = currentValue;
            valueSlider.onValueChanged.AddListener((value) => {
                currentValue = value;
                UpdateLineColorFromHSV(currentHue, currentSaturation, currentValue);
            });
        }
        
        // Connect toggle visuals button
        Button toggleVisualsButton = GameObject.Find("ToggleVisualsButton")?.GetComponent<Button>();
        if (toggleVisualsButton != null)
        {
            toggleVisualsButton.onClick.AddListener(ToggleVisuals);
        }
        
        // Connect line effects button
        Button lineEffectsButton = GameObject.Find("LineEffectsButton")?.GetComponent<Button>();
        if (lineEffectsButton != null)
        {
            lineEffectsButton.onClick.AddListener(() => {
                CycleLineEffect();
                // Update button text to show current effect
                Text buttonText = lineEffectsButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "✨ LINE FX: " + lineEffectMode.ToString();
                }
            });
        }
    }
    
    public void ChangeLineColor(Color newColor)
    {
        currentLineColor = newColor;
        
        // Convert to HSV for UI sliders
        Color.RGBToHSV(newColor, out currentHue, out currentSaturation, out currentValue);
        
        // Create a new trail with the new color (preserves old trail)
        if (trailRenderer != null)
        {
            // Store the current trail renderer in the old trails list
            trailRenderer.emitting = false;
            oldTrails.Add(trailRenderer);
            
            // Create a new GameObject for the new trail renderer
            GameObject newTrailObj = new GameObject("Trail_" + newColor.ToString());
            newTrailObj.transform.position = penObject.transform.position;
            
            // Create a new trail renderer
            TrailRenderer newTrail = newTrailObj.AddComponent<TrailRenderer>();
            
            // Copy all settings from the old trail
            newTrail.time = trailRenderer.time;
            newTrail.startWidth = trailRenderer.startWidth;
            newTrail.endWidth = trailRenderer.endWidth;
            newTrail.minVertexDistance = trailRenderer.minVertexDistance;
            newTrail.numCornerVertices = trailRenderer.numCornerVertices;
            newTrail.numCapVertices = trailRenderer.numCapVertices;
            newTrail.autodestruct = false;
            
            // Clone the existing material to preserve shader and settings
            Material newMat;
            if (trailRenderer.material != null && trailRenderer.material.shader != null)
            {
                Debug.Log($"✓ Original material shader: {trailRenderer.material.shader.name}");
                
                // Use sharedMaterial to avoid instance issues
                Material sourceMat = trailRenderer.sharedMaterial;
                if (sourceMat == null) sourceMat = trailRenderer.material;
                
                // Create new material with the SAME shader
                newMat = new Material(sourceMat.shader);
                
                // Copy all properties from source material
                newMat.CopyPropertiesFromMaterial(sourceMat);
                
                // Override the color - Performance: Use cached property ID
                newMat.color = newColor;
                if (newMat.HasProperty(EmissionColorID))
                {
                    newMat.SetColor(EmissionColorID, newColor * 0.5f);
                }
                
                Debug.Log($"✅ New material created with shader: {newMat.shader.name}");
            }
            else
            {
                Debug.LogWarning("⚠ Original material/shader is null, using fallback");
                // Fallback: try to find a working shader
                Shader shader = Shader.Find("Particles/Standard Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("UI/Default");
                if (shader == null) shader = Shader.Find("Standard");
                
                newMat = new Material(shader);
                newMat.color = newColor;
                // Performance: Use cached property IDs
                if (newMat.HasProperty(MetallicID)) newMat.SetFloat(MetallicID, 0f);
                if (newMat.HasProperty(GlossinessID)) newMat.SetFloat(GlossinessID, 0.5f);
                if (newMat.HasProperty(EmissionColorID))
                {
                    newMat.EnableKeyword("_EMISSION");
                    newMat.SetColor(EmissionColorID, newColor * 0.5f);
                    newMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
            }
            
            newTrail.material = newMat;
            
            // Update the active trail reference
            trailRenderer = newTrail;
            
            // Apply current brightness and effects
            UpdateLineBrightness();
            ApplyLineEffect(newTrail);
            
            Debug.Log($"Line color changed to: RGB({newColor.r:F2}, {newColor.g:F2}, {newColor.b:F2})");
        }
    }
    
    public void UpdateLineColorFromHSV(float h, float s, float v)
    {
        currentHue = h;
        currentSaturation = s;
        currentValue = v;
        currentLineColor = Color.HSVToRGB(h, s, v);
        
        // Update current trail color without creating a new one - Performance: Use cached property ID
        if (trailRenderer != null && trailRenderer.material != null)
        {
            trailRenderer.material.color = currentLineColor;
            if (trailRenderer.material.HasProperty(EmissionColorID))
            {
                trailRenderer.material.SetColor(EmissionColorID, currentLineColor * 0.5f);
            }
            
            // Reapply effects to update colors
            ApplyLineEffect(trailRenderer);
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pauseButtonText != null)
            pauseButtonText.text = isPaused ? "Resume" : "Pause";
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public void ResetSpirograph()
    {
        // Reset position and rotation
        transform.position = startPosition;
        previousPosition = startPosition;
        currentAngle = 0f;
        distance = 0f;
        cycle = 0;
        currentRotationAxis = Vector3.right;
        transform.rotation = Quaternion.identity;
        
        // Clear active trail
        if (trailRenderer != null)
            trailRenderer.Clear();
        
        // Destroy all old trail renderers and their GameObjects
        foreach (TrailRenderer oldTrail in oldTrails)
        {
            if (oldTrail != null)
            {
                Destroy(oldTrail.gameObject); // Destroy the entire GameObject, not just the component
            }
        }
        oldTrails.Clear();
        
        // Unpause if paused
        if (isPaused)
        {
            isPaused = false;
            if (pauseButtonText != null)
                pauseButtonText.text = "Pause";
        }
    }
    
    void RecalculatePath()
    {
        // Recache the path from current world positions
        // Useful if you've manually adjusted path points
        CacheStaticPath();
        
        // Recalculate total length
        totalLength = 0;
        for (int i = 1; i < staticPathCache.Count; i++)
        {
            totalLength += Vector3.Distance(staticPathCache[i-1], staticPathCache[i]);
        }
        if (staticPathCache.Count > 0)
            totalLength += Vector3.Distance(staticPathCache[staticPathCache.Count-1], staticPathCache[0]);
        
        Debug.Log("Path recalculated. New length: " + totalLength);
    }
    
    public void ToggleVisuals()
    {
        visualsVisible = !visualsVisible;
        
        if (visualObject != null)
        {
            // Hide/show the assigned visual object
            visualObject.SetActive(visualsVisible);
        }
        else
        {
            // Hide/show all renderers on this object (except trail renderers)
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!(renderer is TrailRenderer))
                {
                    renderer.enabled = visualsVisible;
                }
            }
        }
        
        Debug.Log("Visual Object: " + (visualsVisible ? "VISIBLE" : "HIDDEN"));
    }
}

/* ===========================================================================================
 * MATHEMATICAL FOUNDATION: SPIROGRAPH GEOMETRY
 * ===========================================================================================
 * 
 * A true spirograph (hypotrochoid/epitrochoid) system consists of:
 * 
 * 1. FIXED CIRCLE (Guide Circle): Radius R, stationary in world space
 *    - This is defined by the pathPoints[] array
 *    - MUST remain stationary for correct spirograph mathematics
 *    - Rotating this creates a rotating reference frame (non-inertial frame)
 * 
 * 2. ROLLING CIRCLE: Radius r, rolls along the inside/outside of the fixed circle
 *    - This is the SpirographRoller object
 *    - Its center follows the path: θ = s/R where s is arc length
 *    - It rotates about its own center: φ = s/r (different rate!)
 * 
 * 3. PEN POINT: Distance d from rolling circle's center
 *    - This is the penOffset creating the traced pattern
 *    - Position: P(t) = (R-r)[cos(θ), sin(θ)] + d[cos((R-r)φ/r), sin((R-r)φ/r)]
 * 
 * WHY ROTATING THE PATH CREATES HARSH LINES:
 * -------------------------------------------
 * When RotateParent rotates the pathPoints[], it creates a rotating reference frame.
 * The roller tries to follow point A, but by the next frame, point A has moved to A'.
 * This creates discontinuous jumps in the path, causing:
 *    - Velocity discontinuities → harsh straight lines
 *    - Incorrect tangent vectors → wrong rotation axis
 *    - Broken Frenet-Serret frame → chaotic rolling behavior
 * 
 * THE SOLUTION:
 * -------------
 * 1. Cache the path in world space at initialization (staticPathCache)
 * 2. Set useWorldSpacePath = true
 * 3. In RotateParent, enable spirographMode = true
 * 4. The visual representation can rotate, but the mathematical path stays fixed
 * 
 * This maintains an INERTIAL REFERENCE FRAME for correct differential geometry.
 * 
 * FRENET-SERRET FRAME (for proper rolling):
 * ------------------------------------------
 * T (Tangent):   Direction of motion along the curve
 * N (Normal):    Points toward center of curvature
 * B (Binormal):  T × N, perpendicular to osculating plane (ROTATION AXIS)
 * 
 * The roller must rotate about the binormal B to achieve true rolling motion.
 * Curvature κ = ||dT/ds|| affects the effective rolling radius.
 * 
 * For perfect spirograph patterns, the path MUST be stationary in an inertial frame.
 * ===========================================================================================
 */

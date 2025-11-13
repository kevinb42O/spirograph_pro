using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this to a PathAgent to give it an amazing glowing trail effect.
/// The trail color automatically matches the agent's color and creates a beautiful glowing effect.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class AmazingLineEffect : MonoBehaviour
{
    [Header("Agent Reference")]
    [Tooltip("The PathAgent this trail follows (auto-detected)")]
    public PathAgent targetAgent;

    [Header("Trail Settings")]
    [SerializeField] private bool recordInWorldSpace = true;
    [SerializeField] private int maxTrailPoints = 10000; // Large limit for full drawings

    [Header("Width Settings")]
    [SerializeField] private AnimationCurve widthCurve;
    [SerializeField] private float maxWidth = 0.4f;

    [Header("Glow Settings")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private float glowIntensity = 3f;
    [SerializeField] private bool pulseGlow = true;
    [SerializeField] private float pulseSpeed = 2f;

    private LineRenderer lineRenderer;
    private Material glowMaterial;
    private Queue<TrailPoint> trailPoints = new Queue<TrailPoint>();
    private float lastRecordTime;
    private float recordInterval = 0.02f; // Record position every 0.02 seconds

    private struct TrailPoint
    {
        public Vector3 position;
        public float timestamp;
    }

    void Awake()
    {
        // Auto-detect PathAgent on same GameObject
        if (targetAgent == null)
        {
            targetAgent = GetComponent<PathAgent>();
        }

        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
        CreateGlowMaterial();
    }

    void SetupLineRenderer()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = recordInWorldSpace;
        lineRenderer.numCornerVertices = 8;
        lineRenderer.numCapVertices = 8;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;

        // Setup default width curve (fades out at end)
        if (widthCurve == null || widthCurve.keys.Length == 0)
        {
            widthCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
        }

        lineRenderer.widthMultiplier = maxWidth;
        lineRenderer.widthCurve = widthCurve;
    }

    void CreateGlowMaterial()
    {
        if (!enableGlow) return;

        // Create glowing material with emission
        glowMaterial = new Material(Shader.Find("Sprites/Default"));
        glowMaterial.SetFloat("_Metallic", 0f);
        glowMaterial.SetFloat("_Glossiness", 1f);

        // Enable emission for glow effect
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        lineRenderer.material = glowMaterial;
    }

    void Update()
    {
        if (targetAgent == null || targetAgent.status != PathAgent.AgentStatus.Active)
            return;

        // Record agent position at intervals
        if (Time.time - lastRecordTime >= recordInterval)
        {
            RecordPosition();
            lastRecordTime = Time.time;
        }

        // Update line renderer
        UpdateTrailRenderer();

        // Update glow effect
        UpdateGlow();
    }

    void RecordPosition()
    {
        TrailPoint point = new TrailPoint
        {
            position = targetAgent.currentPosition,
            timestamp = Time.time
        };

        trailPoints.Enqueue(point);

        // Only limit if we hit the max (prevents memory issues on extremely long drawings)
        if (trailPoints.Count > maxTrailPoints)
        {
            trailPoints.Dequeue();
        }
    }

    void UpdateTrailRenderer()
    {
        int pointCount = trailPoints.Count;
        lineRenderer.positionCount = pointCount;

        if (pointCount == 0) return;

        // Copy positions to line renderer
        int index = 0;
        foreach (TrailPoint point in trailPoints)
        {
            lineRenderer.SetPosition(index, point.position);
            index++;
        }

        // Update gradient to match agent color
        if (targetAgent != null)
        {
            UpdateColorGradient(targetAgent.agentColor);
        }
    }

    void UpdateColorGradient(Color agentColor)
    {
        // Solid agent color throughout the entire trail - no fading!
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(agentColor, 0f);
        colorKeys[1] = new GradientColorKey(agentColor, 1f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f); // Full opacity everywhere
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }

    void UpdateGlow()
    {
        if (!enableGlow || glowMaterial == null || targetAgent == null) return;

        // Get agent color for emission
        Color agentColor = targetAgent.agentColor;

        // Optional pulsing effect
        float intensity = glowIntensity;
        if (pulseGlow)
        {
            intensity *= (Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f); // Pulse between 0.7x and 1.0x
        }

        Color emissionColor = agentColor * intensity;
        glowMaterial.SetColor("_EmissionColor", emissionColor);
    }

    void OnDestroy()
    {
        if (glowMaterial != null)
        {
            Destroy(glowMaterial);
        }
    }

    /// <summary>
    /// Clear the trail (useful when resetting)
    /// </summary>
    public void ClearTrail()
    {
        trailPoints.Clear();
        lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// Manually set the agent color (useful for initialization)
    /// </summary>
    public void SetAgentColor(Color color)
    {
        if (targetAgent != null)
        {
            targetAgent.agentColor = color;
        }
        UpdateColorGradient(color);
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Cinematic camera presets for professional recordings
/// Includes Fly-around, Zoom in, Dolly zoom, Reveal, and more
/// </summary>
public class CameraPresets : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The object to focus on (e.g., PathParent)
    
    [Header("Preset Settings")]
    public float flyAroundRadius = 15f;
    public float flyAroundSpeed = 20f;
    public float flyAroundHeight = 5f;
    public float zoomInStartDistance = 30f;
    public float zoomInEndDistance = 8f;
    public float zoomInDuration = 3f;
    public float revealStartDistance = 50f;
    public float revealEndDistance = 12f;
    public float revealDuration = 4f;
    
    private Camera targetCamera;
    private CameraController cameraController;
    private Coroutine currentPresetCoroutine;
    
    void Start()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }
        
        cameraController = targetCamera?.GetComponent<CameraController>();
        
        if (target == null)
        {
            GameObject parentObj = GameObject.Find("PathParent");
            if (parentObj != null)
            {
                target = parentObj.transform;
            }
        }
    }
    
    /// <summary>
    /// Stop any currently running preset
    /// </summary>
    public void StopCurrentPreset()
    {
        if (currentPresetCoroutine != null)
        {
            StopCoroutine(currentPresetCoroutine);
            currentPresetCoroutine = null;
        }
        
        // Re-enable camera controller if it was disabled
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    /// <summary>
    /// Preset 1: Fly-around (circular orbit around target)
    /// </summary>
    public void StartFlyAround()
    {
        StopCurrentPreset();
        currentPresetCoroutine = StartCoroutine(FlyAroundCoroutine());
    }
    
    IEnumerator FlyAroundCoroutine()
    {
        if (target == null || targetCamera == null)
        {
            Debug.LogWarning("CameraPresets: Missing target or camera for Fly-around");
            yield break;
        }
        
        Debug.Log("🎬 Starting Fly-around preset");
        
        // Disable manual camera control during preset
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        float angle = 0f;
        
        while (true)
        {
            angle += flyAroundSpeed * Time.deltaTime;
            
            // Calculate position on circle around target
            float radians = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * flyAroundRadius,
                flyAroundHeight,
                Mathf.Sin(radians) * flyAroundRadius
            );
            
            targetCamera.transform.position = target.position + offset;
            targetCamera.transform.LookAt(target.position);
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Preset 2: Zoom in (smooth approach to target)
    /// </summary>
    public void StartZoomIn()
    {
        StopCurrentPreset();
        currentPresetCoroutine = StartCoroutine(ZoomInCoroutine());
    }
    
    IEnumerator ZoomInCoroutine()
    {
        if (target == null || targetCamera == null)
        {
            Debug.LogWarning("CameraPresets: Missing target or camera for Zoom In");
            yield break;
        }
        
        Debug.Log("🎬 Starting Zoom In preset");
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        // Get direction from target to camera
        Vector3 direction = (targetCamera.transform.position - target.position).normalized;
        
        // Start position
        Vector3 startPos = target.position + direction * zoomInStartDistance;
        Vector3 endPos = target.position + direction * zoomInEndDistance;
        
        targetCamera.transform.position = startPos;
        
        float elapsed = 0f;
        
        while (elapsed < zoomInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomInDuration;
            
            // Smooth ease-in curve
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            targetCamera.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            targetCamera.transform.LookAt(target.position);
            
            yield return null;
        }
        
        targetCamera.transform.position = endPos;
        targetCamera.transform.LookAt(target.position);
        
        Debug.Log("🎬 Zoom In complete");
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    /// <summary>
    /// Preset 3: Dolly zoom (Vertigo effect - change FOV while moving)
    /// </summary>
    public void StartDollyZoom()
    {
        StopCurrentPreset();
        currentPresetCoroutine = StartCoroutine(DollyZoomCoroutine());
    }
    
    IEnumerator DollyZoomCoroutine()
    {
        if (target == null || targetCamera == null)
        {
            Debug.LogWarning("CameraPresets: Missing target or camera for Dolly Zoom");
            yield break;
        }
        
        Debug.Log("🎬 Starting Dolly Zoom preset (Vertigo effect)");
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        float duration = 3f;
        float startDistance = 15f;
        float endDistance = 25f;
        float startFOV = 60f;
        float endFOV = 40f;
        
        Vector3 direction = (targetCamera.transform.position - target.position).normalized;
        Vector3 startPos = target.position + direction * startDistance;
        Vector3 endPos = target.position + direction * endDistance;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Move camera away while decreasing FOV to maintain apparent size
            targetCamera.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            targetCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, smoothT);
            targetCamera.transform.LookAt(target.position);
            
            yield return null;
        }
        
        // Reverse the effect
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            targetCamera.transform.position = Vector3.Lerp(endPos, startPos, smoothT);
            targetCamera.fieldOfView = Mathf.Lerp(endFOV, startFOV, smoothT);
            targetCamera.transform.LookAt(target.position);
            
            yield return null;
        }
        
        Debug.Log("🎬 Dolly Zoom complete");
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    /// <summary>
    /// Preset 4: Reveal (start distant, move close with dramatic timing)
    /// </summary>
    public void StartReveal()
    {
        StopCurrentPreset();
        currentPresetCoroutine = StartCoroutine(RevealCoroutine());
    }
    
    IEnumerator RevealCoroutine()
    {
        if (target == null || targetCamera == null)
        {
            Debug.LogWarning("CameraPresets: Missing target or camera for Reveal");
            yield break;
        }
        
        Debug.Log("🎬 Starting Reveal preset");
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        // Start from a dramatic high angle and distance
        Vector3 startPos = target.position + new Vector3(0, revealStartDistance * 0.5f, revealStartDistance);
        Vector3 endPos = target.position + new Vector3(0, 5f, revealEndDistance);
        
        targetCamera.transform.position = startPos;
        
        float elapsed = 0f;
        float startFOV = 70f;
        float endFOV = 60f;
        
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / revealDuration;
            
            // Dramatic ease-in curve
            float smoothT = t * t; // Quadratic ease-in
            
            targetCamera.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            targetCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, smoothT);
            targetCamera.transform.LookAt(target.position);
            
            yield return null;
        }
        
        targetCamera.transform.position = endPos;
        targetCamera.fieldOfView = endFOV;
        targetCamera.transform.LookAt(target.position);
        
        Debug.Log("🎬 Reveal complete");
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    /// <summary>
    /// Preset 5: Top-down view (bird's eye view)
    /// </summary>
    public void StartTopDownView()
    {
        StopCurrentPreset();
        currentPresetCoroutine = StartCoroutine(TopDownViewCoroutine());
    }
    
    IEnumerator TopDownViewCoroutine()
    {
        if (target == null || targetCamera == null)
        {
            Debug.LogWarning("CameraPresets: Missing target or camera for Top-Down View");
            yield break;
        }
        
        Debug.Log("🎬 Starting Top-Down View preset");
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        Vector3 startPos = targetCamera.transform.position;
        Quaternion startRot = targetCamera.transform.rotation;
        
        Vector3 endPos = target.position + Vector3.up * 30f;
        Quaternion endRot = Quaternion.Euler(90f, 0f, 0f); // Look straight down
        
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            targetCamera.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            targetCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, smoothT);
            
            yield return null;
        }
        
        targetCamera.transform.position = endPos;
        targetCamera.transform.rotation = endRot;
        
        Debug.Log("🎬 Top-Down View complete");
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    /// <summary>
    /// Preset 6: Spiral in (spiral path moving closer)
    /// </summary>
    public void StartSpiralIn()
    {
        StopCurrentPreset();
        currentPresetCoroutine = StartCoroutine(SpiralInCoroutine());
    }
    
    IEnumerator SpiralInCoroutine()
    {
        if (target == null || targetCamera == null)
        {
            Debug.LogWarning("CameraPresets: Missing target or camera for Spiral In");
            yield break;
        }
        
        Debug.Log("🎬 Starting Spiral In preset");
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        float duration = 5f;
        float elapsed = 0f;
        float startRadius = 30f;
        float endRadius = 10f;
        float startHeight = 20f;
        float endHeight = 5f;
        float rotations = 3f; // Number of full rotations
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Interpolate radius and height
            float radius = Mathf.Lerp(startRadius, endRadius, smoothT);
            float height = Mathf.Lerp(startHeight, endHeight, smoothT);
            
            // Calculate spiral angle
            float angle = t * rotations * 360f;
            float radians = angle * Mathf.Deg2Rad;
            
            // Calculate position
            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * radius,
                height,
                Mathf.Sin(radians) * radius
            );
            
            targetCamera.transform.position = target.position + offset;
            targetCamera.transform.LookAt(target.position);
            
            yield return null;
        }
        
        Debug.Log("🎬 Spiral In complete");
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    /// <summary>
    /// Preset 7: Figure-8 orbit
    /// </summary>
    public void StartFigure8()
    {
        StopCurrentPreset();
        currentPresetCoroutine = StartCoroutine(Figure8Coroutine());
    }
    
    IEnumerator Figure8Coroutine()
    {
        if (target == null || targetCamera == null)
        {
            Debug.LogWarning("CameraPresets: Missing target or camera for Figure-8");
            yield break;
        }
        
        Debug.Log("🎬 Starting Figure-8 preset");
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        float duration = 8f; // One full figure-8
        float elapsed = 0f;
        float radius = 15f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Lemniscate of Bernoulli (figure-8 curve)
            float angle = t * Mathf.PI * 2f;
            float scale = radius / (1f + Mathf.Sin(angle) * Mathf.Sin(angle));
            
            float x = scale * Mathf.Cos(angle);
            float z = scale * Mathf.Sin(angle) * Mathf.Cos(angle);
            
            Vector3 offset = new Vector3(x, 10f, z);
            targetCamera.transform.position = target.position + offset;
            targetCamera.transform.LookAt(target.position);
            
            yield return null;
        }
        
        Debug.Log("🎬 Figure-8 complete");
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    /// <summary>
    /// Get all available preset names
    /// </summary>
    public static string[] GetPresetNames()
    {
        return new string[]
        {
            "Fly-Around",
            "Zoom In",
            "Dolly Zoom",
            "Reveal",
            "Top-Down View",
            "Spiral In",
            "Figure-8"
        };
    }
    
    /// <summary>
    /// Execute a preset by name
    /// </summary>
    public void ExecutePreset(string presetName)
    {
        switch (presetName)
        {
            case "Fly-Around":
                StartFlyAround();
                break;
            case "Zoom In":
                StartZoomIn();
                break;
            case "Dolly Zoom":
                StartDollyZoom();
                break;
            case "Reveal":
                StartReveal();
                break;
            case "Top-Down View":
                StartTopDownView();
                break;
            case "Spiral In":
                StartSpiralIn();
                break;
            case "Figure-8":
                StartFigure8();
                break;
            default:
                Debug.LogWarning($"CameraPresets: Unknown preset '{presetName}'");
                break;
        }
    }
    
    /// <summary>
    /// Execute a preset by index
    /// </summary>
    public void ExecutePreset(int presetIndex)
    {
        string[] presets = GetPresetNames();
        if (presetIndex >= 0 && presetIndex < presets.Length)
        {
            ExecutePreset(presets[presetIndex]);
        }
        else
        {
            Debug.LogWarning($"CameraPresets: Preset index {presetIndex} out of range");
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validates Unity scene setup on entering Play mode.
/// Checks for common issues that would prevent the Spirograph game from running.
/// </summary>
[InitializeOnLoad]
public class PlayModeValidator
{
    static PlayModeValidator()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Run validation after a short delay to ensure all Start() methods have run
            EditorApplication.delayCall += ValidateScene;
        }
    }
    
    private static void ValidateScene()
    {
        Debug.Log("=== SPIROGRAPH VALIDATION STARTED ===");
        
        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();
        List<string> info = new List<string>();
        
        // 1. Check SpirographUIManager
        SpirographUIManager uiManager = Object.FindFirstObjectByType<SpirographUIManager>();
        if (uiManager == null)
        {
            errors.Add("SpirographUIManager not found in scene!");
        }
        else
        {
            info.Add("✓ SpirographUIManager found");
            
            if (uiManager.activeRoller == null)
            {
                errors.Add("SpirographUIManager.activeRoller is NULL! No rotor assigned.");
            }
            else
            {
                info.Add($"✓ Active Rotor: {uiManager.activeRoller.gameObject.name}");
                
                // Check activeRoller pathPoints
                if (uiManager.activeRoller.pathPoints == null || uiManager.activeRoller.pathPoints.Length == 0)
                {
                    errors.Add($"Active Rotor '{uiManager.activeRoller.gameObject.name}' has NO pathPoints assigned!");
                }
                else
                {
                    info.Add($"✓ Active Rotor has {uiManager.activeRoller.pathPoints.Length} path points");
                }
            }
            
            // Check UI component assignments
            if (uiManager.speedSlider == null) warnings.Add("Speed slider not assigned to UIManager");
            if (uiManager.pauseButton == null) warnings.Add("Pause button not assigned to UIManager");
            if (uiManager.multiAgentManager == null) warnings.Add("MultiAgentManager not assigned to UIManager");
            if (uiManager.sharedPathState == null) warnings.Add("SharedPathState not assigned to UIManager");
            if (uiManager.agentPanelUI == null) warnings.Add("AgentPanelUI not assigned to UIManager");
        }
        
        // 2. Check SharedPathState
        SharedPathState sharedState = Object.FindFirstObjectByType<SharedPathState>();
        if (sharedState == null)
        {
            warnings.Add("SharedPathState not found (needed for multi-agent mode)");
        }
        else
        {
            info.Add("✓ SharedPathState found");
            
            if (sharedState.pathPoints == null || sharedState.pathPoints.Length == 0)
            {
                errors.Add("SharedPathState has NO pathPoints! Multi-agent mode will FAIL.");
            }
            else
            {
                info.Add($"✓ SharedPathState has {sharedState.pathPoints.Length} path points");
            }
        }
        
        // 3. Check MultiAgentManager
        MultiAgentManager multiAgentManager = Object.FindFirstObjectByType<MultiAgentManager>();
        if (multiAgentManager == null)
        {
            warnings.Add("MultiAgentManager not found (needed for multi-agent mode)");
        }
        else
        {
            info.Add("✓ MultiAgentManager found");
            
            if (multiAgentManager.sharedState == null)
            {
                errors.Add("MultiAgentManager.sharedState is NULL!");
            }
            else
            {
                info.Add("✓ MultiAgentManager linked to SharedPathState");
            }
            
            if (multiAgentManager.agentPrefab == null && !multiAgentManager.autoCreateAgents)
            {
                warnings.Add("MultiAgentManager has no agentPrefab and autoCreateAgents is FALSE");
            }
        }
        
        // 4. Check AgentPanelUI
        AgentPanelUI agentPanel = Object.FindFirstObjectByType<AgentPanelUI>();
        if (agentPanel == null)
        {
            warnings.Add("AgentPanelUI not found in scene");
        }
        else
        {
            info.Add("✓ AgentPanelUI found");
            
            if (agentPanel.agentPanel == null)
            {
                errors.Add("AgentPanelUI.agentPanel GameObject is NULL! Panel not created.");
            }
            else
            {
                info.Add($"✓ Agent Panel GameObject: {agentPanel.agentPanel.name}");
            }
            
            if (agentPanel.agentListContent == null)
            {
                errors.Add("AgentPanelUI.agentListContent is NULL! Agent cards won't display.");
            }
            else
            {
                info.Add("✓ Agent List Content container exists");
            }
            
            if (agentPanel.globalStatsText == null)
            {
                errors.Add("AgentPanelUI.globalStatsText is NULL! Stats won't display.");
            }
            else
            {
                info.Add("✓ Global Stats Text exists");
            }
            
            if (agentPanel.agentManager == null)
            {
                errors.Add("AgentPanelUI.agentManager is NULL! Panel can't track agents.");
            }
            else
            {
                info.Add("✓ AgentPanelUI linked to MultiAgentManager");
            }
        }
        
        // 5. Check EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            errors.Add("EventSystem not found! UI buttons won't work.");
        }
        else
        {
            info.Add("✓ EventSystem found");
        }
        
        // 6. Check CameraController
        CameraController cameraController = Object.FindFirstObjectByType<CameraController>();
        if (cameraController == null)
        {
            warnings.Add("CameraController not found");
        }
        else
        {
            info.Add("✓ CameraController found");
            
            if (cameraController.target == null)
            {
                warnings.Add("CameraController has no target assigned");
            }
        }
        
        // 7. Check Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            errors.Add("Canvas not found! UI won't render.");
        }
        else
        {
            info.Add($"✓ Canvas found: {canvas.gameObject.name}");
        }
        
        // 8. Check for SpirographRoller instances
        SpirographRoller[] allRotors = Object.FindObjectsByType<SpirographRoller>(FindObjectsSortMode.None);
        if (allRotors.Length == 0)
        {
            errors.Add("NO SpirographRoller components found in scene!");
        }
        else
        {
            info.Add($"✓ Found {allRotors.Length} SpirographRoller(s) in scene");
            
            foreach (var rotor in allRotors)
            {
                if (rotor.pathPoints == null || rotor.pathPoints.Length == 0)
                {
                    errors.Add($"Rotor '{rotor.gameObject.name}' has no pathPoints!");
                }
            }
        }
        
        // Print Results
        Debug.Log("===========================================");
        Debug.Log($"<color=cyan>INFO MESSAGES ({info.Count}):</color>");
        foreach (string message in info)
        {
            Debug.Log($"<color=cyan>{message}</color>");
        }
        
        if (warnings.Count > 0)
        {
            Debug.Log("===========================================");
            Debug.Log($"<color=yellow>WARNINGS ({warnings.Count}):</color>");
            foreach (string warning in warnings)
            {
                Debug.LogWarning(warning);
            }
        }
        
        if (errors.Count > 0)
        {
            Debug.Log("===========================================");
            Debug.Log($"<color=red>ERRORS ({errors.Count}):</color>");
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }
        }
        
        Debug.Log("===========================================");
        
        if (errors.Count == 0 && warnings.Count == 0)
        {
            Debug.Log("<color=green>✓✓✓ ALL CHECKS PASSED! Game should run perfectly. ✓✓✓</color>");
        }
        else if (errors.Count == 0)
        {
            Debug.Log("<color=yellow>⚠ Scene has warnings but should still work. ⚠</color>");
        }
        else
        {
            Debug.Log("<color=red>✖✖✖ CRITICAL ERRORS FOUND! Game may not work correctly. ✖✖✖</color>");
        }
        
        Debug.Log("=== SPIROGRAPH VALIDATION COMPLETED ===");
    }
    
    [MenuItem("Spirograph/Validate Scene Now")]
    public static void ValidateSceneMenuItem()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Validation should run in Play Mode for accurate results. Enter Play Mode first.");
            return;
        }
        
        ValidateScene();
    }
}

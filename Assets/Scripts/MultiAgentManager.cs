using UnityEngine;
using System.Collections.Generic;
// Performance: Removed System.Linq to avoid LINQ allocations

/// <summary>
/// Manages multiple PathAgents drawing simultaneously on a shared path.
/// Handles spawning, color modes, spawn modes, and agent lifecycle.
/// </summary>
public class MultiAgentManager : MonoBehaviour
{
    [Header("Shared State")]
    [Tooltip("The shared state all agents will read from")]
    public SharedPathState sharedState;
    
    [Header("Agent Configuration")]
    [Tooltip("Number of agents to spawn (1-16)")]
    [Range(1, 16)]
    public int agentCount = 1;
    
    [Tooltip("Prefab for agent rotor (must have PathAgent component)")]
    public GameObject agentPrefab;
    
    [Tooltip("Auto-create simple sphere agents if no prefab provided")]
    public bool autoCreateAgents = true;
    
    [Header("Color Mode")]
    public AgentColorMode colorMode = AgentColorMode.Rainbow;
    
    public enum AgentColorMode
    {
        Master,     // All agents use master color
        Rainbow,    // Distribute agents across hue spectrum
        Individual, // Each agent gets a predefined color from palette
        Custom      // Agents can have custom colors set individually
    }
    
    [Header("Spawn Mode")]
    public AgentSpawnMode spawnMode = AgentSpawnMode.Simultaneous;
    
    public enum AgentSpawnMode
    {
        Simultaneous,   // All agents start at once, same position
        Sequential,     // Agents start one after another with delay
        Staggered,      // Agents distributed evenly along path
        Competitive     // Agents start together, race with speed variations
    }
    
    [Header("Spawn Configuration")]
    [Tooltip("Delay between agent spawns in Sequential mode (seconds)")]
    public float sequentialDelay = 0.5f;
    
    [Tooltip("Speed variation for Competitive mode (±%)")]
    [Range(0f, 0.5f)]
    public float competitiveSpeedVariation = 0.2f;
    
    [Header("Active Agents")]
    [Tooltip("List of all spawned agents")]
    public List<PathAgent> agents = new List<PathAgent>();
    
    [Tooltip("Currently selected agent (for camera follow and UI)")]
    public PathAgent selectedAgent = null;
    
    [Header("Global Stats")]
    public int activeAgentCount = 0;
    public int pausedAgentCount = 0;
    public int completedAgentCount = 0;
    public float averageProgress = 0f;
    public float totalDistanceCovered = 0f;
    
    [Header("Multi-Agent Mode")]
    [Tooltip("Is multi-agent mode currently enabled?")]
    public bool isMultiAgentMode = false;
    
    // Events
    public delegate void AgentEventHandler(PathAgent agent);
    public event AgentEventHandler OnAgentSelected;
    public event AgentEventHandler OnAgentCompleted;
    public event System.Action OnAgentsSpawned; // Fired when agents finish spawning
    
    void Start()
    {
        // Validate shared state
        if (sharedState == null)
        {
            Debug.LogError("[MultiAgentManager] No SharedPathState assigned! Creating one...");
            GameObject stateObj = new GameObject("SharedPathState");
            sharedState = stateObj.AddComponent<SharedPathState>();
        }
    }
    
    void Update()
    {
        if (!isMultiAgentMode || agents == null || agents.Count == 0) return;
        
        // Update global stats
        UpdateGlobalStats();
    }
    
    /// <summary>
    /// Enable multi-agent mode and spawn agents
    /// </summary>
    public void EnableMultiAgentMode()
    {
        if (isMultiAgentMode)
        {
            Debug.LogWarning("[MultiAgentManager] Multi-agent mode already enabled");
            return;
        }
        
        isMultiAgentMode = true;
        SpawnAgents();
        Debug.Log($"[MultiAgentManager] Multi-agent mode enabled with {agentCount} agents");
    }
    
    /// <summary>
    /// Disable multi-agent mode and clean up agents
    /// </summary>
    public void DisableMultiAgentMode()
    {
        if (!isMultiAgentMode)
        {
            return;
        }
        
        isMultiAgentMode = false;
        ClearAllAgents();
        Debug.Log("[MultiAgentManager] Multi-agent mode disabled");
    }
    
    /// <summary>
    /// Spawn all agents according to current configuration
    /// </summary>
    public void SpawnAgents()
    {
        // Clear existing agents first
        ClearAllAgents();
        
        // Validate shared state
        if (sharedState == null)
        {
            Debug.LogError("[MultiAgentManager] No SharedPathState assigned! Cannot spawn agents.");
            return;
        }
        
        // Validate path points
        if (sharedState.pathPoints == null || sharedState.pathPoints.Length == 0)
        {
            Debug.LogError("[MultiAgentManager] No path points in SharedPathState! Cannot spawn agents.");
            return;
        }
        
        // Validate agent count - clamp to safe range
        agentCount = Mathf.Clamp(agentCount, 1, 16);
        if (agentCount < 1)
        {
            Debug.LogWarning("[MultiAgentManager] Agent count was less than 1. Clamped to 1.");
            agentCount = 1;
        }
        else if (agentCount > 16)
        {
            Debug.LogWarning("[MultiAgentManager] Agent count exceeded 16. Clamped to 16.");
        }
        
        // Spawn based on mode
        switch (spawnMode)
        {
            case AgentSpawnMode.Simultaneous:
                SpawnAgentsSimultaneous();
                break;
                
            case AgentSpawnMode.Sequential:
                StartCoroutine(SpawnAgentsSequential());
                break;
                
            case AgentSpawnMode.Staggered:
                SpawnAgentsStaggered();
                break;
                
            case AgentSpawnMode.Competitive:
                SpawnAgentsCompetitive();
                break;
        }
        
        // Select first agent by default
        if (agents.Count > 0)
        {
            SelectAgent(0);
        }
        
        Debug.Log($"[MultiAgentManager] Spawned {agents.Count} agents in {spawnMode} mode");
        
        // Notify listeners that agents are ready
        OnAgentsSpawned?.Invoke();
    }
    
    void SpawnAgentsSimultaneous()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            if (agent != null)
            {
                agent.startPositionPercent = 0f; // All start at the same position
                agent.speedMultiplier = 1f;
                // Don't start automatically - let user click Idle button
                Debug.Log($"[MultiAgentManager] Agent {i} created (Idle). Click 'Idle' button to start drawing.");
            }
        }
    }
    
    System.Collections.IEnumerator SpawnAgentsSequential()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            if (agent != null)
            {
                agent.startPositionPercent = 0f;
                agent.speedMultiplier = 1f;
                // Don't start automatically - let user control
            }
            
            if (i < agentCount - 1) // Don't wait after last agent
            {
                yield return new WaitForSeconds(sequentialDelay);
            }
        }
    }
    
    void SpawnAgentsStaggered()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            if (agent != null)
            {
                // Distribute agents evenly along the path
                agent.startPositionPercent = (float)i / agentCount;
                agent.speedMultiplier = 1f;
                
                // Set segment boundaries for visualization
                agent.segmentStart = (float)i / agentCount;
                agent.segmentEnd = (float)(i + 1) / agentCount;
                
                // Don't start automatically - let user control
            }
        }
    }
    
    void SpawnAgentsCompetitive()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            if (agent != null)
            {
                agent.startPositionPercent = 0f; // All start together
                
                // Add random speed variation for competition
                float variation = Random.Range(-competitiveSpeedVariation, competitiveSpeedVariation);
                agent.speedMultiplier = 1f + variation;
                
                // Don't start automatically - let user control
            }
        }
    }
    
    public PathAgent CreateAgent(int index)
    {
        // Validate shared state before creating agent
        if (sharedState == null)
        {
            Debug.LogError("[MultiAgentManager] Cannot create agent - SharedPathState is null!");
            return null;
        }
        
        if (sharedState.pathPoints == null || sharedState.pathPoints.Length == 0)
        {
            Debug.LogError("[MultiAgentManager] Cannot create agent - no path points available!");
            return null;
        }
        
        GameObject agentObj;
        
        // Create agent GameObject
        if (agentPrefab != null)
        {
            agentObj = Instantiate(agentPrefab, transform);
        }
        else if (autoCreateAgents)
        {
            agentObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            agentObj.transform.SetParent(transform);
            agentObj.transform.localScale = Vector3.one * 0.4f;
            
            // Add PathAgent component
            PathAgent agentComponent = agentObj.AddComponent<PathAgent>();
        }
        else
        {
            Debug.LogError("[MultiAgentManager] No agent prefab and autoCreateAgents is false!");
            return null;
        }
        
        agentObj.name = $"Agent_{index}";
        
        // Get or add PathAgent component
        PathAgent agent = agentObj.GetComponent<PathAgent>();
        if (agent == null)
        {
            agent = agentObj.AddComponent<PathAgent>();
        }
        
        // Validate agent was successfully created
        if (agent == null)
        {
            Debug.LogError("[MultiAgentManager] Failed to create PathAgent component!");
            Destroy(agentObj);
            return null;
        }
        
        // Configure agent
        agent.agentIndex = index;
        agent.agentName = $"Agent {index}";
        agent.sharedState = sharedState;
        
        // Set color based on mode
        Color agentColor = sharedState.GetAgentColor(colorMode, index, agentCount);
        agent.agentColor = agentColor;
        
        // Apply color to renderer if exists
        Renderer renderer = agentObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader standardShader = Shader.Find("Standard");
            if (standardShader != null)
            {
                Material mat = new Material(standardShader);
                mat.color = agentColor;
                mat.SetFloat("_Metallic", 0.6f);
                mat.SetFloat("_Glossiness", 0.8f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", agentColor * 1.5f);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                renderer.material = mat;
            }
            else
            {
                Debug.LogWarning("[MultiAgentManager] Standard shader not found - agent may not render correctly");
            }
        }
        
        // Add to agents list
        agents.Add(agent);
        
        // Add spawn animation (scale up from small)
        agentObj.transform.localScale = Vector3.zero;
        StartCoroutine(AnimateAgentSpawn(agentObj.transform));
        
        return agent;
    }
    
    /// <summary>
    /// Animate agent spawning (scale up effect)
    /// </summary>
    System.Collections.IEnumerator AnimateAgentSpawn(Transform agentTransform)
    {
        Vector3 targetScale = Vector3.one * 0.4f; // Final scale for agent
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration && agentTransform != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth ease out
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            agentTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, smoothT);
            yield return null;
        }
        
        if (agentTransform != null)
        {
            agentTransform.localScale = targetScale;
        }
    }
    
    /// <summary>
    /// Clear all spawned agents
    /// </summary>
    public void ClearAllAgents()
    {
        foreach (PathAgent agent in agents)
        {
            if (agent != null)
            {
                Destroy(agent.gameObject);
            }
        }
        
        agents.Clear();
        selectedAgent = null;
        ResetGlobalStats();
        
        Debug.Log("[MultiAgentManager] All agents cleared");
    }
    
    /// <summary>
    /// Select an agent by index (for camera follow and UI focus)
    /// </summary>
    public void SelectAgent(int index)
    {
        // Validate agents list
        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[MultiAgentManager] Cannot select agent - no agents exist.");
            selectedAgent = null;
            return;
        }
        
        // Validate index
        if (index < 0 || index >= agents.Count)
        {
            Debug.LogWarning($"[MultiAgentManager] Invalid agent index: {index}. Valid range is 0-{agents.Count - 1}");
            return;
        }
        
        // Validate agent at index
        PathAgent agent = agents[index];
        if (agent == null)
        {
            Debug.LogWarning($"[MultiAgentManager] Agent at index {index} is null!");
            return;
        }
        
        selectedAgent = agent;
        OnAgentSelected?.Invoke(selectedAgent);
        
        Debug.Log($"[MultiAgentManager] Selected Agent {index}");
    }
    
    /// <summary>
    /// Pause a specific agent
    /// </summary>
    public void PauseAgent(int index)
    {
        if (agents == null || index < 0 || index >= agents.Count)
        {
            Debug.LogWarning($"[MultiAgentManager] Cannot pause agent - invalid index: {index}");
            return;
        }
        
        PathAgent agent = agents[index];
        if (agent != null)
        {
            agent.Pause();
        }
        else
        {
            Debug.LogWarning($"[MultiAgentManager] Cannot pause agent {index} - agent is null");
        }
    }
    
    /// <summary>
    /// Resume a specific agent
    /// </summary>
    public void ResumeAgent(int index)
    {
        if (agents == null || index < 0 || index >= agents.Count)
        {
            Debug.LogWarning($"[MultiAgentManager] Cannot resume agent - invalid index: {index}");
            return;
        }
        
        PathAgent agent = agents[index];
        if (agent != null)
        {
            agent.Resume();
        }
        else
        {
            Debug.LogWarning($"[MultiAgentManager] Cannot resume agent {index} - agent is null");
        }
    }
    
    /// <summary>
    /// Pause all agents
    /// </summary>
    public void PauseAllAgents()
    {
        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[MultiAgentManager] No agents to pause.");
            return;
        }
        
        foreach (PathAgent agent in agents)
        {
            if (agent != null)
            {
                agent.Pause();
            }
        }
    }
    
    /// <summary>
    /// Start all agents (for Idle agents)
    /// </summary>
    public void StartAllAgents()
    {
        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[MultiAgentManager] No agents to start.");
            return;
        }
        
        int startedCount = 0;
        foreach (PathAgent agent in agents)
        {
            if (agent != null && agent.status == PathAgent.AgentStatus.Idle)
            {
                agent.StartDrawing();
                startedCount++;
            }
        }
        Debug.Log($"[MultiAgentManager] Started {startedCount} idle agent(s)");
    }
    
    /// <summary>
    /// Resume all paused agents
    /// </summary>
    public void ResumeAllAgents()
    {
        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[MultiAgentManager] No agents to resume.");
            return;
        }
        
        int resumedCount = 0;
        foreach (PathAgent agent in agents)
        {
            if (agent != null && agent.status == PathAgent.AgentStatus.Paused)
            {
                agent.Resume();
                resumedCount++;
            }
        }
        Debug.Log($"[MultiAgentManager] Resumed {resumedCount} paused agent(s)");
    }
    
    /// <summary>
    /// Reset all agents to starting positions
    /// </summary>
    public void ResetAllAgents()
    {
        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[MultiAgentManager] No agents to reset.");
            ResetGlobalStats();
            return;
        }
        
        int resetCount = 0;
        foreach (PathAgent agent in agents)
        {
            if (agent != null)
            {
                agent.ResetAgent();
                resetCount++;
            }
        }
        
        ResetGlobalStats();
        Debug.Log($"[MultiAgentManager] Reset {resetCount} agent(s)");
    }
    
    /// <summary>
    /// Update global statistics
    /// </summary>
    void UpdateGlobalStats()
    {
        if (agents.Count == 0) return;
        
        activeAgentCount = 0;
        pausedAgentCount = 0;
        completedAgentCount = 0;
        totalDistanceCovered = 0f;
        float totalProgress = 0f;
        
        foreach (PathAgent agent in agents)
        {
            if (agent == null) continue;
            
            switch (agent.status)
            {
                case PathAgent.AgentStatus.Active:
                    activeAgentCount++;
                    break;
                case PathAgent.AgentStatus.Paused:
                    pausedAgentCount++;
                    break;
                case PathAgent.AgentStatus.Completed:
                    completedAgentCount++;
                    break;
            }
            
            totalDistanceCovered += agent.totalDistanceTraveled;
            
            // Calculate progress (cycle + position within cycle)
            float cycleProgress = agent.currentCycle;
            float positionProgress = agent.currentDistance / Mathf.Max(1f, agent.totalPathLength);
            totalProgress += cycleProgress + positionProgress;
        }
        
        averageProgress = totalProgress / agents.Count;
        
        // Update shared state with agent count
        if (sharedState != null)
        {
            sharedState.activeAgentCount = activeAgentCount;
        }
    }
    
    void ResetGlobalStats()
    {
        activeAgentCount = 0;
        pausedAgentCount = 0;
        completedAgentCount = 0;
        averageProgress = 0f;
        totalDistanceCovered = 0f;
    }
    
    /// <summary>
    /// Update agent count (will respawn agents)
    /// </summary>
    public void SetAgentCount(int count)
    {
        count = Mathf.Clamp(count, 1, 16);
        if (count != agentCount)
        {
            agentCount = count;
            if (isMultiAgentMode)
            {
                SpawnAgents(); // Respawn with new count
            }
        }
    }
    
    /// <summary>
    /// Update color mode (will recolor existing agents)
    /// </summary>
    public void SetColorMode(AgentColorMode mode)
    {
        colorMode = mode;
        
        // Validate before recoloring
        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[MultiAgentManager] No agents to recolor.");
            return;
        }
        
        if (sharedState == null)
        {
            Debug.LogWarning("[MultiAgentManager] Cannot recolor agents - SharedPathState is null.");
            return;
        }
        
        // Recolor existing agents
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i] != null)
            {
                Color newColor = sharedState.GetAgentColor(colorMode, i, agents.Count);
                agents[i].SetColor(newColor);
            }
        }
        
        Debug.Log($"[MultiAgentManager] Color mode changed to {mode}");
    }
    
    /// <summary>
    /// Update spawn mode (will respawn agents)
    /// </summary>
    public void SetSpawnMode(AgentSpawnMode mode)
    {
        if (mode != spawnMode)
        {
            spawnMode = mode;
            if (isMultiAgentMode)
            {
                SpawnAgents(); // Respawn with new mode
            }
        }
    }
    
    /// <summary>
    /// Get agent by index
    /// </summary>
    public PathAgent GetAgent(int index)
    {
        if (agents == null || index < 0 || index >= agents.Count)
        {
            return null;
        }
        return agents[index];
    }
    
    /// <summary>
    /// Get list of all agents (for UI display)
    /// </summary>
    public List<PathAgent> GetAllAgents()
    {
        if (agents == null)
        {
            return new List<PathAgent>();
        }
        return new List<PathAgent>(agents);
    }
    
    /// <summary>
    /// Remove a specific agent by index
    /// </summary>
    public void RemoveAgent(int index)
    {
        // Validate agents list
        if (agents == null || agents.Count == 0)
        {
            Debug.LogWarning("[MultiAgentManager] Cannot remove agent - no agents exist");
            return;
        }
        
        // Validate index
        if (index < 0 || index >= agents.Count)
        {
            Debug.LogWarning($"[MultiAgentManager] Cannot remove agent - invalid index: {index}. Valid range: 0-{agents.Count - 1}");
            return;
        }
        
        PathAgent agent = agents[index];
        
        // Clear selection if removed agent was selected
        if (selectedAgent == agent)
        {
            selectedAgent = null;
            
            // Notify UI Manager to exit per-agent control mode
            try
            {
                SpirographUIManager uiManager = FindFirstObjectByType<SpirographUIManager>();
                if (uiManager != null)
                {
                    // Check if UI Manager references this agent
                    if (uiManager.selectedAgent == agent)
                    {
                        Debug.Log("★ Selected agent was deleted - returning to master control");
                        // Clear the UI Manager's reference first to prevent accessing destroyed object
                        uiManager.selectedAgent = null;
                        
                        // Only exit per-agent control if currently in that mode
                        if (uiManager.perAgentControlMode)
                        {
                            uiManager.ExitPerAgentControl();
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MultiAgentManager] Error notifying UI Manager of agent deletion: {e.Message}");
                // Don't rethrow - continue with agent removal
            }
        }
        
        // Destroy the agent GameObject if it exists
        if (agent != null && agent.gameObject != null)
        {
            Destroy(agent.gameObject);
        }
        
        // Remove from list
        agents.RemoveAt(index);
        
        // Update remaining agent indices
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i] != null)
            {
                agents[i].agentIndex = i;
                agents[i].agentName = $"Agent {i}";
                if (agents[i].gameObject != null)
                {
                    agents[i].gameObject.name = $"Agent_{i}";
                }
            }
        }
        
        Debug.Log($"[MultiAgentManager] Agent {index} removed. {agents.Count} agent(s) remaining.");
    }
    
    /// <summary>
    /// OnDisable - Cleanup event subscriptions
    /// </summary>
    void OnDisable()
    {
        // Clear event handlers to prevent memory leaks
        OnAgentSelected = null;
        OnAgentCompleted = null;
        OnAgentsSpawned = null;
    }
    
    /// <summary>
    /// OnDestroy - Final cleanup
    /// </summary>
    void OnDestroy()
    {
        try
        {
            // Clean up all agents
            if (agents != null && agents.Count > 0)
            {
                ClearAllAgents();
            }
            
            // Clear references
            sharedState = null;
            agentPrefab = null;
            selectedAgent = null;
            
            // Clear event handlers
            OnAgentSelected = null;
            OnAgentCompleted = null;
            OnAgentsSpawned = null;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MultiAgentManager] Error during cleanup: {e.Message}");
        }
    }
}

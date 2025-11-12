using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages skybox switching with 5 assignable materials
/// Assign your custom skybox materials in the Inspector
/// </summary>
public class SkyboxManager : MonoBehaviour
{
    [Header("Skybox Materials")]
    [Tooltip("Assign 5 skybox materials in the Inspector")]
    public Material skybox1;
    public Material skybox2;
    public Material skybox3;
    public Material skybox4;
    public Material skybox5;
    
    [Header("Skybox Names (Optional)")]
    [Tooltip("Custom names for dropdown. If empty, uses 'Skybox 1', 'Skybox 2', etc.")]
    public string skybox1Name = "Starfield";
    public string skybox2Name = "Nebula";
    public string skybox3Name = "Space";
    public string skybox4Name = "Cosmic";
    public string skybox5Name = "Galaxy";
    
    private Material[] skyboxMaterials;
    private string[] skyboxNames;
    
    void Awake()
    {
        // Initialize arrays
        skyboxMaterials = new Material[] { skybox1, skybox2, skybox3, skybox4, skybox5 };
        skyboxNames = new string[] { skybox1Name, skybox2Name, skybox3Name, skybox4Name, skybox5Name };
        
        // Set default skybox if none is set
        if (RenderSettings.skybox == null && skybox1 != null)
        {
            SetSkybox(0);
        }
    }
    
    public void SetSkybox(int index)
    {
        if (index >= 0 && index < skyboxMaterials.Length && skyboxMaterials[index] != null)
        {
            RenderSettings.skybox = skyboxMaterials[index];
            DynamicGI.UpdateEnvironment(); // Update lighting
            Debug.Log($"Skybox changed to: {GetSkyboxName(index)}");
        }
        else
        {
            Debug.LogWarning($"Skybox at index {index} is not assigned!");
        }
    }
    
    public string GetSkyboxName(int index)
    {
        if (index >= 0 && index < skyboxNames.Length && !string.IsNullOrEmpty(skyboxNames[index]))
        {
            return skyboxNames[index];
        }
        return $"Skybox {index + 1}";
    }
    
    public string[] GetAllSkyboxNames()
    {
        string[] names = new string[5];
        for (int i = 0; i < 5; i++)
        {
            names[i] = GetSkyboxName(i);
        }
        return names;
    }
    
    public int GetCurrentSkyboxIndex()
    {
        Material currentSkybox = RenderSettings.skybox;
        if (currentSkybox == null) return -1;
        
        for (int i = 0; i < skyboxMaterials.Length; i++)
        {
            if (skyboxMaterials[i] == currentSkybox)
            {
                return i;
            }
        }
        return -1;
    }
}

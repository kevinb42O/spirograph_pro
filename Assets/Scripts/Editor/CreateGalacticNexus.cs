using UnityEngine;
using UnityEditor;

public class CreateGalacticNexus
{
    [MenuItem("Tools/Create Galactic Nexus")]
    static void Create()
    {
        // Create GameObject
        GameObject go = new GameObject("GalacticNexusGenerator");
        go.transform.position = Vector3.zero;
        
        // Add GeometricPatternGenerator component
        GeometricPatternGenerator generator = go.AddComponent<GeometricPatternGenerator>();
        
        // Use reflection to set the private fields
        var type = typeof(GeometricPatternGenerator);
        
        type.GetField("shapeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, GeometricPatternGenerator.ShapeType.GalacticNexus);
        
        type.GetField("numberOfPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, 1000);
        
        type.GetField("radius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, 5f);
        
        type.GetField("frequencyA", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, 5);
        
        type.GetField("frequencyB", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, 13);
        
        type.GetField("generate3D", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, true);
        
        type.GetField("heightAmplitude", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, 3f);
        
        type.GetField("heightFrequency", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, 7);
        
        type.GetField("drawGizmos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, true);
        
        type.GetField("drawConnections", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, true);
        
        type.GetField("gizmoColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(generator, new Color(0f, 1f, 0.8f, 1f));
        
        // Generate the pattern
        type.GetMethod("GeneratePattern", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            ?.Invoke(generator, null);
        
        Debug.Log(" Galactic Nexus Created! Select it in the hierarchy to see the pattern!");
        Selection.activeGameObject = go;
    }
}

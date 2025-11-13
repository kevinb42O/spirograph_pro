using UnityEngine;

public class ParticleTrailEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public Color particleColor = new Color(0.3f, 0.8f, 1f, 0.8f);
    public int maxParticles = 50;
    public float particleLifetime = 2f;
    public float emissionRate = 10f;
    
    [Header("Movement")]
    public float particleSpeed = 100f;
    public Vector2 directionVariance = new Vector2(30f, 30f);
    
    private ParticleSystem particleSystem;
    
    private void Awake()
    {
        SetupParticleSystem();
    }
    
    private void SetupParticleSystem()
    {
        // Create particle system if it doesn't exist
        particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem == null)
        {
            particleSystem = gameObject.AddComponent<ParticleSystem>();
        }
        
        var main = particleSystem.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = 5f;
        main.startColor = particleColor;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = particleSystem.emission;
        emission.rateOverTime = emissionRate;
        
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;
        
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(particleColor, 0f),
                new GradientColorKey(particleColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("UI/Default"));
    }
    
    public void Play()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }
    
    public void Stop()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
}
using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Creates atmospheric ground fog with particle system
    /// Adds production-quality atmospheric effect to enhance scene depth
    /// </summary>
    public class GroundFogEffect : MonoBehaviour
    {
        [Header("Fog Settings")]
        [Tooltip("Fog color")]
        public Color fogColor = new Color(0.7f, 0.75f, 0.85f, 0.3f); // Light blue-white

        [Tooltip("Fog density (particles per second)")]
        [Range(10, 100)]
        public int fogDensity = 40;

        [Tooltip("Fog height above ground")]
        public float fogHeight = 0.5f;

        [Tooltip("Fog movement speed")]
        public float fogSpeed = 0.2f;

        [Tooltip("Fog coverage radius")]
        public float fogRadius = 25f;

        [Header("Animation")]
        [Tooltip("Enable fog drifting animation")]
        public bool enableDrift = true;

        [Tooltip("Drift speed")]
        public float driftSpeed = 0.1f;

        [Header("Debug")]
        public bool showDebug = false;

        private ParticleSystem fogParticles;

        void Start()
        {
            CreateGroundFog();
        }

        void CreateGroundFog()
        {
            GameObject fogObj = new GameObject("GroundFog");
            fogObj.transform.SetParent(transform);
            fogObj.transform.localPosition = new Vector3(0f, fogHeight, 0f);

            fogParticles = fogObj.AddComponent<ParticleSystem>();
            var main = fogParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, fogSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main.startColor = fogColor;
            main.maxParticles = 500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emission
            var emission = fogParticles.emission;
            emission.rateOverTime = fogDensity;

            // Shape (large circle at ground level)
            var shape = fogParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = fogRadius;
            shape.radiusThickness = 0.5f;

            // Velocity over lifetime (drift)
            if (enableDrift)
            {
                var velocityOverLifetime = fogParticles.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-driftSpeed, driftSpeed);
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-driftSpeed, driftSpeed);
            }

            // Size over lifetime (fade in/out)
            var sizeOverLifetime = fogParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0f);
            sizeCurve.AddKey(0.3f, 1f);
            sizeCurve.AddKey(0.7f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color over lifetime (fade alpha)
            var colorOverLifetime = fogParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(fogColor.a, 0.2f),
                new GradientAlphaKey(fogColor.a, 0.8f),
                new GradientAlphaKey(0f, 1f)
            };
            colorOverLifetime.color = gradient;

            // Rotation over lifetime
            var rotationOverLifetime = fogParticles.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-10f, 10f);

            // Renderer
            var renderer = fogParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateFogMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingFudge = -100; // Render behind most things

            if (showDebug)
                Debug.Log("[GroundFogEffect] Ground fog created");
        }

        Material CreateFogMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 2900; // Render before transparent objects
            return mat;
        }
    }
}

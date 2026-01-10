using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Creates ambient atmospheric particles (fireflies, dust motes, magical sparkles)
    /// Adds life and magic to the environment with glowing particles
    /// </summary>
    public class AmbientParticles : MonoBehaviour
    {
        [Header("Fireflies")]
        [Tooltip("Enable fireflies")]
        public bool enableFireflies = true;

        [Tooltip("Number of fireflies")]
        [Range(5, 50)]
        public int fireflyCount = 20;

        [Tooltip("Firefly color")]
        public Color fireflyColor = new Color(0.8f, 1f, 0.3f); // Yellow-green glow

        [Tooltip("Firefly spawn radius")]
        public float fireflyRadius = 20f;

        [Header("Dust Motes")]
        [Tooltip("Enable floating dust motes")]
        public bool enableDustMotes = true;

        [Tooltip("Dust mote density")]
        [Range(10, 100)]
        public int dustMoteDensity = 30;

        [Tooltip("Dust mote color")]
        public Color dustMoteColor = new Color(0.9f, 0.9f, 1f, 0.3f);

        [Header("Magical Sparkles")]
        [Tooltip("Enable magical sparkles around portal area")]
        public bool enableMagicalSparkles = true;

        [Tooltip("Sparkle density")]
        [Range(5, 30)]
        public int sparkleDensity = 15;

        [Tooltip("Sparkle color")]
        public Color sparkleColor = new Color(0.3f, 0.8f, 1f); // Cyan

        [Tooltip("Sparkle center position (portal location)")]
        public Vector3 sparkleCenter = new Vector3(20f, 2f, 20f);

        [Tooltip("Sparkle radius")]
        public float sparkleRadius = 10f;

        [Header("Debug")]
        public bool showDebug = false;

        private ParticleSystem fireflyParticles;
        private ParticleSystem dustMoteParticles;
        private ParticleSystem sparkleParticles;

        void Start()
        {
            if (enableFireflies)
                CreateFireflies();

            if (enableDustMotes)
                CreateDustMotes();

            if (enableMagicalSparkles)
                CreateMagicalSparkles();
        }

        void CreateFireflies()
        {
            GameObject fireflyObj = new GameObject("Fireflies");
            fireflyObj.transform.SetParent(transform);
            fireflyObj.transform.localPosition = Vector3.zero;

            fireflyParticles = fireflyObj.AddComponent<ParticleSystem>();
            var main = fireflyParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = fireflyColor;
            main.maxParticles = fireflyCount * 2;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emission
            var emission = fireflyParticles.emission;
            emission.rateOverTime = fireflyCount / 5f;

            // Shape (sphere around area)
            var shape = fireflyParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = fireflyRadius;
            shape.radiusThickness = 0.8f;

            // Color over lifetime (pulse glow)
            var colorOverLifetime = fireflyParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(0.3f, 0f),
                new GradientAlphaKey(1f, 0.25f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(1f, 0.75f),
                new GradientAlphaKey(0.3f, 1f)
            };
            colorOverLifetime.color = gradient;

            // Velocity over lifetime (random floating)
            var velocityOverLifetime = fireflyParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0f, 0.5f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

            // Noise (erratic movement)
            var noise = fireflyParticles.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.3f;
            noise.octaveCount = 2;

            // Renderer
            var renderer = fireflyParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateGlowMaterial(fireflyColor);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            if (showDebug)
                Debug.Log($"[AmbientParticles] Created {fireflyCount} fireflies");
        }

        void CreateDustMotes()
        {
            GameObject dustObj = new GameObject("DustMotes");
            dustObj.transform.SetParent(transform);
            dustObj.transform.localPosition = new Vector3(0f, 3f, 0f);

            dustMoteParticles = dustObj.AddComponent<ParticleSystem>();
            var main = dustMoteParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 20f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startColor = dustMoteColor;
            main.maxParticles = 200;
            main.gravityModifier = -0.05f; // Float up slowly

            // Emission
            var emission = dustMoteParticles.emission;
            emission.rateOverTime = dustMoteDensity;

            // Shape (box in viewing area)
            var shape = dustMoteParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 6f, 40f);

            // Velocity over lifetime (gentle drift)
            var velocityOverLifetime = dustMoteParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

            // Color over lifetime (fade)
            var colorOverLifetime = dustMoteParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(dustMoteColor.a, 0.2f),
                new GradientAlphaKey(dustMoteColor.a, 0.8f),
                new GradientAlphaKey(0f, 1f)
            };
            colorOverLifetime.color = gradient;

            // Renderer
            var renderer = dustMoteParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateDustMaterial();

            if (showDebug)
                Debug.Log("[AmbientParticles] Created dust motes");
        }

        void CreateMagicalSparkles()
        {
            GameObject sparkleObj = new GameObject("MagicalSparkles");
            sparkleObj.transform.SetParent(transform);
            sparkleObj.transform.position = sparkleCenter;

            sparkleParticles = sparkleObj.AddComponent<ParticleSystem>();
            var main = sparkleParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = sparkleColor;
            main.maxParticles = 100;

            // Emission
            var emission = sparkleParticles.emission;
            emission.rateOverTime = sparkleDensity;

            // Shape (sphere around portal)
            var shape = sparkleParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = sparkleRadius;

            // Color over lifetime (twinkle)
            var colorOverLifetime = sparkleParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.3f),
                new GradientAlphaKey(0.5f, 0.6f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            };
            colorOverLifetime.color = gradient;

            // Velocity over lifetime (spiral upward)
            var velocityOverLifetime = sparkleParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(0.5f);
            velocityOverLifetime.orbitalZ = new ParticleSystem.MinMaxCurve(0.5f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);

            // Renderer
            var renderer = sparkleParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateGlowMaterial(sparkleColor);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            if (showDebug)
                Debug.Log("[AmbientParticles] Created magical sparkles");
        }

        Material CreateGlowMaterial(Color glowColor)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_BaseColor", glowColor);
            mat.SetColor("_EmissionColor", glowColor * 2f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }

        Material CreateDustMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }

        void OnDrawGizmosSelected()
        {
            if (enableFireflies)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, fireflyRadius);
            }

            if (enableMagicalSparkles)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(sparkleCenter, sparkleRadius);
            }
        }
    }
}

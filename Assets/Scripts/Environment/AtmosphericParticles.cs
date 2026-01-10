using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Controls atmospheric particle effects (fog, dust, pollen, insects)
    /// Optimized for Quest 3 VR performance
    /// </summary>
    public class AtmosphericParticles : MonoBehaviour
    {
        [Header("Fog Settings")]
        [Tooltip("Enable drifting fog particles")]
        public bool enableFog = true;

        [Range(10, 100)]
        [Tooltip("Number of fog particles")]
        public int fogParticleCount = 30;

        [Range(0.5f, 5f)]
        [Tooltip("Fog drift speed")]
        public float fogDriftSpeed = 1.5f;

        [Header("Dust Motes Settings")]
        [Tooltip("Enable floating dust motes")]
        public bool enableDustMotes = true;

        [Range(20, 200)]
        [Tooltip("Number of dust particles")]
        public int dustParticleCount = 80;

        [Header("Pollen Settings")]
        [Tooltip("Enable floating pollen")]
        public bool enablePollen = true;

        [Range(10, 100)]
        [Tooltip("Number of pollen particles")]
        public int pollenParticleCount = 40;

        [Header("Insects Settings")]
        [Tooltip("Enable flying insects")]
        public bool enableInsects = true;

        [Range(5, 30)]
        [Tooltip("Number of insect particles")]
        public int insectParticleCount = 15;

        [Header("References")]
        public ParticleSystem fogSystem;
        public ParticleSystem dustSystem;
        public ParticleSystem pollenSystem;
        public ParticleSystem insectSystem;

        void Start()
        {
            ConfigureAllSystems();
        }

        void ConfigureAllSystems()
        {
            if (fogSystem != null && enableFog)
            {
                ConfigureFogSystem(fogSystem);
            }

            if (dustSystem != null && enableDustMotes)
            {
                ConfigureDustSystem(dustSystem);
            }

            if (pollenSystem != null && enablePollen)
            {
                ConfigurePollenSystem(pollenSystem);
            }

            if (insectSystem != null && enableInsects)
            {
                ConfigureInsectSystem(insectSystem);
            }
        }

        void ConfigureFogSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = fogParticleCount;
            main.startLifetime = 20f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startColor = new Color(0.8f, 0.8f, 0.9f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = fogParticleCount / 20f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 5f, 40f);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-fogDriftSpeed, fogDriftSpeed);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-fogDriftSpeed, fogDriftSpeed);

            ps.Play();
        }

        void ConfigureDustSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = dustParticleCount;
            main.startLifetime = 15f;
            main.startSpeed = 0.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startColor = new Color(0.9f, 0.9f, 0.85f, 0.6f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = dustParticleCount / 15f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 8f, 30f);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.2f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

            ps.Play();
        }

        void ConfigurePollenSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = pollenParticleCount;
            main.startLifetime = 12f;
            main.startSpeed = 0.3f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new Color(1f, 0.95f, 0.7f, 0.7f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = pollenParticleCount / 12f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(25f, 6f, 25f);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.1f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

            ps.Play();
        }

        void ConfigureInsectSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = insectParticleCount;
            main.startLifetime = 8f;
            main.startSpeed = 1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
            main.startColor = new Color(0.3f, 0.3f, 0.2f, 0.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = insectParticleCount / 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 4f, 20f);

            // Erratic movement for insects
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 2f;
            noise.frequency = 0.5f;
            noise.scrollSpeed = 1f;

            ps.Play();
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                ConfigureAllSystems();
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRDungeonCrawler.Environment;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Phase 3: Enhanced Particle Effects
    /// Creates atmospheric and interactive particle systems
    /// Run from menu: Tools/VR Dungeon Crawler/Phase 3 - Setup Particle Effects
    /// </summary>
    public class SetupParticlesPhase3 : UnityEditor.Editor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Phase 3 - Setup Atmospheric Particles")]
        public static void SetupAtmosphericParticles()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 3: Setting Up Atmospheric Particles");
            Debug.Log("========================================");

            // Create parent object
            GameObject particlesParent = GameObject.Find("AtmosphericParticles");
            if (particlesParent == null)
            {
                particlesParent = new GameObject("AtmosphericParticles");
                particlesParent.transform.position = Vector3.zero;
                Debug.Log("✓ Created AtmosphericParticles parent");
            }

            // Add control script
            AtmosphericParticles atmParticles = particlesParent.GetComponent<AtmosphericParticles>();
            if (atmParticles == null)
            {
                atmParticles = particlesParent.AddComponent<AtmosphericParticles>();
            }

            // Create fog system
            GameObject fogGO = CreateOrGetChild(particlesParent, "FogSystem");
            ParticleSystem fogPS = SetupParticleSystem(fogGO, "Fog");
            atmParticles.fogSystem = fogPS;
            ConfigureFogParticles(fogPS);
            Debug.Log("✓ Created fog particle system");

            // Create dust system
            GameObject dustGO = CreateOrGetChild(particlesParent, "DustMotes");
            ParticleSystem dustPS = SetupParticleSystem(dustGO, "Dust");
            atmParticles.dustSystem = dustPS;
            ConfigureDustParticles(dustPS);
            Debug.Log("✓ Created dust motes particle system");

            // Create pollen system
            GameObject pollenGO = CreateOrGetChild(particlesParent, "Pollen");
            ParticleSystem pollenPS = SetupParticleSystem(pollenGO, "Pollen");
            atmParticles.pollenSystem = pollenPS;
            ConfigurePollenParticles(pollenPS);
            Debug.Log("✓ Created pollen particle system");

            // Create insect system
            GameObject insectGO = CreateOrGetChild(particlesParent, "Insects");
            ParticleSystem insectPS = SetupParticleSystem(insectGO, "Insects");
            atmParticles.insectSystem = insectPS;
            ConfigureInsectParticles(insectPS);
            Debug.Log("✓ Created insect particle system");

            EditorUtility.SetDirty(particlesParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Atmospheric particles setup complete!");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 3 - Setup Interactive Particles")]
        public static void SetupInteractiveParticles()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 3: Setting Up Interactive Particles");
            Debug.Log("========================================");

            // Create parent object
            GameObject interactiveParent = GameObject.Find("InteractiveParticles");
            if (interactiveParent == null)
            {
                interactiveParent = new GameObject("InteractiveParticles");
                interactiveParent.transform.position = Vector3.zero;
                Debug.Log("✓ Created InteractiveParticles parent");
            }

            // Add control script
            InteractiveParticles interactive = interactiveParent.GetComponent<InteractiveParticles>();
            if (interactive == null)
            {
                interactive = interactiveParent.AddComponent<InteractiveParticles>();
            }

            // Create dust puff prefab
            GameObject dustPuffPrefab = CreateDustPuffPrefab();
            string dustPrefabPath = "Assets/Prefabs/Particles/DustPuff.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs/Particles");
            PrefabUtility.SaveAsPrefabAsset(dustPuffPrefab, dustPrefabPath);
            Object.DestroyImmediate(dustPuffPrefab);
            interactive.dustPuffPrefab = AssetDatabase.LoadAssetAtPath<ParticleSystem>(dustPrefabPath);
            Debug.Log("✓ Created dust puff prefab");

            // Create firefly prefab
            GameObject fireflyPrefab = CreateFireflyPrefab();
            string fireflyPrefabPath = "Assets/Prefabs/Particles/Firefly.prefab";
            PrefabUtility.SaveAsPrefabAsset(fireflyPrefab, fireflyPrefabPath);
            Object.DestroyImmediate(fireflyPrefab);
            interactive.fireflyPrefab = AssetDatabase.LoadAssetAtPath<ParticleSystem>(fireflyPrefabPath);
            Debug.Log("✓ Created firefly prefab");

            EditorUtility.SetDirty(interactiveParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Interactive particles setup complete!");
            Debug.Log("========================================");
        }

        private static GameObject CreateOrGetChild(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            return child;
        }

        private static ParticleSystem SetupParticleSystem(GameObject go, string name)
        {
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = go.AddComponent<ParticleSystem>();
            }

            // Disable auto-play, will be controlled by script
            var main = ps.main;
            main.playOnAwake = false;

            return ps;
        }

        private static void ConfigureFogParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = 30;
            main.startLifetime = 20f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startColor = new Color(0.8f, 0.8f, 0.9f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.rateOverTime = 1.5f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 5f, 40f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        }

        private static void ConfigureDustParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = 80;
            main.startLifetime = 15f;
            main.startSpeed = 0.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startColor = new Color(0.9f, 0.9f, 0.85f, 0.6f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 5f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 8f, 30f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        }

        private static void ConfigurePollenParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = 40;
            main.startLifetime = 12f;
            main.startSpeed = 0.3f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new Color(1f, 0.95f, 0.7f, 0.7f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 3f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(25f, 6f, 25f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        }

        private static void ConfigureInsectParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = 15;
            main.startLifetime = 8f;
            main.startSpeed = 1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
            main.startColor = new Color(0.3f, 0.3f, 0.2f, 0.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 2f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 4f, 20f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 2f;
            noise.frequency = 0.5f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        }

        private static GameObject CreateDustPuffPrefab()
        {
            GameObject dustPuff = new GameObject("DustPuff");
            ParticleSystem ps = dustPuff.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = 1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startColor = new Color(0.7f, 0.65f, 0.6f, 0.5f);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 5, 10)
            });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0, 0.5f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            ));

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

            return dustPuff;
        }

        private static GameObject CreateFireflyPrefab()
        {
            GameObject firefly = new GameObject("Firefly");
            ParticleSystem ps = firefly.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = true;
            main.startLifetime = float.PositiveInfinity;
            main.startSpeed = 0f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 0.9f, 0.5f, 0.8f);
            main.maxParticles = 1;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 1)
            });

            // Pulsing glow
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.9f, 0.5f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.5f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0.5f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            Material mat = renderer.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 2f);

            return firefly;
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 3 - Complete Setup")]
        public static void CompletePhase3Setup()
        {
            Debug.Log("========================================");
            Debug.Log("RUNNING COMPLETE PHASE 3 SETUP");
            Debug.Log("========================================");

            SetupAtmosphericParticles();
            Debug.Log("");
            SetupInteractiveParticles();

            Debug.Log("");
            Debug.Log("========================================");
            Debug.Log("✓✓✓ PHASE 3 COMPLETE!");
            Debug.Log("Fog, dust, pollen, insects, and interactive effects ready!");
            Debug.Log("========================================");
        }
    }
}

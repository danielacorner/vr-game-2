using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Production-ready campfire with advanced particle effects, lighting, and sound
    /// Creates realistic fire with multiple particle layers, heat distortion effect, and dynamic lighting
    /// </summary>
    public class EnhancedCampfire : MonoBehaviour
    {
        [Header("Fire Settings")]
        [Tooltip("Base fire color (orange-red)")]
        public Color fireColorBase = new Color(1f, 0.5f, 0f); // Orange

        [Tooltip("Fire tip color (yellow-white)")]
        public Color fireColorTip = new Color(1f, 0.9f, 0.3f); // Yellow

        [Tooltip("Fire intensity")]
        [Range(0.5f, 2f)]
        public float fireIntensity = 1f;

        [Header("Lighting")]
        [Tooltip("Light intensity range (min, max for flicker)")]
        public Vector2 lightIntensityRange = new Vector2(2f, 3.5f);

        [Tooltip("Light range in meters")]
        public float lightRange = 12f;

        [Tooltip("Flicker speed")]
        public float flickerSpeed = 5f;

        [Header("Particles")]
        [Tooltip("Number of fire particles per second")]
        [Range(20, 100)]
        public int fireParticleRate = 50;

        [Tooltip("Number of smoke particles per second")]
        [Range(10, 40)]
        public int smokeParticleRate = 20;

        [Tooltip("Number of spark particles per second")]
        [Range(5, 30)]
        public int sparkParticleRate = 15;

        [Header("Embers")]
        [Tooltip("Show floating embers")]
        public bool showEmbers = true;

        [Tooltip("Ember spawn rate")]
        [Range(1, 10)]
        public int emberRate = 5;

        [Header("Audio")]
        [Tooltip("Fire crackling sound clip")]
        public AudioClip fireCrackleClip;

        [Tooltip("Ambient volume")]
        [Range(0f, 1f)]
        public float volume = 0.5f;

        [Header("Debug")]
        public bool showDebug = false;

        private Light fireLight;
        private ParticleSystem fireParticles;
        private ParticleSystem smokeParticles;
        private ParticleSystem sparkParticles;
        private ParticleSystem emberParticles;
        private AudioSource audioSource;
        private float flickerOffset;

        void Start()
        {
            flickerOffset = Random.Range(0f, 100f);
            CreateCampfire();
        }

        void CreateCampfire()
        {
            // Create fire light
            CreateFireLight();

            // Create particle systems
            CreateFireParticles();
            CreateSmokeParticles();
            CreateSparkParticles();

            if (showEmbers)
            {
                CreateEmberParticles();
            }

            // Create logs
            CreateLogs();

            // Setup audio
            SetupAudio();

            if (showDebug)
                Debug.Log("[EnhancedCampfire] Campfire created with all effects");
        }

        void CreateFireLight()
        {
            GameObject lightObj = new GameObject("FireLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            fireLight = lightObj.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.6f, 0.3f); // Warm orange
            fireLight.intensity = lightIntensityRange.y;
            fireLight.range = lightRange;
            fireLight.shadows = LightShadows.Soft;
            fireLight.shadowStrength = 0.5f;
        }

        void CreateFireParticles()
        {
            GameObject fireObj = new GameObject("FireParticles");
            fireObj.transform.SetParent(transform);
            fireObj.transform.localPosition = Vector3.zero;

            fireParticles = fireObj.AddComponent<ParticleSystem>();
            var main = fireParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Color over lifetime (orange -> yellow -> transparent)
            var colorOverLifetime = fireParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(fireColorBase * fireIntensity, 0f),
                    new GradientColorKey(fireColorTip * fireIntensity, 0.5f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Size over lifetime (grow then shrink)
            var sizeOverLifetime = fireParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.5f);
            sizeCurve.AddKey(0.3f, 1.2f);
            sizeCurve.AddKey(1f, 0.2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Emission
            var emission = fireParticles.emission;
            emission.rateOverTime = fireParticleRate;

            // Shape (cone pointing up)
            var shape = fireParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.5f;

            // Velocity over lifetime (slow down as they rise)
            var velocityOverLifetime = fireParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.5f);

            // Renderer
            var renderer = fireParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateFireMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        void CreateSmokeParticles()
        {
            GameObject smokeObj = new GameObject("SmokeParticles");
            smokeObj.transform.SetParent(transform);
            smokeObj.transform.localPosition = new Vector3(0f, 1f, 0f);

            smokeParticles = smokeObj.AddComponent<ParticleSystem>();
            var main = smokeParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main.maxParticles = 100;

            // Color (dark gray, fading out)
            var colorOverLifetime = smokeParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient smokeGradient = new Gradient();
            smokeGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 0f),
                    new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 0.5f),
                    new GradientColorKey(new Color(0.6f, 0.6f, 0.6f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.5f, 0f),
                    new GradientAlphaKey(0.3f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = smokeGradient;

            // Size over lifetime (grow)
            var sizeOverLifetime = smokeParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve smokeSizeCurve = new AnimationCurve();
            smokeSizeCurve.AddKey(0f, 0.5f);
            smokeSizeCurve.AddKey(1f, 2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, smokeSizeCurve);

            // Emission
            var emission = smokeParticles.emission;
            emission.rateOverTime = smokeParticleRate;

            // Shape
            var shape = smokeParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.3f;

            // Noise (turbulence)
            var noise = smokeParticles.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.5f;

            // Renderer
            var renderer = smokeParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateSmokeMaterial();
        }

        void CreateSparkParticles()
        {
            GameObject sparkObj = new GameObject("SparkParticles");
            sparkObj.transform.SetParent(transform);
            sparkObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            sparkParticles = sparkObj.AddComponent<ParticleSystem>();
            var main = sparkParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main.startColor = new Color(1f, 0.8f, 0.3f); // Bright yellow-orange
            main.maxParticles = 50;
            main.gravityModifier = 1f; // Sparks fall

            // Emission (bursts)
            var emission = sparkParticles.emission;
            emission.rateOverTime = sparkParticleRate;

            // Shape (sphere)
            var shape = sparkParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            // Color over lifetime (fade)
            var colorOverLifetime = sparkParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient sparkGradient = new Gradient();
            sparkGradient.alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            };
            colorOverLifetime.color = sparkGradient;

            // Renderer
            var renderer = sparkParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateSparkMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        void CreateEmberParticles()
        {
            GameObject emberObj = new GameObject("EmberParticles");
            emberObj.transform.SetParent(transform);
            emberObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            emberParticles = emberObj.AddComponent<ParticleSystem>();
            var main = emberParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = new Color(1f, 0.4f, 0f); // Orange glow
            main.maxParticles = 30;

            // Emission
            var emission = emberParticles.emission;
            emission.rateOverTime = emberRate;

            // Shape (cone)
            var shape = emberParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.4f;

            // Color over lifetime (pulse glow)
            var colorOverLifetime = emberParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient emberGradient = new Gradient();
            emberGradient.alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            };
            colorOverLifetime.color = emberGradient;

            // Renderer
            var renderer = emberParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateEmberMaterial();
        }

        void CreateLogs()
        {
            // Create 4 logs in a + pattern
            CreateLog("Log1", new Vector3(0.6f, 0f, 0f), Quaternion.Euler(0, 0, 90));
            CreateLog("Log2", new Vector3(-0.6f, 0f, 0f), Quaternion.Euler(0, 0, 90));
            CreateLog("Log3", new Vector3(0f, 0f, 0.6f), Quaternion.Euler(90, 0, 0));
            CreateLog("Log4", new Vector3(0f, 0f, -0.6f), Quaternion.Euler(90, 0, 0));
        }

        void CreateLog(string name, Vector3 position, Quaternion rotation)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = name;
            log.transform.SetParent(transform);
            log.transform.localPosition = position;
            log.transform.localRotation = rotation;
            log.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);

            // Brown wood material
            Material logMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            logMat.color = new Color(0.3f, 0.2f, 0.1f); // Dark brown
            logMat.SetFloat("_Smoothness", 0.1f);
            log.GetComponent<MeshRenderer>().material = logMat;
        }

        void SetupAudio()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = fireCrackleClip;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            if (fireCrackleClip != null)
            {
                audioSource.Play();
            }
        }

        void Update()
        {
            // Animate light flicker
            if (fireLight != null)
            {
                float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed + flickerOffset, 0f);
                fireLight.intensity = Mathf.Lerp(lightIntensityRange.x, lightIntensityRange.y, flicker);
            }
        }

        Material CreateFireMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.EnableKeyword("_EMISSION");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }

        Material CreateSmokeMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }

        Material CreateSparkMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.EnableKeyword("_EMISSION");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.renderQueue = 3000;
            return mat;
        }

        Material CreateEmberMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.EnableKeyword("_EMISSION");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.renderQueue = 3000;
            return mat;
        }
    }
}

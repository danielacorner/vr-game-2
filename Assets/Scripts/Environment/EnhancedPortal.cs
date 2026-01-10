using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Production-ready portal with vortex effect, teleportation, and decorations
    /// Creates swirling magical portal with particles, lighting, and scene transition
    /// </summary>
    public class EnhancedPortal : MonoBehaviour
    {
        [Header("Portal Visual")]
        [Tooltip("Portal color (cyan/purple magical theme)")]
        public Color portalColor = new Color(0.3f, 0.8f, 1f); // Cyan

        [Tooltip("Portal rim color")]
        public Color portalRimColor = new Color(0.8f, 0.3f, 1f); // Purple

        [Tooltip("Portal size")]
        public float portalSize = 2.5f;

        [Tooltip("Portal rotation speed")]
        public float rotationSpeed = 30f;

        [Header("Vortex Effect")]
        [Tooltip("Vortex particle density")]
        [Range(20, 100)]
        public int vortexDensity = 60;

        [Tooltip("Vortex speed")]
        public float vortexSpeed = 2f;

        [Header("Teleportation")]
        [Tooltip("Target scene name to load")]
        public string targetSceneName = "DungeonEntrance";

        [Tooltip("Fade duration when teleporting")]
        public float fadeDuration = 1f;

        [Tooltip("Require button press to teleport (false = auto-teleport on enter)")]
        public bool requireButtonPress = false;

        [Tooltip("Button to press for teleportation (if required)")]
        public string teleportButton = "Fire1";

        [Header("Audio")]
        [Tooltip("Portal ambient hum sound")]
        public AudioClip portalHumClip;

        [Tooltip("Teleport activation sound")]
        public AudioClip teleportSound;

        [Tooltip("Portal volume")]
        [Range(0f, 1f)]
        public float volume = 0.3f;

        [Header("Debug")]
        public bool showDebug = false;

        private ParticleSystem vortexParticles;
        private ParticleSystem rimParticles;
        private Light portalLight;
        private AudioSource ambientAudio;
        private AudioSource teleportAudio;
        private bool playerInPortal = false;
        private GameObject portalMesh;
        private Material portalMaterial;

        void Start()
        {
            CreatePortal();
        }

        void CreatePortal()
        {
            // Create portal mesh (spinning disk)
            CreatePortalMesh();

            // Create vortex particles
            CreateVortexParticles();

            // Create rim particles
            CreateRimParticles();

            // Create portal light
            CreatePortalLight();

            // Setup audio
            SetupAudio();

            // Create decorations
            CreatePortalDecorations();

            if (showDebug)
                Debug.Log($"[EnhancedPortal] Portal created, teleports to scene: {targetSceneName}");
        }

        void CreatePortalMesh()
        {
            portalMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            portalMesh.name = "PortalMesh";
            portalMesh.transform.SetParent(transform);
            portalMesh.transform.localPosition = Vector3.zero;
            portalMesh.transform.localRotation = Quaternion.Euler(90, 0, 0);
            portalMesh.transform.localScale = new Vector3(portalSize, 0.1f, portalSize);

            // Remove collider (we'll use a trigger collider on parent)
            Destroy(portalMesh.GetComponent<Collider>());

            // Create swirling portal material
            portalMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            portalMaterial.EnableKeyword("_EMISSION");
            portalMaterial.SetColor("_BaseColor", portalColor);
            portalMaterial.SetColor("_EmissionColor", portalColor * 3f);
            portalMaterial.SetFloat("_Metallic", 0.8f);
            portalMaterial.SetFloat("_Smoothness", 0.9f);
            portalMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            portalMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            portalMaterial.renderQueue = 3000;

            portalMesh.GetComponent<MeshRenderer>().material = portalMaterial;
        }

        void CreateVortexParticles()
        {
            GameObject vortexObj = new GameObject("VortexParticles");
            vortexObj.transform.SetParent(transform);
            vortexObj.transform.localPosition = Vector3.zero;

            vortexParticles = vortexObj.AddComponent<ParticleSystem>();
            var main = vortexParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = portalColor;
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emission
            var emission = vortexParticles.emission;
            emission.rateOverTime = vortexDensity;

            // Shape (circle at portal entrance)
            var shape = vortexParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = portalSize / 2f;

            // Velocity over lifetime (spiral inward)
            var velocityOverLifetime = vortexParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(vortexSpeed);
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(-2f); // Spiral inward
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f); // Move through portal

            // Color over lifetime (fade)
            var colorOverLifetime = vortexParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(portalColor, 0f),
                    new GradientColorKey(portalRimColor, 0.5f),
                    new GradientColorKey(portalColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Size over lifetime (shrink as they spiral in)
            var sizeOverLifetime = vortexParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0.2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Renderer
            var renderer = vortexParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateVortexMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        void CreateRimParticles()
        {
            GameObject rimObj = new GameObject("RimParticles");
            rimObj.transform.SetParent(transform);
            rimObj.transform.localPosition = Vector3.zero;

            rimParticles = rimObj.AddComponent<ParticleSystem>();
            var main = rimParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startColor = portalRimColor;
            main.maxParticles = 100;

            // Emission
            var emission = rimParticles.emission;
            emission.rateOverTime = 30;

            // Shape (circle edge)
            var shape = rimParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = portalSize / 2f;
            shape.radiusThickness = 0f; // Only on edge

            // Velocity over lifetime (orbit)
            var velocityOverLifetime = rimParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(1f);

            // Color over lifetime (pulse)
            var colorOverLifetime = rimParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            };
            colorOverLifetime.color = gradient;

            // Renderer
            var renderer = rimParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateVortexMaterial();
        }

        void CreatePortalLight()
        {
            GameObject lightObj = new GameObject("PortalLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;

            portalLight = lightObj.AddComponent<Light>();
            portalLight.type = LightType.Point;
            portalLight.color = portalColor;
            portalLight.intensity = 3f;
            portalLight.range = 10f;
            portalLight.shadows = LightShadows.Soft;
        }

        void CreatePortalDecorations()
        {
            // Create 4 floating rune stones around portal
            CreateRuneStone("RuneStone_North", new Vector3(0f, 0f, portalSize + 1f));
            CreateRuneStone("RuneStone_East", new Vector3(portalSize + 1f, 0f, 0f));
            CreateRuneStone("RuneStone_South", new Vector3(0f, 0f, -(portalSize + 1f)));
            CreateRuneStone("RuneStone_West", new Vector3(-(portalSize + 1f), 0f, 0f));

            // Create stone platform
            CreatePlatform();
        }

        void CreateRuneStone(string name, Vector3 localPosition)
        {
            GameObject rune = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rune.name = name;
            rune.transform.SetParent(transform);
            rune.transform.localPosition = localPosition + new Vector3(0f, 1f, 0f);
            rune.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);

            // Rune material (glowing cyan)
            Material runeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            runeMat.EnableKeyword("_EMISSION");
            runeMat.SetColor("_BaseColor", new Color(0.4f, 0.4f, 0.5f));
            runeMat.SetColor("_EmissionColor", portalColor * 2f);
            rune.GetComponent<MeshRenderer>().material = runeMat;

            // Add floating animation
            RuneFloatAnimation floatAnim = rune.AddComponent<RuneFloatAnimation>();
            floatAnim.floatHeight = 0.3f;
            floatAnim.floatSpeed = 1f + Random.Range(-0.2f, 0.2f);
        }

        void CreatePlatform()
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            platform.name = "Platform";
            platform.transform.SetParent(transform);
            platform.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            platform.transform.localScale = new Vector3(portalSize * 1.5f, 0.2f, portalSize * 1.5f);

            // Platform material (stone)
            Material platformMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            platformMat.SetColor("_BaseColor", new Color(0.3f, 0.3f, 0.35f));
            platformMat.SetFloat("_Smoothness", 0.2f);
            platform.GetComponent<MeshRenderer>().material = platformMat;
        }

        void SetupAudio()
        {
            // Ambient hum
            ambientAudio = gameObject.AddComponent<AudioSource>();
            ambientAudio.clip = portalHumClip;
            ambientAudio.loop = true;
            ambientAudio.volume = volume;
            ambientAudio.spatialBlend = 1f;
            ambientAudio.maxDistance = 15f;

            if (portalHumClip != null)
            {
                ambientAudio.Play();
            }

            // Teleport sound
            teleportAudio = gameObject.AddComponent<AudioSource>();
            teleportAudio.clip = teleportSound;
            teleportAudio.loop = false;
            teleportAudio.volume = volume * 1.5f;
            teleportAudio.spatialBlend = 0.5f;
        }

        void Update()
        {
            // Rotate portal mesh
            if (portalMesh != null)
            {
                portalMesh.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }

            // Pulse portal light
            if (portalLight != null)
            {
                float pulse = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
                portalLight.intensity = Mathf.Lerp(2f, 4f, pulse);
            }

            // Check for teleport button
            if (playerInPortal && requireButtonPress && Input.GetButtonDown(teleportButton))
            {
                TriggerTeleport();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Check if player entered portal
            if (other.CompareTag("MainCamera") || other.name.Contains("Camera") || other.name.Contains("Player"))
            {
                playerInPortal = true;

                if (showDebug)
                    Debug.Log("[EnhancedPortal] Player entered portal");

                // Auto-teleport if not requiring button press
                if (!requireButtonPress)
                {
                    TriggerTeleport();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("MainCamera") || other.name.Contains("Camera") || other.name.Contains("Player"))
            {
                playerInPortal = false;

                if (showDebug)
                    Debug.Log("[EnhancedPortal] Player exited portal");
            }
        }

        void TriggerTeleport()
        {
            if (showDebug)
                Debug.Log($"[EnhancedPortal] Teleporting to scene: {targetSceneName}");

            // Play teleport sound
            if (teleportAudio != null && teleportSound != null)
            {
                teleportAudio.Play();
            }

            // Start fade and teleport coroutine
            StartCoroutine(FadeAndTeleport());
        }

        IEnumerator FadeAndTeleport()
        {
            // TODO: Add fade effect here (requires UI canvas)
            // For now, just wait for sound to finish
            yield return new WaitForSeconds(fadeDuration);

            // Load target scene
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("[EnhancedPortal] No target scene specified!");
            }
        }

        Material CreateVortexMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.EnableKeyword("_EMISSION");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, portalSize);
        }
    }

    /// <summary>
    /// Makes rune stones float up and down
    /// </summary>
    public class RuneFloatAnimation : MonoBehaviour
    {
        public float floatHeight = 0.3f;
        public float floatSpeed = 1f;

        private Vector3 startPosition;

        void Start()
        {
            startPosition = transform.localPosition;
        }

        void Update()
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
}

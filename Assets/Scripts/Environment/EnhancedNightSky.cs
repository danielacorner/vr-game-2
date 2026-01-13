using UnityEngine;
using System.Collections.Generic;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Creates a realistic night sky with varied star sizes, colors, and a Milky Way band
    /// Much more atmospheric than the basic starfield
    /// </summary>
    public class EnhancedNightSky : MonoBehaviour
    {
        [Header("Star Settings")]
        [Tooltip("Total number of stars")]
        [Range(100, 2000)]
        public int starCount = 800;

        [Tooltip("Star field radius")]
        public float skyRadius = 100f;

        [Tooltip("Star size range")]
        public Vector2 starSizeRange = new Vector2(0.05f, 0.3f);

        [Tooltip("Star brightness range")]
        public Vector2 brightRange = new Vector2(0.5f, 2.0f);

        [Header("Star Colors")]
        [Tooltip("Common white/yellow stars")]
        public Color commonStarColor = new Color(1f, 1f, 0.95f);

        [Tooltip("Hot blue stars (rare)")]
        public Color blueStarColor = new Color(0.7f, 0.85f, 1f);

        [Tooltip("Cool red stars (rare)")]
        public Color redStarColor = new Color(1f, 0.7f, 0.6f);

        [Header("Milky Way")]
        [Tooltip("Create a Milky Way band across the sky")]
        public bool createMilkyWay = true;

        [Tooltip("Number of stars in Milky Way band")]
        public int milkyWayStarCount = 300;

        [Tooltip("Milky Way band thickness (degrees)")]
        [Range(5f, 30f)]
        public float milkyWayThickness = 15f;

        [Tooltip("Milky Way tilt angle")]
        [Range(-90f, 90f)]
        public float milkyWayTilt = 30f;

        [Header("Twinkling")]
        [Tooltip("Enable star twinkling animation")]
        public bool enableTwinkling = true;

        [Tooltip("Percentage of stars that twinkle")]
        [Range(0f, 1f)]
        public float twinklePercentage = 0.3f;

        [Header("Nebula Clouds")]
        [Tooltip("Add faint nebula clouds")]
        public bool createNebulaClouds = true;

        [Tooltip("Number of nebula clouds")]
        [Range(3, 10)]
        public int nebulaCount = 5;

        [Tooltip("Nebula colors")]
        public Color[] nebulaColors = new Color[]
        {
            new Color(0.3f, 0.2f, 0.5f, 0.3f), // Purple
            new Color(0.2f, 0.3f, 0.5f, 0.25f), // Blue
            new Color(0.5f, 0.2f, 0.3f, 0.2f)  // Red
        };

        [Header("Organization")]
        public Transform skyContainer;

        private List<EnhancedStarTwinkle> twinklingStars = new List<EnhancedStarTwinkle>();

        [ContextMenu("Generate Enhanced Night Sky")]
        public void GenerateSky()
        {
            Debug.Log("[EnhancedNightSky] Generating realistic night sky...");

            SetupContainer();
            ClearExistingSky();

            GenerateRandomStars();

            if (createMilkyWay)
            {
                GenerateMilkyWay();
            }

            if (createNebulaClouds)
            {
                GenerateNebulaClouds();
            }

            Debug.Log($"[EnhancedNightSky] ✓ Created {starCount + (createMilkyWay ? milkyWayStarCount : 0)} stars!");
        }

        private void SetupContainer()
        {
            if (skyContainer == null)
            {
                GameObject container = GameObject.Find("EnhancedNightSky");
                if (container == null)
                {
                    container = new GameObject("EnhancedNightSky");
                }
                skyContainer = container.transform;
            }
        }

        private void ClearExistingSky()
        {
            twinklingStars.Clear();

            for (int i = skyContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(skyContainer.GetChild(i).gameObject);
            }
        }

        private void GenerateRandomStars()
        {
            Debug.Log($"[EnhancedNightSky] Creating {starCount} random stars...");

            for (int i = 0; i < starCount; i++)
            {
                // Random position on sphere
                Vector3 direction = Random.onUnitSphere;

                // Only stars above horizon (y > 0)
                if (direction.y < 0)
                {
                    direction.y = -direction.y;
                }

                Vector3 position = direction * skyRadius;

                // Determine star type
                float starType = Random.value;
                Color starColor;
                float sizeMult = 1f;

                if (starType < 0.85f)
                {
                    // Common white/yellow star
                    starColor = commonStarColor;
                }
                else if (starType < 0.93f)
                {
                    // Hot blue star (brighter, larger)
                    starColor = blueStarColor;
                    sizeMult = 1.3f;
                }
                else
                {
                    // Cool red star (dimmer, smaller)
                    starColor = redStarColor;
                    sizeMult = 0.7f;
                }

                float size = Random.Range(starSizeRange.x, starSizeRange.y) * sizeMult;
                float brightness = Random.Range(brightRange.x, brightRange.y);

                CreateStar(position, size, starColor, brightness, i < starCount * twinklePercentage);
            }
        }

        private void GenerateMilkyWay()
        {
            Debug.Log($"[EnhancedNightSky] Creating Milky Way band with {milkyWayStarCount} stars...");

            for (int i = 0; i < milkyWayStarCount; i++)
            {
                // Position along a band across the sky
                float longitude = Random.Range(0f, 360f);
                float latitude = Random.Range(-milkyWayThickness / 2f, milkyWayThickness / 2f);

                // Apply tilt
                Vector3 direction = Quaternion.Euler(milkyWayTilt, 0, 0) *
                                  Quaternion.Euler(latitude, longitude, 0) *
                                  Vector3.forward;

                // Ensure above horizon
                if (direction.y < 0)
                {
                    direction.y = -direction.y;
                    direction.Normalize();
                }

                Vector3 position = direction * skyRadius;

                // Milky Way stars are mostly white/blue
                Color starColor = Color.Lerp(commonStarColor, blueStarColor, Random.Range(0.3f, 0.7f));
                float size = Random.Range(starSizeRange.x * 0.5f, starSizeRange.y * 0.8f);
                float brightness = Random.Range(brightRange.x * 0.7f, brightRange.y * 0.9f);

                CreateStar(position, size, starColor, brightness, Random.value < twinklePercentage * 0.5f);
            }
        }

        private void CreateStar(Vector3 position, float size, Color color, float brightness, bool shouldTwinkle)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.name = "Star";
            star.transform.SetParent(skyContainer);
            star.transform.position = position;
            star.transform.localScale = Vector3.one * size;

            // Face camera origin
            star.transform.LookAt(Vector3.zero);
            star.transform.Rotate(0, 180, 0);

            // Emissive star material
            Renderer starRenderer = star.GetComponent<Renderer>();
            Material starMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            starMat.color = color;
            starMat.EnableKeyword("_EMISSION");
            starMat.SetColor("_EmissionColor", color * brightness);
            starMat.SetFloat("_Smoothness", 1f);
            starRenderer.material = starMat;

            // Remove collider
            DestroyImmediate(star.GetComponent<Collider>());

            // Add twinkling if enabled
            if (enableTwinkling && shouldTwinkle)
            {
                EnhancedStarTwinkle twinkle = star.AddComponent<EnhancedStarTwinkle>();
                twinkle.baseBrightness = brightness;
                twinkle.brightnessVariation = brightness * 0.3f;
                twinkle.twinkleSpeed = Random.Range(0.5f, 2f);
                twinkle.baseColor = color;
                twinklingStars.Add(twinkle);
            }
        }

        private void GenerateNebulaClouds()
        {
            Debug.Log($"[EnhancedNightSky] Creating {nebulaCount} nebula clouds...");

            for (int i = 0; i < nebulaCount; i++)
            {
                // Random position in sky
                Vector3 direction = Random.onUnitSphere;
                if (direction.y < 0.2f) direction.y = 0.2f; // Keep above horizon
                direction.Normalize();

                Vector3 position = direction * (skyRadius * 0.95f);

                // Random nebula color
                Color nebulaColor = nebulaColors[Random.Range(0, nebulaColors.Length)];

                CreateNebulaCloud(position, nebulaColor);
            }
        }

        private void CreateNebulaCloud(Vector3 position, Color color)
        {
            GameObject nebula = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nebula.name = "NebulaCloud";
            nebula.transform.SetParent(skyContainer);
            nebula.transform.position = position;

            // Large, stretched cloud
            float sizeX = Random.Range(15f, 30f);
            float sizeY = Random.Range(10f, 20f);
            float sizeZ = Random.Range(8f, 15f);
            nebula.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);

            // Random rotation
            nebula.transform.rotation = Random.rotation;

            // Transparent, glowing material
            Renderer nebulaRenderer = nebula.GetComponent<Renderer>();
            Material nebulaMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            nebulaMat.SetFloat("_Surface", 1); // Transparent
            nebulaMat.SetFloat("_Blend", 0); // Alpha blend
            nebulaMat.color = color;
            nebulaMat.EnableKeyword("_EMISSION");
            nebulaMat.SetColor("_EmissionColor", color * 0.5f);
            nebulaMat.SetFloat("_Smoothness", 0.9f);

            // Enable transparency
            nebulaMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            nebulaMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            nebulaMat.SetInt("_ZWrite", 0);
            nebulaMat.renderQueue = 3000;

            nebulaRenderer.material = nebulaMat;

            // Remove collider
            DestroyImmediate(nebula.GetComponent<Collider>());
        }

        [ContextMenu("Clear Night Sky")]
        public void ClearSky()
        {
            if (skyContainer != null)
            {
                DestroyImmediate(skyContainer.gameObject);
                Debug.Log("[EnhancedNightSky] Night sky cleared");
            }
        }
    }

    /// <summary>
    /// Makes individual stars twinkle by varying their brightness
    /// </summary>
    public class EnhancedStarTwinkle : MonoBehaviour
    {
        public float baseBrightness = 1f;
        public float brightnessVariation = 0.3f;
        public float twinkleSpeed = 1f;
        public Color baseColor = Color.white;

        private Material starMaterial;
        private float time;
        private float offset;

        private void Start()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                starMaterial = renderer.material;
            }

            // Random time offset for variety
            offset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (starMaterial == null) return;

            time += Time.deltaTime * twinkleSpeed;

            // Smooth sine wave for natural twinkling
            float variation = Mathf.Sin(time + offset) * brightnessVariation;
            float brightness = baseBrightness + variation;

            starMaterial.SetColor("_EmissionColor", baseColor * brightness);
        }
    }
}

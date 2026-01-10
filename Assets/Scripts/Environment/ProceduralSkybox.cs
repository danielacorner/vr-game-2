using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Creates a production-ready procedural skybox with stars, gradient, and atmospheric effects
    /// Generates realistic night sky with twinkling stars and color gradient
    /// </summary>
    public class ProceduralSkybox : MonoBehaviour
    {
        [Header("Skybox Colors")]
        [Tooltip("Color at horizon (bottom)")]
        public Color horizonColor = new Color(0.15f, 0.2f, 0.35f); // Dark blue-purple

        [Tooltip("Color at zenith (top)")]
        public Color zenithColor = new Color(0.05f, 0.05f, 0.15f); // Very dark blue

        [Tooltip("Color of the stars")]
        public Color starColor = new Color(1f, 1f, 0.95f); // Slightly warm white

        [Header("Star Settings")]
        [Tooltip("Number of stars to generate")]
        [Range(100, 2000)]
        public int starCount = 800;

        [Tooltip("Star size range")]
        public Vector2 starSizeRange = new Vector2(0.5f, 2f);

        [Tooltip("Star brightness range")]
        public Vector2 starBrightnessRange = new Vector2(0.3f, 1f);

        [Tooltip("Star twinkle speed")]
        public float twinkleSpeed = 1f;

        [Tooltip("Star twinkle amount")]
        [Range(0f, 1f)]
        public float twinkleAmount = 0.3f;

        [Header("Milky Way")]
        [Tooltip("Show milky way band")]
        public bool showMilkyWay = true;

        [Tooltip("Milky way color")]
        public Color milkyWayColor = new Color(0.2f, 0.2f, 0.3f, 0.3f);

        [Header("Debug")]
        public bool showDebug = false;

        private Material skyboxMaterial;
        private GameObject starsContainer;

        void Start()
        {
            CreateProceduralSkybox();
        }

        void CreateProceduralSkybox()
        {
            // Create skybox material
            skyboxMaterial = new Material(Shader.Find("Skybox/Gradient"));
            if (skyboxMaterial == null)
            {
                skyboxMaterial = new Material(Shader.Find("Skybox/Procedural"));
            }

            // Set skybox colors
            RenderSettings.skybox = skyboxMaterial;

            // Create stars container
            starsContainer = new GameObject("Stars");
            starsContainer.transform.SetParent(transform);
            starsContainer.transform.localPosition = Vector3.zero;

            // Generate stars
            GenerateStars();

            if (showDebug)
                Debug.Log($"[ProceduralSkybox] Created skybox with {starCount} stars");
        }

        void GenerateStars()
        {
            for (int i = 0; i < starCount; i++)
            {
                CreateStar(i);
            }

            // Create milky way if enabled
            if (showMilkyWay)
            {
                CreateMilkyWay();
            }
        }

        void CreateStar(int index)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.name = $"Star_{index}";
            star.transform.SetParent(starsContainer.transform);

            // Random position on sphere
            Vector3 randomDir = Random.onUnitSphere;
            float distance = 400f; // Far away
            star.transform.position = transform.position + randomDir * distance;

            // Make quad face camera origin
            star.transform.LookAt(transform.position);
            star.transform.Rotate(0, 180, 0);

            // Random size
            float size = Random.Range(starSizeRange.x, starSizeRange.y);
            star.transform.localScale = Vector3.one * size;

            // Remove collider
            Destroy(star.GetComponent<Collider>());

            // Create star material
            Material starMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            starMat.EnableKeyword("_EMISSION");

            float brightness = Random.Range(starBrightnessRange.x, starBrightnessRange.y);
            Color finalStarColor = starColor * brightness;

            starMat.SetColor("_BaseColor", finalStarColor);
            starMat.SetColor("_EmissionColor", finalStarColor * 2f);
            starMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            starMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            starMat.SetInt("_ZWrite", 0);
            starMat.renderQueue = 3000;

            star.GetComponent<MeshRenderer>().material = starMat;

            // Add twinkle component
            if (twinkleAmount > 0)
            {
                StarTwinkle twinkle = star.AddComponent<StarTwinkle>();
                twinkle.baseColor = finalStarColor;
                twinkle.twinkleSpeed = twinkleSpeed + Random.Range(-0.5f, 0.5f);
                twinkle.twinkleAmount = twinkleAmount;
            }
        }

        void CreateMilkyWay()
        {
            // Create milky way band (series of quads forming a path across the sky)
            int milkyWaySegments = 50;

            for (int i = 0; i < milkyWaySegments; i++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Quad);
                segment.name = $"MilkyWay_{i}";
                segment.transform.SetParent(starsContainer.transform);

                // Position along a band
                float angle = (i / (float)milkyWaySegments) * 360f;
                float tilt = 30f; // Tilted band

                Vector3 direction = Quaternion.Euler(tilt, angle, 0) * Vector3.forward;
                float distance = 390f;
                segment.transform.position = transform.position + direction * distance;
                segment.transform.LookAt(transform.position);
                segment.transform.Rotate(0, 180, 0);

                // Size
                float width = Random.Range(8f, 15f);
                float height = Random.Range(8f, 15f);
                segment.transform.localScale = new Vector3(width, height, 1f);

                // Remove collider
                Destroy(segment.GetComponent<Collider>());

                // Material
                Material milkyWayMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                milkyWayMat.SetColor("_BaseColor", milkyWayColor);
                milkyWayMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                milkyWayMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                milkyWayMat.SetInt("_ZWrite", 0);
                milkyWayMat.renderQueue = 2900; // Behind stars

                segment.GetComponent<MeshRenderer>().material = milkyWayMat;
            }
        }

        void OnDestroy()
        {
            if (skyboxMaterial != null)
            {
                Destroy(skyboxMaterial);
            }
        }
    }

    /// <summary>
    /// Makes individual stars twinkle by animating their brightness
    /// </summary>
    public class StarTwinkle : MonoBehaviour
    {
        public Color baseColor;
        public float twinkleSpeed = 1f;
        public float twinkleAmount = 0.3f;

        private Material starMaterial;
        private float timeOffset;

        void Start()
        {
            starMaterial = GetComponent<MeshRenderer>().material;
            timeOffset = Random.Range(0f, 100f);
        }

        void Update()
        {
            if (starMaterial == null) return;

            // Calculate twinkle using perlin noise for smooth variation
            float noise = Mathf.PerlinNoise(Time.time * twinkleSpeed + timeOffset, 0f);
            float brightness = Mathf.Lerp(1f - twinkleAmount, 1f + twinkleAmount, noise);

            Color twinkledColor = baseColor * brightness;
            starMaterial.SetColor("_BaseColor", twinkledColor);
            starMaterial.SetColor("_EmissionColor", twinkledColor * 2f);
        }

        void OnDestroy()
        {
            if (starMaterial != null)
            {
                Destroy(starMaterial);
            }
        }
    }
}

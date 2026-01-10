using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Spawns interactive particle effects when player moves or interacts with environment
    /// - Dust puffs when walking
    /// - Disturbed fireflies
    /// </summary>
    public class InteractiveParticles : MonoBehaviour
    {
        [Header("Footstep Dust Settings")]
        [Tooltip("Enable dust puffs when walking")]
        public bool enableFootstepDust = true;

        [Tooltip("Prefab for dust puff particles")]
        public ParticleSystem dustPuffPrefab;

        [Range(0.1f, 2f)]
        [Tooltip("Time between footstep effects")]
        public float footstepInterval = 0.5f;

        [Header("Firefly Settings")]
        [Tooltip("Enable disturbed fireflies")]
        public bool enableFireflies = true;

        [Tooltip("Prefab for firefly particles")]
        public ParticleSystem fireflyPrefab;

        [Range(5, 20)]
        [Tooltip("Number of fireflies around player")]
        public int fireflyCount = 10;

        [Range(1f, 5f)]
        [Tooltip("Radius where fireflies spawn")]
        public float fireflyRadius = 3f;

        private Transform playerTransform;
        private Vector3 lastPlayerPosition;
        private float lastFootstepTime;
        private GameObject fireflyParent;

        void Start()
        {
            // Find player (XR Origin or Camera)
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerTransform = mainCamera.transform;
            }

            if (playerTransform == null)
            {
                Debug.LogWarning("[InteractiveParticles] Could not find player transform");
                return;
            }

            lastPlayerPosition = playerTransform.position;

            if (enableFireflies)
            {
                SpawnFireflies();
            }
        }

        void Update()
        {
            if (playerTransform == null) return;

            // Check for player movement
            float distanceMoved = Vector3.Distance(playerTransform.position, lastPlayerPosition);

            if (enableFootstepDust && distanceMoved > 0.1f && Time.time - lastFootstepTime > footstepInterval)
            {
                SpawnFootstepDust();
                lastFootstepTime = Time.time;
            }

            lastPlayerPosition = playerTransform.position;
        }

        void SpawnFootstepDust()
        {
            if (dustPuffPrefab == null) return;

            // Spawn at player's foot level
            Vector3 spawnPos = playerTransform.position;
            spawnPos.y = 0.1f; // Ground level

            ParticleSystem dust = Instantiate(dustPuffPrefab, spawnPos, Quaternion.identity);
            Destroy(dust.gameObject, 2f); // Auto-cleanup
        }

        void SpawnFireflies()
        {
            if (fireflyPrefab == null) return;

            fireflyParent = new GameObject("Fireflies");
            fireflyParent.transform.SetParent(transform);

            for (int i = 0; i < fireflyCount; i++)
            {
                // Random position around player
                Vector3 offset = Random.insideUnitSphere * fireflyRadius;
                offset.y = Mathf.Abs(offset.y) * 0.5f; // Keep them low

                Vector3 spawnPos = playerTransform.position + offset;

                ParticleSystem firefly = Instantiate(fireflyPrefab, spawnPos, Quaternion.identity, fireflyParent.transform);

                // Configure firefly
                var main = firefly.main;
                main.startLifetime = float.PositiveInfinity; // Persistent
                main.startSize = Random.Range(0.05f, 0.1f);
                main.startColor = new Color(1f, 0.9f, 0.5f, 0.8f);

                // Add glow
                var emission = firefly.emission;
                emission.enabled = true;

                // Add light component
                GameObject lightGO = new GameObject("FireflyLight");
                lightGO.transform.SetParent(firefly.transform);
                Light light = lightGO.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.9f, 0.5f);
                light.intensity = 0.2f;
                light.range = 1.5f;
                light.shadows = LightShadows.None;

                // Make fireflies float around
                StartCoroutine(AnimateFirefly(firefly.transform));
            }
        }

        System.Collections.IEnumerator AnimateFirefly(Transform firefly)
        {
            Vector3 startPos = firefly.position;
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * 0.5f;

                // Sine wave movement
                Vector3 offset = new Vector3(
                    Mathf.Sin(time) * 2f,
                    Mathf.Sin(time * 1.3f) * 0.5f,
                    Mathf.Cos(time) * 2f
                );

                firefly.position = startPos + offset;

                yield return null;
            }
        }
    }
}

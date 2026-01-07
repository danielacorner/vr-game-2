using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Controls campfire visual and audio effects
    /// Central gathering point for outdoor home area
    /// Provides flickering light and ambient crackling sounds
    /// </summary>
    public class CampfireController : MonoBehaviour
    {
        [Header("Visual Effects")]
        [Tooltip("Fire particle system (orange/yellow flames)")]
        public ParticleSystem fireParticles;

        [Tooltip("Smoke particle system (gray smoke rising)")]
        public ParticleSystem smokeParticles;

        [Tooltip("Point light for fire illumination")]
        public Light fireLight;

        [Header("Light Settings")]
        [Tooltip("Minimum light intensity (flicker low)")]
        [Range(0f, 5f)]
        public float minIntensity = 1.5f;

        [Tooltip("Maximum light intensity (flicker high)")]
        [Range(0f, 5f)]
        public float maxIntensity = 2.5f;

        [Tooltip("Speed of the flicker effect")]
        [Range(1f, 10f)]
        public float flickerSpeed = 5f;

        [Tooltip("Color of the fire light")]
        public Color fireColor = new Color(1f, 0.5f, 0.1f);

        [Header("Audio")]
        [Tooltip("Audio source for crackling fire sound")]
        public AudioSource cracklingSound;

        private float flickerTime;

        void Start()
        {
            // Auto-find components if not assigned
            if (fireLight == null)
                fireLight = GetComponentInChildren<Light>();

            if (fireParticles == null)
            {
                ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
                if (systems.Length > 0)
                    fireParticles = systems[0]; // First is fire
                if (systems.Length > 1)
                    smokeParticles = systems[1]; // Second is smoke
            }

            if (cracklingSound == null)
                cracklingSound = GetComponentInChildren<AudioSource>();

            // Configure light if found
            if (fireLight != null)
            {
                fireLight.type = LightType.Point;
                fireLight.color = fireColor;
                fireLight.range = 8f;
                fireLight.intensity = minIntensity;
            }

            // Configure audio if found
            if (cracklingSound != null)
            {
                cracklingSound.loop = true;
                cracklingSound.spatialBlend = 1.0f; // 3D sound
                cracklingSound.minDistance = 3f;
                cracklingSound.maxDistance = 15f;
                cracklingSound.volume = 0.3f;

                if (!cracklingSound.isPlaying)
                    cracklingSound.Play();
            }

            Debug.Log("[CampfireController] Campfire initialized");
        }

        void Update()
        {
            UpdateFlicker();
        }

        private void UpdateFlicker()
        {
            if (fireLight == null) return;

            // Use Perlin noise for natural flickering
            flickerTime += Time.deltaTime * flickerSpeed;
            float flicker = Mathf.PerlinNoise(flickerTime, 0f);

            // Map Perlin noise (0-1) to intensity range
            fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, flicker);
        }

        void OnDrawGizmosSelected()
        {
            // Visualize light range
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 8f);

            // Visualize audio range
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, 15f);
        }
    }
}

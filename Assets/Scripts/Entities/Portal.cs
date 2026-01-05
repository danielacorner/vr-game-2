using UnityEngine;
using VRDungeonCrawler.Core;

namespace VRDungeonCrawler.Entities
{
    /// <summary>
    /// Portal entity that transports player to dungeon
    /// Triggers when player enters the collider
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Portal : MonoBehaviour
    {
        [Header("Portal Settings")]
        [Tooltip("Radius for trigger detection")]
        public float triggerRadius = 1.5f;

        [Header("Visual Effects")]
        [Tooltip("Particle system for portal effect")]
        public ParticleSystem portalParticles;

        [Tooltip("Rotating rings")]
        public Transform outerRing;
        public Transform middleRing;
        public Transform innerRing;

        [Header("Rotation Speeds")]
        public float outerRingSpeed = 0.5f;
        public float middleRingSpeed = -0.8f;
        public float innerRingSpeed = 1.2f;

        private SphereCollider triggerCollider;
        private bool hasTriggered = false;

        private void Awake()
        {
            // Setup trigger collider
            triggerCollider = GetComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerRadius;
        }

        private void Update()
        {
            // Rotate rings
            if (outerRing != null)
                outerRing.Rotate(Vector3.forward, outerRingSpeed * Time.deltaTime * 30f);

            if (middleRing != null)
                middleRing.Rotate(Vector3.forward, middleRingSpeed * Time.deltaTime * 30f);

            if (innerRing != null)
                innerRing.Rotate(Vector3.forward, innerRingSpeed * Time.deltaTime * 30f);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if player entered
            if (hasTriggered) return;

            // Look for XR Origin or Camera (player detection)
            // Also check for any collider that's a child of XR Origin
            bool isPlayer = other.CompareTag("Player") ||
                           other.CompareTag("MainCamera") ||
                           other.name.Contains("XR Origin") ||
                           other.transform.root.name.Contains("XR Origin");

            if (isPlayer)
            {
                Debug.Log("[Portal] Player entered portal!");
                TriggerPortal();
            }
        }

        private void TriggerPortal()
        {
            hasTriggered = true;

            // Flash effect
            if (portalParticles != null)
            {
                var emission = portalParticles.emission;
                emission.rateOverTime = 100f; // Burst of particles
            }

            // Transition to dungeon
            if (GameManager.Instance != null)
            {
                Invoke(nameof(LoadDungeon), 0.5f); // Small delay for effect
            }
        }

        private void LoadDungeon()
        {
            GameManager.Instance.EnterDungeon();
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize trigger radius in editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}

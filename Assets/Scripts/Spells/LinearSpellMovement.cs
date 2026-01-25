using UnityEngine;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.Spells
{
    /// <summary>
    /// Simple linear movement for tier-1 spell projectiles
    /// Moves in a straight line at constant speed with collision detection
    /// </summary>
    public class LinearSpellMovement : MonoBehaviour
    {
        public Vector3 direction = Vector3.forward;
        public float speed = 20f;
        public float lifetime = 5f;
        public SpellData spellData;

        private const float GRACE_PERIOD = 0.1f;
        private float spawnTime;
        private bool hasExploded = false;

        void Start()
        {
            spawnTime = Time.time;
        }

        void Update()
        {
            // Move forward
            transform.position += direction * speed * Time.deltaTime;

            // Check lifetime
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Grace period - don't collide immediately after spawning
            if (Time.time < spawnTime + GRACE_PERIOD)
            {
                return;
            }

            // Ignore triggers
            if (other.isTrigger) return;

            // Ignore player/VR objects
            if (IsPlayerObject(other.gameObject)) return;

            // Only explode once
            if (hasExploded) return;
            hasExploded = true;

            Debug.Log($"[LinearSpellMovement] Hit {other.gameObject.name}, creating explosion!");

            // Get surface normal from collision point
            Vector3 surfaceNormal = Vector3.up; // Default

            // Special handling for terrain
            if (other is TerrainCollider)
            {
                // For terrain, raycast downward from projectile position
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f))
                {
                    if (hit.collider == other)
                    {
                        surfaceNormal = hit.normal;
                    }
                }
            }
            else
            {
                // For other objects, raycast in direction of travel
                RaycastHit hit;
                if (Physics.Raycast(transform.position - direction * 0.5f, direction, out hit, 1f))
                {
                    if (!hit.collider.isTrigger && hit.collider.gameObject == other.gameObject)
                    {
                        surfaceNormal = hit.normal;
                    }
                }
            }

            // Create explosion with ricochet particles
            SpellCaster.CreateSpellExplosion(transform.position, surfaceNormal, direction, speed, spellData);

            // Stop particle emission
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                var emission = ps.emission;
                emission.enabled = false;

                // Detach world-space particles so they persist
                if (ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
                {
                    ps.transform.SetParent(null);
                    Destroy(ps.gameObject, ps.main.startLifetime.constantMax + 1f);
                }
            }

            // Destroy projectile
            Destroy(gameObject, 0.1f);
        }

        private bool IsPlayerObject(GameObject obj)
        {
            if (obj == null) return false;

            // Check entire hierarchy, not just parents
            Transform current = obj.transform;
            while (current != null)
            {
                // Check for XR Origin or Player tag
                if (current.name == "XR Origin" || current.CompareTag("Player"))
                {
                    return true;
                }

                string name = current.name.ToLower();
                // Check for common player/VR object names
                if (name.Contains("xr origin") ||
                    name.Contains("player") ||
                    name.Contains("camera") ||
                    name.Contains("headset") ||
                    name.Contains("hmd") ||
                    name.Contains("main camera") ||
                    name.Contains("controller") ||
                    name.Contains("hand") ||
                    name.Contains("offset"))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}

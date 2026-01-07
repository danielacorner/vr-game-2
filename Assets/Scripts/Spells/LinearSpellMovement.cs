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
            RaycastHit hit;
            if (Physics.Raycast(transform.position - direction * 0.5f, direction, out hit, 1f))
            {
                if (!hit.collider.isTrigger && hit.collider.gameObject == other.gameObject)
                {
                    surfaceNormal = hit.normal;
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
            string name = obj.name.ToLower();

            // Check for common player/VR object names
            if (name.Contains("controller")) return true;
            if (name.Contains("hand")) return true;
            if (name.Contains("camera")) return true;
            if (name.Contains("main camera")) return true;
            if (name.Contains("xr")) return true;
            if (name.Contains("player")) return true;
            if (name.Contains("origin")) return true;
            if (name.Contains("offset")) return true;

            // Check parent hierarchy
            Transform current = obj.transform;
            while (current.parent != null)
            {
                string parentName = current.parent.name.ToLower();
                if (parentName.Contains("xr") ||
                    parentName.Contains("player") ||
                    parentName.Contains("origin"))
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }
    }
}

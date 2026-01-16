using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Spell projectile - handles collision and damage
    /// Can be used for any spell projectile (fireball, ice shard, lightning bolt, etc.)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SpellProjectile : MonoBehaviour
    {
        [Header("Damage")]
        public float damage = 25f;
        public float splashRadius = 0f;
        public LayerMask hitLayers;

        [Header("Visual")]
        public Color spellColor = Color.red;
        public GameObject hitEffect;
        public TrailRenderer trail;
        public Light projectileLight;

        [Header("Auto-Setup")]
        public bool setupOnStart = true;

        private Rigidbody rb;
        private bool hasHit = false;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();

            if (setupOnStart)
            {
                SetupVisuals();
            }
        }

        private void SetupVisuals()
        {
            // Setup trail color
            if (trail != null)
            {
                trail.startColor = spellColor;
                trail.endColor = new Color(spellColor.r, spellColor.g, spellColor.b, 0f);
            }

            // Setup light color
            if (projectileLight != null)
            {
                projectileLight.color = spellColor;
            }

            // Setup particle systems
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                var main = ps.main;
                main.startColor = spellColor;
            }

            // Setup renderer color
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = spellColor;
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.SetColor("_EmissionColor", spellColor * 2f);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;
            hasHit = true;

            Debug.Log($"[Projectile] Hit (trigger) {other.gameObject.name}");

            // Spawn hit effect at collision point
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Destroy projectile immediately to prevent multiple hits
            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasHit) return;
            hasHit = true;

            Debug.Log($"[Projectile] Hit {collision.gameObject.name}");

            // Apply damage
            ApplyDamage(collision);

            // Spawn hit effect
            if (hitEffect != null)
            {
                Vector3 hitPoint = collision.contacts.Length > 0 ?
                    collision.contacts[0].point :
                    transform.position;

                GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Destroy projectile
            Destroy(gameObject);
        }

        private void ApplyDamage(Collision collision)
        {
            // Check if target has a health component
            // TODO: Implement enemy health system
            // For now, just log damage
            Debug.Log($"[Projectile] Would deal {damage} damage to {collision.gameObject.name}");

            // If splash damage
            if (splashRadius > 0)
            {
                ApplySplashDamage(collision.contacts[0].point);
            }
        }

        private void ApplySplashDamage(Vector3 center)
        {
            Collider[] hitColliders = Physics.OverlapSphere(center, splashRadius, hitLayers);

            foreach (Collider hit in hitColliders)
            {
                // Calculate falloff damage based on distance
                float distance = Vector3.Distance(center, hit.transform.position);
                float falloff = 1f - (distance / splashRadius);
                float splashDamage = damage * falloff;

                Debug.Log($"[Projectile] Splash damage {splashDamage} to {hit.gameObject.name}");

                // TODO: Apply damage to health component
            }
        }

        /// <summary>
        /// Update projectile color dynamically
        /// </summary>
        public void SetColor(Color color)
        {
            spellColor = color;
            SetupVisuals();
        }

        /// <summary>
        /// Get damage dealt by this spell projectile
        /// Returns 2 for basic projectiles (tier 1), 5 for advanced (tier 2)
        /// Note: SpellProjectile doesn't have tier info, so returns fixed value
        /// </summary>
        public int GetDamage()
        {
            // Convert float damage to int
            // If damage is default 25f, assume tier 1 (2 damage)
            // For tier 2, damage would typically be higher
            if (damage >= 40f)
                return 5; // Tier 2
            else
                return 2; // Tier 1
        }
    }
}

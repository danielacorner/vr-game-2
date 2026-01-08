using UnityEngine;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.Spells
{
    /// <summary>
    /// Physics-based spell projectile for tier-2 spells
    /// Uses Rigidbody for realistic throwing physics with gravity
    /// Supports bouncing, explosion effects, and element-specific behaviors
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PhysicsSpellProjectile : MonoBehaviour
    {
        [Header("Physics Settings")]
        [Tooltip("Initial throw force")]
        public float throwForce = 15f;

        [Tooltip("Use gravity (true for most spells)")]
        public bool useGravity = true;

        [Tooltip("Gravity multiplier (1 = normal, 0.3 = floaty)")]
        [Range(0f, 2f)]
        public float gravityScale = 1f;

        [Tooltip("Bounciness (0 = no bounce, 1 = perfect bounce)")]
        [Range(0f, 1f)]
        public float bounciness = 0.3f;

        [Tooltip("Number of bounces before exploding (0 = explode on first hit)")]
        public int maxBounces = 0;

        [Header("Lifetime")]
        [Tooltip("Time before auto-destruction")]
        public float lifetime = 8f;

        [Header("Explosion")]
        [Tooltip("Enable explosion on impact")]
        public bool explodeOnImpact = true;

        [Tooltip("Explosion radius")]
        public float explosionRadius = 3f;

        [Tooltip("Explosion force")]
        public float explosionForce = 500f;

        [Header("Spell Data")]
        public SpellData spellData;

        [Header("Debug")]
        public bool showDebug = false;

        private Rigidbody rb;
        private int bounceCount = 0;
        private bool hasExploded = false;
        private float spawnTime;
        private Vector3 lastVelocity;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spawnTime = Time.time;

            // Configure physics material for bounciness
            PhysicsMaterial bounceMat = new PhysicsMaterial("ProjectileBounce");
            bounceMat.bounciness = bounciness;
            bounceMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;

            SphereCollider col = GetComponent<SphereCollider>();
            col.material = bounceMat;

            // Configure rigidbody
            rb.useGravity = useGravity;
            rb.linearDamping = 0.1f; // Slight air resistance
            rb.angularDamping = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (showDebug)
                Debug.Log($"[PhysicsProjectile] Created with throw force {throwForce}, gravity scale {gravityScale}");
        }

        void FixedUpdate()
        {
            // Apply custom gravity scale
            if (useGravity && gravityScale != 1f)
            {
                rb.AddForce(Physics.gravity * (gravityScale - 1f), ForceMode.Acceleration);
            }

            // Store velocity for collision response
            lastVelocity = rb.linearVelocity;

            // Check lifetime
            if (Time.time - spawnTime > lifetime)
            {
                if (explodeOnImpact)
                {
                    Explode(transform.position, Vector3.zero);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Throw the projectile using hand velocity (like throwing a baseball)
        /// </summary>
        /// <param name="handVelocity">Velocity of the controller/hand at release</param>
        /// <param name="velocityBoost">Multiplier for hand velocity (default 1.5x)</param>
        public void ThrowWithVelocity(Vector3 handVelocity, float velocityBoost = 1.5f)
        {
            if (rb != null)
            {
                // Apply hand velocity with boost - pure velocity-based throwing
                Vector3 throwVelocity = handVelocity * velocityBoost;

                rb.linearVelocity = throwVelocity;

                // Add spin based on velocity direction for realism (only if moving)
                if (throwVelocity.magnitude > 0.1f)
                {
                    Vector3 spinAxis = Vector3.Cross(Vector3.up, throwVelocity.normalized);
                    rb.AddTorque(spinAxis * throwVelocity.magnitude * 0.1f, ForceMode.VelocityChange);
                }

                if (showDebug)
                    Debug.Log($"[PhysicsProjectile] Thrown with velocity {throwVelocity.magnitude:F1} m/s (hand: {handVelocity.magnitude:F1}, boost: {velocityBoost}x)");
            }
        }

        /// <summary>
        /// Legacy method - throws in a direction (kept for compatibility)
        /// </summary>
        public void Throw(Vector3 direction)
        {
            if (rb != null)
            {
                rb.AddForce(direction.normalized * throwForce, ForceMode.VelocityChange);

                Vector3 randomTorque = new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(-2f, 2f),
                    Random.Range(-2f, 2f)
                );
                rb.AddTorque(randomTorque, ForceMode.VelocityChange);

                if (showDebug)
                    Debug.Log($"[PhysicsProjectile] Thrown with force {throwForce} in direction {direction}");
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (hasExploded) return;

            bounceCount++;

            if (showDebug)
                Debug.Log($"[PhysicsProjectile] Hit {collision.gameObject.name}, bounce {bounceCount}/{maxBounces}");

            // Check if should explode
            if (bounceCount > maxBounces)
            {
                Vector3 hitPoint = collision.contacts.Length > 0 ?
                    collision.contacts[0].point :
                    transform.position;

                Vector3 hitNormal = collision.contacts.Length > 0 ?
                    collision.contacts[0].normal :
                    Vector3.up;

                if (explodeOnImpact)
                {
                    Explode(hitPoint, hitNormal);
                }
                else
                {
                    OnImpact(hitPoint, hitNormal, collision);
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Create explosion effect at impact point
        /// </summary>
        private void Explode(Vector3 position, Vector3 normal)
        {
            if (hasExploded) return;
            hasExploded = true;

            if (showDebug)
                Debug.Log($"[PhysicsProjectile] Exploding at {position}");

            // Create explosion effect based on spell type
            CreateExplosionEffect(position, normal);

            // Apply explosion force to nearby rigidbodies
            Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                if (hitRb != null && hitRb != rb)
                {
                    hitRb.AddExplosionForce(explosionForce, position, explosionRadius, 0.5f, ForceMode.Impulse);

                    if (showDebug)
                        Debug.Log($"[PhysicsProjectile] Applied explosion force to {hit.name}");
                }
            }

            // Destroy projectile
            Destroy(gameObject);
        }

        /// <summary>
        /// Create visual explosion effect based on spell type
        /// </summary>
        private void CreateExplosionEffect(Vector3 position, Vector3 normal)
        {
            if (spellData == null) return;

            string spellName = spellData.spellName.ToLower();

            // Create explosion sphere
            GameObject explosionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            explosionSphere.name = $"Explosion_{spellData.spellName}";
            explosionSphere.transform.position = position;
            explosionSphere.transform.localScale = Vector3.one * 0.1f;
            Destroy(explosionSphere.GetComponent<Collider>());

            // Create glowing material
            Material explosionMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            explosionMat.EnableKeyword("_EMISSION");
            
            Color emissionColor = spellData.spellColor * 3f;
            explosionMat.SetColor("_BaseColor", spellData.spellColor);
            explosionMat.SetColor("_EmissionColor", emissionColor);
            explosionMat.SetFloat("_Surface", 1); // Transparent
            explosionMat.SetFloat("_Blend", 0);
            explosionMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            explosionMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            explosionMat.SetInt("_ZWrite", 0);
            explosionMat.renderQueue = 3000;

            explosionSphere.GetComponent<MeshRenderer>().material = explosionMat;

            // Add expansion animation
            ExplosionAnimation anim = explosionSphere.AddComponent<ExplosionAnimation>();
            anim.maxScale = explosionRadius * 2f;
            anim.duration = 0.5f;
            anim.spellColor = spellData.spellColor;

            // Create particle burst
            CreateExplosionParticles(position, normal);
        }

        /// <summary>
        /// Create particle burst for explosion
        /// </summary>
        private void CreateExplosionParticles(Vector3 position, Vector3 normal)
        {
            GameObject particleObj = new GameObject("ExplosionParticles");
            particleObj.transform.position = position;

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = spellData != null ? spellData.spellColor : Color.red;
            main.maxParticles = 100;
            main.duration = 0.3f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 50));

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            Color fadeColor = spellData != null ? spellData.spellColor : Color.red;
            fadeColor.a = 0f;
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(fadeColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            Destroy(particleObj, 2f);
        }

        /// <summary>
        /// Called when projectile impacts but doesn't explode
        /// Override for element-specific effects
        /// </summary>
        protected virtual void OnImpact(Vector3 position, Vector3 normal, Collision collision)
        {
            if (showDebug)
                Debug.Log($"[PhysicsProjectile] Impact at {position}");
        }

        void OnDrawGizmosSelected()
        {
            // Visualize explosion radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}

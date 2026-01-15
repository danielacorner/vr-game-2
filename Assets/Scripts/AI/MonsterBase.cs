using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Base class for all dungeon monsters
    /// Handles HP, damage, flash effects, knockback, and death
    /// </summary>
    public enum MonsterType
    {
        Goblin,
        Skeleton,
        Slime
    }

    public class MonsterBase : MonoBehaviour
    {
        [Header("Monster Stats")]
        [Tooltip("Type of monster")]
        public MonsterType monsterType = MonsterType.Goblin;

        [Tooltip("Current hit points")]
        public int currentHP;

        [Tooltip("Maximum hit points")]
        public int maxHP = 10;

        [Header("Damage Response")]
        [Tooltip("Flash color when hit")]
        public Color hitFlashColor = Color.red;

        [Tooltip("Duration of flash effect")]
        public float flashDuration = 0.2f;

        [Tooltip("Number of flash cycles when hit")]
        public int flashCycles = 2;

        [Tooltip("Knockback force when hit")]
        public float knockbackForce = 3f;

        [Header("Death")]
        [Tooltip("Time before removal after death")]
        public float deathRemovalDelay = 2f;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        // Internal state
        private bool isDead = false;
        private bool isFlashing = false;
        private float flashTimer;
        private int currentFlashCycle;
        private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        private Dictionary<MeshRenderer, Material[]> originalMaterials = new Dictionary<MeshRenderer, Material[]>();
        private Rigidbody rb;
        private MonsterSpawner spawner;

        void Awake()
        {
            // Set HP to max at start
            currentHP = maxHP;

            // Get rigidbody or add one
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.linearDamping = 2f;
                rb.angularDamping = 1f;
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }

            // Store all mesh renderers for flash effect
            meshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
            foreach (MeshRenderer renderer in meshRenderers)
            {
                originalMaterials[renderer] = renderer.materials;
            }
        }

        public void SetSpawner(MonsterSpawner spawner)
        {
            this.spawner = spawner;
        }

        void Update()
        {
            // Handle flash effect
            if (isFlashing)
            {
                flashTimer += Time.deltaTime;

                if (flashTimer >= flashDuration)
                {
                    // Toggle flash
                    flashTimer = 0f;
                    currentFlashCycle++;

                    if (currentFlashCycle >= flashCycles * 2) // *2 because we toggle on/off
                    {
                        // End flash
                        isFlashing = false;
                        RestoreOriginalMaterials();
                    }
                    else
                    {
                        // Toggle between hit color and original
                        if (currentFlashCycle % 2 == 0)
                            ApplyHitFlash();
                        else
                            RestoreOriginalMaterials();
                    }
                }
            }
        }

        /// <summary>
        /// Called when monster takes damage
        /// </summary>
        public void TakeDamage(int damage, Vector3 hitDirection)
        {
            if (isDead) return;

            currentHP -= damage;

            if (showDebug)
                Debug.Log($"[MonsterBase] {gameObject.name} took {damage} damage. HP: {currentHP}/{maxHP}");

            // Flash effect
            StartFlashEffect();

            // Knockback
            ApplyKnockback(hitDirection);

            // Check death
            if (currentHP <= 0)
            {
                Die();
            }
        }

        void StartFlashEffect()
        {
            if (isFlashing) return;

            isFlashing = true;
            flashTimer = 0f;
            currentFlashCycle = 0;
            ApplyHitFlash();
        }

        void ApplyHitFlash()
        {
            foreach (MeshRenderer renderer in meshRenderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].color = hitFlashColor;
                }
                renderer.materials = materials;
            }
        }

        void RestoreOriginalMaterials()
        {
            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (originalMaterials.ContainsKey(renderer))
                {
                    renderer.materials = originalMaterials[renderer];
                }
            }
        }

        void ApplyKnockback(Vector3 direction)
        {
            if (rb != null)
            {
                // Normalize direction and apply force
                Vector3 knockbackDir = direction.normalized;
                knockbackDir.y = 0.3f; // Add slight upward component

                rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);

                if (showDebug)
                    Debug.Log($"[MonsterBase] {gameObject.name} knocked back by {knockbackDir * knockbackForce}");
            }
        }

        void Die()
        {
            if (isDead) return;

            isDead = true;

            if (showDebug)
                Debug.Log($"[MonsterBase] {gameObject.name} died!");

            // Notify spawner
            if (spawner != null)
            {
                spawner.OnMonsterDied(monsterType);
            }

            // Play death animation based on monster type
            StartCoroutine(PlayDeathAnimation());
        }

        IEnumerator PlayDeathAnimation()
        {
            // Disable collision during death
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // Unique death animation for each monster type
            switch (monsterType)
            {
                case MonsterType.Goblin:
                    yield return GoblinDeathAnimation();
                    break;
                case MonsterType.Skeleton:
                    yield return SkeletonDeathAnimation();
                    break;
                case MonsterType.Slime:
                    yield return SlimeDeathAnimation();
                    break;
            }

            // Wait a bit then remove
            yield return new WaitForSeconds(deathRemovalDelay);
            Destroy(gameObject);
        }

        IEnumerator GoblinDeathAnimation()
        {
            // Goblin: Spin and shrink
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Quaternion startRotation = transform.rotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Spin rapidly
                transform.Rotate(Vector3.up, 720f * Time.deltaTime);

                // Shrink
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

                yield return null;
            }
        }

        IEnumerator SkeletonDeathAnimation()
        {
            // Skeleton: Collapse into pile of bones (fall apart)
            Transform[] children = GetComponentsInChildren<Transform>();
            List<Rigidbody> boneRigidbodies = new List<Rigidbody>();

            // Add rigidbodies to all parts to make them fall
            foreach (Transform child in children)
            {
                if (child != transform && child.GetComponent<MeshRenderer>() != null)
                {
                    Rigidbody childRb = child.gameObject.AddComponent<Rigidbody>();
                    childRb.mass = 0.1f;
                    childRb.useGravity = true;
                    // Random scatter force
                    Vector3 randomForce = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.5f, 2f),
                        Random.Range(-1f, 1f)
                    );
                    childRb.AddForce(randomForce, ForceMode.Impulse);
                    childRb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                    boneRigidbodies.Add(childRb);
                }
            }

            // Wait for bones to settle
            yield return new WaitForSeconds(1.5f);
        }

        IEnumerator SlimeDeathAnimation()
        {
            // Slime: Splat and melt
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Flatten (splat)
                transform.localScale = new Vector3(
                    startScale.x * (1f + progress * 2f), // Wider
                    startScale.y * (1f - progress * 0.9f), // Flatter
                    startScale.z * (1f + progress * 2f)  // Wider
                );

                // Sink into ground
                transform.position += Vector3.down * Time.deltaTime * 0.5f;

                yield return null;
            }
        }

        /// <summary>
        /// Called when hit by a spell (from trigger collision)
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            // Check if hit by spell projectile
            if (other.CompareTag("Spell") || other.CompareTag("Projectile"))
            {
                int damage = 0;
                bool hitDetected = false;

                // Check for PhysicsSpellProjectile (tier 2 thrown spells)
                var physicsProjectile = other.GetComponent<VRDungeonCrawler.Spells.PhysicsSpellProjectile>();
                if (physicsProjectile != null)
                {
                    damage = physicsProjectile.GetDamage();
                    hitDetected = true;
                }

                // Check for SpellProjectile (tier 1 shot spells)
                if (!hitDetected)
                {
                    var spellProjectile = other.GetComponent<VRDungeonCrawler.Player.SpellProjectile>();
                    if (spellProjectile != null)
                    {
                        damage = spellProjectile.GetDamage();
                        hitDetected = true;
                    }
                }

                if (hitDetected)
                {
                    Vector3 hitDirection = (transform.position - other.transform.position).normalized;
                    TakeDamage(damage, hitDirection);

                    if (showDebug)
                        Debug.Log($"[MonsterBase] {gameObject.name} hit by spell for {damage} damage");
                }
            }
        }
    }
}

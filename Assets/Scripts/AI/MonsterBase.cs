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
        public Color hitFlashColor = new Color(1f, 0f, 0f, 1f); // Bright red

        [Tooltip("Duration of flash effect")]
        public float flashDuration = 0.3f; // Longer flash for visibility

        [Tooltip("Number of flash cycles when hit")]
        public int flashCycles = 2;

        [Tooltip("Knockback force when hit")]
        public float knockbackForce = 6f; // Balanced knockback distance

        [Header("Death")]
        [Tooltip("Time before removal after death")]
        public float deathRemovalDelay = 2f;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = true;

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
            Debug.Log($"[MonsterBase] ==================== AWAKE CALLED for {gameObject.name} ====================");

            // Set HP to max at start
            currentHP = maxHP;
            Debug.Log($"[MonsterBase] HP set to {currentHP}/{maxHP}");

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
                Debug.Log($"[MonsterBase] Created Rigidbody");
            }
            else
            {
                Debug.Log($"[MonsterBase] Found existing Rigidbody");
            }

            // Log all colliders
            Collider[] colliders = GetComponents<Collider>();
            Debug.Log($"[MonsterBase] Found {colliders.Length} colliders on {gameObject.name}:");
            foreach (Collider col in colliders)
            {
                BoxCollider box = col as BoxCollider;
                if (box != null)
                {
                    Debug.Log($"  - BoxCollider: isTrigger={col.isTrigger}, size={box.size}, center={box.center}");
                }
                else
                {
                    Debug.Log($"  - {col.GetType().Name}: isTrigger={col.isTrigger}");
                }
            }

            // Store all mesh renderers for flash effect
            meshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
            foreach (MeshRenderer renderer in meshRenderers)
            {
                originalMaterials[renderer] = renderer.materials;
            }

            Debug.Log($"[MonsterBase] Found {meshRenderers.Count} mesh renderers");
            Debug.Log($"[MonsterBase] ==================== AWAKE COMPLETE ====================");
        }

        void OnDrawGizmosSelected()
        {
            // Draw collider bounds for debugging
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                BoxCollider box = col as BoxCollider;
                if (box != null)
                {
                    Gizmos.color = col.isTrigger ? Color.yellow : Color.green;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(box.center, box.size);
                }
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

            // Spawn damage number indicator
            Vector3 damageNumberPos = transform.position + Vector3.up * 1.5f;
            DamageNumber.Create(damage, damageNumberPos, Color.white);

            // Flash effect (red flash twice)
            StartFlashEffect();

            // Damage taken animation (quick recoil) - VERY OBVIOUS
            StartCoroutine(DamageRecoilAnimation());

            // Also scale up briefly for obvious feedback
            StartCoroutine(ScaleUpAnimation());

            // Knockback
            ApplyKnockback(hitDirection);

            // Stun for 2 seconds
            MonsterAI monsterAI = GetComponent<MonsterAI>();
            if (monsterAI != null)
            {
                monsterAI.Stun(2f);
            }

            // Check death
            if (currentHP <= 0)
            {
                Die();
            }
        }

        void StartFlashEffect()
        {
            if (isFlashing)
            {
                if (showDebug)
                    Debug.Log($"[MonsterBase] {gameObject.name} already flashing, skipping new flash");
                return;
            }

            if (showDebug)
                Debug.Log($"[MonsterBase] ==================== STARTING FLASH EFFECT ====================");
                Debug.Log($"[MonsterBase] {gameObject.name} starting flash with {meshRenderers.Count} renderers");

            isFlashing = true;
            flashTimer = 0f;
            currentFlashCycle = 0;
            ApplyHitFlash();
        }

        void ApplyHitFlash()
        {
            if (showDebug)
                Debug.Log($"[MonsterBase] Applying hit flash to {meshRenderers.Count} renderers");

            int rendererCount = 0;
            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (renderer == null)
                {
                    if (showDebug)
                        Debug.LogWarning($"[MonsterBase] Renderer {rendererCount} is null!");
                    rendererCount++;
                    continue;
                }

                Material[] materials = renderer.materials;
                if (showDebug)
                    Debug.Log($"[MonsterBase] Renderer {rendererCount} has {materials.Length} materials");

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null)
                    {
                        if (showDebug)
                            Debug.LogWarning($"[MonsterBase] Material {i} on renderer {rendererCount} is null!");
                        continue;
                    }

                    Color originalColor = materials[i].color;
                    materials[i].color = hitFlashColor;

                    if (showDebug)
                        Debug.Log($"[MonsterBase] Changed material {i} color from {originalColor} to {hitFlashColor}");
                }
                renderer.materials = materials;
                rendererCount++;
            }

            if (showDebug)
                Debug.Log($"[MonsterBase] Applied flash to {rendererCount} renderers");
        }

        void RestoreOriginalMaterials()
        {
            if (showDebug)
                Debug.Log($"[MonsterBase] Restoring original materials for {meshRenderers.Count} renderers");

            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (originalMaterials.ContainsKey(renderer))
                {
                    renderer.materials = originalMaterials[renderer];
                }
                else if (showDebug)
                {
                    Debug.LogWarning($"[MonsterBase] No original materials found for renderer on {renderer.gameObject.name}");
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

            // Fade out and shrink at the end
            yield return FadeOutAndShrink();

            // Wait a bit then remove
            yield return new WaitForSeconds(0.5f);
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

        IEnumerator FadeOutAndShrink()
        {
            // Final fade and shrink
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            // Get all materials for fading
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            Dictionary<MeshRenderer, Color[]> originalColors = new Dictionary<MeshRenderer, Color[]>();

            // Store original colors and enable transparency
            foreach (MeshRenderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                Color[] colors = new Color[materials.Length];

                for (int i = 0; i < materials.Length; i++)
                {
                    colors[i] = materials[i].color;

                    // Enable transparency
                    materials[i].SetFloat("_Surface", 1); // Transparent
                    materials[i].SetFloat("_Blend", 0); // Alpha blending
                    materials[i].renderQueue = 3000;
                }

                renderer.materials = materials;
                originalColors[renderer] = colors;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Shrink to 10% size
                transform.localScale = Vector3.Lerp(startScale, startScale * 0.1f, progress);

                // Fade out all materials
                foreach (MeshRenderer renderer in renderers)
                {
                    if (renderer == null || !originalColors.ContainsKey(renderer)) continue;

                    Material[] materials = renderer.materials;
                    Color[] origColors = originalColors[renderer];

                    for (int i = 0; i < materials.Length; i++)
                    {
                        Color color = origColors[i];
                        color.a = 1f - progress;
                        materials[i].color = color;
                    }

                    renderer.materials = materials;
                }

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
        /// Quick recoil animation when taking damage
        /// </summary>
        IEnumerator DamageRecoilAnimation()
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.15f;
            float elapsed = 0f;

            // Squeeze animation (quick squash and stretch)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Squash slightly then return to normal
                float squash = 1f - (Mathf.Sin(progress * Mathf.PI) * 0.15f);
                transform.localScale = new Vector3(
                    originalScale.x * (1f + (1f - squash) * 0.2f),
                    originalScale.y * squash,
                    originalScale.z * (1f + (1f - squash) * 0.2f)
                );

                yield return null;
            }

            // Ensure we return to original scale
            transform.localScale = originalScale;
        }

        /// <summary>
        /// Scale up briefly when hit - VERY OBVIOUS visual feedback
        /// </summary>
        IEnumerator ScaleUpAnimation()
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.5f;
            float elapsed = 0f;

            // Scale up to 1.3x then back to normal
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Pulse: grow then shrink
                float scale = 1f + (Mathf.Sin(progress * Mathf.PI) * 0.3f);
                transform.localScale = originalScale * scale;

                yield return null;
            }

            // Ensure we return to original scale
            transform.localScale = originalScale;
        }

        /// <summary>
        /// Called when hit by a spell (from trigger collision)
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (showDebug)
                Debug.Log($"[MonsterBase] OnTriggerEnter with {other.gameObject.name}");

            HandleSpellHit(other.gameObject);
        }

        /// <summary>
        /// Called when hit by a spell (from physics collision)
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            if (showDebug)
                Debug.Log($"[MonsterBase] OnCollisionEnter with {collision.gameObject.name}");

            HandleSpellHit(collision.gameObject);
        }

        /// <summary>
        /// Check if hit object is a spell and apply damage
        /// Searches both the object and its parents for spell components
        /// </summary>
        void HandleSpellHit(GameObject hitObject)
        {
            if (showDebug)
                Debug.Log($"[MonsterBase] HandleSpellHit called with {hitObject.name}");

            int damage = 0;
            bool hitDetected = false;
            GameObject spellObject = null;

            // Check for PhysicsSpellProjectile (tier 2 thrown spells)
            // Search in object and parents (for particle children)
            var physicsProjectile = hitObject.GetComponentInParent<VRDungeonCrawler.Spells.PhysicsSpellProjectile>();
            if (physicsProjectile != null)
            {
                damage = physicsProjectile.GetDamage();
                hitDetected = true;
                spellObject = physicsProjectile.gameObject;

                if (showDebug)
                    Debug.Log($"[MonsterBase] ✓ Detected PhysicsSpellProjectile on {spellObject.name}, damage={damage}");
            }

            // Check for SpellProjectile (tier 1 shot spells)
            if (!hitDetected)
            {
                var spellProjectile = hitObject.GetComponentInParent<VRDungeonCrawler.Player.SpellProjectile>();
                if (spellProjectile != null)
                {
                    damage = spellProjectile.GetDamage();
                    hitDetected = true;
                    spellObject = spellProjectile.gameObject;

                    if (showDebug)
                        Debug.Log($"[MonsterBase] ✓ Detected SpellProjectile on {spellObject.name}, damage={damage}");
                }
            }

            if (hitDetected)
            {
                if (showDebug)
                    Debug.Log($"[MonsterBase] ✓ Spell hit confirmed! Calling TakeDamage with {damage} damage");

                Vector3 hitDirection = (transform.position - spellObject.transform.position).normalized;
                TakeDamage(damage, hitDirection);
            }
            else if (showDebug)
            {
                Debug.Log($"[MonsterBase] ✗ No spell component found on {hitObject.name} or its parents");
            }
        }
    }
}

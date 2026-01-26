using UnityEngine;
using System.Collections;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Simple AI controller for home area animals
    /// Each animal type has unique casual movement behavior
    /// No NavMesh required - uses simple Rigidbody movement
    /// </summary>
    public enum AnimalType
    {
        Rabbit,
        Squirrel,
        Bird,
        Deer,
        Fox
    }

    [RequireComponent(typeof(Rigidbody))]
    public class AnimalAI : MonoBehaviour
    {
        [Header("Animal Type")]
        [Tooltip("Type of animal - determines movement pattern")]
        public AnimalType animalType = AnimalType.Rabbit;

        [Header("Movement Bounds")]
        [Tooltip("Maximum distance from spawn point")]
        public float maxRoamDistance = 12f;

        [Header("Damage Response")]
        [Tooltip("Flash color when hit")]
        public Color hitFlashColor = new Color(1f, 0.8f, 0f, 1f); // Yellow flash (friendly)

        [Tooltip("Duration of flash effect")]
        public float flashDuration = 0.2f;

        [Tooltip("Number of flash cycles when hit")]
        public int flashCycles = 2;

        [Tooltip("Knockback force when hit")]
        public float knockbackForce = 4f; // Lighter than monsters

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        // Common variables
        private Rigidbody rb;
        private Vector3 spawnPosition;
        private Vector3 currentMoveDirection;
        private float nextActionTime;
        private bool isPaused = false;

        // Damage response variables
        private bool isFlashing = false;
        private float flashTimer;
        private int currentFlashCycle;
        private System.Collections.Generic.List<MeshRenderer> meshRenderers = new System.Collections.Generic.List<MeshRenderer>();
        private System.Collections.Generic.Dictionary<MeshRenderer, Material[]> originalMaterials = new System.Collections.Generic.Dictionary<MeshRenderer, Material[]>();
        private System.Collections.Generic.Dictionary<Material, Color> originalColors = new System.Collections.Generic.Dictionary<Material, Color>();
        private bool isStunned = false;
        private float stunEndTime = 0f;

        // Rabbit-specific
        private bool isHopping = false;
        private float hopCooldown = 0f;

        // Squirrel-specific
        private bool isDashing = false;
        private float dashEndTime = 0f;

        // Bird-specific
        private float flyHeight = 2.5f;
        private bool isSwooping = false;
        private float swoopStartTime = 0f;
        private Vector3 swoopStartPosition;

        // Deer-specific
        private bool isGrazing = false;

        // Fox-specific
        private bool isStalking = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;

            // Configure rigidbody
            if (rb != null)
            {
                rb.linearDamping = 2f;
                rb.angularDamping = 1f;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }

            // Cache mesh renderers and original materials for damage flash effect
            meshRenderers.Clear();
            originalMaterials.Clear();
            originalColors.Clear();

            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer != null)
                {
                    meshRenderers.Add(renderer);

                    // Store original materials
                    Material[] materials = renderer.materials;
                    Material[] materialsCopy = new Material[materials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materialsCopy[i] = new Material(materials[i]);
                        if (materials[i].HasProperty("_Color"))
                        {
                            originalColors[materialsCopy[i]] = materials[i].color;
                        }
                    }
                    originalMaterials[renderer] = materialsCopy;
                }
            }

            if (showDebug)
                Debug.Log($"[AnimalAI] Cached {meshRenderers.Count} renderers for damage flash");
        }

        void Start()
        {
            // Ensure velocity is zero
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Set initial direction and state based on animal type
            ChooseRandomDirection();
            InitializeAnimalBehavior();

            if (showDebug)
                Debug.Log($"[AnimalAI] {animalType} {gameObject.name} initialized at {spawnPosition}");
        }

        void Update()
        {
            if (Time.time >= nextActionTime)
            {
                HandleNextAction();
            }

            // Update animal-specific behaviors
            UpdateAnimalBehavior();

            // Update flash effect
            if (isFlashing)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    // Toggle between flash color and original
                    currentFlashCycle++;
                    if (currentFlashCycle >= flashCycles * 2)
                    {
                        // Flash complete
                        RestoreOriginalMaterials();
                        isFlashing = false;
                        currentFlashCycle = 0;
                    }
                    else
                    {
                        // Toggle flash
                        if (currentFlashCycle % 2 == 0)
                        {
                            RestoreOriginalMaterials();
                        }
                        else
                        {
                            ApplyHitFlash();
                        }
                        flashTimer = flashDuration;
                    }
                }
            }

            // Update stun state
            if (isStunned && Time.time >= stunEndTime)
            {
                isStunned = false;
                if (showDebug)
                    Debug.Log($"[AnimalAI] {animalType} recovered from stun");
            }
        }

        void FixedUpdate()
        {
            if (rb == null) return;

            // Move based on current state and animal type
            MoveAnimal();

            // Keep animal within bounds
            CheckBounds();
        }

        void InitializeAnimalBehavior()
        {
            switch (animalType)
            {
                case AnimalType.Rabbit:
                    // Rabbits hop with pauses
                    isPaused = false;
                    nextActionTime = Time.time + Random.Range(0.3f, 0.8f); // Quick hop timing
                    break;

                case AnimalType.Squirrel:
                    // Squirrels dart around erratically
                    isPaused = false;
                    nextActionTime = Time.time + Random.Range(0.5f, 1.5f);
                    break;

                case AnimalType.Bird:
                    // Birds fly smoothly at height
                    isPaused = false;
                    flyHeight = Random.Range(2f, 3.5f);
                    nextActionTime = Time.time + Random.Range(3f, 6f); // Swoops occasionally
                    break;

                case AnimalType.Deer:
                    // Deer graze and walk calmly
                    isGrazing = true;
                    nextActionTime = Time.time + Random.Range(3f, 5f);
                    break;

                case AnimalType.Fox:
                    // Foxes stalk slowly then dash
                    isStalking = true;
                    isPaused = false;
                    nextActionTime = Time.time + Random.Range(2f, 4f);
                    break;
            }
        }

        void HandleNextAction()
        {
            switch (animalType)
            {
                case AnimalType.Rabbit:
                    HandleRabbitAction();
                    break;

                case AnimalType.Squirrel:
                    HandleSquirrelAction();
                    break;

                case AnimalType.Bird:
                    HandleBirdAction();
                    break;

                case AnimalType.Deer:
                    HandleDeerAction();
                    break;

                case AnimalType.Fox:
                    HandleFoxAction();
                    break;
            }
        }

        void HandleRabbitAction()
        {
            if (isPaused)
            {
                // Resume hopping
                isPaused = false;
                ChooseRandomDirection();
                nextActionTime = Time.time + Random.Range(0.3f, 0.8f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Rabbit resuming hops");
            }
            else
            {
                // Pause (rabbits frequently stop to look around)
                isPaused = true;
                nextActionTime = Time.time + Random.Range(1f, 2f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Rabbit pausing");
            }
        }

        void HandleSquirrelAction()
        {
            if (isDashing)
            {
                // Stop dashing
                isDashing = false;
                isPaused = true; // Pause briefly after dash
                nextActionTime = Time.time + Random.Range(0.5f, 1f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Squirrel stopping dash");
            }
            else if (isPaused)
            {
                // Resume moving
                isPaused = false;
                ChooseRandomDirection();

                // Random chance to dash
                if (Random.value < 0.3f)
                {
                    isDashing = true;
                    dashEndTime = Time.time + Random.Range(0.5f, 1f);
                }

                nextActionTime = Time.time + Random.Range(1f, 2f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Squirrel resuming (dash={isDashing})");
            }
            else
            {
                // Pause
                isPaused = true;
                nextActionTime = Time.time + Random.Range(0.3f, 0.8f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Squirrel pausing");
            }
        }

        void HandleBirdAction()
        {
            if (isSwooping)
            {
                // Stop swooping
                isSwooping = false;
                nextActionTime = Time.time + Random.Range(4f, 7f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Bird ending swoop");
            }
            else
            {
                // Start swoop or change direction
                if (Random.value < 0.3f)
                {
                    isSwooping = true;
                    swoopStartTime = Time.time;
                    swoopStartPosition = transform.position;
                    nextActionTime = Time.time + Random.Range(1.5f, 2.5f);

                    if (showDebug)
                        Debug.Log($"[AnimalAI] Bird starting swoop");
                }
                else
                {
                    ChooseRandomDirection();
                    nextActionTime = Time.time + Random.Range(3f, 5f);
                }
            }
        }

        void HandleDeerAction()
        {
            if (isGrazing)
            {
                // Stop grazing, start walking
                isGrazing = false;
                ChooseRandomDirection();
                nextActionTime = Time.time + Random.Range(3f, 5f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Deer walking");
            }
            else
            {
                // Stop and graze
                isGrazing = true;
                nextActionTime = Time.time + Random.Range(2f, 4f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Deer grazing");
            }
        }

        void HandleFoxAction()
        {
            if (isStalking)
            {
                // Stop stalking and dash
                isStalking = false;
                ChooseRandomDirection();
                nextActionTime = Time.time + Random.Range(1f, 2f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Fox dashing");
            }
            else
            {
                // Resume stalking
                isStalking = true;
                ChooseRandomDirection();
                nextActionTime = Time.time + Random.Range(2f, 4f);

                if (showDebug)
                    Debug.Log($"[AnimalAI] Fox stalking");
            }
        }

        void UpdateAnimalBehavior()
        {
            switch (animalType)
            {
                case AnimalType.Rabbit:
                    UpdateRabbitBehavior();
                    break;

                case AnimalType.Squirrel:
                    UpdateSquirrelBehavior();
                    break;

                case AnimalType.Bird:
                    UpdateBirdBehavior();
                    break;
            }
        }

        void UpdateRabbitBehavior()
        {
            // Hopping animation effect - small hops
            if (!isPaused && hopCooldown <= 0f)
            {
                // Apply small hop force
                if (rb != null && !isHopping)
                {
                    rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
                    isHopping = true;
                    hopCooldown = 0.4f; // Hop every 0.4 seconds
                }
            }
            else
            {
                hopCooldown -= Time.deltaTime;
                if (hopCooldown <= 0f)
                {
                    isHopping = false;
                }
            }
        }

        void UpdateSquirrelBehavior()
        {
            // End dash after duration
            if (isDashing && Time.time >= dashEndTime)
            {
                isDashing = false;
            }
        }

        void UpdateBirdBehavior()
        {
            // Smooth swooping motion
            if (isSwooping)
            {
                float swoopDuration = 2f;
                float swoopProgress = (Time.time - swoopStartTime) / swoopDuration;

                if (swoopProgress < 1f)
                {
                    // Sine wave swoop down and back up
                    float swoopOffset = Mathf.Sin(swoopProgress * Mathf.PI) * 1.5f;
                    float targetY = flyHeight - swoopOffset;

                    // Smoothly move toward target height with higher lerp speed
                    Vector3 pos = transform.position;
                    pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 8f); // Increased from 3f
                    transform.position = pos;
                }
            }
            else
            {
                // Maintain flying height with smooth interpolation
                Vector3 pos = transform.position;
                float targetHeight = spawnPosition.y + flyHeight;
                pos.y = Mathf.Lerp(pos.y, targetHeight, Time.deltaTime * 6f); // Increased from 2f
                transform.position = pos;
            }
        }

        void MoveAnimal()
        {
            // Don't move if paused, grazing, or stunned
            if (isPaused || isGrazing || isStunned)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                return;
            }

            // Calculate movement speed based on animal type and state
            float moveSpeed = GetCurrentMoveSpeed();

            Vector3 movement = currentMoveDirection * moveSpeed;
            Vector3 newVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

            rb.linearVelocity = newVelocity;

            // Rotate to face movement direction
            if (currentMoveDirection.sqrMagnitude > 0.01f)
            {
                // Smoothly rotate to face the movement direction
                Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 8f);
            }
        }

        float GetCurrentMoveSpeed()
        {
            switch (animalType)
            {
                case AnimalType.Rabbit:
                    return isPaused ? 0f : 2.5f; // Quick hops

                case AnimalType.Squirrel:
                    return isDashing ? 5f : 2f; // Dash or normal

                case AnimalType.Bird:
                    return 3f; // Smooth flying speed

                case AnimalType.Deer:
                    return isGrazing ? 0f : 1.5f; // Slow, graceful

                case AnimalType.Fox:
                    return isStalking ? 1f : 3.5f; // Stalk slow, dash fast

                default:
                    return 1.5f;
            }
        }

        void ChooseRandomDirection()
        {
            // Pick random direction on XZ plane
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            currentMoveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;

            if (showDebug)
                Debug.Log($"[AnimalAI] {animalType} new direction: {currentMoveDirection}");
        }

        void CheckBounds()
        {
            // If animal gets too far from spawn, turn back
            float distanceFromSpawn = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(spawnPosition.x, 0f, spawnPosition.z)
            );

            if (distanceFromSpawn > maxRoamDistance)
            {
                // Point back toward spawn
                Vector3 toSpawn = (spawnPosition - transform.position).normalized;
                currentMoveDirection = new Vector3(toSpawn.x, 0f, toSpawn.z).normalized;

                if (showDebug)
                    Debug.Log($"[AnimalAI] {animalType} out of bounds, returning to spawn");
            }
        }

        // ========================================
        // DAMAGE SYSTEM
        // ========================================

        /// <summary>
        /// Take damage from spells or other sources
        /// Animals have infinite HP but still react to damage
        /// </summary>
        public void TakeDamage(int damage, Vector3 hitDirection)
        {
            if (showDebug)
                Debug.Log($"[AnimalAI] {animalType} {gameObject.name} took {damage} damage from direction {hitDirection}");

            // Spawn damage number (yellow for friendly animals)
            Vector3 damageNumberPos = transform.position + Vector3.up * 0.5f;
            DamageNumber.Create(damage, damageNumberPos, hitFlashColor);

            // Apply visual flash effect
            StartFlashEffect();

            // Apply knockback
            ApplyKnockback(hitDirection);

            // Apply stun (brief pause)
            isStunned = true;
            stunEndTime = Time.time + 0.5f; // 0.5 second stun

            // Start damage animations
            StartCoroutine(DamageRecoilAnimation());
            StartCoroutine(ScaleUpAnimation());
        }

        /// <summary>
        /// Start the flash effect
        /// </summary>
        void StartFlashEffect()
        {
            isFlashing = true;
            flashTimer = flashDuration;
            currentFlashCycle = 0;
            ApplyHitFlash();
        }

        /// <summary>
        /// Apply hit flash color to all materials
        /// </summary>
        void ApplyHitFlash()
        {
            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].HasProperty("_Color"))
                        {
                            materials[i].color = hitFlashColor;
                        }
                    }
                    renderer.materials = materials;
                }
            }
        }

        /// <summary>
        /// Restore original material colors
        /// </summary>
        void RestoreOriginalMaterials()
        {
            foreach (var kvp in originalMaterials)
            {
                MeshRenderer renderer = kvp.Key;
                Material[] originalMats = kvp.Value;

                if (renderer != null)
                {
                    Material[] currentMats = renderer.materials;
                    for (int i = 0; i < currentMats.Length && i < originalMats.Length; i++)
                    {
                        if (currentMats[i].HasProperty("_Color") && originalColors.ContainsKey(originalMats[i]))
                        {
                            currentMats[i].color = originalColors[originalMats[i]];
                        }
                    }
                    renderer.materials = currentMats;
                }
            }
        }

        /// <summary>
        /// Apply knockback force when hit
        /// </summary>
        void ApplyKnockback(Vector3 direction)
        {
            if (rb != null)
            {
                // Normalize and apply force
                Vector3 knockbackDir = direction.normalized;

                // Add slight upward component for more dramatic effect
                knockbackDir.y = 0.3f;
                knockbackDir.Normalize();

                rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);

                if (showDebug)
                    Debug.Log($"[AnimalAI] {animalType} knocked back with force {knockbackForce}");
            }
        }

        /// <summary>
        /// Damage recoil animation - squash effect
        /// </summary>
        IEnumerator DamageRecoilAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 squashScale = new Vector3(originalScale.x * 0.85f, originalScale.y * 1.15f, originalScale.z * 0.85f);

            float duration = 0.15f;
            float elapsed = 0f;

            // Squash
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, squashScale, t);
                yield return null;
            }

            // Return to normal
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(squashScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Scale up animation - brief size pulse
        /// </summary>
        IEnumerator ScaleUpAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 largeScale = originalScale * 1.1f;

            float duration = 0.1f;
            float elapsed = 0f;

            // Scale up
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, largeScale, t);
                yield return null;
            }

            // Scale back down
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(largeScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Handle collision with spell projectiles
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            HandleSpellHit(other);
        }

        /// <summary>
        /// Handle collision with spell projectiles (alternative collision detection)
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            HandleSpellHit(collision.collider);
        }

        /// <summary>
        /// Process spell hit and apply damage
        /// Uses same robust detection as MonsterBase
        /// </summary>
        void HandleSpellHit(Collider other)
        {
            if (showDebug)
                Debug.Log($"[AnimalAI] {animalType} HandleSpellHit called with {other.gameObject.name}");

            int damage = 0;
            bool hitDetected = false;
            GameObject spellObject = null;

            // Check for PhysicsSpellProjectile (tier 2 thrown spells)
            // Search in object and parents (for particle children)
            var physicsProjectile = other.GetComponentInParent<VRDungeonCrawler.Spells.PhysicsSpellProjectile>();
            if (physicsProjectile != null)
            {
                damage = physicsProjectile.GetDamage();
                hitDetected = true;
                spellObject = physicsProjectile.gameObject;

                if (showDebug)
                    Debug.Log($"[AnimalAI] {animalType} detected PhysicsSpellProjectile, damage={damage}");
            }

            // Check for SpellProjectile (tier 1 shot spells)
            if (!hitDetected)
            {
                var spellProjectile = other.GetComponentInParent<VRDungeonCrawler.Player.SpellProjectile>();
                if (spellProjectile != null)
                {
                    damage = spellProjectile.GetDamage();
                    hitDetected = true;
                    spellObject = spellProjectile.gameObject;

                    if (showDebug)
                        Debug.Log($"[AnimalAI] {animalType} detected SpellProjectile, damage={damage}");
                }
            }

            if (hitDetected && spellObject != null)
            {
                Vector3 hitDirection = (transform.position - spellObject.transform.position).normalized;
                TakeDamage(damage, hitDirection);

                if (showDebug)
                    Debug.Log($"[AnimalAI] {animalType} taking {damage} damage from spell");
            }
            else if (showDebug)
            {
                Debug.Log($"[AnimalAI] {animalType} no spell component found on {other.gameObject.name}");
            }
        }
    }
}

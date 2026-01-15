using UnityEngine;
using System.Collections;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// AI controller for dungeon monsters
    /// Each monster type has unique movement pattern:
    /// - Goblin: Fast darting with sudden direction changes
    /// - Skeleton: Slow shambling with pauses
    /// - Slime: Bouncing with squash/stretch animation
    /// All movement is random and unaware of player
    /// </summary>
    [RequireComponent(typeof(MonsterBase))]
    [RequireComponent(typeof(Rigidbody))]
    public class MonsterAI : MonoBehaviour
    {
        [Header("Movement Bounds")]
        [Tooltip("Maximum distance from spawn point")]
        public float maxRoamDistance = 8f;

        [Header("Goblin Settings")]
        [Tooltip("Goblin movement speed")]
        public float goblinSpeed = 3.5f;

        [Tooltip("Time between direction changes")]
        public float goblinDirectionChangeMin = 1f;
        public float goblinDirectionChangeMax = 2f;

        [Header("Skeleton Settings")]
        [Tooltip("Skeleton movement speed")]
        public float skeletonSpeed = 1.2f;

        [Tooltip("Time between pauses")]
        public float skeletonPauseMin = 2f;
        public float skeletonPauseMax = 4f;

        [Tooltip("Pause duration")]
        public float skeletonPauseDuration = 2f;

        [Tooltip("Sway amount while walking")]
        public float skeletonSwayAmount = 0.05f;

        [Header("Slime Settings")]
        [Tooltip("Slime hop speed")]
        public float slimeSpeed = 2.2f;

        [Tooltip("Time between hops")]
        public float slimeHopMin = 0.5f;
        public float slimeHopMax = 1f;

        [Tooltip("Hop height")]
        public float slimeHopForce = 5f;

        [Tooltip("Squash/stretch animation intensity")]
        public float slimeSquashAmount = 0.3f;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        // Internal state
        private MonsterBase monsterBase;
        private Rigidbody rb;
        private Vector3 spawnPosition;
        private Vector3 currentMoveDirection;
        private float nextActionTime;
        private bool isPaused = false;
        private bool isGrounded = true;
        private Vector3 originalScale;
        private float swayTimer = 0f;

        void Awake()
        {
            monsterBase = GetComponent<MonsterBase>();
            rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            originalScale = transform.localScale;

            // Set random initial direction
            ChooseRandomDirection();
        }

        void Start()
        {
            // Schedule first action based on monster type
            switch (monsterBase.monsterType)
            {
                case MonsterType.Goblin:
                    nextActionTime = Time.time + Random.Range(goblinDirectionChangeMin, goblinDirectionChangeMax);
                    break;
                case MonsterType.Skeleton:
                    nextActionTime = Time.time + Random.Range(skeletonPauseMin, skeletonPauseMax);
                    break;
                case MonsterType.Slime:
                    nextActionTime = Time.time + Random.Range(slimeHopMin, slimeHopMax);
                    break;
            }
        }

        void Update()
        {
            // Update movement based on monster type
            switch (monsterBase.monsterType)
            {
                case MonsterType.Goblin:
                    UpdateGoblinMovement();
                    break;
                case MonsterType.Skeleton:
                    UpdateSkeletonMovement();
                    break;
                case MonsterType.Slime:
                    UpdateSlimeMovement();
                    break;
            }
        }

        void FixedUpdate()
        {
            // Apply physics-based movement
            if (!isPaused)
            {
                switch (monsterBase.monsterType)
                {
                    case MonsterType.Goblin:
                        ApplyGoblinMovement();
                        break;
                    case MonsterType.Skeleton:
                        ApplySkeletonMovement();
                        break;
                    case MonsterType.Slime:
                        // Slime uses hop forces, not continuous movement
                        break;
                }
            }

            // Keep monster within bounds
            CheckBounds();
        }

        #region Goblin Movement
        void UpdateGoblinMovement()
        {
            // Check if time to change direction
            if (Time.time >= nextActionTime)
            {
                ChooseRandomDirection();
                nextActionTime = Time.time + Random.Range(goblinDirectionChangeMin, goblinDirectionChangeMax);

                if (showDebug)
                    Debug.Log($"[MonsterAI] Goblin changing direction to {currentMoveDirection}");
            }
        }

        void ApplyGoblinMovement()
        {
            // Fast darting movement
            Vector3 movement = currentMoveDirection * goblinSpeed;
            rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        }
        #endregion

        #region Skeleton Movement
        void UpdateSkeletonMovement()
        {
            // Check if time to toggle pause/move
            if (Time.time >= nextActionTime)
            {
                isPaused = !isPaused;

                if (isPaused)
                {
                    // Start pause
                    nextActionTime = Time.time + skeletonPauseDuration;
                    rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

                    if (showDebug)
                        Debug.Log($"[MonsterAI] Skeleton pausing for {skeletonPauseDuration}s");
                }
                else
                {
                    // Resume movement with new direction
                    ChooseRandomDirection();
                    nextActionTime = Time.time + Random.Range(skeletonPauseMin, skeletonPauseMax);

                    if (showDebug)
                        Debug.Log($"[MonsterAI] Skeleton resuming movement");
                }
            }

            // Apply subtle sway while moving
            if (!isPaused)
            {
                swayTimer += Time.deltaTime * 2f;
                float swayOffset = Mathf.Sin(swayTimer) * skeletonSwayAmount;
                Vector3 swayPosition = transform.position;
                swayPosition.x += swayOffset * Time.deltaTime;
                transform.position = swayPosition;
            }
        }

        void ApplySkeletonMovement()
        {
            if (!isPaused)
            {
                // Slow shambling movement
                Vector3 movement = currentMoveDirection * skeletonSpeed;
                rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            }
        }
        #endregion

        #region Slime Movement
        void UpdateSlimeMovement()
        {
            // Check if time to hop
            if (Time.time >= nextActionTime && isGrounded)
            {
                StartCoroutine(SlimeHop());
                nextActionTime = Time.time + Random.Range(slimeHopMin, slimeHopMax);
            }
        }

        IEnumerator SlimeHop()
        {
            // Pre-hop squash
            yield return StartCoroutine(SlimeSquash());

            // Apply hop force
            ChooseRandomDirection();
            Vector3 hopDirection = currentMoveDirection * slimeSpeed + Vector3.up * slimeHopForce;
            rb.AddForce(hopDirection, ForceMode.Impulse);
            isGrounded = false;

            if (showDebug)
                Debug.Log($"[MonsterAI] Slime hopping in direction {currentMoveDirection}");

            // Stretch in air
            yield return StartCoroutine(SlimeStretch());

            // Wait for landing
            yield return new WaitUntil(() => isGrounded);

            // Land squash
            yield return StartCoroutine(SlimeSquash());

            // Return to normal
            transform.localScale = originalScale;
        }

        IEnumerator SlimeSquash()
        {
            // Squash animation (wider, shorter)
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 squashedScale = new Vector3(
                originalScale.x * (1f + slimeSquashAmount),
                originalScale.y * (1f - slimeSquashAmount),
                originalScale.z * (1f + slimeSquashAmount)
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, squashedScale, t);
                yield return null;
            }
        }

        IEnumerator SlimeStretch()
        {
            // Stretch animation (narrower, taller)
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 stretchedScale = new Vector3(
                originalScale.x * (1f - slimeSquashAmount * 0.5f),
                originalScale.y * (1f + slimeSquashAmount),
                originalScale.z * (1f - slimeSquashAmount * 0.5f)
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(transform.localScale, stretchedScale, t);
                yield return null;
            }
        }
        #endregion

        #region Shared Utilities
        void ChooseRandomDirection()
        {
            // Pick random direction on XZ plane
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            currentMoveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
        }

        void CheckBounds()
        {
            // If monster gets too far from spawn, turn back
            float distanceFromSpawn = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                                       new Vector3(spawnPosition.x, 0f, spawnPosition.z));

            if (distanceFromSpawn > maxRoamDistance)
            {
                // Point back toward spawn
                Vector3 toSpawn = (spawnPosition - transform.position).normalized;
                currentMoveDirection = new Vector3(toSpawn.x, 0f, toSpawn.z).normalized;

                if (showDebug)
                    Debug.Log($"[MonsterAI] {gameObject.name} out of bounds, returning to spawn");
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            // Detect ground for slime
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true;
            }
        }

        void OnCollisionStay(Collision collision)
        {
            // Stay grounded
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true;
            }
        }

        void OnCollisionExit(Collision collision)
        {
            // Left ground
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = false;
            }
        }
        #endregion
    }
}

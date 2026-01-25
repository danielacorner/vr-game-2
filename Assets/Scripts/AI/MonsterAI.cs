using UnityEngine;
using System.Collections;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// AI controller for dungeon monsters
    /// Monsters use casual patrol pattern by default, but become aggressive when player is nearby or when damaged
    /// </summary>
    [RequireComponent(typeof(MonsterBase))]
    [RequireComponent(typeof(Rigidbody))]
    public class MonsterAI : MonoBehaviour
    {
        [Header("Casual Patrol Settings")]
        [Tooltip("Slow walk speed")]
        public float walkSpeed = 1.2f;

        [Tooltip("Minimum time walking before stopping")]
        public float walkTimeMin = 3f;

        [Tooltip("Maximum time walking before stopping")]
        public float walkTimeMax = 6f;

        [Tooltip("Minimum pause duration")]
        public float pauseTimeMin = 2f;

        [Tooltip("Maximum pause duration")]
        public float pauseTimeMax = 4f;

        [Header("Aggro/Chase Settings")]
        [Tooltip("Distance at which monster detects and chases player")]
        public float aggroRange = 2f;

        [Tooltip("Speed when chasing player (faster than patrol)")]
        public float chaseSpeed = 3.5f;

        [Tooltip("How long to chase player after losing sight")]
        public float aggroCooldownTime = 5f;

        [Header("Movement Bounds")]
        [Tooltip("Maximum distance from spawn point")]
        public float maxRoamDistance = 8f;

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
        private bool isStunned = false;
        private float stunEndTime;

        // Aggro state
        private bool isAggro = false;
        private Transform playerTarget = null;
        private float lastAggroTime = 0f;

        void Awake()
        {
            monsterBase = GetComponent<MonsterBase>();
            rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;

            // Set random initial direction
            ChooseRandomDirection();
        }

        void Start()
        {
            // Ensure rigidbody velocity is zero before starting patrol
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Find player (XR Origin)
            GameObject xrOrigin = GameObject.Find("XR Origin");
            if (xrOrigin != null)
            {
                playerTarget = xrOrigin.transform;
            }
            else
            {
                // Fallback: try to find by tag
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTarget = player.transform;
                }
                else
                {
                    Debug.LogWarning($"[MonsterAI] {gameObject.name} could not find player target");
                }
            }

            // Start with walking
            isPaused = false;
            nextActionTime = Time.time + Random.Range(walkTimeMin, walkTimeMax);

            if (showDebug)
                Debug.Log($"[MonsterAI] {gameObject.name} started patrol at position {transform.position}");
        }

        void Update()
        {
            // Check if stunned
            if (isStunned)
            {
                if (Time.time >= stunEndTime)
                {
                    isStunned = false;

                    if (showDebug)
                        Debug.Log($"[MonsterAI] {gameObject.name} stun ended, resuming movement");
                }
                return; // Don't process normal movement while stunned
            }

            // Check for player proximity to trigger aggro
            if (playerTarget != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

                // Trigger aggro if player is within range
                if (distanceToPlayer <= aggroRange)
                {
                    if (!isAggro)
                    {
                        TriggerAggro();
                    }
                    lastAggroTime = Time.time;
                }
                else if (isAggro)
                {
                    // Check if aggro should cooldown (player out of range for too long)
                    if (Time.time - lastAggroTime > aggroCooldownTime)
                    {
                        isAggro = false;
                        isPaused = false;
                        ChooseRandomDirection();
                        nextActionTime = Time.time + Random.Range(walkTimeMin, walkTimeMax);

                        if (showDebug)
                            Debug.Log($"[MonsterAI] {gameObject.name} lost aggro, returning to patrol");
                    }
                }
            }

            // If in aggro mode, skip patrol logic
            if (isAggro)
            {
                return;
            }

            // Normal patrol logic
            // Check if time to change state
            if (Time.time >= nextActionTime)
            {
                if (isPaused)
                {
                    // Resume walking
                    isPaused = false;
                    ChooseRandomDirection();
                    nextActionTime = Time.time + Random.Range(walkTimeMin, walkTimeMax);

                    if (showDebug)
                        Debug.Log($"[MonsterAI] {gameObject.name} resuming walk");
                }
                else
                {
                    // Stop and pause
                    isPaused = true;
                    nextActionTime = Time.time + Random.Range(pauseTimeMin, pauseTimeMax);

                    if (showDebug)
                        Debug.Log($"[MonsterAI] {gameObject.name} pausing");
                }
            }
        }

        void FixedUpdate()
        {
            if (rb == null) return;

            // Don't move if stunned
            if (isStunned)
            {
                // Stop horizontal movement when stunned
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                return;
            }

            // Aggro/Chase behavior
            if (isAggro && playerTarget != null)
            {
                // Calculate direction to player
                Vector3 toPlayer = (playerTarget.position - transform.position).normalized;
                Vector3 chaseDirection = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;

                // Rotate to face player
                if (chaseDirection.magnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(chaseDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                }

                // Move aggressively toward player
                Vector3 movement = chaseDirection * chaseSpeed;
                Vector3 newVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
                rb.linearVelocity = newVelocity;

                if (showDebug)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
                    Debug.Log($"[MonsterAI] {gameObject.name} chasing player (distance={distanceToPlayer:F1})");
                }

                return;
            }

            // Normal patrol behavior
            if (!isPaused)
            {
                // Rotate to face movement direction
                if (currentMoveDirection.magnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
                }

                // Walk slowly in current direction
                Vector3 movement = currentMoveDirection * walkSpeed;
                Vector3 newVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

                // Debug log if velocity is unexpectedly high
                if (rb.linearVelocity.magnitude > walkSpeed * 2f && showDebug)
                {
                    Debug.LogWarning($"[MonsterAI] {gameObject.name} has high velocity {rb.linearVelocity.magnitude:F2}, resetting to walk speed");
                }

                rb.linearVelocity = newVelocity;
            }
            else
            {
                // Stop horizontal movement when paused
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            // Keep monster within bounds (only during patrol, not during chase)
            if (!isAggro)
            {
                CheckBounds();
            }
        }

        void ChooseRandomDirection()
        {
            // Pick random direction on XZ plane
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            currentMoveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;

            if (showDebug)
                Debug.Log($"[MonsterAI] {gameObject.name} new direction: {currentMoveDirection}");
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

                // Reset action timer to allow normal pause cycle to resume
                // This prevents the "racing forever" bug
                if (!isPaused)
                {
                    nextActionTime = Time.time + Random.Range(walkTimeMin, walkTimeMax);
                }

                if (showDebug)
                    Debug.Log($"[MonsterAI] {gameObject.name} out of bounds (distance={distanceFromSpawn:F1}), returning to spawn");
            }
        }

        /// <summary>
        /// Stun the monster for a duration (called by MonsterBase when hit)
        /// Also triggers aggro toward player
        /// </summary>
        public void Stun(float duration)
        {
            isStunned = true;
            stunEndTime = Time.time + duration;

            // Stop movement immediately
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            // Getting hit triggers aggro
            TriggerAggro();

            if (showDebug)
                Debug.Log($"[MonsterAI] {gameObject.name} stunned for {duration} seconds and now aggro");
        }

        /// <summary>
        /// Triggers aggressive behavior toward player
        /// Called when player gets close or when monster is damaged
        /// </summary>
        public void TriggerAggro()
        {
            if (isAggro) return; // Already aggro

            isAggro = true;
            isPaused = false; // Stop pausing
            lastAggroTime = Time.time;

            if (showDebug)
                Debug.Log($"[MonsterAI] {gameObject.name} triggered aggro - now chasing player!");
        }
    }
}

using UnityEngine;
using System.Collections;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// AI controller for dungeon monsters
    /// All monsters use casual patrol pattern: move slowly, stop occasionally, change direction
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

            if (!isPaused)
            {
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

            // Keep monster within bounds
            CheckBounds();
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

                if (showDebug)
                    Debug.Log($"[MonsterAI] {gameObject.name} out of bounds, returning to spawn");
            }
        }

        /// <summary>
        /// Stun the monster for a duration (called by MonsterBase when hit)
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

            if (showDebug)
                Debug.Log($"[MonsterAI] {gameObject.name} stunned for {duration} seconds");
        }
    }
}

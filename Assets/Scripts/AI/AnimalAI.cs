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

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        // Common variables
        private Rigidbody rb;
        private Vector3 spawnPosition;
        private Vector3 currentMoveDirection;
        private float nextActionTime;
        private bool isPaused = false;

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
                    Vector3 targetPos = transform.position;
                    targetPos.y = flyHeight - swoopOffset;

                    // Smoothly move toward target height
                    Vector3 pos = transform.position;
                    pos.y = Mathf.Lerp(pos.y, targetPos.y, Time.deltaTime * 3f);
                    transform.position = pos;
                }
            }
            else
            {
                // Maintain flying height
                Vector3 pos = transform.position;
                float targetHeight = spawnPosition.y + flyHeight;
                pos.y = Mathf.Lerp(pos.y, targetHeight, Time.deltaTime * 2f);
                transform.position = pos;
            }
        }

        void MoveAnimal()
        {
            // Don't move if paused or grazing
            if (isPaused || isGrazing)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                return;
            }

            // Calculate movement speed based on animal type and state
            float moveSpeed = GetCurrentMoveSpeed();

            Vector3 movement = currentMoveDirection * moveSpeed;
            Vector3 newVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

            rb.linearVelocity = newVelocity;
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
    }
}

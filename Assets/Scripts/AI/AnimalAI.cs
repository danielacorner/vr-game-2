using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Animal behavior AI with flee and wander states
    /// Uses NavMesh for pathfinding on terrain
    /// Animals flee when player approaches, otherwise wander naturally
    /// Reacts to spell damage with flash and bounce effects (Zelda-style)
    /// </summary>
    public enum AnimalState
    {
        Idle,
        Wander,
        Flee
    }

    public enum AnimalType
    {
        Rabbit,
        Squirrel,
        Bird
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class AnimalAI : MonoBehaviour
    {
        [Header("Animal Type")]
        [Tooltip("Type of animal for movement style")]
        public AnimalType animalType = AnimalType.Rabbit;

        [Header("Detection")]
        [Tooltip("Maximum distance to detect player presence")]
        public float detectionRadius = 15f;

        [Tooltip("Distance at which animal starts fleeing")]
        public float fleeDistance = 3f;

        [Tooltip("Layer mask for player detection")]
        public LayerMask playerLayer = -1; // All layers by default

        [Header("Movement")]
        [Tooltip("Walking speed during wander")]
        public float walkSpeed = 1.5f;

        [Tooltip("Running speed during flee")]
        public float fleeSpeed = 4f;

        [Tooltip("Maximum distance to wander from spawn point")]
        public float wanderRadius = 10f;

        [Tooltip("Time to stay idle before wandering again")]
        public float idleTime = 1f;

        [Tooltip("Minimum distance to flee from player")]
        public float minFleeDistance = 10f;

        [Header("Animal-Specific Movement")]
        [Tooltip("Hopping force for rabbits (applies to Y axis)")]
        public float hopForce = 3f;

        [Tooltip("Time between hops for rabbits")]
        public float hopInterval = 0.5f;

        [Tooltip("Erratic movement frequency for squirrels")]
        public float erraticFrequency = 0.2f;

        [Tooltip("Flying height for birds")]
        public float flyingHeight = 2f;

        [Header("Damage Reaction")]
        [Tooltip("Flash color when hit (Zelda-style)")]
        public Color hitFlashColor = Color.white;

        [Tooltip("Duration of flash effect")]
        public float flashDuration = 0.2f;

        [Tooltip("Bounce force when hit (should be small, ~2 for 0.5m bounce)")]
        public float bounceForce = 2f;

        [Tooltip("Number of flash cycles when hit")]
        public int flashCycles = 3;

        [Header("Animation")]
        [Tooltip("Optional animator for animal animations")]
        public Animator animator;

        [Header("Debug")]
        [Tooltip("Show debug information in console")]
        public bool showDebug = false;

        private NavMeshAgent agent;
        private Rigidbody rb;
        private AnimalState currentState = AnimalState.Idle;
        private Transform player;
        private float stateTimer;
        private Vector3 homePosition;
        private bool isInitialized = false;
        private float hopTimer;
        private float erraticTimer;

        // Damage reaction
        private bool isFlashing = false;
        private float flashTimer;
        private int currentFlashCycle;
        private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        private Dictionary<MeshRenderer, Color[]> originalColors = new Dictionary<MeshRenderer, Color[]>();
        private Dictionary<MeshRenderer, Material[]> originalMaterials = new Dictionary<MeshRenderer, Material[]>();

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>();

            // If no rigidbody, add one for physics reactions
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                rb.linearDamping = 2f;
                rb.angularDamping = 1f;
                rb.useGravity = true; // IMPORTANT: Enable gravity!
                rb.constraints = RigidbodyConstraints.FreezeRotation; // Keep upright
                rb.isKinematic = true; // NavMesh controls movement normally
            }

            // Ensure gravity is always enabled
            if (rb != null)
            {
                rb.useGravity = true;
            }

            homePosition = transform.position;

            // Store all mesh renderers for flash effect
            meshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
            foreach (MeshRenderer renderer in meshRenderers)
            {
                // Store original materials
                originalMaterials[renderer] = renderer.materials;

                // Store original colors
                Color[] colors = new Color[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    colors[i] = renderer.materials[i].color;
                }
                originalColors[renderer] = colors;
            }
        }

        void Start()
        {
            // Find player (VR camera or XR Origin)
            FindPlayer();

            // Configure NavMeshAgent based on animal type
            if (agent != null)
            {
                ConfigureMovementForAnimalType();
                agent.angularSpeed = 120f;
                agent.acceleration = 8f;

                // Check if NavMesh exists at current position
                NavMeshHit hit;
                if (!NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
                {
                    Debug.LogWarning($"[AnimalAI] {gameObject.name} is not on NavMesh! Animal won't be able to move. Please bake NavMesh on terrain.");
                }
                else if (showDebug)
                {
                    Debug.Log($"[AnimalAI] {gameObject.name} is on NavMesh, can navigate");
                }
            }

            // Start wandering immediately (no idle time on spawn)
            ChangeState(AnimalState.Wander);
            isInitialized = true;

            if (showDebug)
                Debug.Log($"[AnimalAI] {animalType} {gameObject.name} initialized at {homePosition}");
        }

        void ConfigureMovementForAnimalType()
        {
            switch (animalType)
            {
                case AnimalType.Rabbit:
                    walkSpeed = 2f;  // Hops are quick
                    fleeSpeed = 5f;  // Fast hopping when scared
                    agent.speed = walkSpeed;
                    break;

                case AnimalType.Squirrel:
                    walkSpeed = 2.5f; // Quick, erratic movement
                    fleeSpeed = 6f;   // Very fast when fleeing
                    agent.speed = walkSpeed;
                    break;

                case AnimalType.Bird:
                    walkSpeed = 3f;   // Flying speed
                    fleeSpeed = 7f;   // Fast flying when scared
                    agent.speed = walkSpeed;
                    agent.baseOffset = flyingHeight; // Fly above ground
                    break;
            }
        }

        void FindPlayer()
        {
            // Try multiple methods to find player
            GameObject playerGO = GameObject.FindGameObjectWithTag("MainCamera");
            if (playerGO == null)
                playerGO = GameObject.Find("Main Camera");
            if (playerGO == null)
            {
                // Try to find XR Origin
                GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
                if (xrOrigin != null)
                {
                    Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
                    if (cameraOffset != null)
                    {
                        Transform mainCam = cameraOffset.Find("Main Camera");
                        if (mainCam != null)
                            playerGO = mainCam.gameObject;
                    }
                }
            }

            if (playerGO != null)
            {
                player = playerGO.transform;
                if (showDebug)
                    Debug.Log($"[AnimalAI] Found player: {player.name}");
            }
            else
            {
                Debug.LogWarning("[AnimalAI] Could not find player! Animal will only wander.");
            }
        }

        void Update()
        {
            if (!isInitialized || agent == null) return;

            // Handle flash effect
            if (isFlashing)
            {
                UpdateFlashEffect();
            }

            // Update state based on player distance
            UpdateStateTransitions();

            // Execute current state behavior
            switch (currentState)
            {
                case AnimalState.Idle:
                    UpdateIdle();
                    break;
                case AnimalState.Wander:
                    UpdateWander();
                    UpdateAnimalSpecificMovement();
                    break;
                case AnimalState.Flee:
                    UpdateFlee();
                    UpdateAnimalSpecificMovement();
                    break;
            }
        }

        void UpdateAnimalSpecificMovement()
        {
            switch (animalType)
            {
                case AnimalType.Rabbit:
                    UpdateRabbitHopping();
                    break;

                case AnimalType.Squirrel:
                    UpdateSquirrelErraticMovement();
                    break;

                case AnimalType.Bird:
                    UpdateBirdFlying();
                    break;
            }
        }

        void UpdateRabbitHopping()
        {
            hopTimer += Time.deltaTime;

            // Hop when moving and enough time has passed
            if (agent.velocity.magnitude > 0.1f && hopTimer >= hopInterval)
            {
                // Apply hop force
                if (rb != null && !rb.isKinematic)
                {
                    rb.AddForce(Vector3.up * hopForce, ForceMode.Impulse);
                }
                else if (rb != null)
                {
                    // Kinematic mode - animate hop
                    StartCoroutine(HopAnimation());
                }

                hopTimer = 0f;
            }
        }

        System.Collections.IEnumerator HopAnimation()
        {
            Vector3 startPos = transform.position;
            float hopHeight = 0.3f;
            float hopDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < hopDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / hopDuration;

                // Parabolic hop arc
                float heightOffset = Mathf.Sin(t * Mathf.PI) * hopHeight;
                transform.position = startPos + Vector3.up * heightOffset;

                yield return null;
            }
        }

        void UpdateSquirrelErraticMovement()
        {
            erraticTimer += Time.deltaTime;

            if (erraticTimer >= erraticFrequency && agent.hasPath)
            {
                // Add random offset to current path
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0f,
                    Random.Range(-0.5f, 0.5f)
                );

                Vector3 newDestination = agent.destination + randomOffset;
                agent.SetDestination(newDestination);

                erraticTimer = 0f;
            }
        }

        void UpdateBirdFlying()
        {
            // Birds bob up and down while flying
            float bobAmount = Mathf.Sin(Time.time * 2f) * 0.15f;
            agent.baseOffset = flyingHeight + bobAmount;
        }

        void UpdateStateTransitions()
        {
            if (player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Flee if player too close
            if (distanceToPlayer < fleeDistance && currentState != AnimalState.Flee)
            {
                ChangeState(AnimalState.Flee);
            }
            // Return to wander if player far enough and currently fleeing
            else if (currentState == AnimalState.Flee && distanceToPlayer > detectionRadius)
            {
                ChangeState(AnimalState.Wander);
            }
        }

        void ChangeState(AnimalState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            stateTimer = 0f;

            switch (newState)
            {
                case AnimalState.Idle:
                    if (agent != null)
                    {
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                    }
                    if (showDebug)
                        Debug.Log($"[AnimalAI] {gameObject.name} -> Idle");
                    break;

                case AnimalState.Wander:
                    if (agent != null)
                    {
                        agent.speed = walkSpeed;
                        agent.isStopped = false;
                    }
                    if (showDebug)
                        Debug.Log($"[AnimalAI] {gameObject.name} -> Wander");
                    break;

                case AnimalState.Flee:
                    if (agent != null)
                    {
                        agent.speed = fleeSpeed;
                        agent.isStopped = false;
                    }
                    if (showDebug)
                        Debug.Log($"[AnimalAI] {gameObject.name} -> Flee");
                    break;
            }

            // Optional: Trigger animator parameters
            if (animator != null)
            {
                animator.SetInteger("State", (int)newState);
            }
        }

        void UpdateIdle()
        {
            stateTimer += Time.deltaTime;

            if (stateTimer > idleTime)
            {
                ChangeState(AnimalState.Wander);
            }
        }

        void UpdateWander()
        {
            // If no path or reached destination, pick new wander point
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint = GetRandomWanderPoint();

                if (randomPoint != Vector3.zero)
                {
                    agent.SetDestination(randomPoint);

                    if (showDebug)
                        Debug.Log($"[AnimalAI] {gameObject.name} wandering to {randomPoint}");
                }
                else
                {
                    // Couldn't find valid point
                    if (showDebug)
                        Debug.LogWarning($"[AnimalAI] {gameObject.name} couldn't find wander point, going idle");

                    // Go idle briefly, then try again
                    ChangeState(AnimalState.Idle);
                }
            }
        }

        void UpdateFlee()
        {
            if (player == null)
            {
                ChangeState(AnimalState.Wander);
                return;
            }

            // Run away from player
            Vector3 fleeDirection = transform.position - player.position;
            fleeDirection.y = 0;
            fleeDirection.Normalize();

            Vector3 fleeTarget = transform.position + fleeDirection * minFleeDistance;

            // Find valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleeTarget, out hit, minFleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        Vector3 GetRandomWanderPoint()
        {
            // Pick random point within wander radius from home
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += homePosition;
            randomDirection.y = homePosition.y; // Keep at ground level

            // Find valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return Vector3.zero; // Invalid point
        }

        /// <summary>
        /// Called when animal is hit by a spell (Zelda-style reaction)
        /// Flash white and bounce upward
        /// </summary>
        public void OnSpellHit(Vector3 hitDirection, float hitForce = 0f)
        {
            if (showDebug)
                Debug.Log($"[AnimalAI] {gameObject.name} hit by spell!");

            // Start flash effect
            StartFlashEffect();

            // Apply bounce force
            ApplyBounce(hitDirection, hitForce);

            // Flee from hit direction
            ChangeState(AnimalState.Flee);
        }

        void StartFlashEffect()
        {
            if (isFlashing) return; // Already flashing

            isFlashing = true;
            flashTimer = 0f;
            currentFlashCycle = 0;
        }

        void UpdateFlashEffect()
        {
            flashTimer += Time.deltaTime;

            // Calculate flash state (on/off)
            float cycleProgress = flashTimer / (flashDuration * flashCycles);
            bool flashOn = Mathf.FloorToInt(cycleProgress * flashCycles * 2) % 2 == 0;

            // Apply flash to all renderers
            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (flashOn)
                    {
                        // Flash color
                        materials[i].color = hitFlashColor;
                    }
                    else
                    {
                        // Original color
                        if (originalColors.ContainsKey(renderer) && i < originalColors[renderer].Length)
                        {
                            materials[i].color = originalColors[renderer][i];
                        }
                    }
                }
                renderer.materials = materials;
            }

            // End flash effect
            if (flashTimer >= flashDuration * flashCycles)
            {
                isFlashing = false;
                RestoreOriginalColors();
            }
        }

        void RestoreOriginalColors()
        {
            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                if (originalColors.ContainsKey(renderer))
                {
                    for (int i = 0; i < materials.Length && i < originalColors[renderer].Length; i++)
                    {
                        materials[i].color = originalColors[renderer][i];
                    }
                    renderer.materials = materials;
                }
            }
        }

        void ApplyBounce(Vector3 hitDirection, float hitForce)
        {
            if (rb == null) return;

            // Temporarily disable kinematic for bounce
            bool wasKinematic = rb.isKinematic;
            rb.isKinematic = false;
            rb.useGravity = true; // Ensure gravity is on during bounce

            // Disable NavMeshAgent to prevent interference with physics
            bool wasAgentEnabled = false;
            if (agent != null)
            {
                wasAgentEnabled = agent.enabled;
                agent.enabled = false;
            }

            // Apply small upward bounce force
            Vector3 bounceDir = (Vector3.up + hitDirection.normalized * 0.2f).normalized;
            float totalForce = bounceForce + hitForce;
            rb.AddForce(bounceDir * totalForce, ForceMode.Impulse);

            // Re-enable kinematic after animal lands
            StartCoroutine(ReEnableKinematic(wasKinematic, wasAgentEnabled));
        }

        System.Collections.IEnumerator ReEnableKinematic(bool wasKinematic, bool wasAgentEnabled)
        {
            // Wait until animal is back on ground
            yield return new WaitForSeconds(0.5f);

            // Wait until velocity is near zero (landed)
            if (rb != null)
            {
                while (rb.linearVelocity.magnitude > 0.1f)
                {
                    yield return new WaitForSeconds(0.05f);
                }
            }

            // Re-enable Rigidbody kinematic mode
            if (rb != null && wasKinematic)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero; // Stop any remaining movement
            }

            // Re-enable NavMeshAgent
            if (agent != null && wasAgentEnabled)
            {
                agent.enabled = true;
            }
        }

        /// <summary>
        /// Called by spell projectiles when they hit this animal
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            // Check if hit by spell projectile
            if (other.gameObject.layer == LayerMask.NameToLayer("Projectile") ||
                other.name.Contains("Projectile") ||
                other.name.Contains("Spell") ||
                other.name.Contains("Fireball") ||
                other.name.Contains("Ice") ||
                other.name.Contains("Lightning") ||
                other.name.Contains("Wind"))
            {
                Vector3 hitDirection = (transform.position - other.transform.position).normalized;
                OnSpellHit(hitDirection);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Home position
            Gizmos.color = Color.green;
            Vector3 home = Application.isPlaying ? homePosition : transform.position;
            Gizmos.DrawWireSphere(home, 0.5f);

            // Wander radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(home, wanderRadius);

            // Detection radius
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Flee distance
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, fleeDistance);

            // Current destination
            if (Application.isPlaying && agent != null && agent.hasPath)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, agent.destination);
                Gizmos.DrawWireSphere(agent.destination, 0.3f);
            }
        }
    }
}

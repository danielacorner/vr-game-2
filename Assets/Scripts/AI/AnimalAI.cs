using UnityEngine;
using UnityEngine.AI;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Animal behavior AI with flee and wander states
    /// Uses NavMesh for pathfinding on terrain
    /// Animals flee when player approaches, otherwise wander naturally
    /// </summary>
    public enum AnimalState
    {
        Idle,
        Wander,
        Flee
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class AnimalAI : MonoBehaviour
    {
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
        public float idleTime = 3f;

        [Tooltip("Minimum distance to flee from player")]
        public float minFleeDistance = 10f;

        [Header("Animation")]
        [Tooltip("Optional animator for animal animations")]
        public Animator animator;

        [Header("Debug")]
        [Tooltip("Show debug information in console")]
        public bool showDebug = false;

        private NavMeshAgent agent;
        private AnimalState currentState = AnimalState.Idle;
        private Transform player;
        private float stateTimer;
        private Vector3 homePosition;
        private bool isInitialized = false;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            homePosition = transform.position;
        }

        void Start()
        {
            // Find player (VR camera or XR Origin)
            FindPlayer();

            // Configure NavMeshAgent
            if (agent != null)
            {
                agent.speed = walkSpeed;
                agent.angularSpeed = 120f;
                agent.acceleration = 8f;
            }

            ChangeState(AnimalState.Idle);
            isInitialized = true;

            if (showDebug)
                Debug.Log($"[AnimalAI] {gameObject.name} initialized at {homePosition}");
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
                    break;
                case AnimalState.Flee:
                    UpdateFlee();
                    break;
            }
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
                }
                else
                {
                    // Couldn't find valid point, go idle
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

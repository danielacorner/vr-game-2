using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Controls animal animations based on movement state
    /// Handles procedural animations for walk/run cycles without requiring rigged models
    /// Works with AnimalAI to sync animation states
    /// </summary>
    public class AnimalAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Speed multiplier for animation playback")]
        [Range(0.5f, 3f)]
        public float animationSpeed = 1f;

        [Tooltip("Body part to bob up and down during movement")]
        public Transform bodyTransform;

        [Tooltip("Legs or appendages to animate")]
        public Transform[] legTransforms;

        [Header("Bob Animation")]
        [Tooltip("How much the body bobs up and down")]
        [Range(0f, 0.2f)]
        public float bobAmount = 0.05f;

        [Tooltip("Speed of bobbing motion")]
        [Range(1f, 10f)]
        public float bobSpeed = 5f;

        [Header("Leg Animation")]
        [Tooltip("How far legs swing")]
        [Range(0f, 45f)]
        public float legSwingAngle = 20f;

        [Tooltip("Speed of leg swing")]
        [Range(1f, 15f)]
        public float legSwingSpeed = 8f;

        [Header("State Speeds")]
        [Tooltip("Animation speed during idle (slow breathing)")]
        public float idleSpeedMultiplier = 0.3f;

        [Tooltip("Animation speed during walk")]
        public float walkSpeedMultiplier = 1f;

        [Tooltip("Animation speed during flee/run")]
        public float fleeSpeedMultiplier = 2f;

        private float animationTime;
        private Vector3 initialBodyPosition;
        private Quaternion[] initialLegRotations;
        private AnimalAI animalAI;
        private AnimalState currentState;

        void Awake()
        {
            animalAI = GetComponent<AnimalAI>();
            AutoFindBodyParts();

            // Store initial positions/rotations
            if (bodyTransform != null)
            {
                initialBodyPosition = bodyTransform.localPosition;
            }

            if (legTransforms != null && legTransforms.Length > 0)
            {
                initialLegRotations = new Quaternion[legTransforms.Length];
                for (int i = 0; i < legTransforms.Length; i++)
                {
                    if (legTransforms[i] != null)
                    {
                        initialLegRotations[i] = legTransforms[i].localRotation;
                    }
                }
            }
        }

        /// <summary>
        /// Automatically finds body and leg transforms if not assigned
        /// Looks for child objects with "Body" and "Leg" in their names
        /// </summary>
        private void AutoFindBodyParts()
        {
            // Auto-find body if not assigned
            if (bodyTransform == null)
            {
                Transform[] children = GetComponentsInChildren<Transform>();
                foreach (Transform child in children)
                {
                    if (child != transform && child.name.Contains("Body"))
                    {
                        bodyTransform = child;
                        Debug.Log($"[AnimalAnimationController] Auto-found body: {child.name}");
                        break;
                    }
                }
            }

            // Auto-find legs/wings if not assigned
            if (legTransforms == null || legTransforms.Length == 0)
            {
                System.Collections.Generic.List<Transform> appendages = new System.Collections.Generic.List<Transform>();
                Transform[] children = GetComponentsInChildren<Transform>();

                foreach (Transform child in children)
                {
                    if (child != transform && (child.name.Contains("Leg") || child.name.Contains("Wing")))
                    {
                        appendages.Add(child);
                    }
                }

                if (appendages.Count > 0)
                {
                    legTransforms = appendages.ToArray();
                    Debug.Log($"[AnimalAnimationController] Auto-found {appendages.Count} legs/wings");
                }
            }
        }

        void Update()
        {
            if (animalAI == null) return;

            // Get current state from AnimalAI
            currentState = GetCurrentState();

            // Determine animation speed based on state
            float speedMultiplier = GetSpeedMultiplier(currentState);

            // Update animation time
            animationTime += Time.deltaTime * animationSpeed * speedMultiplier;

            // Apply animations
            AnimateBody(speedMultiplier);
            AnimateLegs(speedMultiplier);
        }

        private AnimalState GetCurrentState()
        {
            // Access the current state from AnimalAI via reflection or make it public
            // For now, we'll use a simple approach based on velocity
            if (animalAI != null)
            {
                var agent = animalAI.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    if (agent.velocity.magnitude < 0.1f)
                        return AnimalState.Idle;
                    else if (agent.velocity.magnitude > 2.5f)
                        return AnimalState.Flee;
                    else
                        return AnimalState.Wander;
                }
            }
            return AnimalState.Idle;
        }

        private float GetSpeedMultiplier(AnimalState state)
        {
            switch (state)
            {
                case AnimalState.Idle:
                    return idleSpeedMultiplier;
                case AnimalState.Wander:
                    return walkSpeedMultiplier;
                case AnimalState.Flee:
                    return fleeSpeedMultiplier;
                default:
                    return 1f;
            }
        }

        private void AnimateBody(float speedMultiplier)
        {
            if (bodyTransform == null) return;

            // Bob up and down
            float bobOffset = Mathf.Sin(animationTime * bobSpeed) * bobAmount * speedMultiplier;
            bodyTransform.localPosition = initialBodyPosition + new Vector3(0f, bobOffset, 0f);
        }

        private void AnimateLegs(float speedMultiplier)
        {
            if (legTransforms == null || legTransforms.Length == 0) return;

            // Animate each leg with phase offset
            for (int i = 0; i < legTransforms.Length; i++)
            {
                if (legTransforms[i] == null) continue;

                // Phase offset for each leg (alternating pattern)
                float phaseOffset = (i % 2 == 0) ? 0f : Mathf.PI;

                // Calculate swing angle
                float swingAngle = Mathf.Sin(animationTime * legSwingSpeed + phaseOffset) * legSwingAngle * speedMultiplier;

                // Apply rotation (swinging forward/back on X axis)
                legTransforms[i].localRotation = initialLegRotations[i] * Quaternion.Euler(swingAngle, 0f, 0f);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualize body and leg transforms
            if (bodyTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(bodyTransform.position, 0.1f);
            }

            if (legTransforms != null)
            {
                Gizmos.color = Color.cyan;
                foreach (Transform leg in legTransforms)
                {
                    if (leg != null)
                    {
                        Gizmos.DrawWireSphere(leg.position, 0.05f);
                    }
                }
            }
        }
    }
}

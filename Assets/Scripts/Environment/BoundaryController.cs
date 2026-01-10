using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Enforces scene boundaries to prevent player and animals from falling off edges
    /// Creates an invisible boundary around the playable area
    /// Applies soft push-back force when entities approach edges
    /// </summary>
    public class BoundaryController : MonoBehaviour
    {
        [Header("Boundary Settings")]
        [Tooltip("Center point of the playable area")]
        public Vector3 boundaryCenter = Vector3.zero;

        [Tooltip("Radius of the playable area (50m terrain = 25m radius)")]
        public float boundaryRadius = 24f;

        [Tooltip("Distance from edge where push-back starts")]
        public float warningDistance = 2f;

        [Tooltip("Strength of push-back force")]
        public float pushBackStrength = 10f;

        [Tooltip("Apply to player")]
        public bool affectPlayer = true;

        [Tooltip("Apply to animals")]
        public bool affectAnimals = true;

        [Header("Visual Feedback")]
        [Tooltip("Show warning indicator when approaching edge")]
        public bool showWarning = true;

        [Tooltip("Color of boundary warning")]
        public Color warningColor = new Color(1f, 0.5f, 0f, 0.3f);

        [Header("Debug")]
        [Tooltip("Show boundary gizmos in scene view")]
        public bool showDebug = true;

        private Transform playerTransform;
        private CharacterController playerController;

        void Start()
        {
            // Find player
            FindPlayer();
        }

        void FindPlayer()
        {
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin != null)
            {
                playerTransform = xrOrigin.transform;
                playerController = xrOrigin.GetComponent<CharacterController>();

                if (showDebug)
                    Debug.Log($"[BoundaryController] Found player: {playerTransform.name}");
            }
            else
            {
                Debug.LogWarning("[BoundaryController] Could not find player (XR Origin)");
            }
        }

        void Update()
        {
            if (affectPlayer && playerTransform != null)
            {
                EnforceBoundary(playerTransform, playerController);
            }
        }

        /// <summary>
        /// Public method that animals can call to check boundaries
        /// </summary>
        public bool IsNearBoundary(Vector3 position, out Vector3 pushBackDirection)
        {
            pushBackDirection = Vector3.zero;

            float distanceFromCenter = Vector3.Distance(
                new Vector3(position.x, 0f, position.z),
                new Vector3(boundaryCenter.x, 0f, boundaryCenter.z)
            );

            float distanceFromEdge = boundaryRadius - distanceFromCenter;

            if (distanceFromEdge < warningDistance)
            {
                // Calculate push-back direction (toward center)
                Vector2 toCenter = new Vector2(
                    boundaryCenter.x - position.x,
                    boundaryCenter.z - position.z
                ).normalized;

                pushBackDirection = new Vector3(toCenter.x, 0f, toCenter.y);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Enforces boundary constraints on any transform
        /// </summary>
        public void EnforceBoundary(Transform target, CharacterController controller = null)
        {
            if (target == null) return;

            Vector3 position = target.position;
            Vector3 pushBack;

            if (IsNearBoundary(position, out pushBack))
            {
                // Calculate push-back strength based on proximity
                float distanceFromCenter = Vector3.Distance(
                    new Vector3(position.x, 0f, position.z),
                    new Vector3(boundaryCenter.x, 0f, boundaryCenter.z)
                );
                float distanceFromEdge = boundaryRadius - distanceFromCenter;
                float pushBackFactor = 1f - (distanceFromEdge / warningDistance);

                // Apply push-back
                Vector3 correction = pushBack * pushBackStrength * pushBackFactor * Time.deltaTime;

                if (controller != null)
                {
                    // Use CharacterController move for smooth physics
                    controller.Move(correction);
                }
                else
                {
                    // Direct transform movement
                    target.position += correction;
                }

                // Hard clamp if somehow went past boundary
                Vector3 clampedPos = ClampToBoundary(position);
                if (clampedPos != position)
                {
                    if (controller != null)
                    {
                        controller.Move(clampedPos - position);
                    }
                    else
                    {
                        target.position = clampedPos;
                    }
                }
            }
        }

        /// <summary>
        /// Hard clamps a position to within the boundary
        /// </summary>
        public Vector3 ClampToBoundary(Vector3 position)
        {
            Vector2 flatPos = new Vector2(position.x, position.z);
            Vector2 flatCenter = new Vector2(boundaryCenter.x, boundaryCenter.z);

            float distance = Vector2.Distance(flatPos, flatCenter);

            if (distance > boundaryRadius)
            {
                // Clamp to boundary edge
                Vector2 direction = (flatPos - flatCenter).normalized;
                Vector2 clampedFlat = flatCenter + direction * boundaryRadius;

                return new Vector3(clampedFlat.x, position.y, clampedFlat.y);
            }

            return position;
        }

        /// <summary>
        /// Checks if a destination is valid (within boundaries)
        /// Used by animal AI to validate wander points
        /// </summary>
        public bool IsWithinBoundary(Vector3 position)
        {
            float distanceFromCenter = Vector3.Distance(
                new Vector3(position.x, 0f, position.z),
                new Vector3(boundaryCenter.x, 0f, boundaryCenter.z)
            );

            return distanceFromCenter <= boundaryRadius;
        }

        void OnDrawGizmos()
        {
            if (!showDebug) return;

            // Draw boundary circle
            Gizmos.color = Color.green;
            DrawGizmoCircle(boundaryCenter, boundaryRadius, 64);

            // Draw warning zone
            Gizmos.color = warningColor;
            DrawGizmoCircle(boundaryCenter, boundaryRadius - warningDistance, 64);

            // Draw center point
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(boundaryCenter, 1f);
        }

        void DrawGizmoCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 point = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }
    }
}

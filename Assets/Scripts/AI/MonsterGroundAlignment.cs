using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Ensures monsters stay properly aligned with the ground
    /// Continuously adjusts Y position so the collider bottom touches the floor
    /// </summary>
    public class MonsterGroundAlignment : MonoBehaviour
    {
        [Header("Grounding Settings")]
        [Tooltip("Distance to raycast down when checking for ground")]
        public float raycastDistance = 5f;

        [Tooltip("Layer mask for ground detection")]
        public LayerMask groundLayers = -1; // All layers by default

        [Tooltip("How smoothly to adjust to ground level (0=instant, higher=smoother)")]
        [Range(0f, 0.5f)]
        public float adjustmentSmoothing = 0f; // Instant by default

        [Header("Visual Mesh Info")]
        [Tooltip("Height offset from monster origin to VISUAL mesh bottom (accounts for 1.5x scaling)")]
        public float visualBottomOffset = 0f;

        [Tooltip("Visual mesh scale factor applied to monsters")]
        public float visualScaleFactor = 1.5f;

        [Header("Debug")]
        [Tooltip("Show debug information")]
        public bool showDebug = false;

        private BoxCollider physicsCollider;
        private Rigidbody rb;
        private float targetYPosition;
        private bool isGrounded = false;

        void Start()
        {
            Debug.Log($"[MonsterGroundAlignment] START on {gameObject.name} at {transform.position}");

            // Find the physics collider (non-trigger)
            BoxCollider[] colliders = GetComponents<BoxCollider>();
            foreach (BoxCollider col in colliders)
            {
                if (!col.isTrigger)
                {
                    physicsCollider = col;
                    break;
                }
            }

            if (physicsCollider == null)
            {
                Debug.LogError($"[MonsterGroundAlignment] No physics collider found on {gameObject.name}!");
                enabled = false;
                return;
            }

            rb = GetComponent<Rigidbody>();

            // Calculate VISUAL mesh bottom offset (accounts for 1.5x scaling)
            // Visual mesh is scaled 1.5x, so it extends further down than the collider
            // Bottom of scaled visual = center.y - (size.y * scaleFactor / 2)
            visualBottomOffset = physicsCollider.center.y - (physicsCollider.size.y * visualScaleFactor / 2f);

            Debug.Log($"[MonsterGroundAlignment] {gameObject.name}: Physics collider center={physicsCollider.center}, size={physicsCollider.size}, visualBottomOffset={visualBottomOffset:F3}");

            // Initial ground alignment
            AlignToGround(instant: true);

            Debug.Log($"[MonsterGroundAlignment] {gameObject.name} aligned to ground at {transform.position}");
        }

        void FixedUpdate()
        {
            // Continuously align to ground
            AlignToGround(instant: false);
        }

        void AlignToGround(bool instant)
        {
            // Raycast down from above the monster
            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up * 2f; // Start well above monster

            if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance, groundLayers))
            {
                isGrounded = true;

                // Calculate where monster origin should be so VISUAL bottom touches ground
                // If scaled visual bottom is at local Y = -0.75, and ground is at Y = 0,
                // then monster origin should be at Y = 0 - (-0.75) = 0.75
                targetYPosition = hit.point.y - visualBottomOffset;

                if (instant)
                {
                    Debug.Log($"[MonsterGroundAlignment] {gameObject.name}: Hit {hit.collider.gameObject.name} at ground Y={hit.point.y:F3}, moving from Y={transform.position.y:F3} to Y={targetYPosition:F3}");
                }

                // Adjust position
                if (instant || adjustmentSmoothing <= 0.001f)
                {
                    // Instant adjustment
                    Vector3 pos = transform.position;
                    pos.y = targetYPosition;
                    transform.position = pos;

                    // Reset velocity to prevent bouncing
                    if (rb != null)
                    {
                        Vector3 vel = rb.linearVelocity;
                        vel.y = 0f;
                        rb.linearVelocity = vel;
                    }
                }
                else
                {
                    // Smooth adjustment
                    Vector3 pos = transform.position;
                    float currentY = pos.y;

                    // Only adjust if difference is significant
                    if (Mathf.Abs(currentY - targetYPosition) > 0.01f)
                    {
                        pos.y = Mathf.Lerp(currentY, targetYPosition, adjustmentSmoothing);
                        transform.position = pos;

                        // Dampen vertical velocity
                        if (rb != null)
                        {
                            Vector3 vel = rb.linearVelocity;
                            vel.y *= 0.5f;
                            rb.linearVelocity = vel;
                        }
                    }
                }
            }
            else
            {
                isGrounded = false;
                Debug.LogWarning($"[MonsterGroundAlignment] {gameObject.name}: NO GROUND FOUND! Raycast from {rayStart} down {raycastDistance} units. Current pos: {transform.position}");
            }
        }

        void OnDrawGizmosSelected()
        {
            if (physicsCollider == null)
            {
                BoxCollider[] colliders = GetComponents<BoxCollider>();
                foreach (BoxCollider col in colliders)
                {
                    if (!col.isTrigger)
                    {
                        physicsCollider = col;
                        break;
                    }
                }
            }

            if (physicsCollider != null)
            {
                // Draw collider bottom plane
                Vector3 bottomCenter = transform.position + transform.rotation * new Vector3(
                    physicsCollider.center.x,
                    physicsCollider.center.y - physicsCollider.size.y / 2f,
                    physicsCollider.center.z
                );

                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(bottomCenter, new Vector3(physicsCollider.size.x, 0.01f, physicsCollider.size.z));

                // Draw raycast line
                Gizmos.color = Color.yellow;
                Vector3 rayStart = transform.position + Vector3.up * 0.5f;
                Gizmos.DrawLine(rayStart, rayStart + Vector3.down * raycastDistance);
            }
        }
    }
}

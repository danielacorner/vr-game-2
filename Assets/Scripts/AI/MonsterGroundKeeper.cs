using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Ensures monsters stay properly positioned on terrain/ground
    /// Prevents sinking through terrain by continuously checking ground level
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MonsterGroundKeeper : MonoBehaviour
    {
        [Tooltip("How often to check ground level (seconds)")]
        public float checkInterval = 0.1f;

        [Tooltip("Maximum distance to check for ground below")]
        public float raycastDistance = 5f;

        [Tooltip("Offset above ground to maintain")]
        public float groundOffset = 0.05f;

        [Tooltip("How fast to correct position if sinking")]
        public float correctionSpeed = 5f;

        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        [Tooltip("Wait this long after spawn before checking (let physics settle)")]
        public float initialDelay = 1f;

        private Rigidbody rb;
        private float nextCheckTime;
        private float lowestPointOffset; // How far below transform.position the lowest mesh vertex is
        private float spawnTime;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spawnTime = Time.time;

            // Calculate the lowest point of the monster's mesh
            CalculateLowestPoint();
        }

        void CalculateLowestPoint()
        {
            float lowestLocalY = float.MaxValue;
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter mf in meshFilters)
            {
                if (mf.mesh == null) continue;

                Vector3[] vertices = mf.mesh.vertices;
                Transform meshTransform = mf.transform;

                foreach (Vector3 localVert in vertices)
                {
                    // Convert to monster's local space
                    Vector3 monsterLocalVert = transform.InverseTransformPoint(meshTransform.TransformPoint(localVert));
                    if (monsterLocalVert.y < lowestLocalY)
                    {
                        lowestLocalY = monsterLocalVert.y;
                    }
                }
            }

            // Store the offset (will be negative, e.g. -1.2 means lowest point is 1.2m below pivot)
            lowestPointOffset = lowestLocalY;

            if (showDebug)
                Debug.Log($"[GroundKeeper] {gameObject.name} lowest vertex offset from pivot: {lowestPointOffset:F2}m");
        }

        void FixedUpdate()
        {
            // Wait for initial delay after spawn (let physics settle)
            if (Time.time < spawnTime + initialDelay)
                return;

            // Check ground periodically
            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;
                CheckAndCorrectGroundPosition();
            }
        }

        void CheckAndCorrectGroundPosition()
        {
            // Raycast down from center to find ground
            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;

            // Temporarily disable own colliders to avoid self-hit
            Collider[] ownColliders = GetComponents<Collider>();
            foreach (Collider col in ownColliders)
                col.enabled = false;

            bool hitGround = Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance);

            // Re-enable colliders
            foreach (Collider col in ownColliders)
                col.enabled = true;

            if (hitGround)
            {
                // Calculate where monster's pivot should be so lowest vertex is at ground + offset
                // lowestPointOffset is negative (e.g. -1.2m below pivot)
                // So: pivot.y + lowestPointOffset = ground + groundOffset
                // Therefore: desiredPivotY = ground + groundOffset - lowestPointOffset
                float groundY = hit.point.y;
                float desiredPivotY = groundY + groundOffset - lowestPointOffset;
                float currentPivotY = transform.position.y;
                float difference = desiredPivotY - currentPivotY;

                // If sinking below ground significantly, correct it
                if (difference > 0.1f) // Pivot needs to move up (monster sinking)
                {
                    if (showDebug)
                        Debug.LogWarning($"[GroundKeeper] {gameObject.name} sinking! Ground={groundY:F2}, current pivot Y={currentPivotY:F2}, should be {desiredPivotY:F2}, correcting +{difference:F2}m...");

                    // Gradually move up to correct position
                    Vector3 correctedPos = transform.position;
                    correctedPos.y += difference * correctionSpeed * Time.fixedDeltaTime;
                    transform.position = correctedPos;

                    // Also zero out downward velocity to prevent further sinking
                    if (rb.linearVelocity.y < 0f)
                    {
                        Vector3 vel = rb.linearVelocity;
                        vel.y = 0f;
                        rb.linearVelocity = vel;
                    }
                }
                else if (difference < -0.3f) // More than 30cm above ground (floating)
                {
                    // Let gravity naturally bring it down, but log it
                    if (showDebug)
                        Debug.Log($"[GroundKeeper] {gameObject.name} floating {-difference:F2}m above ground, letting gravity handle it");
                }
            }
            else
            {
                if (showDebug)
                    Debug.LogWarning($"[GroundKeeper] {gameObject.name} raycast missed ground from {rayStart}");
            }
        }

        void OnDrawGizmosSelected()
        {
            // Draw raycast line
            Gizmos.color = Color.yellow;
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * raycastDistance);

            // Draw lowest point indicator (lowestPointOffset is negative, so we add it to move down)
            Gizmos.color = Color.green;
            Vector3 lowestPoint = transform.position + new Vector3(0f, lowestPointOffset, 0f);
            Gizmos.DrawWireSphere(lowestPoint, 0.1f);

            // Draw line from pivot to lowest point
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, lowestPoint);
        }
    }
}

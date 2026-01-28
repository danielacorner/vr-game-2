using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Creates invisible walls around the play area to prevent falling off
    /// </summary>
    public class PlayAreaBoundary : MonoBehaviour
    {
        [Header("Boundary Settings")]
        [Tooltip("Radius of the play area")]
        public float boundaryRadius = 25f;

        [Tooltip("Height of invisible walls")]
        public float wallHeight = 5f;

        [Tooltip("Number of wall segments (more = smoother circle)")]
        public int wallSegments = 32;

        [Tooltip("Show visual debug walls")]
        public bool showVisualWalls = false;

        [Header("Player Containment")]
        [Tooltip("Gently push player back if they reach boundary")]
        public bool softPushBack = true;

        [Tooltip("Push back force multiplier")]
        public float pushBackForce = 2f;

        private Transform player;

        void Start()
        {
            CreateInvisibleWalls();
            FindPlayer();
        }

        [ContextMenu("Create Boundary Walls")]
        void CreateInvisibleWalls()
        {
            Debug.Log("[PlayAreaBoundary] Creating invisible boundary walls...");

            // Create parent container
            GameObject boundaryContainer = new GameObject("InvisibleBoundaryWalls");
            boundaryContainer.transform.SetParent(transform);
            boundaryContainer.transform.localPosition = Vector3.zero;

            float angleStep = 360f / wallSegments;

            for (int i = 0; i < wallSegments; i++)
            {
                float angle = i * angleStep;
                float nextAngle = (i + 1) * angleStep;

                Vector3 point1 = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * boundaryRadius,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * boundaryRadius
                );

                Vector3 point2 = new Vector3(
                    Mathf.Cos(nextAngle * Mathf.Deg2Rad) * boundaryRadius,
                    0,
                    Mathf.Sin(nextAngle * Mathf.Deg2Rad) * boundaryRadius
                );

                Vector3 center = (point1 + point2) / 2f;
                float width = Vector3.Distance(point1, point2);

                // Create wall segment
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"BoundaryWall_{i}";
                wall.transform.SetParent(boundaryContainer.transform);
                wall.transform.position = center + Vector3.up * (wallHeight / 2f);
                wall.transform.localScale = new Vector3(width, wallHeight, 0.1f);

                // Face inward
                wall.transform.LookAt(new Vector3(0, wall.transform.position.y, 0));

                // Make invisible or semi-transparent
                Renderer renderer = wall.GetComponent<Renderer>();
                if (showVisualWalls)
                {
                    // Semi-transparent blue for debugging
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0.2f, 0.5f, 1f, 0.3f);
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetFloat("_Blend", 0); // Alpha blend
                    renderer.material = mat;
                }
                else
                {
                    // Completely invisible
                    renderer.enabled = false;
                }

                // Ensure collider is present
                BoxCollider collider = wall.GetComponent<BoxCollider>();
                collider.isTrigger = false; // Solid wall

                // Add physics material for no bounce
                PhysicsMaterial physicsMat = new PhysicsMaterial("BoundaryWall");
                physicsMat.bounciness = 0f;
                physicsMat.frictionCombine = PhysicsMaterialCombine.Maximum;
                physicsMat.staticFriction = 1f;
                physicsMat.dynamicFriction = 1f;
                collider.material = physicsMat;
            }

            Debug.Log($"[PlayAreaBoundary] Created {wallSegments} invisible wall segments");
        }

        void FindPlayer()
        {
            // Find the XR Origin (player)
            Unity.XR.CoreUtils.XROrigin xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                player = xrOrigin.transform;
                Debug.Log($"[PlayAreaBoundary] Found player: {player.name}");
            }
        }

        void Update()
        {
            if (!softPushBack || player == null) return;

            // Check if player is near boundary
            Vector3 playerPos = player.position;
            float distanceFromCenter = new Vector3(playerPos.x, 0, playerPos.z).magnitude;

            if (distanceFromCenter > boundaryRadius - 1f) // 1m warning zone
            {
                // Gently push player toward center
                Vector3 toCenter = -new Vector3(playerPos.x, 0, playerPos.z).normalized;
                float pushStrength = Mathf.Clamp01((distanceFromCenter - (boundaryRadius - 1f)) / 1f);

                Vector3 pushVelocity = toCenter * pushBackForce * pushStrength * Time.deltaTime;
                player.position += pushVelocity;
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                // Draw boundary circle in editor
                Gizmos.color = Color.cyan;
                int segments = 64;
                float angleStep = 360f / segments;

                for (int i = 0; i < segments; i++)
                {
                    float angle = i * angleStep;
                    float nextAngle = (i + 1) * angleStep;

                    Vector3 point1 = transform.position + new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * boundaryRadius,
                        1f,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * boundaryRadius
                    );

                    Vector3 point2 = transform.position + new Vector3(
                        Mathf.Cos(nextAngle * Mathf.Deg2Rad) * boundaryRadius,
                        1f,
                        Mathf.Sin(nextAngle * Mathf.Deg2Rad) * boundaryRadius
                    );

                    Gizmos.DrawLine(point1, point2);
                }
            }
        }

        [ContextMenu("Clear Boundary Walls")]
        void ClearWalls()
        {
            Transform container = transform.Find("InvisibleBoundaryWalls");
            if (container != null)
            {
                DestroyImmediate(container.gameObject);
                Debug.Log("[PlayAreaBoundary] Boundary walls cleared");
            }
        }
    }
}

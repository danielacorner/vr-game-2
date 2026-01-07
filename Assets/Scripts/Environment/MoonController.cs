using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Controls moon movement in figure-8 infinity pattern (lemniscate curve)
    /// Provides directional lighting for outdoor home area
    /// Moon moves very slowly and never sets, creating atmospheric moonlit environment
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class MoonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Horizontal radius of the figure-8 pattern")]
        public float orbitRadius = 30f;

        [Tooltip("Vertical radius of the figure-8 pattern")]
        public float orbitRadiusVertical = 15f;

        [Tooltip("Height of the moon above the terrain center")]
        public float orbitHeight = 25f;

        [Tooltip("Time in seconds for one complete figure-8 cycle (600 = 10 minutes)")]
        public float cycleTime = 600f;

        [Header("Lighting")]
        [Tooltip("Reference to the directional light component")]
        public Light moonLight;

        [Tooltip("Intensity of the moonlight")]
        [Range(0f, 1f)]
        public float lightIntensity = 0.4f;

        [Tooltip("Color of the moonlight (cool blue-white)")]
        public Color moonColor = new Color(0.7f, 0.8f, 1f);

        [Header("Look At Target")]
        [Tooltip("Point the moon should look at (usually terrain center)")]
        public Vector3 lookAtTarget = Vector3.zero;

        private float time;

        void Start()
        {
            // Get or add light component
            if (moonLight == null)
            {
                moonLight = GetComponent<Light>();
                if (moonLight == null)
                {
                    moonLight = gameObject.AddComponent<Light>();
                }
            }

            // Configure directional light
            moonLight.type = LightType.Directional;
            moonLight.color = moonColor;
            moonLight.intensity = lightIntensity;
            moonLight.shadows = LightShadows.Soft;

            Debug.Log("[MoonController] Moon initialized with figure-8 pattern");
        }

        void Update()
        {
            UpdateMoonPosition();
            UpdateMoonRotation();
        }

        private void UpdateMoonPosition()
        {
            // Increment time (normalized to 0-2π for full cycle)
            time += Time.deltaTime / cycleTime * 2f * Mathf.PI;

            // Lemniscate curve (figure-8):
            // x(t) = a * sin(t)
            // y(t) = constant height
            // z(t) = a * sin(t) * cos(t) = (a/2) * sin(2t)

            float x = orbitRadius * Mathf.Sin(time);
            float y = orbitHeight + orbitRadiusVertical * Mathf.Sin(time * 2f); // Add vertical variation
            float z = orbitRadius * Mathf.Sin(time) * Mathf.Cos(time);

            transform.position = new Vector3(x, y, z);
        }

        private void UpdateMoonRotation()
        {
            // Always point moon at terrain center
            Vector3 direction = lookAtTarget - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Draw the figure-8 orbit path
            Gizmos.color = Color.yellow;
            int segments = 100;
            Vector3 previousPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments * 2f * Mathf.PI;
                float x = orbitRadius * Mathf.Sin(t);
                float y = orbitHeight + orbitRadiusVertical * Mathf.Sin(t * 2f);
                float z = orbitRadius * Mathf.Sin(t) * Mathf.Cos(t);

                Vector3 point = new Vector3(x, y, z);

                if (i > 0)
                {
                    Gizmos.DrawLine(previousPoint, point);
                }

                previousPoint = point;
            }

            // Draw look-at target
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lookAtTarget, 1f);
            Gizmos.DrawLine(transform.position, lookAtTarget);
        }
    }
}

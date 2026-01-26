using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Floating damage number indicator that pops up and floats away
    /// Uses TextMesh for reliable 3D rendering in VR
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("Duration before fading out")]
        public float lifetime = 1.5f;

        [Tooltip("Upward float speed")]
        public float floatSpeed = 1.5f;

        [Tooltip("Sideways drift amount")]
        public float sidewaysDrift = 0.5f;

        [Tooltip("Scale animation curve")]
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1.2f, 1f, 1f);

        [Header("Visual")]
        [Tooltip("Text color")]
        public Color textColor = new Color(1f, 0f, 0f, 1f); // Bright red

        [Tooltip("Character size in world units (0.2m = 20cm)")]
        public float characterSize = 0.1f; // 10cm per character

        [Header("Debug")]
        public bool showDebug = true;

        private TextMesh textMesh;
        private MeshRenderer textRenderer;
        private float spawnTime;
        private Vector3 floatDirection;
        private Vector3 startScale;
        private Color startColor;

        /// <summary>
        /// Initialize damage number with damage amount and spawn position
        /// </summary>
        public void Initialize(int damage, Vector3 worldPosition)
        {
            spawnTime = Time.time;
            transform.position = worldPosition;

            if (showDebug)
                Debug.Log($"[DamageNumber] Creating damage number '{damage}' at {worldPosition}");

            // Random sideways direction
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            floatDirection = new Vector3(
                Mathf.Cos(angle) * sidewaysDrift,
                floatSpeed,
                Mathf.Sin(angle) * sidewaysDrift
            );

            // Create TextMesh
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.text = damage.ToString();
            textMesh.characterSize = characterSize; // 0.1m = 10cm per character
            textMesh.fontSize = 64; // High resolution for clarity
            textMesh.color = textColor;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = FontStyle.Bold;

            // Get MeshRenderer
            textRenderer = GetComponent<MeshRenderer>();
            if (textRenderer != null)
            {
                // Use Unlit shader for consistent brightness
                textRenderer.material.shader = Shader.Find("Unlit/Color");
                textRenderer.material.color = textColor;

                // Enable sorting to render on top
                textRenderer.sortingOrder = 100;
            }

            startColor = textColor;
            startScale = transform.localScale;

            if (showDebug)
                Debug.Log($"[DamageNumber] TextMesh created: text='{textMesh.text}', charSize={characterSize}, position={transform.position}");
        }

        void Update()
        {
            if (textMesh == null) return;

            float elapsed = Time.time - spawnTime;
            float progress = elapsed / lifetime;

            // Float upward and sideways
            transform.position += floatDirection * Time.deltaTime;

            // Make text always face camera (billboard effect)
            if (Camera.main != null)
            {
                transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                                Camera.main.transform.rotation * Vector3.up);
            }

            // Scale animation
            if (progress < 0.5f)
            {
                float scaleProgress = progress * 2f;
                float scale = scaleCurve.Evaluate(scaleProgress);
                transform.localScale = startScale * scale;
            }

            // Fade out in last 30% of lifetime
            if (progress > 0.7f)
            {
                float fadeProgress = (progress - 0.7f) / 0.3f;
                Color fadeColor = startColor;
                fadeColor.a = 1f - fadeProgress;
                textMesh.color = fadeColor;

                if (textRenderer != null)
                {
                    textRenderer.material.color = fadeColor;
                }
            }

            // Destroy after lifetime
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Static helper to spawn a damage number at a position
        /// Creates a fixed 0.2m wide damage number
        /// </summary>
        public static void Create(int damage, Vector3 worldPosition, Color? color = null)
        {
            GameObject damageNumberObj = new GameObject("DamageNumber");
            DamageNumber damageNumber = damageNumberObj.AddComponent<DamageNumber>();

            if (color.HasValue)
            {
                damageNumber.textColor = color.Value;
            }

            damageNumber.Initialize(damage, worldPosition);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Floating damage number indicator that pops up and floats away
    /// Uses Unity UI Text for better Quest 3 performance than TextMeshPro
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
        public Color textColor = Color.white;

        [Tooltip("Font size")]
        public int fontSize = 48;

        private Canvas canvas;
        private Text text;
        private CanvasGroup canvasGroup;
        private float spawnTime;
        private Vector3 floatDirection;
        private Vector3 startScale;

        /// <summary>
        /// Initialize damage number with damage amount and spawn position
        /// </summary>
        public void Initialize(int damage, Vector3 worldPosition)
        {
            spawnTime = Time.time;
            transform.position = worldPosition;

            // Random sideways direction
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            floatDirection = new Vector3(
                Mathf.Cos(angle) * sidewaysDrift,
                floatSpeed,
                Mathf.Sin(angle) * sidewaysDrift
            );

            // Create canvas
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            // Set canvas size and scale for VR
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 100);
            canvasRect.localScale = Vector3.one * 0.01f; // Small for world space

            // Add canvas group for fading
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Create text GameObject
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform);
            text = textObj.AddComponent<Text>();

            // Configure text
            text.text = damage.ToString();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;

            // Add outline for better visibility
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            // Set text rect
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Store start scale
            startScale = transform.localScale;
        }

        void Update()
        {
            if (canvas == null) return;

            float elapsed = Time.time - spawnTime;
            float progress = elapsed / lifetime;

            // Float upward and sideways
            transform.position += floatDirection * Time.deltaTime;

            // Make canvas always face camera
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
                canvasGroup.alpha = 1f - fadeProgress;
            }

            // Destroy after lifetime
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Static helper to spawn a damage number at a position
        /// </summary>
        public static void Create(int damage, Vector3 worldPosition, Color? color = null)
        {
            GameObject damageNumberObj = new GameObject("DamageNumber");
            DamageNumber damageNumber = damageNumberObj.AddComponent<DamageNumber>();

            if (color.HasValue)
                damageNumber.textColor = color.Value;

            damageNumber.Initialize(damage, worldPosition);
        }
    }
}

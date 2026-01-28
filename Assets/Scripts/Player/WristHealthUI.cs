using UnityEngine;
using UnityEngine.UI;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Displays health hearts on player's wrists like a watch
    /// </summary>
    public class WristHealthUI : MonoBehaviour
    {
        [Header("Wrist References")]
        [Tooltip("Left hand transform (attach UI here)")]
        public Transform leftHand;

        [Tooltip("Right hand transform (attach UI here)")]
        public Transform rightHand;

        [Header("UI Settings")]
        [Tooltip("Distance behind hand to place UI")]
        public float offsetBehindHand = 0.08f;

        [Tooltip("Size of heart icons")]
        public float heartSize = 0.02f;

        [Tooltip("Spacing between hearts")]
        public float heartSpacing = 0.025f;

        [Tooltip("Show on both wrists or just left")]
        public bool showOnBothWrists = true;

        [Header("Colors")]
        public Color fullHeartColor = new Color(1f, 0.2f, 0.2f); // Red
        public Color emptyHeartColor = new Color(0.3f, 0.3f, 0.3f); // Gray

        [Header("Debug")]
        public bool showDebug = false;

        private GameObject leftWristUI;
        private GameObject rightWristUI;
        private Image[] leftHearts;
        private Image[] rightHearts;

        void Start()
        {
            // Auto-find hands if not assigned
            if (leftHand == null || rightHand == null)
            {
                FindHands();
            }

            // Create wrist UIs
            if (leftHand != null)
            {
                leftWristUI = CreateWristUI(leftHand, "LeftWristHealthUI");
                leftHearts = leftWristUI.GetComponentsInChildren<Image>();
            }

            if (showOnBothWrists && rightHand != null)
            {
                rightWristUI = CreateWristUI(rightHand, "RightWristHealthUI");
                rightHearts = rightWristUI.GetComponentsInChildren<Image>();
            }

            // Subscribe to health changes
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.onHealthChanged.AddListener(UpdateHearts);
                // Initial update
                UpdateHearts(PlayerHealth.Instance.currentHealth, PlayerHealth.Instance.maxHealth);
            }
            else
            {
                Debug.LogError("[WristHealthUI] PlayerHealth instance not found!");
            }
        }

        void FindHands()
        {
            // Look for hand controller transforms
            GameObject xrOrigin = GameObject.Find("XR Origin");
            if (xrOrigin != null)
            {
                Transform[] allTransforms = xrOrigin.GetComponentsInChildren<Transform>();
                foreach (Transform t in allTransforms)
                {
                    string name = t.name.ToLower();
                    if (name.Contains("left") && (name.Contains("hand") || name.Contains("controller")))
                    {
                        leftHand = t;
                        if (showDebug)
                            Debug.Log($"[WristHealthUI] Found left hand: {t.name}");
                    }
                    else if (name.Contains("right") && (name.Contains("hand") || name.Contains("controller")))
                    {
                        rightHand = t;
                        if (showDebug)
                            Debug.Log($"[WristHealthUI] Found right hand: {t.name}");
                    }
                }
            }
        }

        GameObject CreateWristUI(Transform handTransform, string name)
        {
            // Create canvas for wrist
            GameObject uiRoot = new GameObject(name);
            uiRoot.transform.SetParent(handTransform);

            // Position behind hand (like a watch)
            uiRoot.transform.localPosition = new Vector3(0f, 0f, -offsetBehindHand);
            uiRoot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Face towards player

            // Create canvas
            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Scale canvas to be small (wrist-sized)
            RectTransform canvasRect = uiRoot.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 50);
            canvasRect.localScale = Vector3.one * 0.0005f; // Very small for wrist scale

            // Add canvas scaler for consistency
            CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            // Create horizontal layout for hearts
            GameObject heartsContainer = new GameObject("HeartsContainer");
            heartsContainer.transform.SetParent(uiRoot.transform);

            RectTransform containerRect = heartsContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup layout = heartsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = heartSpacing * 1000f; // Scale up for UI space
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Create heart icons (max health count - use ceiling to handle fractional hearts)
            int maxHealth = PlayerHealth.Instance != null ? Mathf.CeilToInt(PlayerHealth.Instance.maxHealth) : 5;
            for (int i = 0; i < maxHealth; i++)
            {
                CreateHeartIcon(heartsContainer.transform, i);
            }

            if (showDebug)
                Debug.Log($"[WristHealthUI] Created {name} with {maxHealth} hearts");

            return uiRoot;
        }

        void CreateHeartIcon(Transform parent, int index)
        {
            GameObject heartObj = new GameObject($"Heart_{index}");
            heartObj.transform.SetParent(parent);

            Image heartImage = heartObj.AddComponent<Image>();

            // Create simple heart shape using Unity's built-in sprites
            // For now, use a circle - can be replaced with actual heart sprite
            heartImage.sprite = CreateCircleSprite();
            heartImage.color = fullHeartColor;

            RectTransform rect = heartObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(heartSize * 1000f, heartSize * 1000f);
        }

        Sprite CreateCircleSprite()
        {
            // Create a simple circle texture for hearts
            int resolution = 32;
            Texture2D tex = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];

            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution / 2f - 1f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * resolution + x] = dist <= radius ? Color.white : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
        }

        void UpdateHearts(float currentHealth, float maxHealth)
        {
            if (showDebug)
                Debug.Log($"[WristHealthUI] Updating hearts: {currentHealth:F1}/{maxHealth:F1}");

            UpdateWristHearts(leftHearts, Mathf.CeilToInt(currentHealth));
            if (showOnBothWrists)
            {
                UpdateWristHearts(rightHearts, Mathf.CeilToInt(currentHealth));
            }
        }

        void UpdateWristHearts(Image[] hearts, int currentHealth)
        {
            if (hearts == null) return;

            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] != null)
                {
                    // Full hearts for current health, empty for lost health
                    hearts[i].color = i < currentHealth ? fullHeartColor : emptyHeartColor;
                }
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.onHealthChanged.RemoveListener(UpdateHearts);
            }
        }
    }
}

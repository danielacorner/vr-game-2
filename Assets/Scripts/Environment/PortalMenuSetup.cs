using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Runtime setup for portal menu UI
    /// Creates the button and configures the canvas
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class PortalMenuSetup : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Auto-setup UI on start")]
        public bool autoSetup = true;

        private Canvas canvas;
        private Button travelButton;

        void Awake()
        {
            // Create canvas GameObject if it doesn't exist
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("PortalMenuCanvas");
                // Parent to portal so it moves with the portal
                canvasObj.transform.SetParent(transform, false);

                // Position will be updated dynamically in PortalMenu to face player
                // Start at portal center, slightly forward
                canvasObj.transform.localPosition = Vector3.zero;
                canvasObj.transform.localRotation = Quaternion.identity;

                canvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                // Add BoxCollider for spell system to detect UI
                BoxCollider uiCollider = canvasObj.AddComponent<BoxCollider>();
                uiCollider.isTrigger = true;
                uiCollider.size = new Vector3(3f, 2f, 0.1f); // Match canvas size (3m x 2m)

                // Set layer to UI for XR interaction
                canvasObj.layer = LayerMask.NameToLayer("UI");

                Debug.Log("[PortalMenuSetup] Created PortalMenuCanvas GameObject parented to Portal");
            }
        }

        void Start()
        {
            if (autoSetup)
            {
                SetupUI();
            }
        }

        public void SetupUI()
        {
            // Canvas was already created in Awake(), just verify it exists
            if (canvas == null)
            {
                Debug.LogError("[PortalMenuSetup] Canvas is null! This should not happen.");
                return;
            }

            // Configure canvas for World Space VR
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            // Set world camera for proper XR interaction
            // Try Camera.main first, then find any camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
                Debug.Log($"[PortalMenuSetup] Set canvas world camera to {mainCamera.name}");
            }
            else
            {
                Debug.LogWarning("[PortalMenuSetup] No camera found! Canvas may not render properly.");
            }

            // Scale canvas appropriately for VR - MUCH LARGER for visibility
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(300, 200);  // Larger canvas
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);  // 3m x 2m in world space

            // Set up CanvasScaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.dynamicPixelsPerUnit = 100;
            }

            // Create background panel
            GameObject panel = CreatePanel();

            // Create travel button
            CreateTravelButton(panel.transform);

            // TEMPORARY: Don't hide canvas for testing
            // Hide canvas initially
            // canvas.gameObject.SetActive(false);
            Debug.Log("[PortalMenuSetup] Canvas LEFT VISIBLE for testing");

            // Register with parent PortalMenu
            PortalMenu portalMenu = GetComponentInParent<PortalMenu>();
            if (portalMenu != null)
            {
                portalMenu.menuCanvas = canvas;
                Debug.Log("[PortalMenuSetup] Registered canvas with PortalMenu");
            }
            else
            {
                Debug.LogError("[PortalMenuSetup] Could not find PortalMenu in parent! Menu will not work.");
            }

            Debug.Log("[PortalMenuSetup] Portal menu UI created successfully");
        }

        GameObject CreatePanel()
        {
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            return panel;
        }

        void CreateTravelButton(Transform parent)
        {
            // Button GameObject
            GameObject buttonObj = new GameObject("TravelButton");
            buttonObj.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(280, 180); // Much larger - nearly full canvas
            buttonRect.anchoredPosition = Vector2.zero;

            // Button component
            travelButton = buttonObj.AddComponent<Button>();

            // Button image - BRIGHT CYAN for visibility
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0f, 1f, 1f, 1f); // Bright cyan

            // Button colors
            ColorBlock colors = travelButton.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.9f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
            colors.pressedColor = new Color(0.15f, 0.5f, 0.8f, 1f);
            colors.selectedColor = new Color(0.25f, 0.65f, 0.95f, 1f);
            travelButton.colors = colors;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Try TextMeshPro first, fall back to regular Text
            try
            {
                TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                tmpText.text = "⚔️ Enter the Dungeon 🚪";
                tmpText.fontSize = 36;
                tmpText.alignment = TextAlignmentOptions.Center;
                tmpText.color = Color.white;
                tmpText.fontStyle = FontStyles.Bold;
                tmpText.enableAutoSizing = true;
                tmpText.fontSizeMin = 18;
                tmpText.fontSizeMax = 48;

                // Try to load default TMP font
                TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (defaultFont != null)
                {
                    tmpText.font = defaultFont;
                }

                Debug.Log("[PortalMenuSetup] Created TextMeshPro button text");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PortalMenuSetup] TextMeshPro not available, using legacy Text: {e.Message}");
                // Fallback to legacy Text component
                Text legacyText = textObj.AddComponent<Text>();
                legacyText.text = "Enter the Dungeon";
                legacyText.fontSize = 32;
                legacyText.alignment = TextAnchor.MiddleCenter;
                legacyText.color = Color.white;
                legacyText.fontStyle = FontStyle.Bold;
                legacyText.resizeTextForBestFit = true;
                legacyText.resizeTextMinSize = 18;
                legacyText.resizeTextMaxSize = 48;
            }

            // Register button with PortalMenu
            PortalMenu portalMenu = GetComponentInParent<PortalMenu>();
            if (portalMenu != null)
            {
                portalMenu.travelButton = travelButton;
                Debug.Log("[PortalMenuSetup] Registered button with PortalMenu");
            }
        }

        public Button GetTravelButton()
        {
            return travelButton;
        }
    }
}

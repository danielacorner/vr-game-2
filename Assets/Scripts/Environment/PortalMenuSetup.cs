using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

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

                // DISABLED: BoxCollider might be causing physics issues
                // BoxCollider uiCollider = canvasObj.AddComponent<BoxCollider>();
                // uiCollider.isTrigger = true;
                // uiCollider.size = new Vector3(3f, 2f, 0.1f); // Match canvas size (3m x 2m)

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

            // Scale canvas appropriately for VR - larger for complex menu
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 280);  // Larger menu
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);  // 0.01 scale = 2m x 2.8m
            canvasRect.pivot = new Vector2(0.5f, 0.5f);  // Pivot at center

            // Set up CanvasScaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.dynamicPixelsPerUnit = 100;
            }

            // Add TrackedDeviceGraphicRaycaster for XR controller interaction
            // First remove any existing raycaster
            var existingRaycaster = canvas.gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (existingRaycaster != null)
            {
                Destroy(existingRaycaster);
            }

            // Add the XR-specific raycaster
            var xrRaycaster = canvas.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();

            // Set canvas and all children to UI layer (5) for XR interaction detection
            canvas.gameObject.layer = 5; // UI layer
            Debug.Log("[PortalMenuSetup] Added TrackedDeviceGraphicRaycaster and set to UI layer");

            // Create main menu layout
            GameObject mainPanel = CreateMainPanel();
            CreateComplexMenuLayout(mainPanel.transform);

            // Create records panel (floating to the right)
            CreateRecordsPanel();

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

        GameObject CreateMainPanel()
        {
            GameObject panel = new GameObject("MainPanel");
            panel.layer = 5; // UI layer
            panel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.12f, 0.1f, 0.95f); // Brown dungeon-like color

            return panel;
        }

        void CreateComplexMenuLayout(Transform parent)
        {
            float panelWidth = 200f;
            float yPos = 110f; // Start from top

            // Title: "Enter the Dungeon"
            CreateTitle(parent, yPos);
            yPos -= 35f;

            // Class selection buttons (Sorcerer, Wizard)
            CreateClassSelectionButtons(parent, yPos);
            yPos -= 70f;

            // "Ready?" text
            CreateReadyText(parent, yPos);
            yPos -= 30f;

            // Enter button
            CreateEnterButton(parent, yPos);
        }

        void CreateTitle(Transform parent, float yPos)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.layer = 5;
            titleObj.transform.SetParent(parent, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(180, 30);
            titleRect.anchoredPosition = new Vector2(0, yPos);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "Enter the Dungeon";
            titleText.fontSize = 24;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.9f, 0.4f); // Gold color
            titleText.fontStyle = FontStyle.Bold;

            try {
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }
        }

        private string selectedClass = "Sorcerer"; // Default selection

        void CreateClassSelectionButtons(Transform parent, float yPos)
        {
            // Container for class buttons
            float buttonWidth = 85f;
            float buttonHeight = 60f;
            float spacing = 10f;
            float startX = -(buttonWidth + spacing/2);

            // Sorcerer button
            CreateClassButton(parent, "Sorcerer", "Fire", startX, yPos, buttonWidth, buttonHeight, true);

            // Wizard button
            CreateClassButton(parent, "Wizard", "Book", startX + buttonWidth + spacing, yPos, buttonWidth, buttonHeight, false);
        }

        void CreateClassButton(Transform parent, string className, string iconText, float xPos, float yPos, float width, float height, bool isDefault)
        {
            GameObject buttonObj = new GameObject(className + "Button");
            buttonObj.layer = 5;
            buttonObj.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(width, height);
            buttonRect.anchoredPosition = new Vector2(xPos, yPos);

            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();

            // Set colors based on selection
            Color normalColor = isDefault ? new Color(0.2f, 0.4f, 0.2f, 0.9f) : new Color(0.25f, 0.25f, 0.25f, 0.9f);
            buttonImage.color = normalColor;

            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.3f, 0.95f);
            colors.pressedColor = new Color(0.15f, 0.35f, 0.15f, 1f);
            colors.selectedColor = new Color(0.25f, 0.45f, 0.25f, 0.95f);
            button.colors = colors;

            // Add click listener
            button.onClick.AddListener(() => OnClassSelected(className));

            // Icon text (placeholder for actual icon)
            GameObject iconObj = new GameObject("Icon");
            iconObj.layer = 5;
            iconObj.transform.SetParent(buttonObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(width - 10, 30);
            iconRect.anchoredPosition = new Vector2(0, 5);

            Text iconTextComp = iconObj.AddComponent<Text>();
            iconTextComp.text = iconText;
            iconTextComp.fontSize = 16;
            iconTextComp.alignment = TextAnchor.MiddleCenter;
            iconTextComp.color = new Color(1f, 0.8f, 0.3f);
            iconTextComp.fontStyle = FontStyle.Bold;

            try {
                iconTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }

            // Class name text
            GameObject nameObj = new GameObject("ClassName");
            nameObj.layer = 5;
            nameObj.transform.SetParent(buttonObj.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameRect.sizeDelta = new Vector2(width - 10, 20);
            nameRect.anchoredPosition = new Vector2(0, -15);

            Text nameText = nameObj.AddComponent<Text>();
            nameText.text = className;
            nameText.fontSize = 14;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = Color.white;

            try {
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }
        }

        void OnClassSelected(string className)
        {
            selectedClass = className;
            Debug.Log($"[PortalMenuSetup] Class selected: {className}");
            // Would update button visuals here in a more complete implementation
        }

        void CreateReadyText(Transform parent, float yPos)
        {
            GameObject readyObj = new GameObject("ReadyText");
            readyObj.layer = 5;
            readyObj.transform.SetParent(parent, false);

            RectTransform readyRect = readyObj.AddComponent<RectTransform>();
            readyRect.anchorMin = new Vector2(0.5f, 0.5f);
            readyRect.anchorMax = new Vector2(0.5f, 0.5f);
            readyRect.sizeDelta = new Vector2(180, 25);
            readyRect.anchoredPosition = new Vector2(0, yPos);

            Text readyText = readyObj.AddComponent<Text>();
            readyText.text = "Ready?";
            readyText.fontSize = 20;
            readyText.alignment = TextAnchor.MiddleCenter;
            readyText.color = new Color(1f, 0.9f, 0.4f); // Gold color
            readyText.fontStyle = FontStyle.Bold;

            try {
                readyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }
        }

        void CreateEnterButton(Transform parent, float yPos)
        {
            GameObject buttonObj = new GameObject("EnterButton");
            buttonObj.layer = 5;
            buttonObj.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(180, 45);
            buttonRect.anchoredPosition = new Vector2(0, yPos);

            travelButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.25f, 0.2f, 0.95f);

            ColorBlock colors = travelButton.colors;
            colors.normalColor = new Color(0.3f, 0.25f, 0.2f, 0.95f);
            colors.highlightedColor = new Color(0.4f, 0.35f, 0.25f, 0.98f);
            colors.pressedColor = new Color(0.25f, 0.2f, 0.15f, 1f);
            travelButton.colors = colors;

            // Icon (torch icon placeholder on left)
            GameObject iconObj = new GameObject("Icon");
            iconObj.layer = 5;
            iconObj.transform.SetParent(buttonObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.sizeDelta = new Vector2(30, 30);
            iconRect.anchoredPosition = new Vector2(20, 0);

            Text iconText = iconObj.AddComponent<Text>();
            iconText.text = "T"; // Torch icon placeholder
            iconText.fontSize = 20;
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.color = new Color(1f, 0.6f, 0.2f); // Orange torch color
            iconText.fontStyle = FontStyle.Bold;

            try {
                iconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.layer = 5;
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(40, 0); // Offset for icon
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = "Enter the Dungeon";
            buttonText.fontSize = 18;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = new Color(1f, 0.9f, 0.4f); // Gold color
            buttonText.fontStyle = FontStyle.Bold;

            try {
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }

            // Register with PortalMenu
            PortalMenu portalMenu = GetComponentInParent<PortalMenu>();
            if (portalMenu != null)
            {
                portalMenu.travelButton = travelButton;
                Debug.Log("[PortalMenuSetup] Registered enter button with PortalMenu");
            }
        }

        void CreateRecordsPanel()
        {
            GameObject recordsObj = new GameObject("RecordsPanel");
            recordsObj.layer = 5;
            recordsObj.transform.SetParent(canvas.transform, false);

            RectTransform recordsRect = recordsObj.AddComponent<RectTransform>();
            recordsRect.anchorMin = new Vector2(0.5f, 0.5f);
            recordsRect.anchorMax = new Vector2(0.5f, 0.5f);
            recordsRect.sizeDelta = new Vector2(90, 120);
            recordsRect.anchoredPosition = new Vector2(150, -20); // Offset to the right
            recordsRect.localEulerAngles = new Vector3(0, 30, 0); // 30 degree angle

            Image recordsImage = recordsObj.AddComponent<Image>();
            recordsImage.color = new Color(0.18f, 0.15f, 0.12f, 0.95f); // Slightly different brown

            // Records title
            GameObject titleObj = new GameObject("RecordsTitle");
            titleObj.layer = 5;
            titleObj.transform.SetParent(recordsObj.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(80, 20);
            titleRect.anchoredPosition = new Vector2(0, -15);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "Records";
            titleText.fontSize = 16;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.9f, 0.4f); // Gold
            titleText.fontStyle = FontStyle.Bold;

            try {
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }

            // Records entries
            CreateRecordEntry(recordsObj.transform, "Fire", "189 (SP)", -35);
            CreateRecordEntry(recordsObj.transform, "Book", "None", -60);
            CreateRecordEntry(recordsObj.transform, "T", "None", -85);
        }

        void CreateRecordEntry(Transform parent, string iconText, string scoreText, float yPos)
        {
            GameObject entryObj = new GameObject("RecordEntry");
            entryObj.layer = 5;
            entryObj.transform.SetParent(parent, false);

            RectTransform entryRect = entryObj.AddComponent<RectTransform>();
            entryRect.anchorMin = new Vector2(0.5f, 1f);
            entryRect.anchorMax = new Vector2(0.5f, 1f);
            entryRect.sizeDelta = new Vector2(80, 20);
            entryRect.anchoredPosition = new Vector2(0, yPos);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.layer = 5;
            iconObj.transform.SetParent(entryObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.sizeDelta = new Vector2(20, 20);
            iconRect.anchoredPosition = new Vector2(10, 0);

            Text iconTextComp = iconObj.AddComponent<Text>();
            iconTextComp.text = iconText;
            iconTextComp.fontSize = 12;
            iconTextComp.alignment = TextAnchor.MiddleCenter;
            iconTextComp.color = new Color(1f, 0.8f, 0.3f); // Gold
            iconTextComp.fontStyle = FontStyle.Bold;

            try {
                iconTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }

            // Score text
            GameObject scoreObj = new GameObject("Score");
            scoreObj.layer = 5;
            scoreObj.transform.SetParent(entryObj.transform, false);

            RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 0.5f);
            scoreRect.anchorMax = new Vector2(1, 0.5f);
            scoreRect.sizeDelta = new Vector2(-30, 20);
            scoreRect.anchoredPosition = new Vector2(15, 0);

            Text scoreTextComp = scoreObj.AddComponent<Text>();
            scoreTextComp.text = scoreText;
            scoreTextComp.fontSize = 12;
            scoreTextComp.alignment = TextAnchor.MiddleRight;
            scoreTextComp.color = new Color(1f, 0.9f, 0.4f); // Gold
            scoreTextComp.fontStyle = FontStyle.Normal;

            try {
                scoreTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }
        }


        public Button GetTravelButton()
        {
            return travelButton;
        }
    }
}

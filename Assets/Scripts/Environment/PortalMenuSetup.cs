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

        // Icon textures
        private Texture2D fireballIcon;
        private Texture2D spellbookIcon;
        private Texture2D torchIcon;
        private Texture2D swordIcon;
        private Texture2D decorativeBorder;

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
                // Use TrackedDeviceGraphicRaycaster for VR UI interaction
                canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();

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
            // Generate procedural icon textures at runtime
            GenerateIcons();

            if (autoSetup)
            {
                SetupUI();
            }
        }

        void GenerateIcons()
        {
            fireballIcon = GenerateFireballIcon(64, 64);
            spellbookIcon = GenerateSpellbookIcon(64, 64);
            torchIcon = GenerateTorchIcon(64, 64);
            swordIcon = GenerateSwordIcon(64, 64);
            decorativeBorder = GenerateDecorativeBorder(256, 256);
        }

        Texture2D GenerateFireballIcon(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            int centerX = width / 2;
            int centerY = height / 2;
            float maxRadius = Mathf.Min(width, height) / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float normalizedDist = distance / maxRadius;

                    Color color;
                    if (normalizedDist < 0.4f)
                    {
                        // Yellow center (hot core)
                        color = Color.Lerp(Color.yellow, new Color(1f, 0.8f, 0f), normalizedDist / 0.4f);
                    }
                    else if (normalizedDist < 0.8f)
                    {
                        // Orange middle
                        float t = (normalizedDist - 0.4f) / 0.4f;
                        color = Color.Lerp(new Color(1f, 0.6f, 0f), new Color(1f, 0.3f, 0f), t);
                    }
                    else if (normalizedDist < 1f)
                    {
                        // Red edge with falloff
                        float t = (normalizedDist - 0.8f) / 0.2f;
                        color = Color.Lerp(new Color(1f, 0.2f, 0f), new Color(0.8f, 0f, 0f, 0f), t);
                    }
                    else
                    {
                        color = Color.clear;
                    }

                    pixels[y * width + x] = color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        Texture2D GenerateSpellbookIcon(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            // Brown book background
            Color bookColor = new Color(0.4f, 0.3f, 0.2f, 1f);
            Color pageColor = new Color(0.95f, 0.9f, 0.8f, 1f);
            Color lineColor = new Color(0.3f, 0.25f, 0.2f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = Color.clear;

                    // Book cover (slightly inset)
                    if (x >= 8 && x < width - 8 && y >= 5 && y < height - 5)
                    {
                        color = bookColor;

                        // Pages showing on right side
                        if (x >= width / 2 + 2 && x < width - 10 && y >= 8 && y < height - 8)
                        {
                            color = pageColor;

                            // Horizontal lines on pages
                            if ((y - 12) % 6 == 0 && x < width - 14)
                            {
                                color = lineColor;
                            }
                        }

                        // Book spine in middle
                        if (x >= width / 2 - 2 && x < width / 2 + 2)
                        {
                            color = new Color(0.3f, 0.2f, 0.15f, 1f);
                        }
                    }

                    pixels[y * width + x] = color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        Texture2D GenerateTorchIcon(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            Color woodColor = new Color(0.4f, 0.25f, 0.15f, 1f);
            Color flameOrange = new Color(1f, 0.5f, 0f, 1f);
            Color flameYellow = new Color(1f, 0.9f, 0.3f, 1f);

            int centerX = width / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = Color.clear;

                    // Wooden handle (bottom 2/3)
                    if (y < height * 2 / 3)
                    {
                        int handleWidth = width / 4;
                        if (x >= centerX - handleWidth / 2 && x < centerX + handleWidth / 2)
                        {
                            color = woodColor;
                        }
                    }
                    // Flame (top 1/3)
                    else
                    {
                        float flameY = y - (height * 2 / 3);
                        float flameHeight = height / 3f;
                        float dx = (x - centerX) / (float)(width / 3f);
                        float dy = flameY / flameHeight;

                        // Flame shape (teardrop)
                        float distance = Mathf.Sqrt(dx * dx + dy * dy * 0.5f);

                        if (distance < 0.8f)
                        {
                            // Yellow center
                            if (distance < 0.4f)
                            {
                                color = flameYellow;
                            }
                            else
                            {
                                // Orange outer flame
                                float t = (distance - 0.4f) / 0.4f;
                                color = Color.Lerp(flameYellow, flameOrange, t);
                            }

                            // Add transparency at edges
                            if (distance > 0.6f)
                            {
                                float alpha = 1f - (distance - 0.6f) / 0.2f;
                                color.a *= alpha;
                            }
                        }
                    }

                    pixels[y * width + x] = color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        Texture2D GenerateSwordIcon(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            Color bladeColor = new Color(0.7f, 0.75f, 0.8f, 1f); // Silver/steel
            Color hiltColor = new Color(0.4f, 0.3f, 0.2f, 1f); // Brown leather
            Color crossguardColor = new Color(0.6f, 0.5f, 0.3f, 1f); // Bronze/brass

            int centerX = width / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = Color.clear;

                    // Hilt (bottom 20%)
                    if (y < height * 0.2f)
                    {
                        int hiltWidth = width / 5;
                        if (x >= centerX - hiltWidth / 2 && x < centerX + hiltWidth / 2)
                        {
                            color = hiltColor;
                        }
                    }
                    // Crossguard (20-30% height)
                    else if (y >= height * 0.2f && y < height * 0.3f)
                    {
                        int crossguardWidth = (int)(width * 0.6f);
                        if (x >= centerX - crossguardWidth / 2 && x < centerX + crossguardWidth / 2)
                        {
                            color = crossguardColor;
                        }
                    }
                    // Blade (top 70%)
                    else
                    {
                        float bladeY = y - (height * 0.3f);
                        float bladeHeight = height * 0.7f;
                        float normalizedY = bladeY / bladeHeight;

                        // Blade tapers from wide at base to point at tip
                        int bladeWidthAtBase = width / 4;
                        int bladeWidthAtY = (int)(bladeWidthAtBase * (1f - normalizedY * 0.9f));

                        if (x >= centerX - bladeWidthAtY / 2 && x < centerX + bladeWidthAtY / 2)
                        {
                            color = bladeColor;

                            // Add highlight down center of blade
                            if (x >= centerX - 1 && x < centerX + 1)
                            {
                                color = Color.Lerp(bladeColor, Color.white, 0.3f);
                            }
                        }
                    }

                    pixels[y * width + x] = color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        Texture2D GenerateDecorativeBorder(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            // Border colors - bronze/brass medieval style
            Color borderMain = new Color(0.55f, 0.45f, 0.3f, 1f);
            Color borderHighlight = new Color(0.7f, 0.6f, 0.4f, 1f);
            Color borderShadow = new Color(0.35f, 0.3f, 0.2f, 1f);
            Color borderDark = new Color(0.25f, 0.2f, 0.15f, 1f);

            int borderThickness = 8;
            int cornerSize = 16;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = Color.clear;

                    // Distance from edges
                    int distFromLeft = x;
                    int distFromRight = width - 1 - x;
                    int distFromTop = height - 1 - y;
                    int distFromBottom = y;

                    int minDist = Mathf.Min(distFromLeft, distFromRight, distFromTop, distFromBottom);

                    // Main border area
                    if (minDist < borderThickness)
                    {
                        // Add some texture/noise to the border
                        float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);

                        // Gradient effect - lighter on outer edge, darker inside
                        if (minDist < 2)
                        {
                            color = Color.Lerp(borderShadow, borderHighlight, noise);
                        }
                        else if (minDist < 5)
                        {
                            color = Color.Lerp(borderMain, borderHighlight, noise * 0.5f);
                        }
                        else
                        {
                            color = Color.Lerp(borderDark, borderMain, noise * 0.3f);
                        }
                    }

                    // Decorative corners
                    bool isInCorner = false;

                    // Top-left corner
                    if (x < cornerSize && y > height - cornerSize)
                    {
                        int cornerX = x;
                        int cornerY = height - 1 - y;
                        float cornerDist = Mathf.Sqrt(cornerX * cornerX + cornerY * cornerY);

                        if (cornerDist < cornerSize)
                        {
                            isInCorner = true;
                            float t = cornerDist / cornerSize;
                            color = Color.Lerp(borderHighlight, borderDark, t);

                            // Add decorative lines
                            if ((int)(cornerDist) % 4 == 0)
                            {
                                color = Color.Lerp(color, borderHighlight, 0.3f);
                            }
                        }
                    }

                    // Top-right corner
                    if (x >= width - cornerSize && y > height - cornerSize)
                    {
                        int cornerX = width - 1 - x;
                        int cornerY = height - 1 - y;
                        float cornerDist = Mathf.Sqrt(cornerX * cornerX + cornerY * cornerY);

                        if (cornerDist < cornerSize)
                        {
                            isInCorner = true;
                            float t = cornerDist / cornerSize;
                            color = Color.Lerp(borderHighlight, borderDark, t);

                            if ((int)(cornerDist) % 4 == 0)
                            {
                                color = Color.Lerp(color, borderHighlight, 0.3f);
                            }
                        }
                    }

                    // Bottom-left corner
                    if (x < cornerSize && y < cornerSize)
                    {
                        int cornerX = x;
                        int cornerY = y;
                        float cornerDist = Mathf.Sqrt(cornerX * cornerX + cornerY * cornerY);

                        if (cornerDist < cornerSize)
                        {
                            isInCorner = true;
                            float t = cornerDist / cornerSize;
                            color = Color.Lerp(borderHighlight, borderDark, t);

                            if ((int)(cornerDist) % 4 == 0)
                            {
                                color = Color.Lerp(color, borderHighlight, 0.3f);
                            }
                        }
                    }

                    // Bottom-right corner
                    if (x >= width - cornerSize && y < cornerSize)
                    {
                        int cornerX = width - 1 - x;
                        int cornerY = y;
                        float cornerDist = Mathf.Sqrt(cornerX * cornerX + cornerY * cornerY);

                        if (cornerDist < cornerSize)
                        {
                            isInCorner = true;
                            float t = cornerDist / cornerSize;
                            color = Color.Lerp(borderHighlight, borderDark, t);

                            if ((int)(cornerDist) % 4 == 0)
                            {
                                color = Color.Lerp(color, borderHighlight, 0.3f);
                            }
                        }
                    }

                    pixels[y * width + x] = color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        void AddBorderToElement(GameObject element, Vector2 sizeDelta)
        {
            GameObject borderObj = new GameObject("DecorativeBorder");
            borderObj.layer = 5;
            borderObj.transform.SetParent(element.transform, false);

            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            RawImage borderImage = borderObj.AddComponent<RawImage>();
            borderImage.texture = decorativeBorder;
            borderImage.raycastTarget = false; // Don't block button clicks
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

            // Add decorative border
            AddBorderToElement(panel, panelRect.sizeDelta);

            return panel;
        }

        void CreateComplexMenuLayout(Transform parent)
        {
            float panelWidth = 200f;
            float yPos = 110f; // Start from top

            // Title: "Enter the Dungeon"
            CreateTitle(parent, yPos);
            // Subtitle: "Enter the Dungeon"
            yPos -= 30f; // Space after title
            CreateSubtitle(parent, yPos);
            yPos -= 50f; // More space after subtitle


            // Class selection buttons (Sorcerer, Wizard)
            CreateClassSelectionButtons(parent, yPos);
            yPos -= 70f;

            // "Ready?" text
            CreateReadyText(parent, yPos);
            yPos -= 40f; // More space before Enter button

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
            titleText.text = "Run Preparation";
            titleText.fontSize = 16; // Reduced by 50%
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.9f, 0.4f); // Gold color
            titleText.fontStyle = FontStyle.Bold;

            try {
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            } catch { }
        }

        void CreateSubtitle(Transform parent, float yPos)
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
            titleText.text = "Select Class";
            titleText.fontSize = 12; // Reduced by 50%
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

            // Center the buttons horizontally
            float leftButtonX = -(buttonWidth/2 + spacing/2);
            float rightButtonX = (buttonWidth/2 + spacing/2);

            // Sorcerer button (left)
            CreateClassButton(parent, "Sorcerer", "Fire", leftButtonX, yPos, buttonWidth, buttonHeight, true);

            // Wizard button (right)
            CreateClassButton(parent, "Wizard", "Book", rightButtonX, yPos, buttonWidth, buttonHeight, false);
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
            buttonObj.AddComponent<VRDungeonCrawler.Player.XRButtonHighlight>(); // Enable XR hover highlighting

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

            // Icon image
            GameObject iconObj = new GameObject("Icon");
            iconObj.layer = 5;
            iconObj.transform.SetParent(buttonObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(40, 40); // Square icon
            iconRect.anchoredPosition = new Vector2(0, 5);

            RawImage iconImage = iconObj.AddComponent<RawImage>();
            // Set the appropriate texture based on iconText
            if (iconText == "Fire")
            {
                iconImage.texture = fireballIcon;
            }
            else if (iconText == "Book")
            {
                iconImage.texture = spellbookIcon;
            }

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

            // Add decorative border
            AddBorderToElement(buttonObj, new Vector2(width, height));
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
            readyText.fontSize = 10; // Reduced by 50%
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
            buttonObj.AddComponent<VRDungeonCrawler.Player.XRButtonHighlight>(); // Enable XR hover highlighting
            buttonImage.color = new Color(0.3f, 0.25f, 0.2f, 0.95f);

            ColorBlock colors = travelButton.colors;
            colors.normalColor = new Color(0.3f, 0.25f, 0.2f, 0.95f);
            colors.highlightedColor = new Color(0.4f, 0.35f, 0.25f, 0.98f);
            colors.pressedColor = new Color(0.25f, 0.2f, 0.15f, 1f);
            travelButton.colors = colors;

            // Icon (sword icon on left)
            GameObject iconObj = new GameObject("Icon");
            iconObj.layer = 5;
            iconObj.transform.SetParent(buttonObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.sizeDelta = new Vector2(35, 35);
            iconRect.anchoredPosition = new Vector2(25, 0);

            RawImage iconImage = iconObj.AddComponent<RawImage>();
            iconImage.texture = swordIcon;

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

            // Add decorative border
            AddBorderToElement(buttonObj, new Vector2(180, 45));
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

            // Add decorative border
            AddBorderToElement(recordsObj, new Vector2(90, 120));
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
            iconRect.sizeDelta = new Vector2(18, 18);
            iconRect.anchoredPosition = new Vector2(12, 0);

            RawImage iconImage = iconObj.AddComponent<RawImage>();
            // Set the appropriate texture based on iconText
            if (iconText == "Fire")
            {
                iconImage.texture = fireballIcon;
            }
            else if (iconText == "Book")
            {
                iconImage.texture = spellbookIcon;
            }
            else if (iconText == "T")
            {
                iconImage.texture = torchIcon;
            }

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

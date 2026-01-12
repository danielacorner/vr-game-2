using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using VRDungeonCrawler.UI;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Editor tool to create game mode menu UI
    /// </summary>
    public static class CreateGameModeMenu
    {
        [MenuItem("Tools/VR Dungeon Crawler/Create Game Mode Menu")]
        public static void CreateMenu()
        {
            Debug.Log("========================================");
            Debug.Log("Creating Game Mode Menu");
            Debug.Log("========================================");

            // Create root canvas object
            GameObject canvasGO = new GameObject("GameModeMenu");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            
            GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();
            
            GameModeMenu menuScript = canvasGO.AddComponent<GameModeMenu>();

            // Set canvas size
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(800, 600);
            canvasRect.localScale = new Vector3(0.001f, 0.001f, 0.001f); // Scale down for VR

            // Create panel background
            GameObject panelGO = new GameObject("MenuPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Dark semi-transparent

            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Create title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform, false);
            
            TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "SELECT GAME MODE";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 0.95f);
            titleRect.sizeDelta = Vector2.zero;

            // Create mode buttons
            GameObject standardButton = CreateModeButton("StandardButton", "⚔ STANDARD", panelGO.transform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.7f));
            GameObject challengeButton = CreateModeButton("ChallengeButton", "🔥 CHALLENGE", panelGO.transform, new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.53f));
            GameObject endlessButton = CreateModeButton("EndlessButton", "∞ ENDLESS", panelGO.transform, new Vector2(0.1f, 0.21f), new Vector2(0.9f, 0.36f));

            // Create selected mode text
            GameObject selectedTextGO = new GameObject("SelectedModeText");
            selectedTextGO.transform.SetParent(panelGO.transform, false);
            
            TextMeshProUGUI selectedText = selectedTextGO.AddComponent<TextMeshProUGUI>();
            selectedText.text = "Mode: Standard";
            selectedText.fontSize = 32;
            selectedText.alignment = TextAlignmentOptions.Center;
            selectedText.color = new Color(0.2f, 0.8f, 0.2f);

            RectTransform selectedRect = selectedTextGO.GetComponent<RectTransform>();
            selectedRect.anchorMin = new Vector2(0.1f, 0.13f);
            selectedRect.anchorMax = new Vector2(0.9f, 0.19f);
            selectedRect.sizeDelta = Vector2.zero;

            // Create description text
            GameObject descTextGO = new GameObject("DescriptionText");
            descTextGO.transform.SetParent(panelGO.transform, false);
            
            TextMeshProUGUI descText = descTextGO.AddComponent<TextMeshProUGUI>();
            descText.text = "Classic dungeon crawl with balanced difficulty";
            descText.fontSize = 24;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = Color.white;

            RectTransform descRect = descTextGO.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.06f);
            descRect.anchorMax = new Vector2(0.9f, 0.12f);
            descRect.sizeDelta = Vector2.zero;

            // Create Play button
            GameObject playButton = CreateActionButton("PlayButton", "▶ PLAY", panelGO.transform, new Vector2(0.25f, 0.02f), new Vector2(0.75f, 0.10f), new Color(0.2f, 0.7f, 0.2f));

            // Wire up references in menu script
            menuScript.menuPanel = panelGO;
            menuScript.standardModeButton = standardButton.GetComponent<Button>();
            menuScript.challengeModeButton = challengeButton.GetComponent<Button>();
            menuScript.endlessModeButton = endlessButton.GetComponent<Button>();
            menuScript.playButton = playButton.GetComponent<Button>();
            menuScript.selectedModeText = selectedText;
            menuScript.modeDescriptionText = descText;

            Debug.Log("✓ Created Game Mode Menu GameObject");
            Debug.Log("========================================");
            Debug.Log("Now save this as a prefab at: Assets/Prefabs/UI/GameModeMenu.prefab");
            Debug.Log("========================================");

            // Select the created object
            Selection.activeGameObject = canvasGO;
        }

        private static GameObject CreateModeButton(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);

            Button button = buttonGO.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.4f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.25f, 1f);
            colors.selectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
            button.colors = colors;

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.sizeDelta = Vector2.zero;

            // Add text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 36;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return buttonGO;
        }

        private static GameObject CreateActionButton(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = color;

            Button button = buttonGO.AddComponent<Button>();

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.sizeDelta = Vector2.zero;

            // Add text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 40;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return buttonGO;
        }
    }
}

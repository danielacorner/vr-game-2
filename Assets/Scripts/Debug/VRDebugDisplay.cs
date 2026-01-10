using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace VRDungeonCrawler.Debugging
{
    /// <summary>
    /// Shows Unity console logs in VR as floating text
    /// Attach to any GameObject and it will create a floating debug panel
    /// </summary>
    public class VRDebugDisplay : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Show the debug panel")]
        public bool showDebugPanel = true;

        [Tooltip("Maximum number of log messages to show")]
        [Range(5, 20)]
        public int maxLogMessages = 10;

        [Tooltip("Distance from camera")]
        public float distanceFromCamera = 2f;

        [Tooltip("Panel height offset")]
        public float heightOffset = 0.5f;

        [Header("Filter")]
        [Tooltip("Only show logs containing this text (empty = show all)")]
        public string filterText = "PlayerMovementController";

        private Canvas debugCanvas;
        private TextMeshProUGUI debugText;
        private List<string> logMessages = new List<string>();
        private Camera mainCamera;

        void Start()
        {
            if (!showDebugPanel) return;

            CreateDebugPanel();
            Application.logMessageReceived += HandleLog;
        }

        void CreateDebugPanel()
        {
            // Create canvas
            GameObject canvasObj = new GameObject("VR_DebugCanvas");
            canvasObj.transform.SetParent(transform);

            debugCanvas = canvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.WorldSpace;

            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10;

            // Set canvas size and position
            RectTransform canvasRect = debugCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(600, 400);
            canvasRect.localScale = Vector3.one * 0.001f; // Small scale for VR

            // Create background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Create text
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(canvasObj.transform, false);

            debugText = textObj.AddComponent<TextMeshProUGUI>();
            debugText.fontSize = 18;
            debugText.color = Color.white;
            debugText.alignment = TextAlignmentOptions.TopLeft;
            debugText.enableWordWrapping = true;
            debugText.text = "VR Debug Console\nWaiting for logs...\n";

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, -20);
            textRect.anchoredPosition = Vector2.zero;

            UnityEngine.Debug.Log("[VRDebugDisplay] Debug panel created - you should see this in your headset!");
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!showDebugPanel) return;

            // Filter logs if needed
            if (!string.IsNullOrEmpty(filterText))
            {
                if (!logString.Contains(filterText))
                    return;
            }

            // Add color based on log type
            string coloredLog = logString;
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    coloredLog = $"<color=red>❌ {logString}</color>";
                    break;
                case LogType.Warning:
                    coloredLog = $"<color=yellow>⚠️ {logString}</color>";
                    break;
                case LogType.Log:
                    coloredLog = $"<color=white>{logString}</color>";
                    break;
            }

            // Add to list
            logMessages.Add(coloredLog);

            // Keep only max messages
            if (logMessages.Count > maxLogMessages)
            {
                logMessages.RemoveAt(0);
            }

            // Update display
            UpdateDebugText();
        }

        void UpdateDebugText()
        {
            if (debugText != null)
            {
                debugText.text = "=== VR DEBUG CONSOLE ===\n" + string.Join("\n", logMessages);
            }
        }

        void LateUpdate()
        {
            if (!showDebugPanel || debugCanvas == null) return;

            // Position panel in front of camera
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                Vector3 targetPosition = mainCamera.transform.position +
                                        mainCamera.transform.forward * distanceFromCamera +
                                        Vector3.up * heightOffset;

                debugCanvas.transform.position = targetPosition;
                debugCanvas.transform.LookAt(mainCamera.transform);
                debugCanvas.transform.Rotate(0, 180, 0); // Face the camera
            }
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        [ContextMenu("Toggle Debug Panel")]
        public void ToggleDebugPanel()
        {
            showDebugPanel = !showDebugPanel;
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(showDebugPanel);
            }
        }
    }
}

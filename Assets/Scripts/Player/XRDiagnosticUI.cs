using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Creates a Canvas with diagnostic info visible in VR
    /// </summary>
    public class XRDiagnosticUI : MonoBehaviour
    {
        private Canvas canvas;
        private Text statusText; // Using built-in UI.Text instead of TextMeshPro
        private TrackedPoseDriver tpd;

        void Start()
        {
            CreateCanvas();
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                tpd = mainCam.GetComponent<TrackedPoseDriver>();
            }
        }

        void CreateCanvas()
        {
            // Create world-space canvas - FIXED position, not relative to anything
            GameObject canvasObj = new GameObject("DiagnosticCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Position directly at spawn point, facing +Z direction
            canvas.transform.position = new Vector3(0, 2f, 3f); // 2m up, 3m forward from origin
            canvas.transform.rotation = Quaternion.Euler(0, 180, 0); // Face back toward origin

            RectTransform rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1000, 800); // Bigger panel
            rect.localScale = Vector3.one * 0.005f; // Smaller scale = bigger apparent size

            // Add background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Add text - using built-in UI.Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            statusText = textObj.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 32; // Larger font
            statusText.color = Color.yellow; // Yellow on black should be very visible
            statusText.alignment = TextAnchor.UpperLeft;
            statusText.text = "LOADING...";
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, -20);
            textRect.anchoredPosition = Vector2.zero;
        }

        void Update()
        {
            if (statusText == null) return;

            string status = "=== XR DIAGNOSTIC ===\n\n";

            // XR System status
            status += $"XR Enabled: {XRSettings.enabled}\n";
            status += $"XR Device: {XRSettings.loadedDeviceName}\n";
            status += $"XR Active: {XRSettings.isDeviceActive}\n\n";

            // Subsystems
            List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            status += $"XR Subsystems: {subsystems.Count}\n";
            if (subsystems.Count > 0)
            {
                status += $"Running: {subsystems[0].running}\n\n";
            }
            else
            {
                status += "NO SUBSYSTEMS!\n\n";
            }

            // Camera info
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                status += $"Camera Pos: {mainCam.transform.position:F2}\n";
                status += $"Camera Rot: {mainCam.transform.eulerAngles:F1}\n\n";

                if (tpd != null)
                {
                    status += $"TPD Enabled: {tpd.enabled}\n";
                    status += $"TPD Type: {tpd.trackingType}\n";
                }
                else
                {
                    status += "NO TrackedPoseDriver!\n";
                }
            }
            else
            {
                status += "NO MAIN CAMERA!\n";
            }

            status += $"\n\nIf you CAN read this,\nrendering works.\n";
            status += $"If text doesn't move\nwhen you move head,\nTRACKING IS BROKEN.";

            statusText.text = status;

            // Keep canvas facing camera
            if (Camera.main != null)
            {
                Vector3 dir = Camera.main.transform.position - canvas.transform.position;
                if (dir != Vector3.zero)
                {
                    canvas.transform.rotation = Quaternion.LookRotation(-dir);
                }
            }
        }
    }
}

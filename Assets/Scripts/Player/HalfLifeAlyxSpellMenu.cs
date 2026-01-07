using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using System.Collections.Generic;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Half-Life Alyx style radial spell menu
    /// - Click joystick to open menu at hand position
    /// - 3D circular spell icons fan out in radial pattern
    /// - Move joystick to select, release to confirm
    /// - Expandable design for 4+ spells
    /// </summary>
    public class HalfLifeAlyxSpellMenu : MonoBehaviour
    {
        [Header("Configuration")]
        public bool isLeftHand = true;

        [Header("Menu Settings")]
        [Tooltip("Distance from hand where menu appears")]
        public float menuDistance = 0.15f;

        [Tooltip("Radius of the spell icon circle")]
        public float iconRadius = 0.15f;

        [Tooltip("Size of each 3D spell icon")]
        public float iconSize = 0.1f; // Doubled from 0.05f

        [Tooltip("Distance to detect hand hovering over spell")]
        public float hoverDetectionRadius = 0.08f;

        [Tooltip("Joystick deadzone for selection")]
        [Range(0f, 0.9f)]
        public float selectionDeadzone = 0.3f;

        [Tooltip("Tilt angle toward player (degrees) - makes top/bottom spells easier to reach")]
        [Range(0f, 45f)]
        public float menuTiltAngle = -40f;

        [Header("Visual Settings")]
        public Color normalColor = new Color(1f, 1f, 1f, 0.7f);
        public Color highlightColor = new Color(1f, 0.8f, 0f, 1f);
        public float highlightScale = 1.3f;

        [Header("References")]
        public Transform handTransform;
        public HandPoseController handPoseController;

        private InputDevice device;
        private bool deviceFound = false;
        private bool menuOpen = false;
        private bool joystickButtonHeld = false;

        private List<SpellIconObject> spellIcons = new List<SpellIconObject>();
        private GameObject menuRoot;
        private int hoveredIndex = -1;
        private int previousHoveredIndex = -1; // Track previous hover for haptic feedback
        private Vector2 joystickInput;

        // Fixed menu position (set when menu opens)
        private Vector3 fixedMenuPosition;
        // Rotation is calculated every frame for true billboard behavior

        // Cache the VR camera transform for billboard rotation
        private Transform vrCameraTransform;

        // Cache turn providers to disable them during spell selection
        private SnapTurnProvider snapTurnProvider;
        private ContinuousTurnProvider continuousTurnProvider;

        private class SpellIconObject
        {
            public GameObject gameObject;
            public MeshRenderer renderer;
            public SpellData spell;
            public Vector3 targetPosition;
            public int index;
        }

        private void Start()
        {
            // Find hand transform if not set
            if (handTransform == null)
                handTransform = transform;

            // Auto-find hand pose controller
            if (handPoseController == null)
                handPoseController = GetComponentInChildren<HandPoseController>();

            FindDevice();
            CreateMenuRoot();
            FindVRCamera();
            FindTurnProviders();
        }

        private void FindTurnProviders()
        {
            // Find turn providers to disable during spell selection
            snapTurnProvider = FindAnyObjectByType<SnapTurnProvider>();
            continuousTurnProvider = FindAnyObjectByType<ContinuousTurnProvider>();

            if (snapTurnProvider != null)
                Debug.Log("[AlyxSpellMenu] ✓ Found SnapTurnProvider");
            if (continuousTurnProvider != null)
                Debug.Log("[AlyxSpellMenu] ✓ Found ContinuousTurnProvider");
        }

        private void FindVRCamera()
        {
            // Try to find the actual VR camera (Main Camera under Camera Offset)
            GameObject cameraOffset = GameObject.Find("Camera Offset");
            if (cameraOffset != null)
            {
                // Look for Main Camera child of Camera Offset
                Transform mainCamTransform = cameraOffset.transform.Find("Main Camera");
                if (mainCamTransform != null)
                {
                    vrCameraTransform = mainCamTransform;
                    Debug.Log($"[AlyxSpellMenu] ✓ Found VR Main Camera: {vrCameraTransform.name} (child of Camera Offset)");
                    return;
                }
            }

            // Fallback: Try to find camera by tag
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject cam in cameras)
            {
                // Find the one that's a child of Camera Offset
                if (cam.transform.parent != null && cam.transform.parent.name.Contains("Camera Offset"))
                {
                    vrCameraTransform = cam.transform;
                    Debug.Log($"[AlyxSpellMenu] ✓ Found VR camera by tag: {vrCameraTransform.name}");
                    return;
                }
            }

            // Last resort: Use first MainCamera
            if (cameras.Length > 0)
            {
                vrCameraTransform = cameras[0].transform;
                Debug.LogWarning($"[AlyxSpellMenu] Using first MainCamera as fallback: {vrCameraTransform.name}");
            }
            else
            {
                Debug.LogError("[AlyxSpellMenu] Could not find VR camera!");
            }
        }

        private void FindDevice()
        {
            var desiredCharacteristics = isLeftHand ?
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller :
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, devices);

            if (devices.Count > 0)
            {
                device = devices[0];
                deviceFound = true;
                Debug.Log($"[AlyxSpellMenu] ✓ Found {(isLeftHand ? "LEFT" : "RIGHT")} controller: {device.name}");
            }
        }

        private void CreateMenuRoot()
        {
            menuRoot = new GameObject("SpellMenuRoot");
            menuRoot.transform.SetParent(transform);
            menuRoot.SetActive(false);
        }

        private void Update()
        {
            if (!deviceFound)
            {
                if (Time.frameCount % 60 == 0)
                    FindDevice();
                return;
            }

            // Check joystick button (primary2DAxisClick)
            bool joystickClick = false;
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool clickValue))
            {
                joystickClick = clickValue;
            }

            // Get joystick input for selection
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            {
                joystickInput = axis;
            }

            // Open menu on joystick click
            if (joystickClick && !joystickButtonHeld)
            {
                joystickButtonHeld = true;
                if (!menuOpen)
                {
                    OpenMenu();
                }
            }
            // Close menu on release
            else if (!joystickClick && joystickButtonHeld)
            {
                joystickButtonHeld = false;
                if (menuOpen)
                {
                    SelectSpellAndClose();
                }
            }

            // Update menu if open
            if (menuOpen)
            {
                UpdateMenuPosition();
                UpdateSelection();
                UpdateVisuals();
            }
        }

        private void OpenMenu()
        {
            menuOpen = true;

            // Disable turn providers to prevent snap-turning during spell selection
            if (snapTurnProvider != null)
                snapTurnProvider.enabled = false;
            if (continuousTurnProvider != null)
                continuousTurnProvider.enabled = false;
            Debug.Log("[AlyxSpellMenu] Turn providers disabled");

            // IMPORTANT: Unparent menu so it's independent in world space
            // This prevents parent transforms from interfering with billboard rotation
            menuRoot.transform.SetParent(null);
            Debug.Log("[AlyxSpellMenu] ✓ Menu unparented - now independent in world space");

            // Lock menu position at hand location when it opens
            if (handTransform != null)
            {
                fixedMenuPosition = handTransform.position;
                menuRoot.transform.position = fixedMenuPosition;
                Debug.Log($"[AlyxSpellMenu] Menu position locked at: {fixedMenuPosition}");
            }
            else
            {
                Debug.LogError("[AlyxSpellMenu] handTransform is null!");
            }

            // Set initial rotation to face headset position (independent of head orientation)
            if (vrCameraTransform != null)
            {
                Vector3 directionToHead = (vrCameraTransform.position - fixedMenuPosition).normalized;
                if (directionToHead != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToHead, Vector3.up);
                    // Apply tilt toward player (rotate around the menu's local right axis)
                    Vector3 localRight = lookRotation * Vector3.right;
                    Quaternion tiltRotation = Quaternion.AngleAxis(menuTiltAngle, localRight);
                    menuRoot.transform.rotation = tiltRotation * lookRotation;
                    Debug.Log($"[AlyxSpellMenu] Initial rotation set to face headset position with {menuTiltAngle}° tilt");
                }
            }

            // Create spell icons
            CreateSpellIcons();

            // Show menu
            menuRoot.SetActive(true);
            Debug.Log($"[AlyxSpellMenu] Menu activated. menuRoot: {menuRoot.name}");

            // Set hand to summon-ready pose
            if (handPoseController != null)
            {
                handPoseController.SetPose(HandPoseState.SummonReady);
            }

            Debug.Log($"[AlyxSpellMenu] ✅ Menu opened on {(isLeftHand ? "LEFT" : "RIGHT")} hand with {spellIcons.Count} spells");
        }

        private void CreateSpellIcons()
        {
            // Clear existing icons
            foreach (var icon in spellIcons)
            {
                if (icon.gameObject != null)
                    Destroy(icon.gameObject);
            }
            spellIcons.Clear();

            if (SpellManager.Instance == null || SpellManager.Instance.availableSpells.Count == 0)
            {
                Debug.LogWarning("[AlyxSpellMenu] No spells available!");
                return;
            }

            int spellCount = SpellManager.Instance.availableSpells.Count;
            float angleStep = 360f / spellCount;

            for (int i = 0; i < spellCount; i++)
            {
                SpellData spell = SpellManager.Instance.availableSpells[i];

                // Calculate position in circle
                // Start at 90° (top) and go clockwise for intuitive compass layout
                // This ensures: 0=top, 1=right, 2=bottom, 3=left
                float angle = (90f - i * angleStep) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * iconRadius,
                    Mathf.Sin(angle) * iconRadius,
                    0f
                );

                // Create hollow glassy sphere container
                GameObject iconObj = new GameObject($"SpellIcon_{spell.spellName}");
                iconObj.transform.SetParent(menuRoot.transform);
                iconObj.transform.localPosition = offset;
                iconObj.transform.localScale = Vector3.one * iconSize;

                // Create outer hollow sphere
                GameObject outerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                outerSphere.name = "GlassSphere";
                outerSphere.transform.SetParent(iconObj.transform);
                outerSphere.transform.localPosition = Vector3.zero;
                outerSphere.transform.localScale = Vector3.one;

                // Remove collider
                Collider collider = outerSphere.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);

                // Create glassy translucent material
                MeshRenderer renderer = outerSphere.GetComponent<MeshRenderer>();
                Material glassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                // Translucent glass with spell color tint
                Color glassColor = spell.spellColor;
                glassColor.a = 0.3f; // Translucent
                glassMat.color = glassColor;
                glassMat.SetFloat("_Metallic", 0.1f);
                glassMat.SetFloat("_Smoothness", 0.95f); // Very smooth/glassy

                // Enable transparency (requires URP with alpha)
                glassMat.SetFloat("_Surface", 1); // Transparent
                glassMat.SetFloat("_Blend", 0); // Alpha blend
                glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                glassMat.SetInt("_ZWrite", 0);
                glassMat.renderQueue = 3000; // Transparent queue

                renderer.material = glassMat;

                // Add animated spell representation inside
                SpellIconAnimator animator = iconObj.AddComponent<SpellIconAnimator>();
                animator.spellData = spell;

                // Store icon data
                SpellIconObject iconData = new SpellIconObject
                {
                    gameObject = iconObj,
                    renderer = renderer, // Renderer of the glass sphere
                    spell = spell,
                    targetPosition = offset,
                    index = i
                };
                spellIcons.Add(iconData);
            }
        }

        private void UpdateMenuPosition()
        {
            if (menuRoot == null)
            {
                Debug.LogWarning("[AlyxSpellMenu] UpdateMenuPosition: menuRoot is null!");
                return;
            }

            // Keep position fixed at hand location
            menuRoot.transform.position = fixedMenuPosition;

            // Make menu face the headset's POSITION (not rotation)
            // This makes it independent of head tilt/orientation
            if (vrCameraTransform != null)
            {
                // Calculate direction from menu to headset
                Vector3 directionToHead = (vrCameraTransform.position - menuRoot.transform.position).normalized;

                // Create a rotation that looks at the headset, with up = world up (no roll)
                if (directionToHead != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToHead, Vector3.up);
                    // Apply tilt toward player (rotate around the menu's local right axis)
                    Vector3 localRight = lookRotation * Vector3.right;
                    Quaternion tiltRotation = Quaternion.AngleAxis(menuTiltAngle, localRight);
                    menuRoot.transform.rotation = tiltRotation * lookRotation;
                }
            }
        }

        private void UpdateSelection()
        {
            // Half-Life Alyx style: Selection based on hand position proximity
            if (handTransform == null || spellIcons.Count == 0)
            {
                hoveredIndex = -1;
                return;
            }

            Vector3 handPos = handTransform.position;
            float closestDistance = float.MaxValue;
            int closestIndex = -1;

            // Find which spell icon is closest to the hand
            for (int i = 0; i < spellIcons.Count; i++)
            {
                Vector3 iconWorldPos = spellIcons[i].gameObject.transform.position;
                float distance = Vector3.Distance(handPos, iconWorldPos);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            // Only hover if hand is within detection radius
            if (closestDistance <= hoverDetectionRadius)
            {
                hoveredIndex = closestIndex;
            }
            else
            {
                hoveredIndex = -1;
            }

            // Send haptic feedback when hovering over a new spell
            if (hoveredIndex != previousHoveredIndex && hoveredIndex >= 0)
            {
                if (deviceFound && device.isValid)
                {
                    // Light haptic pulse when entering hover state
                    device.SendHapticImpulse(0, 0.3f, 0.05f);
                    Debug.Log($"[AlyxSpellMenu] Haptic feedback for hovering spell {hoveredIndex}");
                }
            }

            previousHoveredIndex = hoveredIndex;
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < spellIcons.Count; i++)
            {
                SpellIconObject icon = spellIcons[i];
                bool isHovered = (i == hoveredIndex);

                // Scale whole icon based on hover
                float targetScale = isHovered ? (iconSize * highlightScale) : iconSize;
                icon.gameObject.transform.localScale = Vector3.Lerp(
                    icon.gameObject.transform.localScale,
                    Vector3.one * targetScale,
                    Time.deltaTime * 15f
                );

                // Update glass sphere material - glow when hovered
                if (icon.renderer != null)
                {
                    Material glassMat = icon.renderer.material;

                    if (isHovered)
                    {
                        // Brighter, less transparent when hovered
                        Color glowColor = highlightColor;
                        glowColor.a = 0.5f;
                        glassMat.color = Color.Lerp(glassMat.color, glowColor, Time.deltaTime * 10f);
                    }
                    else
                    {
                        // Normal translucent tint
                        Color normalColor = icon.spell.spellColor;
                        normalColor.a = 0.3f;
                        glassMat.color = Color.Lerp(glassMat.color, normalColor, Time.deltaTime * 10f);
                    }
                }
            }
        }

        private void SelectSpellAndClose()
        {
            // Select hovered spell
            if (hoveredIndex >= 0 && hoveredIndex < spellIcons.Count)
            {
                SpellData selectedSpell = spellIcons[hoveredIndex].spell;
                SpellManager.Instance?.SelectSpell(selectedSpell);
                Debug.Log($"[AlyxSpellMenu] Selected: {selectedSpell.spellName}");
            }

            CloseMenu();
        }

        private void CloseMenu()
        {
            menuOpen = false;
            hoveredIndex = -1;

            // Re-enable turn providers
            if (snapTurnProvider != null)
                snapTurnProvider.enabled = true;
            if (continuousTurnProvider != null)
                continuousTurnProvider.enabled = true;
            Debug.Log("[AlyxSpellMenu] Turn providers re-enabled");

            // Hide menu
            menuRoot.SetActive(false);

            // Re-parent menu back to controller for organization
            menuRoot.transform.SetParent(transform);
            menuRoot.transform.localPosition = Vector3.zero;
            menuRoot.transform.localRotation = Quaternion.identity;

            // Return hand to relaxed pose
            if (handPoseController != null)
            {
                handPoseController.SetPose(HandPoseState.Relaxed);
            }

            Debug.Log("[AlyxSpellMenu] Menu closed");
        }

        private void OnDestroy()
        {
            // Clean up icons
            foreach (var icon in spellIcons)
            {
                if (icon.gameObject != null)
                    Destroy(icon.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize menu position
            if (handTransform != null)
            {
                Vector3 menuPos = handTransform.position + handTransform.forward * menuDistance;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(menuPos, iconRadius);
            }
        }
    }
}

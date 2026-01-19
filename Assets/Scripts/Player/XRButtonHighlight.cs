using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Ensures buttons highlight properly when hovered by XR controllers
    /// Add this to any UI button that should respond to XR pointer hover
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class XRButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Button button;
        private Image buttonImage;
        private ColorBlock colors;
        private bool isHovering = false;

        void Start()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();

            if (button != null)
            {
                colors = button.colors;
                // Ensure transition mode is ColorTint
                button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button == null || !button.interactable)
                return;

            isHovering = true;

            // Force visual highlight
            if (buttonImage != null)
            {
                buttonImage.color = colors.highlightedColor;
            }

            Debug.Log($"[XRButtonHighlight] Pointer entered: {gameObject.name}, setting color to {colors.highlightedColor}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (button == null)
                return;

            isHovering = false;

            // Reset to normal color
            if (buttonImage != null)
            {
                buttonImage.color = colors.normalColor;
            }

            Debug.Log($"[XRButtonHighlight] Pointer exited: {gameObject.name}, resetting color to {colors.normalColor}");
        }

        void OnDisable()
        {
            isHovering = false;
            if (buttonImage != null)
            {
                buttonImage.color = colors.normalColor;
            }
        }
    }
}

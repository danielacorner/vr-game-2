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
        private ColorBlock originalColors;
        private bool isHovering = false;

        void Start()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                originalColors = button.colors;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button == null || !button.interactable)
                return;

            isHovering = true;

            // Force the button to highlighted state
            button.OnPointerEnter(eventData);

            Debug.Log($"[XRButtonHighlight] Pointer entered: {gameObject.name}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (button == null)
                return;

            isHovering = false;

            // Force the button back to normal state
            button.OnPointerExit(eventData);

            Debug.Log($"[XRButtonHighlight] Pointer exited: {gameObject.name}");
        }

        void OnDisable()
        {
            isHovering = false;
        }
    }
}

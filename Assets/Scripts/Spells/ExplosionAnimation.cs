using UnityEngine;

namespace VRDungeonCrawler.Spells
{
    /// <summary>
    /// Simple explosion sphere expansion animation
    /// Expands and fades out over time
    /// </summary>
    public class ExplosionAnimation : MonoBehaviour
    {
        [Tooltip("Maximum scale the explosion reaches")]
        public float maxScale = 5f;

        [Tooltip("Duration of the explosion animation in seconds")]
        public float duration = 0.5f;

        [Tooltip("Color of the spell explosion")]
        public Color spellColor = Color.red;

        private float startTime;
        private Material mat;

        void Start()
        {
            startTime = Time.time;
            mat = GetComponent<MeshRenderer>().material;
        }

        void Update()
        {
            float progress = (Time.time - startTime) / duration;

            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Expand
            float scale = Mathf.Lerp(0.1f, maxScale, progress);
            transform.localScale = Vector3.one * scale;

            // Fade out
            Color baseColor = spellColor;
            baseColor.a = 1f - progress;
            mat.SetColor("_BaseColor", baseColor);

            Color emissionColor = spellColor * 3f * (1f - progress);
            mat.SetColor("_EmissionColor", emissionColor);
        }
    }
}

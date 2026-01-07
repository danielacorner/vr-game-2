using UnityEngine;

namespace VRDungeonCrawler.Spells
{
    /// <summary>
    /// Simple component to spin the cyclone core
    /// Creates rotation effect for wind-based spells
    /// </summary>
    public class CycloneSpinner : MonoBehaviour
    {
        [Tooltip("Rotation speed in degrees per second")]
        public float spinSpeed = 360f;

        void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }
    }
}

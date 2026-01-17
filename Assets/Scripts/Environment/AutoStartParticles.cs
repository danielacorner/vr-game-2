using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Automatically starts a particle system on Awake
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class AutoStartParticles : MonoBehaviour
    {
        void Awake()
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
                Debug.Log($"[AutoStartParticles] Started particles on {gameObject.name}");
            }
        }
    }
}

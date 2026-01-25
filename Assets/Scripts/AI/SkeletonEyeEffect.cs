using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Controls skeleton eye effects - turns fiery red when aggro
    /// </summary>
    public class SkeletonEyeEffect : MonoBehaviour
    {
        [Header("Eye Colors")]
        public Color normalEyeColor = new Color(0.2f, 1f, 0.3f); // Eerie green
        public Color aggroEyeColor = new Color(1f, 0.1f, 0f); // Fiery red

        [Header("Fire Effect")]
        [Tooltip("Enable particle fire effect when aggro")]
        public bool enableFireParticles = true;

        [Tooltip("Number of fire particles")]
        public int fireParticleCount = 10;

        [Header("Debug")]
        public bool showDebug = false;

        private Transform leftEye;
        private Transform rightEye;
        private Material leftEyeMaterial;
        private Material rightEyeMaterial;
        private ParticleSystem leftEyeFire;
        private ParticleSystem rightEyeFire;
        private MonsterAI monsterAI;
        private bool isAggro = false;
        private bool wasAggro = false;

        void Start()
        {
            monsterAI = GetComponent<MonsterAI>();

            // Find eyes
            FindEyes();

            // Create fire particle systems
            if (enableFireParticles && leftEye != null && rightEye != null)
            {
                leftEyeFire = CreateEyeFireEffect(leftEye);
                rightEyeFire = CreateEyeFireEffect(rightEye);

                // Start with fire off
                if (leftEyeFire != null) leftEyeFire.Stop();
                if (rightEyeFire != null) rightEyeFire.Stop();
            }
        }

        void FindEyes()
        {
            // Eyes are in hierarchy: Head -> LeftSocket/RightSocket -> LeftEye/RightEye
            Transform head = transform.Find("Head");
            if (head != null)
            {
                Transform leftSocket = head.Find("LeftSocket");
                if (leftSocket != null)
                {
                    leftEye = leftSocket.Find("LeftEye");
                    if (leftEye != null)
                    {
                        Renderer renderer = leftEye.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            leftEyeMaterial = renderer.material; // Get instance
                        }
                    }
                }

                Transform rightSocket = head.Find("RightSocket");
                if (rightSocket != null)
                {
                    rightEye = rightSocket.Find("RightEye");
                    if (rightEye != null)
                    {
                        Renderer renderer = rightEye.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            rightEyeMaterial = renderer.material; // Get instance
                        }
                    }
                }
            }

            if (showDebug)
            {
                Debug.Log($"[SkeletonEyeEffect] Found eyes - Left: {leftEye != null}, Right: {rightEye != null}");
            }
        }

        ParticleSystem CreateEyeFireEffect(Transform eyeTransform)
        {
            GameObject fireObj = new GameObject($"{eyeTransform.name}_Fire");
            fireObj.transform.SetParent(eyeTransform);
            fireObj.transform.localPosition = Vector3.zero;
            fireObj.transform.localRotation = Quaternion.identity;

            ParticleSystem ps = fireObj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 0.5f;
            main.startSize = 0.03f;
            main.startColor = new Color(1f, 0.3f, 0f, 0.8f); // Orange-red fire
            main.gravityModifier = -0.2f; // Slight upward drift
            main.maxParticles = fireParticleCount;

            var emission = ps.emission;
            emission.rateOverTime = fireParticleCount * 2;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.02f;

            // Color over lifetime (fade to yellow/white at end)
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.2f, 0f), 0f), // Red-orange
                    new GradientColorKey(new Color(1f, 0.6f, 0f), 0.5f), // Orange
                    new GradientColorKey(new Color(1f, 1f, 0.5f), 1f) // Yellow-white
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Size over lifetime (shrink)
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0.2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            return ps;
        }

        void Update()
        {
            // Check aggro state
            if (monsterAI != null)
            {
                var field = typeof(MonsterAI).GetField("isAggro", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    isAggro = (bool)field.GetValue(monsterAI);
                }
            }

            // Update eye color based on aggro state
            if (isAggro != wasAggro)
            {
                // State changed
                if (isAggro)
                {
                    // Transition to aggro
                    SetEyeColor(aggroEyeColor);
                    if (enableFireParticles)
                    {
                        if (leftEyeFire != null) leftEyeFire.Play();
                        if (rightEyeFire != null) rightEyeFire.Play();
                    }

                    if (showDebug)
                        Debug.Log("[SkeletonEyeEffect] Eyes turned fiery red - aggro!");
                }
                else
                {
                    // Transition to normal
                    SetEyeColor(normalEyeColor);
                    if (enableFireParticles)
                    {
                        if (leftEyeFire != null) leftEyeFire.Stop();
                        if (rightEyeFire != null) rightEyeFire.Stop();
                    }

                    if (showDebug)
                        Debug.Log("[SkeletonEyeEffect] Eyes returned to normal green");
                }

                wasAggro = isAggro;
            }

            // Animate eye intensity when aggro (pulsing effect)
            if (isAggro)
            {
                float pulse = Mathf.Sin(Time.time * 8f) * 0.2f + 0.8f; // Pulse between 0.6 and 1.0
                Color pulsedColor = aggroEyeColor * pulse;
                SetEyeColor(pulsedColor);
            }
        }

        void SetEyeColor(Color color)
        {
            if (leftEyeMaterial != null)
            {
                leftEyeMaterial.color = color;
                if (leftEyeMaterial.HasProperty("_EmissionColor"))
                {
                    leftEyeMaterial.SetColor("_EmissionColor", color * 2f);
                }
            }

            if (rightEyeMaterial != null)
            {
                rightEyeMaterial.color = color;
                if (rightEyeMaterial.HasProperty("_EmissionColor"))
                {
                    rightEyeMaterial.SetColor("_EmissionColor", color * 2f);
                }
            }
        }

        void OnDestroy()
        {
            // Clean up material instances
            if (leftEyeMaterial != null)
                Destroy(leftEyeMaterial);
            if (rightEyeMaterial != null)
                Destroy(rightEyeMaterial);
        }
    }
}

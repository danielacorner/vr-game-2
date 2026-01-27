using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Procedural animation for Slime monsters
    /// Handles bouncing, jiggling, and squashing animations
    /// </summary>
    public class SlimeAnimator : MonoBehaviour
    {
        [Header("Animation Speeds")]
        [Tooltip("Speed of bounce cycle animation")]
        public float bounceCycleSpeed = 4f;

        [Tooltip("Speed of idle jiggle")]
        public float jiggleSpeed = 3f;

        [Tooltip("Speed of attack animation")]
        public float attackSpeed = 2.5f;

        [Header("Bounce Animation")]
        [Tooltip("How much slime stretches vertically during bounce")]
        public float bounceStretchAmount = 0.3f;

        [Tooltip("How much slime squashes horizontally during bounce")]
        public float bounceSquashAmount = 0.2f;

        [Tooltip("How high slime bounces off ground")]
        public float bounceHeight = 0.15f;

        [Header("Idle Animation")]
        [Tooltip("How much body jiggles during idle")]
        public float jiggleAmount = 0.05f;

        [Tooltip("How much eyes wander during idle")]
        public float eyeWanderAmount = 5f;

        [Tooltip("How much blob parts wobble")]
        public float blobWobbleAmount = 10f;

        [Header("Attack Animation")]
        [Tooltip("How much slime squashes during attack lunge")]
        public float attackSquashAmount = 0.5f;

        [Tooltip("How far slime lunges forward")]
        public float attackLungeDistance = 0.4f;

        [Header("Detection")]
        [Tooltip("Speed threshold to detect moving")]
        public float moveSpeedThreshold = 0.1f;

        [Tooltip("Enable debug logging")]
        public bool showDebug = false;

        // Bone references
        private Transform body;
        private Transform core;
        private Transform leftEyeBase;
        private Transform rightEyeBase;
        private Transform leftPupil;
        private Transform rightPupil;
        private Transform mouth;
        private Transform[] blobParts;

        // Original transforms
        private Vector3 bodyOriginalScale;
        private Vector3 bodyOriginalPos;
        private Quaternion bodyOriginalRot;
        private Quaternion leftEyeOriginal;
        private Quaternion rightEyeOriginal;
        private Quaternion[] blobPartOriginalRot;

        // Animation state
        private float bounceCycle = 0f;
        private float jiggleCycle = 0f;
        private float attackCycle = 0f;
        private bool isAttacking = false;
        private float attackCooldown = 0f;

        // Movement tracking
        private float currentSpeed;
        private Rigidbody rb;

        // AI reference
        private MonsterAI monsterAI;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            monsterAI = GetComponent<MonsterAI>();

            FindBones();
            StoreOriginalTransforms();

            if (showDebug)
                Debug.Log($"[SlimeAnimator] Initialized for {gameObject.name}");
        }

        void FindBones()
        {
            // Find bones by name (created by MonsterBuilder)
            body = transform.Find("Body");
            if (body != null)
            {
                core = body.Find("Core");
                leftEyeBase = body.Find("LeftEyeBase");
                rightEyeBase = body.Find("RightEyeBase");
                mouth = body.Find("Mouth");

                // Find pupils
                if (leftEyeBase != null) leftPupil = leftEyeBase.Find("LeftPupil");
                if (rightEyeBase != null) rightPupil = rightEyeBase.Find("RightPupil");

                // Find blob parts
                int blobPartCount = 3; // BlobPart0, BlobPart1, BlobPart2
                System.Collections.Generic.List<Transform> blobPartsList = new System.Collections.Generic.List<Transform>();
                for (int i = 0; i < blobPartCount; i++)
                {
                    Transform blobPart = body.Find($"BlobPart{i}");
                    if (blobPart != null)
                        blobPartsList.Add(blobPart);
                }
                blobParts = blobPartsList.ToArray();
            }

            if (showDebug)
            {
                Debug.Log($"[SlimeAnimator] Found bones: body={body != null}, core={core != null}, " +
                    $"leftEye={leftEyeBase != null}, rightEye={rightEyeBase != null}, " +
                    $"blobParts={blobParts.Length}");
            }
        }

        void StoreOriginalTransforms()
        {
            if (body != null)
            {
                bodyOriginalScale = body.localScale;
                bodyOriginalPos = body.localPosition;
                bodyOriginalRot = body.localRotation;
            }

            if (leftEyeBase != null) leftEyeOriginal = leftEyeBase.localRotation;
            if (rightEyeBase != null) rightEyeOriginal = rightEyeBase.localRotation;

            // Store blob part rotations
            if (blobParts != null && blobParts.Length > 0)
            {
                blobPartOriginalRot = new Quaternion[blobParts.Length];
                for (int i = 0; i < blobParts.Length; i++)
                {
                    if (blobParts[i] != null)
                        blobPartOriginalRot[i] = blobParts[i].localRotation;
                }
            }
        }

        void Update()
        {
            if (body == null) return;

            // Calculate current speed
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            currentSpeed = horizontalVelocity.magnitude;

            // Update attack cooldown
            if (attackCooldown > 0f)
                attackCooldown -= Time.deltaTime;

            // Check if we should trigger attack animation
            bool isAggro = monsterAI != null && monsterAI.IsAggro;
            if (isAggro && !isAttacking && attackCooldown <= 0f)
            {
                // Attack more frequently when aggro (10% chance per frame = attack every ~0.5 seconds)
                if (Random.value < 0.1f)
                {
                    StartAttack();
                }
            }

            // Animate based on current state
            if (isAttacking)
            {
                AnimateAttack();
            }
            else if (currentSpeed > moveSpeedThreshold)
            {
                AnimateBounce();
            }
            else
            {
                AnimateIdle();
            }
        }

        void AnimateBounce()
        {
            if (body == null) return;

            // Advance bounce cycle
            bounceCycle += Time.deltaTime * bounceCycleSpeed;

            // Bounce motion (sine wave)
            float bounceProgress = Mathf.Sin(bounceCycle);
            float bounceAbsolute = Mathf.Abs(bounceProgress);

            // Vertical stretch/squash (stretch at top, squash at bottom)
            float verticalScale = 1f + (bounceProgress * bounceStretchAmount);
            float horizontalScale = 1f - (bounceAbsolute * bounceSquashAmount);

            body.localScale = new Vector3(
                bodyOriginalScale.x * horizontalScale,
                bodyOriginalScale.y * verticalScale,
                bodyOriginalScale.z * horizontalScale
            );

            // Vertical bounce position
            float yOffset = bounceAbsolute * bounceHeight;
            body.localPosition = bodyOriginalPos + Vector3.up * yOffset;

            // Blob parts wobble during bounce
            if (blobParts != null)
            {
                for (int i = 0; i < blobParts.Length; i++)
                {
                    if (blobParts[i] == null) continue;

                    float phaseOffset = i * Mathf.PI * 0.5f;
                    float wobble = Mathf.Sin(bounceCycle * 2f + phaseOffset) * blobWobbleAmount;
                    blobParts[i].localRotation = blobPartOriginalRot[i] * Quaternion.Euler(wobble, wobble, wobble);
                }
            }

            // Eyes look in movement direction
            AnimateEyeTracking();
        }

        void AnimateIdle()
        {
            if (body == null) return;

            // Advance jiggle cycle
            jiggleCycle += Time.deltaTime * jiggleSpeed;

            // Gentle jiggle (multiple frequencies for organic look)
            float jiggleX = Mathf.Sin(jiggleCycle * 1.3f) * jiggleAmount;
            float jiggleY = Mathf.Sin(jiggleCycle * 1.7f) * jiggleAmount;
            float jiggleZ = Mathf.Sin(jiggleCycle * 1.1f) * jiggleAmount;

            body.localPosition = bodyOriginalPos + new Vector3(jiggleX, jiggleY, jiggleZ);

            // Subtle scale pulsing (breathing)
            float scalePulse = 1f + (Mathf.Sin(jiggleCycle * 0.7f) * 0.03f);
            body.localScale = bodyOriginalScale * scalePulse;

            // Blob parts wobble gently
            if (blobParts != null)
            {
                for (int i = 0; i < blobParts.Length; i++)
                {
                    if (blobParts[i] == null) continue;

                    float phaseOffset = i * Mathf.PI * 0.666f;
                    float wobble = Mathf.Sin(jiggleCycle + phaseOffset) * (blobWobbleAmount * 0.5f);
                    blobParts[i].localRotation = blobPartOriginalRot[i] * Quaternion.Euler(wobble, 0, wobble);
                }
            }

            // Eyes wander randomly
            AnimateEyeWander();
        }

        void AnimateEyeWander()
        {
            if (leftEyeBase != null)
            {
                float leftX = Mathf.Sin(jiggleCycle * 0.8f) * eyeWanderAmount;
                float leftY = Mathf.Cos(jiggleCycle * 0.6f) * eyeWanderAmount;
                leftEyeBase.localRotation = leftEyeOriginal * Quaternion.Euler(leftY, leftX, 0);
            }

            if (rightEyeBase != null)
            {
                float rightX = Mathf.Sin(jiggleCycle * 0.7f + Mathf.PI * 0.3f) * eyeWanderAmount;
                float rightY = Mathf.Cos(jiggleCycle * 0.9f) * eyeWanderAmount;
                rightEyeBase.localRotation = rightEyeOriginal * Quaternion.Euler(rightY, rightX, 0);
            }
        }

        void AnimateEyeTracking()
        {
            // Both eyes look in velocity direction
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                Vector3 lookDir = rb.linearVelocity.normalized;
                float angle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;

                if (leftEyeBase != null)
                    leftEyeBase.localRotation = leftEyeOriginal * Quaternion.Euler(0, angle * 0.3f, 0);

                if (rightEyeBase != null)
                    rightEyeBase.localRotation = rightEyeOriginal * Quaternion.Euler(0, angle * 0.3f, 0);
            }
        }

        void StartAttack()
        {
            isAttacking = true;
            attackCycle = 0f;
            attackCooldown = 1.5f; // 1.5 second cooldown between attacks

            if (showDebug)
                Debug.Log($"[SlimeAnimator] {gameObject.name} starting attack!");
        }

        void AnimateAttack()
        {
            if (body == null) return;

            // Advance attack cycle
            attackCycle += Time.deltaTime * attackSpeed;

            // Attack animation lasts ~1 second
            float attackProgress = attackCycle / 1f;

            if (attackProgress >= 1f)
            {
                // Attack finished - restore original transforms
                isAttacking = false;
                body.localScale = bodyOriginalScale;
                body.localPosition = bodyOriginalPos;
                return;
            }

            // Windup and lunge (first 60% is windup, last 40% is lunge)
            if (attackProgress < 0.6f)
            {
                // Windup: squash down and pull back
                float windupProgress = attackProgress / 0.6f;
                float squash = Mathf.Lerp(1f, 1f - attackSquashAmount, windupProgress);
                float stretch = Mathf.Lerp(1f, 1f + attackSquashAmount * 0.5f, windupProgress);

                body.localScale = new Vector3(
                    bodyOriginalScale.x * stretch,
                    bodyOriginalScale.y * squash,
                    bodyOriginalScale.z * stretch
                );

                // Pull back slightly
                body.localPosition = bodyOriginalPos - Vector3.forward * (windupProgress * 0.1f);
            }
            else
            {
                // Lunge: stretch forward and return
                float lungeProgress = (attackProgress - 0.6f) / 0.4f;

                // First half of lunge: stretch forward
                if (lungeProgress < 0.5f)
                {
                    float stretchAmount = lungeProgress * 2f;
                    body.localScale = new Vector3(
                        bodyOriginalScale.x * (1f - stretchAmount * 0.3f),
                        bodyOriginalScale.y * (1f + stretchAmount * 0.5f),
                        bodyOriginalScale.z * (1f - stretchAmount * 0.3f)
                    );

                    body.localPosition = bodyOriginalPos + Vector3.forward * (stretchAmount * attackLungeDistance);
                }
                else
                {
                    // Second half: return to normal
                    float returnProgress = (lungeProgress - 0.5f) * 2f;
                    body.localScale = Vector3.Lerp(
                        new Vector3(bodyOriginalScale.x * 0.7f, bodyOriginalScale.y * 1.5f, bodyOriginalScale.z * 0.7f),
                        bodyOriginalScale,
                        returnProgress
                    );

                    body.localPosition = Vector3.Lerp(
                        bodyOriginalPos + Vector3.forward * attackLungeDistance,
                        bodyOriginalPos,
                        returnProgress
                    );
                }
            }

            // Blob parts go crazy during attack
            if (blobParts != null)
            {
                for (int i = 0; i < blobParts.Length; i++)
                {
                    if (blobParts[i] == null) continue;

                    float phaseOffset = i * Mathf.PI * 0.5f;
                    float wobble = Mathf.Sin(attackCycle * 10f + phaseOffset) * blobWobbleAmount * 2f;
                    blobParts[i].localRotation = blobPartOriginalRot[i] * Quaternion.Euler(wobble, wobble, wobble);
                }
            }
        }
    }
}

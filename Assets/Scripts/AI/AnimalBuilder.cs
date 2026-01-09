using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Procedurally builds realistic-looking animals with proper colors
    /// Creates rabbits, squirrels, and birds with realistic body parts and textures
    /// Each animal has distinct colors for body, face, ears, nose, etc.
    /// </summary>
    public static class AnimalBuilder
    {
        /// <summary>
        /// Creates a rabbit with realistic colors (brown/gray fur, pink nose, white tail)
        /// </summary>
        public static GameObject CreateRabbit(Transform parent = null)
        {
            GameObject rabbit = new GameObject("Rabbit");
            if (parent != null)
                rabbit.transform.SetParent(parent);

            // === BODY (realistic brown/tan fur) ===
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(rabbit.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.3f, 0.25f, 0.35f);
            ApplyMaterial(body, new Color(0.7f, 0.55f, 0.4f)); // Warm tan-brown fur

            // === UNDERBELLY (lighter cream color) ===
            GameObject belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.7f, 0.3f);
            belly.transform.localScale = new Vector3(0.85f, 0.6f, 0.7f);
            ApplyMaterial(belly, new Color(0.95f, 0.9f, 0.85f)); // Cream belly

            // === HEAD ===
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(rabbit.transform);
            head.transform.localPosition = new Vector3(0f, 0.1f, 0.25f);
            head.transform.localScale = new Vector3(0.22f, 0.2f, 0.22f);
            ApplyMaterial(head, new Color(0.72f, 0.57f, 0.42f)); // Slightly lighter brown

            // === FACE PATCH (lighter around face) ===
            GameObject facePatch = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            facePatch.name = "FacePatch";
            facePatch.transform.SetParent(head.transform);
            facePatch.transform.localPosition = new Vector3(0f, -0.2f, 0.5f);
            facePatch.transform.localScale = new Vector3(0.8f, 0.6f, 0.7f);
            ApplyMaterial(facePatch, new Color(0.9f, 0.85f, 0.75f)); // Light tan face

            // === NOSE (pink) ===
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.3f, 0.7f);
            nose.transform.localScale = new Vector3(0.2f, 0.15f, 0.2f);
            ApplyMaterial(nose, new Color(1f, 0.75f, 0.8f)); // Pink nose

            // === LEFT EAR (long rabbit ears) ===
            GameObject leftEar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.4f, 0.8f, 0f);
            leftEar.transform.localRotation = Quaternion.Euler(-10f, 0f, -15f);
            leftEar.transform.localScale = new Vector3(0.15f, 0.5f, 0.1f);
            ApplyMaterial(leftEar, new Color(0.6f, 0.5f, 0.4f)); // Same as body

            // === RIGHT EAR ===
            GameObject rightEar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.4f, 0.8f, 0f);
            rightEar.transform.localRotation = Quaternion.Euler(-10f, 0f, 15f);
            rightEar.transform.localScale = new Vector3(0.15f, 0.5f, 0.1f);
            ApplyMaterial(rightEar, new Color(0.6f, 0.5f, 0.4f));

            // === EYES (black) ===
            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.35f, 0.15f, 0.6f);
            leftEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(leftEye, Color.black);

            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.35f, 0.15f, 0.6f);
            rightEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(rightEye, Color.black);

            // === TAIL (white fluffy) ===
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tail.name = "Tail";
            tail.transform.SetParent(rabbit.transform);
            tail.transform.localPosition = new Vector3(0f, 0.05f, -0.3f);
            tail.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(tail, new Color(0.95f, 0.95f, 0.95f)); // White tail

            // === LEGS ===
            CreateRabbitLeg(rabbit.transform, "FrontLeftLeg", new Vector3(-0.1f, -0.15f, 0.12f));
            CreateRabbitLeg(rabbit.transform, "FrontRightLeg", new Vector3(0.1f, -0.15f, 0.12f));
            CreateRabbitLeg(rabbit.transform, "BackLeftLeg", new Vector3(-0.1f, -0.15f, -0.08f), true);
            CreateRabbitLeg(rabbit.transform, "BackRightLeg", new Vector3(0.1f, -0.15f, -0.08f), true);

            return rabbit;
        }

        static void CreateRabbitLeg(Transform parent, string name, Vector3 position, bool isBackLeg = false)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = isBackLeg ? new Vector3(0.08f, 0.15f, 0.08f) : new Vector3(0.06f, 0.1f, 0.06f);
            ApplyMaterial(leg, new Color(0.58f, 0.48f, 0.38f)); // Darker brown for legs

            // Foot (wider at bottom for back legs)
            if (isBackLeg)
            {
                GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foot.name = "Foot";
                foot.transform.SetParent(leg.transform);
                foot.transform.localPosition = new Vector3(0f, -0.7f, 0.3f);
                foot.transform.localScale = new Vector3(1.2f, 0.5f, 1.5f);
                ApplyMaterial(foot, new Color(0.58f, 0.48f, 0.38f));
            }
        }

        /// <summary>
        /// Creates a squirrel with realistic colors (reddish-brown fur, bushy tail)
        /// </summary>
        public static GameObject CreateSquirrel(Transform parent = null)
        {
            GameObject squirrel = new GameObject("Squirrel");
            if (parent != null)
                squirrel.transform.SetParent(parent);

            // === BODY (realistic reddish-brown fur) ===
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(squirrel.transform);
            body.transform.localPosition = new Vector3(0f, 0f, 0f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            body.transform.localScale = new Vector3(0.15f, 0.2f, 0.15f);
            ApplyMaterial(body, new Color(0.8f, 0.45f, 0.25f)); // Rich reddish-brown

            // === UNDERBELLY (white/cream) ===
            GameObject belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            belly.transform.localScale = new Vector3(0.8f, 0.5f, 0.9f);
            ApplyMaterial(belly, new Color(0.98f, 0.95f, 0.9f)); // Off-white belly

            // === HEAD ===
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(squirrel.transform);
            head.transform.localPosition = new Vector3(0f, 0.05f, 0.22f);
            head.transform.localScale = new Vector3(0.18f, 0.16f, 0.18f);
            ApplyMaterial(head, new Color(0.82f, 0.47f, 0.27f)); // Slightly lighter reddish-brown

            // === CHEEK PATCHES (lighter) ===
            GameObject leftCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftCheek.name = "LeftCheek";
            leftCheek.transform.SetParent(head.transform);
            leftCheek.transform.localPosition = new Vector3(-0.6f, -0.1f, 0.4f);
            leftCheek.transform.localScale = new Vector3(0.4f, 0.35f, 0.4f);
            ApplyMaterial(leftCheek, new Color(0.95f, 0.9f, 0.85f));

            GameObject rightCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightCheek.name = "RightCheek";
            rightCheek.transform.SetParent(head.transform);
            rightCheek.transform.localPosition = new Vector3(0.6f, -0.1f, 0.4f);
            rightCheek.transform.localScale = new Vector3(0.4f, 0.35f, 0.4f);
            ApplyMaterial(rightCheek, new Color(0.95f, 0.9f, 0.85f));

            // === NOSE (pink/brown) ===
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.2f, 0.75f);
            nose.transform.localScale = Vector3.one * 0.18f;
            ApplyMaterial(nose, new Color(0.5f, 0.3f, 0.2f)); // Dark brown nose

            // === EARS (rounded) ===
            GameObject leftEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.45f, 0.6f, 0.1f);
            leftEar.transform.localScale = new Vector3(0.3f, 0.35f, 0.2f);
            ApplyMaterial(leftEar, new Color(0.7f, 0.4f, 0.2f));

            GameObject rightEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.45f, 0.6f, 0.1f);
            rightEar.transform.localScale = new Vector3(0.3f, 0.35f, 0.2f);
            ApplyMaterial(rightEar, new Color(0.7f, 0.4f, 0.2f));

            // === EYES ===
            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.3f, 0.1f, 0.65f);
            leftEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(leftEye, Color.black);

            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.3f, 0.1f, 0.65f);
            rightEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(rightEye, Color.black);

            // === BUSHY TAIL (reddish-brown, large) ===
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tail.name = "Tail";
            tail.transform.SetParent(squirrel.transform);
            tail.transform.localPosition = new Vector3(0f, 0.15f, -0.25f);
            tail.transform.localScale = new Vector3(0.2f, 0.35f, 0.2f);
            ApplyMaterial(tail, new Color(0.75f, 0.45f, 0.25f)); // Lighter reddish-brown

            // === LEGS (small, agile) ===
            CreateSquirrelLeg(squirrel.transform, "FrontLeftLeg", new Vector3(-0.08f, -0.12f, 0.08f));
            CreateSquirrelLeg(squirrel.transform, "FrontRightLeg", new Vector3(0.08f, -0.12f, 0.08f));
            CreateSquirrelLeg(squirrel.transform, "BackLeftLeg", new Vector3(-0.08f, -0.12f, -0.06f));
            CreateSquirrelLeg(squirrel.transform, "BackRightLeg", new Vector3(0.08f, -0.12f, -0.06f));

            return squirrel;
        }

        static void CreateSquirrelLeg(Transform parent, string name, Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.05f, 0.08f, 0.05f);
            ApplyMaterial(leg, new Color(0.68f, 0.38f, 0.18f));
        }

        /// <summary>
        /// Creates a bird with realistic colors (blue/brown body, yellow beak)
        /// </summary>
        public static GameObject CreateBird(Transform parent = null)
        {
            GameObject bird = new GameObject("Bird");
            if (parent != null)
                bird.transform.SetParent(parent);

            // === BODY (blue/gray) ===
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(bird.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.2f, 0.18f, 0.25f);
            ApplyMaterial(body, new Color(0.4f, 0.5f, 0.7f)); // Blue-gray

            // === CHEST (lighter color) ===
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            chest.name = "Chest";
            chest.transform.SetParent(body.transform);
            chest.transform.localPosition = new Vector3(0f, -0.3f, 0.6f);
            chest.transform.localScale = new Vector3(0.9f, 0.7f, 0.6f);
            ApplyMaterial(chest, new Color(0.9f, 0.85f, 0.7f)); // Cream chest

            // === HEAD ===
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(bird.transform);
            head.transform.localPosition = new Vector3(0f, 0.08f, 0.18f);
            head.transform.localScale = new Vector3(0.16f, 0.15f, 0.16f);
            ApplyMaterial(head, new Color(0.42f, 0.52f, 0.72f));

            // === CROWN/FOREHEAD (slightly darker blue-gray) ===
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.SetParent(head.transform);
            crown.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            crown.transform.localScale = new Vector3(0.9f, 0.5f, 0.8f);
            ApplyMaterial(crown, new Color(0.38f, 0.48f, 0.68f)); // Slightly darker blue

            // === BEAK (yellow/orange) ===
            GameObject beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beak.name = "Beak";
            beak.transform.SetParent(head.transform);
            beak.transform.localPosition = new Vector3(0f, -0.15f, 0.9f);
            beak.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            beak.transform.localScale = new Vector3(0.12f, 0.15f, 0.2f);
            ApplyMaterial(beak, new Color(1f, 0.8f, 0.2f)); // Orange-yellow beak

            // === EYES with white rings (common in birds) ===
            // Left eye ring (white/cream)
            GameObject leftEyeRing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEyeRing.name = "LeftEyeRing";
            leftEyeRing.transform.SetParent(head.transform);
            leftEyeRing.transform.localPosition = new Vector3(-0.4f, 0.1f, 0.5f);
            leftEyeRing.transform.localScale = Vector3.one * 0.2f;
            ApplyMaterial(leftEyeRing, new Color(0.95f, 0.93f, 0.88f)); // Off-white ring

            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(leftEyeRing.transform);
            leftEye.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            leftEye.transform.localScale = Vector3.one * 0.6f;
            ApplyMaterial(leftEye, Color.black);

            // Right eye ring (white/cream)
            GameObject rightEyeRing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEyeRing.name = "RightEyeRing";
            rightEyeRing.transform.SetParent(head.transform);
            rightEyeRing.transform.localPosition = new Vector3(0.4f, 0.1f, 0.5f);
            rightEyeRing.transform.localScale = Vector3.one * 0.2f;
            ApplyMaterial(rightEyeRing, new Color(0.95f, 0.93f, 0.88f)); // Off-white ring

            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(rightEyeRing.transform);
            rightEye.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            rightEye.transform.localScale = Vector3.one * 0.6f;
            ApplyMaterial(rightEye, Color.black);

            // === WINGS (blue with darker tips) ===
            GameObject leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWing.name = "LeftWing";
            leftWing.transform.SetParent(bird.transform);
            leftWing.transform.localPosition = new Vector3(-0.15f, 0f, 0f);
            leftWing.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            leftWing.transform.localScale = new Vector3(0.3f, 0.05f, 0.2f);
            ApplyMaterial(leftWing, new Color(0.35f, 0.45f, 0.65f)); // Darker blue

            // Left wing tip (even darker, like primary feathers)
            GameObject leftWingTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWingTip.name = "LeftWingTip";
            leftWingTip.transform.SetParent(leftWing.transform);
            leftWingTip.transform.localPosition = new Vector3(-0.65f, 0f, 0f);
            leftWingTip.transform.localScale = new Vector3(0.4f, 1.05f, 0.9f);
            ApplyMaterial(leftWingTip, new Color(0.25f, 0.32f, 0.48f)); // Very dark blue-gray

            GameObject rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWing.name = "RightWing";
            rightWing.transform.SetParent(bird.transform);
            rightWing.transform.localPosition = new Vector3(0.15f, 0f, 0f);
            rightWing.transform.localRotation = Quaternion.Euler(0f, 0f, 20f);
            rightWing.transform.localScale = new Vector3(0.3f, 0.05f, 0.2f);
            ApplyMaterial(rightWing, new Color(0.35f, 0.45f, 0.65f));

            // Right wing tip (even darker, like primary feathers)
            GameObject rightWingTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWingTip.name = "RightWingTip";
            rightWingTip.transform.SetParent(rightWing.transform);
            rightWingTip.transform.localPosition = new Vector3(0.65f, 0f, 0f);
            rightWingTip.transform.localScale = new Vector3(0.4f, 1.05f, 0.9f);
            ApplyMaterial(rightWingTip, new Color(0.25f, 0.32f, 0.48f)); // Very dark blue-gray

            // === TAIL FEATHERS (with darker tip) ===
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.name = "Tail";
            tail.transform.SetParent(bird.transform);
            tail.transform.localPosition = new Vector3(0f, 0.02f, -0.2f);
            tail.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.15f, 0.05f, 0.2f);
            ApplyMaterial(tail, new Color(0.3f, 0.4f, 0.6f));

            // Tail tip (darker feathers)
            GameObject tailTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailTip.name = "TailTip";
            tailTip.transform.SetParent(tail.transform);
            tailTip.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            tailTip.transform.localScale = new Vector3(0.95f, 1.05f, 0.5f);
            ApplyMaterial(tailTip, new Color(0.22f, 0.3f, 0.45f)); // Dark blue-gray tip

            // === LEGS (thin) ===
            CreateBirdLeg(bird.transform, "LeftLeg", new Vector3(-0.06f, -0.15f, 0f));
            CreateBirdLeg(bird.transform, "RightLeg", new Vector3(0.06f, -0.15f, 0f));

            return bird;
        }

        static void CreateBirdLeg(Transform parent, string name, Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.02f, 0.08f, 0.02f);
            ApplyMaterial(leg, new Color(0.8f, 0.6f, 0.3f)); // Orange-brown legs
        }

        static void ApplyMaterial(GameObject obj, Color color)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                mat.SetFloat("_Smoothness", 0.3f); // Matte finish for fur/feathers
                renderer.material = mat;
            }

            // Remove collider (will be added at root)
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }
        }
    }
}

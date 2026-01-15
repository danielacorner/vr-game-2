using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Procedurally builds Polytopia-style angular dungeon monsters
    /// Creates goblins, skeletons, and slimes using geometric cube/box shapes
    /// All monsters use sharp angular forms for consistent Polytopia aesthetic
    /// </summary>
    public static class MonsterBuilder
    {
        /// <summary>
        /// Creates a goblin monster (small, green, aggressive-looking)
        /// HP: 6, Fast movement
        /// </summary>
        public static GameObject CreateGoblin(Transform parent = null)
        {
            GameObject goblin = new GameObject("Goblin");
            if (parent != null)
                goblin.transform.SetParent(parent);

            // === BODY (green, small, hunched) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateTrapezoid(0.8f));
            body.name = "Body";
            body.transform.SetParent(goblin.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.25f, 0.3f, 0.2f);
            ApplyMaterial(body, new Color(0.3f, 0.6f, 0.2f)); // Dark green

            // === HEAD (large, angular cube) ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            head.name = "Head";
            head.transform.SetParent(goblin.transform);
            head.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            head.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            ApplyMaterial(head, new Color(0.35f, 0.65f, 0.25f)); // Lighter green

            // === EYES (yellow, glowing) ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.25f, 0.1f, 0.45f);
            leftEye.transform.localScale = new Vector3(0.25f, 0.3f, 0.15f);
            ApplyMaterial(leftEye, new Color(1f, 0.9f, 0.2f)); // Yellow eyes

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.25f, 0.1f, 0.45f);
            rightEye.transform.localScale = new Vector3(0.25f, 0.3f, 0.15f);
            ApplyMaterial(rightEye, new Color(1f, 0.9f, 0.2f));

            // === PUPILS (black) ===
            GameObject leftPupil = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftPupil.name = "LeftPupil";
            leftPupil.transform.SetParent(leftEye.transform);
            leftPupil.transform.localPosition = new Vector3(0f, 0f, 0.6f);
            leftPupil.transform.localScale = new Vector3(0.5f, 0.5f, 0.3f);
            ApplyMaterial(leftPupil, Color.black);

            GameObject rightPupil = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightPupil.name = "RightPupil";
            rightPupil.transform.SetParent(rightEye.transform);
            rightPupil.transform.localPosition = new Vector3(0f, 0f, 0.6f);
            rightPupil.transform.localScale = new Vector3(0.5f, 0.5f, 0.3f);
            ApplyMaterial(rightPupil, Color.black);

            // === EARS (pointed, angular) ===
            GameObject leftEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateWedge());
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.6f, 0.3f, 0f);
            leftEar.transform.localRotation = Quaternion.Euler(0f, 90f, 45f);
            leftEar.transform.localScale = new Vector3(0.2f, 0.35f, 0.15f);
            ApplyMaterial(leftEar, new Color(0.32f, 0.62f, 0.22f));

            GameObject rightEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateWedge());
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.6f, 0.3f, 0f);
            rightEar.transform.localRotation = Quaternion.Euler(0f, -90f, -45f);
            rightEar.transform.localScale = new Vector3(0.2f, 0.35f, 0.15f);
            ApplyMaterial(rightEar, new Color(0.32f, 0.62f, 0.22f));

            // === MOUTH (dark rectangle) ===
            GameObject mouth = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            mouth.name = "Mouth";
            mouth.transform.SetParent(head.transform);
            mouth.transform.localPosition = new Vector3(0f, -0.25f, 0.48f);
            mouth.transform.localScale = new Vector3(0.4f, 0.15f, 0.08f);
            ApplyMaterial(mouth, new Color(0.1f, 0.1f, 0.1f));

            // === ARMS (thin, angular) ===
            CreateGoblinArm(goblin.transform, "LeftArm", new Vector3(-0.18f, 0.05f, 0f), -1);
            CreateGoblinArm(goblin.transform, "RightArm", new Vector3(0.18f, 0.05f, 0f), 1);

            // === LEGS (short, stubby) ===
            CreateGoblinLeg(goblin.transform, "LeftLeg", new Vector3(-0.08f, -0.22f, 0f));
            CreateGoblinLeg(goblin.transform, "RightLeg", new Vector3(0.08f, -0.22f, 0f));

            return goblin;
        }

        static void CreateGoblinArm(Transform parent, string name, Vector3 position, int side)
        {
            GameObject arm = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            arm.name = name;
            arm.transform.SetParent(parent);
            arm.transform.localPosition = position;
            arm.transform.localScale = new Vector3(0.06f, 0.2f, 0.06f);
            ApplyMaterial(arm, new Color(0.3f, 0.55f, 0.2f));

            // Hand
            GameObject hand = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            hand.name = "Hand";
            hand.transform.SetParent(arm.transform);
            hand.transform.localPosition = new Vector3(0f, -0.7f, 0f);
            hand.transform.localScale = new Vector3(1.5f, 0.4f, 1.5f);
            ApplyMaterial(hand, new Color(0.28f, 0.5f, 0.18f));
        }

        static void CreateGoblinLeg(Transform parent, string name, Vector3 position)
        {
            GameObject leg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
            ApplyMaterial(leg, new Color(0.25f, 0.5f, 0.15f));

            // Foot
            GameObject foot = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            foot.name = "Foot";
            foot.transform.SetParent(leg.transform);
            foot.transform.localPosition = new Vector3(0f, -0.8f, 0.3f);
            foot.transform.localScale = new Vector3(1.2f, 0.4f, 1.8f);
            ApplyMaterial(foot, new Color(0.2f, 0.45f, 0.1f));
        }

        /// <summary>
        /// Creates a skeleton monster (bony, white/gray, angular)
        /// HP: 10, Wandering movement
        /// </summary>
        public static GameObject CreateSkeleton(Transform parent = null)
        {
            GameObject skeleton = new GameObject("Skeleton");
            if (parent != null)
                skeleton.transform.SetParent(parent);

            // === BODY (rib cage - angular trapezoid) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateTrapezoid(0.7f));
            body.name = "Body";
            body.transform.SetParent(skeleton.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.3f, 0.35f, 0.2f);
            ApplyMaterial(body, new Color(0.95f, 0.95f, 0.9f)); // Bone white

            // === RIBS (horizontal lines) ===
            for (int i = 0; i < 3; i++)
            {
                GameObject rib = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
                rib.name = $"Rib{i}";
                rib.transform.SetParent(body.transform);
                rib.transform.localPosition = new Vector3(0f, 0.2f - i * 0.3f, 0.55f);
                rib.transform.localScale = new Vector3(0.9f, 0.08f, 0.08f);
                ApplyMaterial(rib, new Color(0.85f, 0.85f, 0.8f)); // Slightly darker
            }

            // === SKULL (angular cube) ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            head.name = "Head";
            head.transform.SetParent(skeleton.transform);
            head.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            head.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);
            ApplyMaterial(head, new Color(0.98f, 0.98f, 0.95f));

            // === EYE SOCKETS (black, deep) ===
            GameObject leftSocket = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftSocket.name = "LeftSocket";
            leftSocket.transform.SetParent(head.transform);
            leftSocket.transform.localPosition = new Vector3(-0.3f, 0.15f, 0.45f);
            leftSocket.transform.localScale = new Vector3(0.35f, 0.4f, 0.2f);
            ApplyMaterial(leftSocket, Color.black);

            GameObject rightSocket = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightSocket.name = "RightSocket";
            rightSocket.transform.SetParent(head.transform);
            rightSocket.transform.localPosition = new Vector3(0.3f, 0.15f, 0.45f);
            rightSocket.transform.localScale = new Vector3(0.35f, 0.4f, 0.2f);
            ApplyMaterial(rightSocket, Color.black);

            // === GLOWING EYES (eerie green) ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(leftSocket.transform);
            leftEye.transform.localPosition = new Vector3(0f, 0f, 0.6f);
            leftEye.transform.localScale = new Vector3(0.5f, 0.5f, 0.3f);
            ApplyMaterial(leftEye, new Color(0.2f, 1f, 0.3f)); // Eerie green glow

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(rightSocket.transform);
            rightEye.transform.localPosition = new Vector3(0f, 0f, 0.6f);
            rightEye.transform.localScale = new Vector3(0.5f, 0.5f, 0.3f);
            ApplyMaterial(rightEye, new Color(0.2f, 1f, 0.3f));

            // === JAW (angular) ===
            GameObject jaw = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            jaw.name = "Jaw";
            jaw.transform.SetParent(head.transform);
            jaw.transform.localPosition = new Vector3(0f, -0.55f, 0.25f);
            jaw.transform.localScale = new Vector3(0.8f, 0.2f, 0.6f);
            ApplyMaterial(jaw, new Color(0.9f, 0.9f, 0.85f));

            // === TEETH (small cubes) ===
            for (int i = 0; i < 4; i++)
            {
                GameObject tooth = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
                tooth.name = $"Tooth{i}";
                tooth.transform.SetParent(jaw.transform);
                tooth.transform.localPosition = new Vector3(-0.3f + i * 0.2f, 0.6f, 0.7f);
                tooth.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
                ApplyMaterial(tooth, Color.white);
            }

            // === BONE ARMS (thin, angular) ===
            CreateSkeletonArm(skeleton.transform, "LeftArm", new Vector3(-0.2f, 0.1f, 0f), -1);
            CreateSkeletonArm(skeleton.transform, "RightArm", new Vector3(0.2f, 0.1f, 0f), 1);

            // === BONE LEGS (thin) ===
            CreateSkeletonLeg(skeleton.transform, "LeftLeg", new Vector3(-0.1f, -0.25f, 0f));
            CreateSkeletonLeg(skeleton.transform, "RightLeg", new Vector3(0.1f, -0.25f, 0f));

            return skeleton;
        }

        static void CreateSkeletonArm(Transform parent, string name, Vector3 position, int side)
        {
            // Upper arm
            GameObject upperArm = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            upperArm.name = name + "Upper";
            upperArm.transform.SetParent(parent);
            upperArm.transform.localPosition = position;
            upperArm.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
            ApplyMaterial(upperArm, new Color(0.95f, 0.95f, 0.9f));

            // Lower arm
            GameObject lowerArm = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            lowerArm.name = name + "Lower";
            lowerArm.transform.SetParent(upperArm.transform);
            lowerArm.transform.localPosition = new Vector3(0f, -1.1f, 0f);
            lowerArm.transform.localScale = new Vector3(1f, 0.9f, 1f);
            ApplyMaterial(lowerArm, new Color(0.9f, 0.9f, 0.85f));

            // Bony hand
            GameObject hand = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            hand.name = "Hand";
            hand.transform.SetParent(lowerArm.transform);
            hand.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            hand.transform.localScale = new Vector3(1.5f, 0.4f, 1.5f);
            ApplyMaterial(hand, new Color(0.88f, 0.88f, 0.83f));
        }

        static void CreateSkeletonLeg(Transform parent, string name, Vector3 position)
        {
            // Upper leg
            GameObject upperLeg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            upperLeg.name = name + "Upper";
            upperLeg.transform.SetParent(parent);
            upperLeg.transform.localPosition = position;
            upperLeg.transform.localScale = new Vector3(0.06f, 0.2f, 0.06f);
            ApplyMaterial(upperLeg, new Color(0.95f, 0.95f, 0.9f));

            // Lower leg
            GameObject lowerLeg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            lowerLeg.name = name + "Lower";
            lowerLeg.transform.SetParent(upperLeg.transform);
            lowerLeg.transform.localPosition = new Vector3(0f, -1.1f, 0f);
            lowerLeg.transform.localScale = new Vector3(1f, 0.9f, 1f);
            ApplyMaterial(lowerLeg, new Color(0.9f, 0.9f, 0.85f));

            // Foot
            GameObject foot = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            foot.name = "Foot";
            foot.transform.SetParent(lowerLeg.transform);
            foot.transform.localPosition = new Vector3(0f, -0.8f, 0.3f);
            foot.transform.localScale = new Vector3(1.3f, 0.3f, 1.8f);
            ApplyMaterial(foot, new Color(0.88f, 0.88f, 0.83f));
        }

        /// <summary>
        /// Creates a slime monster (blob, translucent green, bouncy)
        /// HP: 8, Bouncing movement
        /// </summary>
        public static GameObject CreateSlime(Transform parent = null)
        {
            GameObject slime = new GameObject("Slime");
            if (parent != null)
                slime.transform.SetParent(parent);

            // === MAIN BODY (large angular blob) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateTrapezoid(0.6f));
            body.name = "Body";
            body.transform.SetParent(slime.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.35f, 0.3f, 0.35f);
            ApplyMaterial(body, new Color(0.3f, 0.9f, 0.4f), 0.6f); // Translucent green

            // === INNER CORE (darker, visible through body) ===
            GameObject core = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            core.name = "Core";
            core.transform.SetParent(body.transform);
            core.transform.localPosition = new Vector3(0f, 0f, 0f);
            core.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            ApplyMaterial(core, new Color(0.2f, 0.7f, 0.3f), 0.8f); // Darker translucent

            // === EYES (large, googly) ===
            GameObject leftEyeBase = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEyeBase.name = "LeftEyeBase";
            leftEyeBase.transform.SetParent(body.transform);
            leftEyeBase.transform.localPosition = new Vector3(-0.3f, 0.3f, 0.6f);
            leftEyeBase.transform.localScale = new Vector3(0.4f, 0.45f, 0.15f);
            ApplyMaterial(leftEyeBase, Color.white);

            GameObject rightEyeBase = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEyeBase.name = "RightEyeBase";
            rightEyeBase.transform.SetParent(body.transform);
            rightEyeBase.transform.localPosition = new Vector3(0.3f, 0.3f, 0.6f);
            rightEyeBase.transform.localScale = new Vector3(0.4f, 0.45f, 0.15f);
            ApplyMaterial(rightEyeBase, Color.white);

            // === PUPILS (black) ===
            GameObject leftPupil = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftPupil.name = "LeftPupil";
            leftPupil.transform.SetParent(leftEyeBase.transform);
            leftPupil.transform.localPosition = new Vector3(0f, 0f, 0.7f);
            leftPupil.transform.localScale = new Vector3(0.5f, 0.6f, 0.4f);
            ApplyMaterial(leftPupil, Color.black);

            GameObject rightPupil = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightPupil.name = "RightPupil";
            rightPupil.transform.SetParent(rightEyeBase.transform);
            rightPupil.transform.localPosition = new Vector3(0f, 0f, 0.7f);
            rightPupil.transform.localScale = new Vector3(0.5f, 0.6f, 0.4f);
            ApplyMaterial(rightPupil, Color.black);

            // === MOUTH (simple rectangle) ===
            GameObject mouth = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            mouth.name = "Mouth";
            mouth.transform.SetParent(body.transform);
            mouth.transform.localPosition = new Vector3(0f, -0.1f, 0.65f);
            mouth.transform.localScale = new Vector3(0.6f, 0.08f, 0.08f);
            ApplyMaterial(mouth, new Color(0.1f, 0.3f, 0.1f));

            // === BOTTOM BLOB PARTS (to give blobby appearance) ===
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f * Mathf.Deg2Rad;
                float radius = 0.25f;
                Vector3 blobPos = new Vector3(Mathf.Cos(angle) * radius, -0.7f, Mathf.Sin(angle) * radius);

                GameObject blob = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
                blob.name = $"BlobPart{i}";
                blob.transform.SetParent(body.transform);
                blob.transform.localPosition = blobPos;
                blob.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
                ApplyMaterial(blob, new Color(0.25f, 0.85f, 0.35f), 0.7f);
            }

            return slime;
        }

        /// <summary>
        /// Helper method to create a GameObject with a custom mesh
        /// </summary>
        static GameObject CreateWithMesh(Mesh mesh)
        {
            GameObject obj = new GameObject();
            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            obj.AddComponent<MeshRenderer>();
            return obj;
        }

        static void ApplyMaterial(GameObject obj, Color color, float alpha = 1f)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(color.r, color.g, color.b, alpha);

                // If translucent, enable transparency
                if (alpha < 1f)
                {
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetFloat("_Blend", 0); // Alpha
                    mat.renderQueue = 3000; // Transparent queue
                    mat.SetOverrideTag("RenderType", "Transparent");
                }

                mat.SetFloat("_Smoothness", 0.3f);
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

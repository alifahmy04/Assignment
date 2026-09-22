#if UNITY_EDITOR
using ElementalAnomaly.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElementalAnomaly.Editor
{
    [InitializeOnLoad]
    public static class PrototypeSceneSetup
    {
        private const string PlayerName = "Prototype Player";
        private const string FloorName = "Prototype Test Floor";
        private const string MaterialFolder = "Assets/Materials";

        static PrototypeSceneSetup()
        {
            EditorApplication.delayCall += EnsurePersistentPrototype;
        }

        [MenuItem("Elemental Anomaly/Setup Persistent Prototype")]
        public static void EnsurePersistentPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                return;
            }

            GameObject player = GameObject.Find(PlayerName);
            if (player == null)
            {
                player = CreatePlayer();
            }
            else if (player.GetComponent<PrototypePlayerController>() == null)
            {
                player.AddComponent<PrototypePlayerController>();
            }

            if (GameObject.Find(FloorName) == null)
            {
                CreateFloor();
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = CreateCamera();
            }

            PrototypeThirdPersonCamera cameraController =
                mainCamera.GetComponent<PrototypeThirdPersonCamera>();
            if (cameraController == null)
            {
                cameraController = mainCamera.gameObject.AddComponent<PrototypeThirdPersonCamera>();
            }

            Transform cameraTarget = player.transform.Find("Camera Target");
            if (cameraTarget == null)
            {
                var targetObject = new GameObject("Camera Target");
                targetObject.transform.SetParent(player.transform, false);
                targetObject.transform.localPosition = new Vector3(0f, 1.4f, 0f);
                cameraTarget = targetObject.transform;
            }

            cameraController.Target = cameraTarget;
            mainCamera.transform.SetPositionAndRotation(
                new Vector3(0f, 3f, -5f),
                Quaternion.Euler(15f, 0f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject(PlayerName);
            Undo.RegisterCreatedObjectUndo(player, "Create persistent prototype player");
            player.transform.position = Vector3.zero;

            var controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1f, 0f);
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.stepOffset = 0.3f;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Player Visual";
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(
                "PrototypePlayer.mat", new Color(0.15f, 0.55f, 1f));

            var target = new GameObject("Camera Target");
            target.transform.SetParent(player.transform, false);
            target.transform.localPosition = new Vector3(0f, 1.4f, 0f);

            player.AddComponent<PrototypePlayerController>();
            return player;
        }

        private static GameObject CreateFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(floor, "Create persistent prototype floor");
            floor.name = FloorName;
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(20f, 0.2f, 20f);
            floor.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(
                "PrototypeFloor.mat", new Color(0.16f, 0.18f, 0.22f));
            return floor;
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create persistent prototype camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(0f, 3f, -5f),
                Quaternion.Euler(15f, 0f, 0f));
            cameraObject.AddComponent<AudioListener>();
            return cameraObject.AddComponent<Camera>();
        }

        private static Material GetOrCreateMaterial(string fileName, Color color)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            string path = $"{MaterialFolder}/{fileName}";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif

#if UNITY_EDITOR
using ElementalAnomaly.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElementalAnomaly.Editor
{
    [InitializeOnLoad]
    public static class PrototypeCombatSetup
    {
        static PrototypeCombatSetup()
        {
            EditorApplication.delayCall += EnsureCombatPrototype;
        }

        [MenuItem("Elemental Anomaly/Setup Fire Progression and Melee")]
        public static void EnsureCombatPrototype()
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

            GameObject player = GameObject.Find("Prototype Player");
            if (player == null)
            {
                return;
            }

            if (player.GetComponent<PrototypePlayerProgression>() == null)
            {
                player.AddComponent<PrototypePlayerProgression>();
            }

            if (player.GetComponent<PrototypePlayerHealth>() == null)
            {
                player.AddComponent<PrototypePlayerHealth>();
            }

            if (player.GetComponent<PrototypeFireInteractor>() == null)
            {
                player.AddComponent<PrototypeFireInteractor>();
            }

            PrototypeMeleeWeapon weapon = player.GetComponent<PrototypeMeleeWeapon>();
            if (weapon == null)
            {
                weapon = player.AddComponent<PrototypeMeleeWeapon>();
            }

            Transform weaponVisual = player.transform.Find("Melee Weapon");
            if (weaponVisual == null)
            {
                GameObject baton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                baton.name = "Melee Weapon";
                baton.transform.SetParent(player.transform, false);
                baton.transform.localPosition = new Vector3(0.45f, 1.05f, 0.6f);
                baton.transform.localScale = new Vector3(0.14f, 0.14f, 0.9f);
                Object.DestroyImmediate(baton.GetComponent<Collider>());
                baton.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(
                    "PrototypeWeapon.mat", new Color(0.55f, 0.6f, 0.7f));
                weaponVisual = baton.transform;
            }

            SerializedObject weaponSerialized = new SerializedObject(weapon);
            weaponSerialized.FindProperty("weaponVisual").objectReferenceValue = weaponVisual;
            weaponSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject reactor = GameObject.Find("Fire Reactor");
            if (reactor != null && reactor.GetComponent<PrototypeFireReactor>() == null)
            {
                reactor.AddComponent<PrototypeFireReactor>();
            }

            GameObject exitDoor = GameObject.Find("Exit Door");
            if (exitDoor != null && exitDoor.GetComponent<PrototypeFireExitBarrier>() == null)
            {
                exitDoor.AddComponent<PrototypeFireExitBarrier>();
            }

            AddEnemyTarget("Enemy_EyeDrone");
            AddEnemyTarget("Enemy_QuadShell");
            EnsureRoomProp("Prop_HealthPack_Tube.fbx", new Vector3(-3.2f, 0.25f, -2.7f), 0.55f);
            EnsurePlaceholderProp("Fire Safety Canister", new Vector3(-3.2f, 0.7f, -2.7f));
            EnsureAmbientFireVent("Ambient Fire Vent Left", new Vector3(-4.8f, 1.1f, -0.6f), 90f);
            EnsureAmbientFireVent("Ambient Fire Vent Right", new Vector3(4.8f, 1.1f, -0.6f), -90f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureRoomProp(string fileName, Vector3 position, float scale)
        {
            string objectName = fileName.Replace(".fbx", string.Empty);
            if (GameObject.Find(objectName) != null)
            {
                return;
            }

            GameObject room = GameObject.Find("Fire Reactor Room");
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{fileName}");
            if (room == null || asset == null)
            {
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            instance.name = objectName;
            instance.transform.SetParent(room.transform, true);
            instance.transform.position = position;
            instance.transform.localScale = Vector3.one * scale;
            Undo.RegisterCreatedObjectUndo(instance, "Add missing prototype prop");
        }

        private static void EnsurePlaceholderProp(string objectName, Vector3 position)
        {
            if (GameObject.Find(objectName) != null)
            {
                return;
            }

            GameObject room = GameObject.Find("Fire Reactor Room");
            if (room == null)
            {
                return;
            }

            GameObject canister = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            canister.name = objectName;
            canister.transform.SetParent(room.transform, true);
            canister.transform.position = position;
            canister.transform.localScale = new Vector3(0.35f, 0.7f, 0.35f);
            canister.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(
                "PrototypeSafetyCanister.mat", new Color(0.95f, 0.75f, 0.08f));
            Undo.RegisterCreatedObjectUndo(canister, "Add placeholder prototype prop");
        }

        private static void AddEnemyTarget(string objectName)
        {
            GameObject enemy = GameObject.Find(objectName);
            if (enemy == null)
            {
                return;
            }

            if (enemy.GetComponent<Collider>() == null)
            {
                BoxCollider collider = enemy.AddComponent<BoxCollider>();
                collider.center = Vector3.up * 0.7f;
                collider.size = new Vector3(1.4f, 1.4f, 1.4f);
            }

            if (enemy.GetComponent<PrototypeDummyEnemy>() == null)
            {
                enemy.AddComponent<PrototypeDummyEnemy>();
            }
        }

        private static void EnsureAmbientFireVent(string objectName, Vector3 position, float yaw)
        {
            if (GameObject.Find(objectName) != null)
            {
                return;
            }

            GameObject room = GameObject.Find("Fire Reactor Room");
            if (room == null)
            {
                return;
            }

            GameObject vent = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vent.name = objectName;
            vent.transform.SetParent(room.transform, false);
            vent.transform.position = position;
            vent.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            vent.transform.localScale = new Vector3(0.45f, 0.12f, 0.45f);
            vent.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(
                "PrototypeAmbientFire.mat", new Color(1f, 0.1f, 0.01f), true);

            var source = vent.AddComponent<PrototypeAmbientFireSource>();
            var lightObject = new GameObject("Ambient Fire Light");
            lightObject.transform.SetParent(vent.transform, false);
            lightObject.transform.localPosition = Vector3.up * 0.35f;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.12f, 0.02f);
            light.intensity = 1.5f;
            light.range = 3f;
            SerializedObject sourceSerialized = new SerializedObject(source);
            sourceSerialized.FindProperty("sourceLight").objectReferenceValue = light;
            sourceSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material GetOrCreateMaterial(string fileName, Color color, bool emission = false)
        {
            string path = $"Assets/Materials/{fileName}";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 3f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
#endif

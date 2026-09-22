#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using ElementalAnomaly.Prototype;

namespace ElementalAnomaly.Editor
{
    [InitializeOnLoad]
    public static class PrototypeFireAttackSetup
    {
        private static bool importing;
        static PrototypeFireAttackSetup() { EditorApplication.delayCall += Install; }
        [MenuItem("Elemental Anomaly/Setup Fire Attack and Effects")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/SampleScene.unity") return;
            var player = Object.FindFirstObjectByType<PrototypePlayerProgression>();
            if (player == null) return;
            var oldHud = GameObject.Find("Fire Combat HUD");
            if (oldHud != null && oldHud.GetComponent<PrototypeFireHud>()?.status != null) return;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null)
            {
                if (!importing)
                {
                    importing = true;
                    AssetDatabase.importPackageCompleted += ResourcesReady;
                    TMP_PackageResourceImporter.ImportResources(true, false, false);
                }
                return;
            }
            var combat = player.GetComponent<PrototypeFireCombat>() ?? player.gameObject.AddComponent<PrototypeFireCombat>();
            var glow = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            glow.SetColor("_BaseColor", new Color(1f, 0.32f, 0.02f));
            const string materialPath = "Assets/Materials/FireAttackGlow.mat";
            var savedGlow = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (savedGlow == null) { AssetDatabase.CreateAsset(glow, materialPath); savedGlow = glow; }
            else Object.DestroyImmediate(glow);

            const string prefabPath = "Assets/Prefabs/PrototypeFireball.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.name = "Fireball";
                Object.DestroyImmediate(ball.GetComponent<Collider>());
                ball.transform.localScale = Vector3.one * 0.24f;
                ball.GetComponent<Renderer>().sharedMaterial = savedGlow;
                ball.AddComponent<PrototypeFireProjectile>();
                Effects(ball.transform, Vector3.zero, savedGlow, 0.1f);
                prefab = PrefabUtility.SaveAsPrefabAsset(ball, prefabPath);
                Object.DestroyImmediate(ball);
            }
            combat.projectilePrefab = prefab.GetComponent<PrototypeFireProjectile>();
            var reactor = GameObject.Find("Fire Reactor");
            if (reactor != null)
            {
                Effects(reactor.transform, new Vector3(0, 1.8f, 0), savedGlow, 0.5f);
                var ring = reactor.transform.Find("Reactor Ring");
                if (ring != null)
                {
                    ring.localRotation = Quaternion.Euler(15, 0, 20);
                    if (ring.GetComponent<PrototypeReactorSpin>() == null)
                        ring.gameObject.AddComponent<PrototypeReactorSpin>().degreesPerSecond = new Vector3(10, 35, 15);
                }
            }
            foreach (var source in Object.FindObjectsByType<PrototypeAmbientFireSource>(FindObjectsSortMode.None))
                Effects(source.transform, new Vector3(0, 0.15f, 0), savedGlow, 0.22f);

            var canvasObject = oldHud != null ? oldHud : new GameObject("Fire Combat HUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            var hud = canvasObject.GetComponent<PrototypeFireHud>() ?? canvasObject.AddComponent<PrototypeFireHud>();
            hud.progression = player;
            hud.combat = combat;
            var panel = Rect("Fire Panel", canvasObject.transform, new Vector2(24, 24), new Vector2(550, 126));
            panel.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.02f, 0.025f, 0.04f, 0.92f);
            hud.status = Label("Instructions", panel, new Vector2(14, 10), new Vector2(525, 55), font, "E: Absorb reactor | Left click: Melee");
            var meter = Rect("Fire Meter", panel, new Vector2(14, 75), new Vector2(280, 36));
            hud.meter = meter.gameObject;
            var track = Rect("Charge Track", meter, Vector2.zero, new Vector2(280, 9));
            track.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.25f, 0.12f, 0.07f);
            hud.fill = Rect("Charge Fill", track, Vector2.zero, Vector2.zero);
            hud.fill.anchorMin = Vector2.zero;
            hud.fill.anchorMax = Vector2.one;
            hud.fill.offsetMin = hud.fill.offsetMax = Vector2.zero;
            hud.fill.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(1, 0.35f, 0.05f);
            hud.charges = Label("Charges", meter, new Vector2(0, 12), new Vector2(280, 24), font, "FIRE");
            var reticle = Label("Fire Reticle", canvasObject.transform, Vector2.zero, new Vector2(30, 30), font, "+");
            reticle.rectTransform.anchorMin = reticle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            reticle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            reticle.alignment = TextAlignmentOptions.Center;
            hud.reticle = reticle.gameObject;
            hud.reticle.SetActive(false);
            hud.meter.SetActive(false);
            foreach (var graphic in canvasObject.GetComponentsInChildren<UnityEngine.UI.Graphic>(true)) graphic.raycastTarget = false;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Fire attack, meter and source effects saved to SampleScene.");
        }
        private static void ResourcesReady(string package)
        {
            AssetDatabase.importPackageCompleted -= ResourcesReady;
            importing = false;
            EditorApplication.delayCall += Install;
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
        private static TMP_Text Label(string name, Transform parent, Vector2 position, Vector2 size, TMP_FontAsset font, string text)
        {
            var label = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = 18;
            label.color = Color.white;
            label.text = text;
            return label;
        }
        private static void Effects(Transform parent, Vector3 position, Material material, float radius)
        {
            if (parent.Find("Fire Particles") != null) return;
            var effect = new GameObject("Fire Particles");
            effect.transform.SetParent(parent, false);
            effect.transform.localPosition = position;
            effect.transform.localScale = new Vector3(1f / parent.lossyScale.x, 1f / parent.lossyScale.y, 1f / parent.lossyScale.z);
            effect.transform.rotation = Quaternion.Euler(-90, 0, 0);
            var ps = effect.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 0.15f, 0.01f), new Color(1, 0.85f, 0.15f));
            main.scalingMode = ParticleSystemScalingMode.Shape;
            var emission = ps.emission; emission.rateOverTime = 45;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.radius = radius; shape.angle = 15;
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 0));
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            ps.Play();
        }
    }
}
#endif

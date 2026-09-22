using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeFireReactor : MonoBehaviour, IPrototypeInteractable
    {
        [SerializeField] private Renderer reactorCore;
        [SerializeField] private Renderer reactorRing;
        [SerializeField] private Light reactorLight;

        public string Prompt => "Press E to break Fire suppression";

        private void Awake()
        {
            if (reactorCore == null)
            {
                Transform core = transform.Find("Reactor Core");
                reactorCore = core != null ? core.GetComponent<Renderer>() : null;
            }

            if (reactorLight == null)
            {
                reactorLight = GetComponentInChildren<Light>();
            }

            if (reactorRing == null)
            {
                Transform ring = transform.Find("Reactor Ring");
                reactorRing = ring != null ? ring.GetComponent<Renderer>() : null;
            }

            SetReactorGlow(true);
        }

        public bool CanInteract(PrototypePlayerProgression progression)
        {
            return progression != null && !progression.FireUnlocked;
        }

        public void Interact(PrototypePlayerProgression progression)
        {
            progression.UnlockFire();
            SetReactorGlow(false);

            PrototypeFireExitBarrier exit = Object.FindFirstObjectByType<PrototypeFireExitBarrier>();
            if (exit != null)
            {
                exit.Unlock();
            }

            Debug.Log("Fire manipulation restored. Ambient Fire sources are now absorbable.");
        }

        private void SetReactorGlow(bool active)
        {
            if (reactorLight != null)
            {
                reactorLight.enabled = active;
                reactorLight.intensity = active ? 4f : 0f;
            }

            SetRendererGlow(reactorCore, active);
            SetRendererGlow(reactorRing, active);
        }

        private static void SetRendererGlow(Renderer target, bool active)
        {
            if (target == null) return;
            Material material = target.material;
            if (material == null) return;

            material.color = active ? new Color(1f, 0.08f, 0.01f) : new Color(0.08f, 0.03f, 0.02f);
            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", active ? new Color(1f, 0.04f, 0.005f) * 7f : Color.black);
        }
    }
}

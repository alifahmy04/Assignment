using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeFireReactor : MonoBehaviour, IPrototypeInteractable
    {
        [SerializeField] private Renderer reactorCore;
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
        }

        public bool CanInteract(PrototypePlayerProgression progression)
        {
            return progression != null && !progression.FireUnlocked;
        }

        public void Interact(PrototypePlayerProgression progression)
        {
            progression.UnlockFire();
            if (reactorLight != null)
            {
                reactorLight.intensity = 7f;
            }

            if (reactorCore != null && reactorCore.sharedMaterial != null &&
                reactorCore.sharedMaterial.HasProperty("_EmissionColor"))
            {
                reactorCore.sharedMaterial.SetColor("_EmissionColor", new Color(1f, 0.04f, 0.005f) * 7f);
            }

            PrototypeFireExitBarrier exit = Object.FindFirstObjectByType<PrototypeFireExitBarrier>();
            if (exit != null)
            {
                exit.Unlock();
            }

            Debug.Log("Fire manipulation restored. Ambient Fire sources are now absorbable.");
        }
    }
}

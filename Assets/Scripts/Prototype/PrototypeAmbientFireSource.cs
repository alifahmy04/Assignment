using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeAmbientFireSource : MonoBehaviour, IPrototypeInteractable
    {
        [SerializeField] private Light sourceLight;
        [SerializeField] private Renderer sourceRenderer;
        private bool depleted;

        public string Prompt => "Press E to absorb ambient Fire";

        public bool CanInteract(PrototypePlayerProgression progression)
        {
            return !depleted && progression != null && progression.FireUnlocked &&
                   progression.FireCharges < progression.MaximumFireCharges;
        }

        public void Interact(PrototypePlayerProgression progression)
        {
            if (!CanInteract(progression) || !progression.AddFireCharge())
            {
                return;
            }

            depleted = true;
            foreach (var particles in GetComponentsInChildren<ParticleSystem>())
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (sourceLight != null)
            {
                sourceLight.intensity = 0f;
                sourceLight.enabled = false;
            }

            if (sourceRenderer == null)
            {
                sourceRenderer = GetComponent<Renderer>();
            }

            if (sourceRenderer != null)
            {
                Material depletedMaterial = sourceRenderer.material;
                depletedMaterial.color = new Color(0.08f, 0.03f, 0.02f);
                if (depletedMaterial.HasProperty("_EmissionColor"))
                {
                    depletedMaterial.SetColor("_EmissionColor", Color.black);
                }
            }

            Collider sourceCollider = GetComponent<Collider>();
            if (sourceCollider != null)
            {
                sourceCollider.enabled = false;
            }

            Debug.Log($"Ambient Fire absorbed. Charges: {progression.FireCharges}/{progression.MaximumFireCharges}");
        }
    }
}

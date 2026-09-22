using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeAmbientFireSource : MonoBehaviour, IPrototypeInteractable
    {
        [SerializeField] private Light sourceLight;
        [SerializeField] private Renderer sourceRenderer;
        [SerializeField, Min(0.1f)] private float regenerationSeconds = 20f;
        private bool depleted;
        private float regenerateAt;
        private Material runtimeMaterial;
        private Color originalColor, originalEmission;
        private float originalLightIntensity;
        private bool originalLightEnabled, originalColliderEnabled;
        private Collider sourceCollider;
        private ParticleSystem[] fireParticles;

        private void Awake()
        {
            if (sourceRenderer == null) sourceRenderer = GetComponent<Renderer>();
            if (sourceLight == null) sourceLight = GetComponentInChildren<Light>();
            sourceCollider = GetComponent<Collider>();
            fireParticles = GetComponentsInChildren<ParticleSystem>();
            if (sourceRenderer != null)
            {
                runtimeMaterial = sourceRenderer.material;
                originalColor = runtimeMaterial.color;
                if (runtimeMaterial.HasProperty("_EmissionColor"))
                    originalEmission = runtimeMaterial.GetColor("_EmissionColor");
            }
            if (sourceLight != null)
            {
                originalLightIntensity = sourceLight.intensity;
                originalLightEnabled = sourceLight.enabled;
            }
            if (sourceCollider != null) originalColliderEnabled = sourceCollider.enabled;
        }

        private void Update()
        {
            if (!depleted || Time.time < regenerateAt) return;
            depleted = false;
            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = originalColor;
                if (runtimeMaterial.HasProperty("_EmissionColor"))
                    runtimeMaterial.SetColor("_EmissionColor", originalEmission);
            }
            if (sourceLight != null)
            {
                sourceLight.intensity = originalLightIntensity;
                sourceLight.enabled = originalLightEnabled;
            }
            if (sourceCollider != null) sourceCollider.enabled = originalColliderEnabled;
            foreach (var particles in fireParticles) particles.Play(true);
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }

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
            regenerateAt = Time.time + regenerationSeconds;
            foreach (var particles in fireParticles)
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

            if (sourceCollider != null)
            {
                sourceCollider.enabled = false;
            }

            Debug.Log($"Ambient Fire absorbed. Charges: {progression.FireCharges}/{progression.MaximumFireCharges}");
        }
    }
}

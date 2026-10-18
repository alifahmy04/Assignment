using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    // Passive equipment visuals; the existing component identity preserves scene references.
    [RequireComponent(typeof(PrototypePlayerProgression), typeof(PrototypeFireCombat))]
    public sealed class PrototypeBurnBright : MonoBehaviour
    {
        public Light flameLight;
        public ParticleSystem[] handFlames;
        [Min(0f)] public float lightIntensity = 5f;

        public bool IsActive { get; private set; }

        private PrototypePlayerProgression progression;
        private PrototypePlayerController player;
        private PrototypePlayerHealth health;
        private PrototypeFireCombat combat;
        private float startedAt;

        private void Awake()
        {
            progression = GetComponent<PrototypePlayerProgression>();
            player = GetComponent<PrototypePlayerController>();
            health = GetComponent<PrototypePlayerHealth>();
            combat = GetComponent<PrototypeFireCombat>();
            EndAbility();
        }

        private void LateUpdate()
        {
            bool shouldLight = combat != null && combat.isActiveAndEnabled && combat.FireMode &&
                progression != null && progression.FireUnlocked && (player == null || !player.InputLocked) &&
                (health == null || health.CurrentHealth > 0);
            if (!shouldLight)
            {
                if (IsActive) EndAbility();
                return;
            }
            if (!IsActive)
            {
                IsActive = true;
                startedAt = Time.time;
                if (flameLight != null) flameLight.enabled = true;
                if (handFlames != null)
                    foreach (var flame in handFlames)
                        if (flame != null) flame.Play(true);
            }
            if (flameLight != null)
            {
                float fade = Mathf.Clamp01((Time.time - startedAt) / .2f);
                float flicker = .94f + .12f * Mathf.PerlinNoise(Time.time * 6f, 9f);
                flameLight.intensity = lightIntensity * fade * flicker;
            }
        }

        private void EndAbility()
        {
            IsActive = false;
            if (flameLight != null) { flameLight.intensity = 0f; flameLight.enabled = false; }
            if (handFlames != null)
                foreach (var flame in handFlames)
                    if (flame != null) flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnDisable() => EndAbility();
    }
}

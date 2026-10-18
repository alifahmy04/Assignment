using UnityEngine;
using TMPro;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeFireSuppressionSystem : MonoBehaviour, IPrototypeInteractable
    {
        public Transform evacuationDoor;
        public ParticleSystem[] sprinklers;
        public Light[] warningLights;
        public Light releaseLight;
        public TMP_Text doorStatus;
        public TMP_Text detectorLabel;
        public AudioSource alarm;
        public AudioSource water;
        [Min(0f)] public float releaseDelay = 1.5f;
        [Min(0.1f)] public float openingSeconds = 2f;
        [Min(0.1f)] public float doorTravel = 4.4f;
        [Min(1f)] public float suppressionSeconds = 12f;

        public bool Activated { get; private set; }
        public bool RouteOpen { get; private set; }
        private Vector3 closedDoorPosition;
        private float activatedAt;
        private bool cycleFinished;

        public string Prompt => "Heat detector | 1: Fire mode | Hold RMB to aim | LMB: Shoot";

        public string GetPrompt(PrototypePlayerProgression progression)
        {
            if (Activated || progression == null) return string.Empty;
            return progression.FireUnlocked ? Prompt : "Heat detector | Restore fire at the reactor first";
        }

        // This is a shooting target. E must never activate the safety system.
        public bool CanInteract(PrototypePlayerProgression progression) => false;
        public void Interact(PrototypePlayerProgression progression) { }

        private void Awake()
        {
            if (evacuationDoor != null) closedDoorPosition = evacuationDoor.localPosition;
            if (sprinklers != null)
                foreach (var sprinkler in sprinklers)
                    if (sprinkler != null) sprinkler.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetWarningIntensity(0f);
            if (releaseLight != null) { releaseLight.color = Color.red; releaseLight.intensity = 0.7f; }
            if (doorStatus != null) doorStatus.text = "EVACUATION DOOR | SECURITY LOCKED\nLOCAL FIRE RESPONSE RELEASE";
        }

        public void Activate()
        {
            if (Activated || evacuationDoor == null) return;
            Activated = true;
            activatedAt = Time.time;
            if (sprinklers != null)
                foreach (var sprinkler in sprinklers)
                    if (sprinkler != null) sprinkler.Play(true);
            if (alarm != null) alarm.Play();
            if (water != null) water.Play();
            if (detectorLabel != null) detectorLabel.text = "HEAT DETECTOR | ACTIVATED\nLOCAL SUPPRESSION IN PROGRESS";
            if (doorStatus != null) doorStatus.text = "FIRE RESPONSE ACTIVE\nRELEASING EVACUATION LOCK...";
        }

        private void Update()
        {
            if (!Activated) return;
            float elapsed = Time.time - activatedAt;
            float t = Mathf.Clamp01((elapsed - releaseDelay) / openingSeconds);
            if (evacuationDoor != null)
                evacuationDoor.localPosition = closedDoorPosition + Vector3.up * (doorTravel * Mathf.SmoothStep(0f, 1f, t));

            if (!RouteOpen && t >= 1f)
            {
                RouteOpen = true;
                if (doorStatus != null) doorStatus.text = "EVACUATION ROUTE OPEN\nPROCEED TO TRANSFER BAY";
                if (releaseLight != null) { releaseLight.color = Color.green; releaseLight.intensity = 1.5f; }
            }

            if (!cycleFinished && elapsed >= suppressionSeconds)
            {
                cycleFinished = true;
                StopEffects();
                if (detectorLabel != null) detectorLabel.text = "LOCAL SUPPRESSION COMPLETE\nEVACUATION RELEASE LATCHED";
            }
            SetWarningIntensity(cycleFinished ? 0.35f : 0.5f + 2.5f * (0.5f + 0.5f * Mathf.Sin(elapsed * Mathf.PI * 3f)));
        }

        private void SetWarningIntensity(float intensity)
        {
            if (warningLights == null) return;
            foreach (var light in warningLights)
                if (light != null) light.intensity = intensity;
        }

        private void StopEffects()
        {
            if (alarm != null) alarm.Stop();
            if (water != null) water.Stop();
            if (sprinklers == null) return;
            foreach (var sprinkler in sprinklers)
                if (sprinkler != null) sprinkler.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnDisable()
        {
            StopEffects();
            SetWarningIntensity(0f);
        }
    }
}

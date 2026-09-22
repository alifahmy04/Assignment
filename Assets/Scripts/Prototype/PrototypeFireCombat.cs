using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalAnomaly.Prototype
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(PrototypePlayerProgression))]
    public sealed class PrototypeFireCombat : MonoBehaviour
    {
        public PrototypeFireProjectile projectilePrefab;
        public bool FireMode { get; private set; }
        public string Message { get; private set; }
        private PrototypePlayerProgression progression;
        private float nextShot, messageUntil;
        private void Awake() => progression = GetComponent<PrototypePlayerProgression>();
        private void Update()
        {
            if (Time.time > messageUntil) Message = "";
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.digit1Key.wasPressedThisFrame)
            {
                if (progression.FireUnlocked) FireMode = true;
                else Notify("Fire suppressed - absorb the reactor with E first.");
            }
            if (keyboard != null && keyboard.digit2Key.wasPressedThisFrame) FireMode = false;
            if (!FireMode || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || Time.time < nextShot) return;
            if (projectilePrefab == null) return;
            if (!progression.SpendFireCharge())
            {
                Notify("No Fire charges - absorb a glowing vent with E.");
                return;
            }
            nextShot = Time.time + 0.45f;
            var camera = Camera.main;
            var ray = camera != null ? camera.ViewportPointToRay(new Vector3(0.5f, 0.5f)) : new Ray(transform.position + Vector3.up, transform.forward);
            Vector3 aim = ray.GetPoint(40f);
            float nearest = 40f;
            foreach (var hit in Physics.RaycastAll(ray, 40f, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform) && hit.distance < nearest)
                { nearest = hit.distance; aim = hit.point; }
            Vector3 origin = transform.position + Vector3.up * 1.35f;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.LookRotation((aim - origin).normalized));
            projectile.Owner = transform;
        }
        private void Notify(string text) { Message = text; messageUntil = Time.time + 2.5f; }
    }
}

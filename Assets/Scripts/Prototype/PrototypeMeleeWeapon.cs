using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeMeleeWeapon : MonoBehaviour
    {
        [SerializeField] private Transform weaponVisual;
        [SerializeField, Min(0.1f)] private float attackCooldown = 0.55f;
        [SerializeField, Min(0.1f)] private float hitRange = 1.35f;
        [SerializeField, Min(0.1f)] private float hitRadius = 0.55f;
        [SerializeField, Min(1)] private int damage = 1;

        private float cooldownTimer;
        private Quaternion restingRotation;
        private float swingTimer;
        private PrototypePlayerController player;
        private PrototypeFireCombat fireCombat;
        public bool IsEquipped { get; private set; } = true;

        public void SetEquipped(bool equipped)
        {
            IsEquipped = equipped;
            swingTimer = 0f;
            UpdateWeaponVisibility();
        }

        private void Awake()
        {
            player = GetComponent<PrototypePlayerController>();
            fireCombat = GetComponent<PrototypeFireCombat>();
            if (weaponVisual == null)
            {
                Transform existing = transform.Find("Melee Weapon");
                weaponVisual = existing;
            }

            if (weaponVisual != null)
            {
                restingRotation = weaponVisual.localRotation;
            }
        }

        private void Update()
        {
            UpdateWeaponVisibility();
            cooldownTimer -= Time.deltaTime;
            swingTimer -= Time.deltaTime;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                TryAttack();
            }

            if (weaponVisual != null)
            {
                float swing = swingTimer > 0f ? Mathf.Sin((1f - swingTimer / 0.18f) * Mathf.PI) * 100f : 0f;
                weaponVisual.localRotation = restingRotation * Quaternion.Euler(swing, 0f, 0f);
            }
        }

        private void UpdateWeaponVisibility()
        {
            if (weaponVisual == null) return;
            bool visible = IsEquipped && (fireCombat == null || !fireCombat.FireMode);
            if (weaponVisual.gameObject.activeSelf != visible) weaponVisual.gameObject.SetActive(visible);
        }

        private void TryAttack()
        {
            if (!IsEquipped || (player != null && player.InputLocked)) return;
            if (fireCombat != null && fireCombat.FireMode) return;
            if (cooldownTimer > 0f)
            {
                return;
            }

            cooldownTimer = attackCooldown;
            swingTimer = 0.18f;

            Vector3 center = transform.position + transform.forward * hitRange + Vector3.up;
            Collider[] hits = Physics.OverlapSphere(center, hitRadius);
            foreach (Collider hit in hits)
            {
                PrototypeDummyEnemy enemy = hit.GetComponentInParent<PrototypeDummyEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + transform.forward * hitRange + Vector3.up, hitRadius);
        }
    }
}

using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeDummyEnemy : MonoBehaviour
    {
        [SerializeField, Min(1)] private int hitPoints = 3;
        [SerializeField, Min(1)] private int contactDamage = 1;
        [SerializeField, Min(0.1f)] private float contactDamageCooldown = 1f;
        private Renderer targetRenderer;
        private Color originalColor = Color.white;
        private float nextContactDamage;

        private void Awake()
        {
            targetRenderer = GetComponentInChildren<Renderer>();
            if (targetRenderer != null && targetRenderer.sharedMaterial != null)
            {
                originalColor = targetRenderer.sharedMaterial.color;
            }
        }

        public void TakeDamage(int amount)
        {
            hitPoints -= amount;
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.red;
                CancelInvoke(nameof(ResetColor));
                Invoke(nameof(ResetColor), 0.15f);
            }

            if (hitPoints <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        public void TryDamagePlayer(PrototypePlayerHealth playerHealth)
        {
            if (playerHealth == null || Time.time < nextContactDamage) return;
            nextContactDamage = Time.time + contactDamageCooldown;
            playerHealth.TakeDamage(contactDamage);
        }

        private void ResetColor()
        {
            if (targetRenderer != null)
            {
                targetRenderer.material.color = originalColor;
            }
        }
    }
}

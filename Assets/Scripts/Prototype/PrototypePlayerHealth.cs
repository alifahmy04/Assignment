using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypePlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximumHealth = 5;
        public int MaximumHealth => maximumHealth;
        public int CurrentHealth { get; private set; }

        private void Awake() => CurrentHealth = maximumHealth;

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            Debug.Log($"Player damaged. Health: {CurrentHealth}/{maximumHealth}");
        }
    }
}

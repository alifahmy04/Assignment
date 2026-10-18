using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypePlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximumHealth = 5;
        public int MaximumHealth => maximumHealth;
        public int CurrentHealth { get; private set; }
        public bool CanHeal => CurrentHealth > 0 && CurrentHealth < maximumHealth;
        public event System.Action Died;

        private void Awake() => CurrentHealth = maximumHealth;

        public bool TryHeal(int amount)
        {
            if (amount <= 0 || !CanHeal) return false;
            CurrentHealth += Mathf.Min(amount, maximumHealth - CurrentHealth);
            return true;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            Debug.Log($"Player damaged. Health: {CurrentHealth}/{maximumHealth}");
            if (CurrentHealth == 0) Died?.Invoke();
        }
    }
}

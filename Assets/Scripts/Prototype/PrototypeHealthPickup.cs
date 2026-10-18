using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeHealthPickup : MonoBehaviour, IPrototypeInteractable
    {
        [Min(1)] public int healingAmount = 2;
        public string itemName = "health pack";
        private bool consumed;

        public string Prompt => $"[E] Use {itemName} (restore up to {healingAmount} HP)";

        public string GetPrompt(PrototypePlayerProgression progression)
        {
            if (consumed || !isActiveAndEnabled || progression == null) return string.Empty;
            var health = progression.GetComponent<PrototypePlayerHealth>();
            if (health == null) return string.Empty;
            if (health.CurrentHealth <= 0) return "Medical supplies | Cannot use while incapacitated";
            return health.CanHeal ? Prompt : "Medical supplies | Health full";
        }

        public bool CanInteract(PrototypePlayerProgression progression)
        {
            if (consumed || !isActiveAndEnabled || progression == null || healingAmount <= 0) return false;
            var health = progression.GetComponent<PrototypePlayerHealth>();
            return health != null && health.CanHeal;
        }

        public void Interact(PrototypePlayerProgression progression)
        {
            if (!CanInteract(progression)) return;
            var health = progression.GetComponent<PrototypePlayerHealth>();
            if (!health.TryHeal(healingAmount)) return;
            consumed = true;
            gameObject.SetActive(false);
        }
    }
}

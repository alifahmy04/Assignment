using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeMeleeToolPickup : MonoBehaviour, IPrototypeInteractable
    {
        public PrototypeMeleeWeapon weapon;
        public string Prompt => "[E] Pick up maintenance baton";
        public string GetPrompt(PrototypePlayerProgression progression) =>
            CanInteract(progression) && gameObject.activeInHierarchy ? Prompt : string.Empty;
        public bool CanInteract(PrototypePlayerProgression progression)
        {
            return weapon != null && !weapon.IsEquipped;
        }

        public void Interact(PrototypePlayerProgression progression)
        {
            if (!CanInteract(progression)) return;
            weapon.SetEquipped(true);
            gameObject.SetActive(false);
        }
    }
}

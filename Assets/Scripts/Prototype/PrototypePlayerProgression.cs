using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    // Tracks the prototype's single-level Fire progression state.
    public sealed class PrototypePlayerProgression : MonoBehaviour
    {
        [SerializeField] private int maximumFireCharges = 3;

        public bool FireUnlocked { get; private set; }
        public int FireCharges { get; private set; }
        public int MaximumFireCharges => maximumFireCharges;

        public bool SpendFireCharge()
        {
            if (!FireUnlocked || FireCharges <= 0) return false;
            FireCharges--;
            return true;
        }

        public void UnlockFire()
        {
            FireUnlocked = true;
            FireCharges = Mathf.Max(FireCharges, 1);
        }

        public bool AddFireCharge()
        {
            if (!FireUnlocked || FireCharges >= maximumFireCharges)
            {
                return false;
            }

            FireCharges++;
            return true;
        }
    }
}

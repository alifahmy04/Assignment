using UnityEngine;
using TMPro;
namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeFireHud : MonoBehaviour
    {
        public PrototypePlayerProgression progression;
        public PrototypeFireCombat combat;
        public GameObject meter;
        public RectTransform fill;
        public TMP_Text status;
        public TMP_Text charges;
        public GameObject reticle;
        private void Update()
        {
            if (progression == null || combat == null || meter == null || reticle == null || fill == null || charges == null || status == null) return;
            meter.SetActive(progression.FireUnlocked);
            reticle.SetActive(combat.FireMode);
            fill.anchorMax = new Vector2((float)progression.FireCharges / Mathf.Max(1, progression.MaximumFireCharges), 1f);
            charges.text = $"FIRE  {progression.FireCharges} / {progression.MaximumFireCharges}";
            status.text = !string.IsNullOrEmpty(combat.Message) ? combat.Message :
                (!progression.FireUnlocked ? "FIRE SUPPRESSED | E at reactor to unlock\nMELEE | Left click to attack" :
                (combat.FireMode ? "FIRE | Left click to shoot | 2: Melee" : "MELEE | Left click to attack | 1: Fire") + "\nE: Absorb nearby glowing vent (single use)");
        }
    }
}

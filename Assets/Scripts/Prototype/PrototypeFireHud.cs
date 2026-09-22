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
        public PrototypePlayerHealth health;
        public GameObject healthMeter;
        public RectTransform healthFill;
        public TMP_Text healthText;
        private void Update()
        {
            if (progression == null || combat == null || meter == null || reticle == null || fill == null || charges == null || status == null) return;
            meter.SetActive(progression.FireUnlocked);
            reticle.SetActive(true);
            fill.anchorMax = new Vector2((float)progression.FireCharges / Mathf.Max(1, progression.MaximumFireCharges), 1f);
            charges.text = $"FIRE  {progression.FireCharges} / {progression.MaximumFireCharges}";
            if (health != null && healthMeter != null && healthFill != null && healthText != null)
            {
                healthMeter.SetActive(true);
                healthFill.anchorMax = new Vector2((float)health.CurrentHealth / health.MaximumHealth, 1f);
                healthText.text = $"HP  {health.CurrentHealth} / {health.MaximumHealth}";
            }
            status.text = !string.IsNullOrEmpty(combat.Message) ? combat.Message :
                (!progression.FireUnlocked ? "FIRE SUPPRESSED | E at reactor to unlock\nMELEE | Left click to attack" :
                (combat.FireMode ? "FIRE | Left click to shoot | 2: Melee" : "MELEE | Left click to attack | 1: Fire") + "\nE: Absorb glowing vent | Vents regenerate in 20s");
        }
    }
}

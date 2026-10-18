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
        public GameObject interactionPanel;
        public TMP_Text interactionText;
        private PrototypeMeleeWeapon melee;
        private PrototypePlayerController player;
        private PrototypeFireInteractor interactor;
        private void Start()
        {
            if (progression != null) melee = progression.GetComponent<PrototypeMeleeWeapon>();
            if (progression != null) player = progression.GetComponent<PrototypePlayerController>();
            if (progression != null) interactor = progression.GetComponent<PrototypeFireInteractor>();
        }
        private void Update()
        {
            if (progression == null || combat == null || meter == null || reticle == null || fill == null || charges == null || status == null) return;
            meter.SetActive(progression.FireUnlocked);
            reticle.SetActive(player == null || !player.InputLocked);
            fill.anchorMax = new Vector2((float)progression.FireCharges / Mathf.Max(1, progression.MaximumFireCharges), 1f);
            charges.text = $"FIRE  {progression.FireCharges} / {progression.MaximumFireCharges}";
            if (health != null && healthMeter != null && healthFill != null && healthText != null)
            {
                healthMeter.SetActive(true);
                healthFill.anchorMax = new Vector2((float)health.CurrentHealth / health.MaximumHealth, 1f);
                healthText.text = $"HP  {health.CurrentHealth} / {health.MaximumHealth}";
            }
            status.text = player != null && player.InputLocked
                ? "FIRE RESTORATION | Security override in progress"
                : melee != null && !melee.IsEquipped
                ? "UNARMED | Locate the maintenance baton"
                : !string.IsNullOrEmpty(combat.Message) ? combat.Message :
                (!progression.FireUnlocked ? "FIRE SUPPRESSED | Reach the reactor\nMELEE | Left click to attack" :
                (combat.FireMode ? "FIRE | Left click to shoot | 2: Melee" : "MELEE | Left click to attack | 1: Fire") + "\nAbsorb fire at glowing vents to recharge");
        }

        private void LateUpdate()
        {
            if (interactionPanel == null || interactionText == null) return;
            string prompt = interactor != null && interactor.isActiveAndEnabled &&
                (player == null || !player.InputLocked) ? interactor.CurrentPrompt : string.Empty;
            bool visible = !string.IsNullOrEmpty(prompt);
            if (interactionPanel.activeSelf != visible) interactionPanel.SetActive(visible);
            if (!visible) return;
            if (interactionText.text != prompt) interactionText.text = prompt;
            interactionText.color = interactor.InteractionAvailable ? Color.white : new Color(1f, 0.8f, 0.5f);
        }

        private void OnDisable()
        {
            if (interactionPanel != null) interactionPanel.SetActive(false);
        }
    }
}

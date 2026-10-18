using UnityEngine;
using TMPro;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeContainmentOpening : MonoBehaviour
    {
        public Transform player;
        public PrototypeMeleeWeapon weapon;
        public PrototypePlayerProgression progression;
        public Transform leftDoor;
        public Transform rightDoor;
        public TMP_Text announcement;
        public TMP_Text doorStatus;
        public Light[] alarmLights;
        public AudioSource alarmSource;
        [Min(0f)] public float releaseDelay = 5f;
        [Min(0.1f)] public float openingSeconds = 2.5f;
        [Min(0.1f)] public float doorTravel = 1.75f;

        public bool DoorReleased { get; private set; }
        public bool LeftChamber { get; private set; }
        private Vector3 leftClosed, rightClosed, doorwayPosition, exitDirection;
        private float elapsed;
        private int messageState = -1;
        private PrototypeReactorCinematic reactorCinematic;
        private PrototypeFireSuppressionSystem fireSuppression;

        private void Start()
        {
            if (leftDoor == null || rightDoor == null || player == null)
            {
                Debug.LogError("Containment opening needs player and both door panels.", this);
                enabled = false;
                return;
            }
            leftClosed = leftDoor.localPosition;
            rightClosed = rightDoor.localPosition;
            doorwayPosition = (leftDoor.position + rightDoor.position) * 0.5f;
            exitDirection = leftDoor.forward;
            var camera = Camera.main;
            if (camera != null && camera.GetComponent<PrototypeThirdPersonCamera>() != null)
                camera.transform.SetPositionAndRotation(player.position + new Vector3(0f, 3f, -4f), Quaternion.Euler(20f, 0f, 0f));
            if (weapon != null) weapon.SetEquipped(false);
            reactorCinematic = Object.FindFirstObjectByType<PrototypeReactorCinematic>();
            fireSuppression = Object.FindFirstObjectByType<PrototypeFireSuppressionSystem>();
            if (doorStatus != null) doorStatus.text = "C-01 | SEALED\nSUPPRESSION ACTIVE";
            if (alarmSource != null && alarmSource.clip != null)
            {
                alarmSource.loop = true;
                alarmSource.Play();
            }
            SetMessage(0, "OBSERVATION C-01", "You wake in a containment chamber. Your elemental powers are suppressed.");
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= releaseDelay)
            {
                DoorReleased = true;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - releaseDelay) / openingSeconds));
                leftDoor.localPosition = leftClosed + Vector3.left * (doorTravel * t);
                rightDoor.localPosition = rightClosed + Vector3.right * (doorTravel * t);
                if (doorStatus != null && messageState < 3)
                    doorStatus.text = "LOCAL EVACUATION\nSECTOR LOCKDOWN ACTIVE";
            }
            if (DoorReleased && Vector3.Dot(player.position - doorwayPosition, exitDirection) > 1.5f)
                LeftChamber = true;

            if (alarmLights != null)
                foreach (var light in alarmLights)
                    if (light != null) light.intensity = LeftChamber ? 0.6f : 0.9f + 0.65f * (0.5f + 0.5f * Mathf.Sin(elapsed * Mathf.PI));
            if (LeftChamber && alarmSource != null && alarmSource.isPlaying) alarmSource.Stop();

            if (reactorCinematic != null && reactorCinematic.IsPlaying)
                SetMessage(6, "SUPPRESSION OVERRIDE", "Fire restored. Reactor shutdown and security release in progress.");
            else if (progression != null && progression.FireUnlocked)
            {
                if (fireSuppression != null && fireSuppression.RouteOpen)
                    SetMessage(8, "OBJECTIVE | REACH THE EVACUATION TRANSFER BAY", "Proceed through the released door and reach the marked bay to leave Fire Sector F-01.");
                else if (fireSuppression != null && fireSuppression.Activated)
                    SetMessage(7, "LOCAL FIRE RESPONSE", "Sprinklers activated. The evacuation door is releasing.");
                else if (fireSuppression != null)
                    SetMessage(5, "OBJECTIVE | RELEASE THE EVACUATION DOOR", "The reactor gate is open. In the exit passage, heat the marked detector with a fire shot to trigger an emergency release.");
                else
                    SetMessage(5, "FIRE RESTORED", "You have overwhelmed the suppression. The reactor hall's exit is now open.");
            }
            else if (LeftChamber && weapon != null && weapon.IsEquipped)
                SetMessage(4, "OBJECTIVE | REACH THE REACTOR", "The sector remains locked. Search the reactor hall for a way to restore fire.");
            else if (LeftChamber)
                SetMessage(3, "OBJECTIVE | ARM YOURSELF", "Take the maintenance baton from the lit rack in the corridor.");
            else if (DoorReleased)
                SetMessage(2, "FACILITY AUTOMATION | LOCAL EVACUATION", "Preserve viable subject. Proceed to Fire Sector F-01. Outer security remains locked.");
            else if (elapsed >= 2f)
                SetMessage(1, "FACILITY AUTOMATION | COOLANT PRESSURE FAILURE", "Containment cooling is unstable. Initiating local evacuation to preserve the test subject.");
        }

        private void SetMessage(int state, string title, string body)
        {
            if (messageState == state) return;
            messageState = state;
            if (announcement != null) announcement.text = "<b>" + title + "</b>\n" + body;
        }

    }
}

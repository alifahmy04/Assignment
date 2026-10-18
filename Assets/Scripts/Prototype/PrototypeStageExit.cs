using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PrototypeStageExit : MonoBehaviour
    {
        public PrototypePlayerController player;
        public PrototypePlayerProgression progression;
        public PrototypeFireSuppressionSystem fireSuppression;
        public PrototypePlayerFailure stageFlow;
        private BoxCollider arrivalZone;

        private void Awake() => arrivalZone = GetComponent<BoxCollider>();

        private void Update()
        {
            if (player == null || progression == null || fireSuppression == null || stageFlow == null ||
                stageFlow.HasFailed || stageFlow.HasCompleted || !progression.FireUnlocked ||
                !fireSuppression.RouteOpen || !arrivalZone.enabled) return;

            // Check the player's centre directly, independent of Rigidbody trigger callback rules.
            Vector3 point = transform.InverseTransformPoint(player.transform.TransformPoint(Vector3.up)) - arrivalZone.center;
            Vector3 half = arrivalZone.size * .5f;
            if (Mathf.Abs(point.x) <= half.x && Mathf.Abs(point.y) <= half.y && Mathf.Abs(point.z) <= half.z)
                stageFlow.CompleteStage();
        }
    }
}

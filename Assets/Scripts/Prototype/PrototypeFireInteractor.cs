using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalAnomaly.Prototype
{
    public interface IPrototypeInteractable
    {
        string Prompt { get; }
        string GetPrompt(PrototypePlayerProgression progression);
        bool CanInteract(PrototypePlayerProgression progression);
        void Interact(PrototypePlayerProgression progression);
    }

    [RequireComponent(typeof(PrototypePlayerProgression))]
    public sealed class PrototypeFireInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float interactionRadius = 2.4f;

        private PrototypePlayerProgression progression;
        private PrototypePlayerController player;
        private readonly Collider[] nearbyColliders = new Collider[64];
        public string CurrentPrompt { get; private set; } = string.Empty;
        public bool InteractionAvailable { get; private set; }

        private void Awake()
        {
            progression = GetComponent<PrototypePlayerProgression>();
            player = GetComponent<PrototypePlayerController>();
        }

        private void Update()
        {
            CurrentPrompt = string.Empty;
            InteractionAvailable = false;
            if (player != null && player.InputLocked) return;

            IPrototypeInteractable interactable = FindClosestInteractable();
            if (interactable == null) return;
            CurrentPrompt = interactable.GetPrompt(progression);
            InteractionAvailable = interactable.CanInteract(progression);
            if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame || !InteractionAvailable)
            {
                return;
            }

            interactable.Interact(progression);
            // Pickup, charge and cinematic state can change immediately on interaction.
            CurrentPrompt = player != null && player.InputLocked ? string.Empty : interactable.GetPrompt(progression);
            InteractionAvailable = interactable.CanInteract(progression);
        }

        private IPrototypeInteractable FindClosestInteractable()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius,
                nearbyColliders, ~0, QueryTriggerInteraction.Collide);
            Collider[] hits = nearbyColliders;
            if (count == nearbyColliders.Length)
            {
                hits = Physics.OverlapSphere(transform.position, interactionRadius, ~0, QueryTriggerInteraction.Collide);
                count = hits.Length;
            }
            IPrototypeInteractable closest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider hit = hits[i];
                IPrototypeInteractable candidate = hit.GetComponentInParent<IPrototypeInteractable>();
                if (candidate == null || !(candidate is Behaviour behaviour) || !behaviour.isActiveAndEnabled ||
                    string.IsNullOrEmpty(candidate.GetPrompt(progression)))
                {
                    continue;
                }

                float distance = (hit.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = candidate;
                }
            }

            return closest;
        }

        private void OnDisable()
        {
            CurrentPrompt = string.Empty;
            InteractionAvailable = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.05f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}

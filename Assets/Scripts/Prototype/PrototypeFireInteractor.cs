using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalAnomaly.Prototype
{
    public interface IPrototypeInteractable
    {
        string Prompt { get; }
        bool CanInteract(PrototypePlayerProgression progression);
        void Interact(PrototypePlayerProgression progression);
    }

    [RequireComponent(typeof(PrototypePlayerProgression))]
    public sealed class PrototypeFireInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float interactionRadius = 2.4f;

        private PrototypePlayerProgression progression;

        private void Awake()
        {
            progression = GetComponent<PrototypePlayerProgression>();
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            {
                return;
            }

            IPrototypeInteractable interactable = FindClosestInteractable();
            if (interactable != null && interactable.CanInteract(progression))
            {
                interactable.Interact(progression);
            }
        }

        private IPrototypeInteractable FindClosestInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
            IPrototypeInteractable closest = null;
            float closestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                IPrototypeInteractable candidate = hit.GetComponentInParent<IPrototypeInteractable>();
                if (candidate == null || !candidate.CanInteract(progression))
                {
                    continue;
                }

                float distance = (hit.transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = candidate;
                }
            }

            return closest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.05f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}

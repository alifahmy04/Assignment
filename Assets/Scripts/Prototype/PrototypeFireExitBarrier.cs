using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeFireExitBarrier : MonoBehaviour
    {
        [SerializeField] private float openHeight = 3.3f;
        [SerializeField] private float openSpeed = 3f;
        private bool unlocked;
        private Vector3 closedPosition;

        private void Awake()
        {
            closedPosition = transform.position;
        }

        public void Unlock()
        {
            unlocked = true;
        }

        private void Update()
        {
            if (!unlocked)
            {
                return;
            }

            Vector3 openPosition = closedPosition + Vector3.up * openHeight;
            transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);
        }
    }
}

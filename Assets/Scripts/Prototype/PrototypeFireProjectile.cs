using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeFireProjectile : MonoBehaviour
    {
        public Transform Owner { get; set; }
        public float speed = 16f;
        public int damage = 2;
        private float remaining = 3f;
        private void Update()
        {
            float step = speed * Time.deltaTime;
            RaycastHit? nearest = null;
            foreach (var hit in Physics.SphereCastAll(transform.position, 0.12f, transform.forward, step, ~0, QueryTriggerInteraction.Ignore))
            {
                if (Owner != null && hit.transform.IsChildOf(Owner)) continue;
                if (!nearest.HasValue || hit.distance < nearest.Value.distance) nearest = hit;
            }
            if (nearest.HasValue)
            {
                nearest.Value.collider.GetComponentInParent<PrototypeDummyEnemy>()?.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            transform.position += transform.forward * step;
            remaining -= Time.deltaTime;
            if (remaining <= 0f) Destroy(gameObject);
        }
    }
}

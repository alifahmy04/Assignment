using UnityEngine;
namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeReactorSpin : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0f, 35f, 0f);
        private void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
    }
}

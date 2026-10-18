using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    // Only this physical target reacts to fire; nearby walls and doors do not.
    public sealed class PrototypeHeatDetector : MonoBehaviour
    {
        public PrototypeFireSuppressionSystem suppression;

        public void ReceiveFire()
        {
            if (suppression != null) suppression.Activate();
        }
    }
}

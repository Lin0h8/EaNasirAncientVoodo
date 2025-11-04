using UnityEngine;
using System.Linq;

namespace NeuralNetwork_IHNMAIMS
{
    [CreateAssetMenu(fileName = "New Status Effect", menuName = "Rune System/Spell Effects/Apply Status")]
    public class ApplyStatusEffect : SpellEffect
    {
        public enum StatusType { SpeedBoost, Shield, DamageAmp }

        public StatusType statusType;
        public float duration = 5f;
        public float magnitude = 1.5f;


        public override void Apply(GameObject target, RuneData[] sourceRunes)
        {
            var statusReceiver = target.GetComponent<IStatusReceiver>();
            if (statusReceiver != null)
            {
                Debug.Log($"Applying {statusType} to {target.name} for {duration}s with magnitude {magnitude}");
                statusReceiver.ApplyStatus(statusType, duration, magnitude);
            }
        }
    }

    public interface IStatusReceiver
    {
        void ApplyStatus(ApplyStatusEffect.StatusType type, float duration, float magnitude);
    }
}

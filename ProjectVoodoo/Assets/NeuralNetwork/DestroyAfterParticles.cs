using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    [RequireComponent(typeof(ParticleSystem))]
    public class DestroyAfterParticles : MonoBehaviour
    {
        private ParticleSystem ps;

        private void Start()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (ps != null && !ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
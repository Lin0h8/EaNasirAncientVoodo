using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    [RequireComponent(typeof(Rigidbody))]
    public class RuneProjectile : MonoBehaviour
    {
        private float speed = 20f;
        private float lifeTime = 10f;
        private bool useGravity = true;

        private Rigidbody _rb;
        private bool _triggered;

        public RuneData[] Runes { get; private set; }
        public RuneMagicController Controller { get; private set; }

        public void Init(RuneMagicController controller, RuneData[] runes, Vector3 initialVelocity, float speed, float lifeTime, bool useGravity)
        {
            Controller = controller;
            Runes = runes;
            this.speed = speed;
            this.lifeTime = lifeTime;
            this.useGravity = useGravity;
            EnsurePhysics();
            _rb.linearVelocity = initialVelocity;
        }

        private void Awake()
        {
            EnsurePhysics();
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void EnsurePhysics()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
                if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
                _rb.useGravity = useGravity;
            }

            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger = false;
                ((SphereCollider)col).radius = 0.1f;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_triggered) return;
            _triggered = true;

            Vector3 hitPoint = transform.position;
            if (collision.contactCount > 0)
            {
                hitPoint = collision.GetContact(0).point;
            }

            if (Controller != null && Runes != null && Runes.Length > 0)
            {
                Controller.GenerateSpell(Runes, hitPoint);
            }

            Destroy(gameObject);
        }
    }
}
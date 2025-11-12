using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    [RequireComponent(typeof(Rigidbody))]
    public class RuneProjectile : MonoBehaviour
    {
        private bool _initialized = false;
        private Rigidbody _rb;
        private bool _triggered;
        private float lifeTime = 10f;
        private float speed = 20f;
        private bool useGravity = true;
        public RuneMagicController Controller { get; private set; }
        public RuneData[] Runes { get; private set; }

        public void Init(RuneMagicController controller, RuneData[] runes, Vector3 initialVelocity, float speed, float lifeTime, bool useGravity)
        {
            Controller = controller;
            Runes = runes;
            this.speed = speed;
            this.lifeTime = lifeTime;
            this.useGravity = useGravity;

            EnsurePhysics();

            _rb.useGravity = useGravity;
            _rb.linearVelocity = initialVelocity;

            IgnorePlayerCollision();

            _initialized = true;

            Destroy(gameObject, lifeTime);
        }

        private void Awake()
        {
            EnsurePhysics();
            IgnorePlayerCollision();
        }

        private void EnsurePhysics()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
                if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                _rb.useGravity = useGravity;
            }

            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger = false;
                ((SphereCollider)col).radius = 0.3f;
            }
        }

        private void IgnorePlayerCollision()
        {
            Collider myCol = GetComponent<Collider>();
            if (myCol == null) return;

            GameObject playerGo = null;
            try
            {
                playerGo = GameObject.FindWithTag("Player");
            }
            catch { playerGo = null; }

            if (playerGo != null)
            {
                var playerCols = playerGo.GetComponentsInChildren<Collider>();
                foreach (var pc in playerCols)
                {
                    if (pc != null)
                        Physics.IgnoreCollision(myCol, pc, true);
                }
                return;
            }

            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                var controllerCols = playerController.GetComponentsInChildren<Collider>();
                foreach (var pc in controllerCols)
                {
                    if (pc != null)
                        Physics.IgnoreCollision(myCol, pc, true);
                }
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

        private void Start()
        {
            if (!_initialized)
            {
                Destroy(gameObject, 1f);
            }
        }
    }
}
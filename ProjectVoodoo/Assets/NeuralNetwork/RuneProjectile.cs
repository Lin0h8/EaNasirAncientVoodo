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

            // Apply gravity setting after ensuring physics
            _rb.useGravity = useGravity;
            _rb.linearVelocity = initialVelocity;

            // Prevent colliding with the player
            IgnorePlayerCollision();

            _initialized = true;

            // Start destruction timer
            Destroy(gameObject, lifeTime);
        }

        private void Awake()
        {
            EnsurePhysics();
            // also attempt to ignore player collision early if instantiated without Init yet
            IgnorePlayerCollision();
        }

        private void EnsurePhysics()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
                if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection
                _rb.useGravity = useGravity;
            }

            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger = false;
                ((SphereCollider)col).radius = 0.3f; // Increased from 0.1f for better collision detection
            }
        }

        // Ignore collisions with the player by tag first, then by PlayerController component as a fallback.
        private void IgnorePlayerCollision()
        {
            Collider myCol = GetComponent<Collider>();
            if (myCol == null) return;

            // First try: find GameObject with tag "Player"
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

            // Fallback: find PlayerController in scene (works if your player uses PlayerController)
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

            Debug.Log($"RuneProjectile hit: {collision.gameObject.name} at {hitPoint}");

            if (Controller != null && Runes != null && Runes.Length > 0)
            {
                Controller.GenerateSpell(Runes, hitPoint);
            }

            Destroy(gameObject);
        }

        private void Start()
        {
            // Only destroy if not initialized (fallback)
            if (!_initialized)
            {
                Debug.LogWarning($"RuneProjectile on {gameObject.name} was not initialized via Init(). Destroying.");
                Destroy(gameObject, 1f);
            }
        }
    }
}
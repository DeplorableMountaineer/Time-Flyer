using UnityEngine;
using Weapons;

namespace Player {
    [RequireComponent(typeof(Rigidbody2D))] [DisallowMultipleComponent]
    public class Player : MonoBehaviour {
        private Rigidbody2D _rigidbody;
        private float _lastShotTime = 0;
        private Transform _transform;


        [SerializeField] private float maxBackwardAcceleration = 1;
        [SerializeField] private float maxForwardAcceleration = 20;
        [SerializeField] private float maxSidewaysAcceleration = 5;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float rotationRate = 180;
        [SerializeField] private float missileSpeed = 15;
        [SerializeField] private float attackRange = 5;
        [SerializeField] private float minTimeBetweenShots = .5f;
        [SerializeField] private GameObject missilePrefab = null;

        public void Move(Vector2 targetVelocity) {
            Vector3 delta = (targetVelocity - _rigidbody.velocity)/Time.smoothDeltaTime;
            float orientation = Mathf.Atan2(targetVelocity.y, targetVelocity.x)*Mathf.Rad2Deg;
            float maxAcceleration = orientation > 45 && orientation < 135
                ? maxForwardAcceleration
                : orientation < -45 && orientation > -135
                    ? maxBackwardAcceleration
                    : maxSidewaysAcceleration;

            delta = Vector3.ClampMagnitude(delta, maxAcceleration);
            _rigidbody.AddForce(delta/_rigidbody.mass, ForceMode2D.Force);
        }

        public void Rotate(float rate) {
            _rigidbody.rotation += rate*Time.smoothDeltaTime;
        }

        public void Fire() {
            if(Time.time - _lastShotTime < minTimeBetweenShots) return;
            _lastShotTime = Time.time;
            GameObject projectile = Instantiate(missilePrefab, _transform.position, _transform.rotation);
            Missile missile = projectile.GetComponent<Missile>();
            if(!missile) return;
            missile.Launch(attackRange, missileSpeed, gameObject);
        }

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _transform = transform;
        }

        private void Update() {
            Rotate(-rotationRate*Input.GetAxis("Rotate"));
            if(Input.GetButton("Fire1")) Fire();
        }

        private void FixedUpdate() {
            Vector2 motion = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            if(motion.magnitude < 1) {
                _rigidbody.velocity /= 2;
            }
            else {
                motion = transform.TransformDirection(motion);
                Move(motion*maxSpeed);
            }
        }

    }
}

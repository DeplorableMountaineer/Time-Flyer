using UnityEngine;
using Weapons;

namespace Player {
    /**
     * The player ship pawn
     */
    [RequireComponent(typeof(Rigidbody2D))] [DisallowMultipleComponent]
    public class Player : MonoBehaviour {
        private Rigidbody2D _rigidbody;
        private float _lastShotTime = 0;
        private Transform _transform;

        public float AttackRange => attackRange;

        [SerializeField] private float maxBackwardAcceleration = 1;
        [SerializeField] private float maxForwardAcceleration = 20;
        [SerializeField] private float maxSidewaysAcceleration = 5;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float rotationRate = 180;
        [SerializeField] private float missileSpeed = 15;
        [SerializeField] private float attackRange = 5;
        [SerializeField] private float minTimeBetweenShots = .5f;
        [SerializeField] private GameObject missilePrefab = null;
        [SerializeField] private float damagePerShot = 50;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _transform = transform;
            Game.Game.Instance.UpdateScore();

            if(Game.Game.Instance.CurrentLevel > 5) { //ship gets better on higher levels
                float multiplier = Mathf.Pow(Game.Game.Instance.CurrentLevel - 5, .25f);
                missileSpeed *= multiplier;
                attackRange *= multiplier;
                damagePerShot *= multiplier;
                minTimeBetweenShots /= multiplier;
                maxSpeed *= multiplier;
            }

            if(!Game.Game.Instance.EasyMode) return;
            missileSpeed *= 2;
            attackRange *= 2;
            damagePerShot *= 2;
            minTimeBetweenShots /= 2;
        }

        private void Update() {
            /*if(Game.Game.Instance.EasyMode) {
                Rotate(-rotationRate*AutopilotRotate);
                if(AutopilotFire) Fire();
                AutopilotFire = false;
                return;
            }*/

            Rotate(-rotationRate*Input.GetAxis("Rotate"));
            if(Input.GetButton("Fire1")) Fire();
        }

        private void FixedUpdate() {
            Vector2 motion = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            /*
            if(Game.Game.Instance.EasyMode) {
                motion = AutopilotMotion;
            }
            */

            if(motion.magnitude < 1) {
                _rigidbody.velocity /= 2;
            }
            else {
                motion = transform.TransformDirection(motion);
                Move(motion*maxSpeed);
            }
        }

        /**
         * Accelerate to target velocity
         */
        private void Move(Vector2 targetVelocity) {
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

        /**
         * Rotate at the given rate
         */
        private void Rotate(float rate) {
            _rigidbody.rotation += rate*Time.smoothDeltaTime;
        }

        /**
         * True when ship is ready to fire (enough time elapsed to "reload")
         */
        public bool ReadyToFire() {
            return Time.time - _lastShotTime >= minTimeBetweenShots;
        }

        /**
         * Fire a missile if ready
         */
        private void Fire() {
            if(!ReadyToFire()) return;
            _lastShotTime = Time.time;
            GameObject projectile = Instantiate(missilePrefab, _transform.position, _transform.rotation);
            Missile missile = projectile.GetComponent<Missile>();
            if(!missile) return;
            missile.Launch(AttackRange, missileSpeed, gameObject, damagePerShot);
        }
    }
}

using System;
using UnityEngine;
using Weapons;
using Random = UnityEngine.Random;

namespace Enemy {
    [RequireComponent(typeof(Steering2D)), DisallowMultipleComponent]
    public class EnemyAi : MonoBehaviour {
        private Steering2D _steering;
        private Rigidbody2D _rigidbody;
        private Rigidbody2D _target;
        private Transform _simpleTarget;
        private AiMode _aiMode = AiMode.Seek;
        private Vector2 _targetDelta = default;
        private float _targetDistance = Mathf.Infinity;
        private float _lastShotTime = 0;
        private Transform _transform;
        private Collider2D _collider;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];

        [SerializeField] private float maxAcceleration = 20;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float rotationRate = 90;
        [SerializeField] private float missileSpeed = 15;
        [SerializeField] private float attackRange = 5;
        [SerializeField] private float minDistanceFromTarget = 2;
        [SerializeField] private float maxDistanceFromTarget = 10;
        [SerializeField] private float minTimeBetweenShots = 1f;
        [SerializeField] private GameObject missilePrefab = null;

        public void SetAsLeader() {
            _aiMode = AiMode.Seek;
        }

        public void SetAsFlock() {
            _aiMode = AiMode.Flock;
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
            _steering = GetComponent<Steering2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _transform = transform;
            FindPlayer();
        }

        private void FixedUpdate() {
            if(!_target) TargetLost();
            MaybeAttack();
            switch(_aiMode) {
                case AiMode.Wander:
                    Wander();
                    break;
                case AiMode.Seek:
                    Seek();
                    if(_targetDistance < minDistanceFromTarget) _aiMode = AiMode.Flee;
                    break;
                case AiMode.Flee:
                    Flee();
                    if(_targetDistance > maxDistanceFromTarget) _aiMode = AiMode.Seek;
                    break;
                case AiMode.Flock:
                    Flock();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TargetLost() {
            if(_aiMode == AiMode.Wander) return;
            FindPlayer();
            if(_target) return;
            if(_aiMode != AiMode.Flock) _aiMode = AiMode.Wander;
        }

        private void MaybeAttack() {
            if(!_target) {
                //cannot attack -- no target
                _targetDistance = Mathf.Infinity;
                _targetDelta = default;
                FaceMovement();
                return;
            }

            _targetDelta = _target.position - _rigidbody.position;
            _targetDistance = _targetDelta.magnitude;
            if(_targetDistance > attackRange) {
                //cannot attack -- out of range
                FaceMovement();
                return;
            }

            //can attack, so face where target will be when missile arrives
            float prediction = attackRange/missileSpeed;
            if(missileSpeed > _targetDistance/prediction) prediction = _targetDistance/missileSpeed;
            _targetDelta += _target.velocity*prediction;
            FaceTarget();

            if(Random.value*minTimeBetweenShots >= Time.smoothDeltaTime) return; //fire irregularly

            //check for obstruction
            int size = Physics2D.RaycastNonAlloc(_transform.position, _targetDelta.normalized,
                _hits, attackRange);
            for(int i = 0; i < size; i++) {
                if(_hits[i].collider == _collider) continue; //don't count self
                if(_hits[i].rigidbody.velocity.magnitude >= missileSpeed*.9f) continue; // don't count missiles
                if(_hits[i].transform == _target.transform) continue; //don't count target
                return;
            }

            Fire();
        }

        private void Flock() {
        }

        private void Wander() {
        }

        private void FaceMovement() {
            Vector2 direction = _rigidbody.velocity;
            float orientation = _steering.Face(direction);
            _steering.Align(orientation, rotationRate,
                1, 5);
        }

        private void FaceTarget() {
            float orientation = _steering.Face(_targetDelta);
            _steering.Align(orientation, rotationRate,
                1, 5);
        }

        private void Seek() {
            Vector2 acceleration;
            if(_target)
                acceleration = _steering.Pursue(_target.position, _target.velocity,
                    maxAcceleration);
            else acceleration = _steering.Seek(_simpleTarget.position, maxAcceleration);
            _steering.UpdateSteering(acceleration, maxSpeed);
        }

        private void Flee() {
            Vector2 acceleration;
            if(_target)
                acceleration = _steering.Evade(_target.position, _target.velocity,
                    maxAcceleration);
            else acceleration = _steering.Flee(_simpleTarget.position, maxAcceleration);
            _steering.UpdateSteering(acceleration, maxSpeed);
        }

        private void FindPlayer() {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if(obj) _target = obj.GetComponent<Rigidbody2D>();
            if(_target) {
                _simpleTarget = _target.transform;
                return;
            }

            Player.Player player = FindObjectOfType<Player.Player>();
            if(player) _target = player.GetComponent<Rigidbody2D>();
            if(_target) {
                _simpleTarget = _target.transform;
                return;
            }

            Camera playerCamera = Camera.main;
            if(playerCamera != null) _target = playerCamera.GetComponent<Rigidbody2D>();
            if(_target) {
                _simpleTarget = _target.transform;
                return;
            }

            _target = null;
            if(obj) {
                _simpleTarget = obj.GetComponent<Transform>();
                return;
            }

            if(player) {
                _simpleTarget = player.GetComponent<Transform>();
                return;
            }

            if(playerCamera != null) {
                _simpleTarget = playerCamera.GetComponent<Transform>();
                return;
            }

            _simpleTarget = null;
        }
    }

    public enum AiMode {
        Wander,
        Seek,
        Flee,
        Flock
    }
}

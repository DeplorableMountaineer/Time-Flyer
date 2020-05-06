using System;
using Pawn;
using UnityEngine;
using Weapons;
using Random = UnityEngine.Random;

//TODO spawn waves of flocks

namespace Enemy {
    [RequireComponent(typeof(Steering2D), typeof(MovingBody)),
     DisallowMultipleComponent]
    public class EnemyAi : MonoBehaviour {
        private Steering2D _steering;
        private Rigidbody2D _rigidbody;
        private MovingBody _movingBody;
        private Rigidbody2D _target;
        private Transform _simpleTarget;
        private AiMode _aiMode = AiMode.Seek;
        private Vector2 _targetDelta = default;
        private float _targetDistance = Mathf.Infinity;
        private float _lastShotTime = 0;
        private Transform _transform;
        private Collider2D _collider;
        private float _wanderState = 0;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];
        private Flock _flock;
        private float _targetSpeed;
        private bool _isLeader = false;

        [SerializeField] private float maxAcceleration = 5;
        [SerializeField] private float maxSpeed = 3;
        [SerializeField] private float fleeSpeed = 6;
        [SerializeField] private float flockSpeed = 6;
        [SerializeField] private float rotationRate = 180;
        [SerializeField] private float missileSpeed = 8;
        [SerializeField] private float attackRange = 8;
        [SerializeField] private float minDistanceFromTarget = 3;
        [SerializeField] private float maxDistanceFromTarget = 7;
        [SerializeField] private float minTimeBetweenShots = 1f;
        [SerializeField] private GameObject missilePrefab = null;
        [SerializeField] private float damagePerShot = 30;
        [SerializeField] private float separationStrength = 10;
        [SerializeField] private float separationThreshold = 4;
        [SerializeField] private float cohesionTargetRadius = 3;
        [SerializeField] private float cohesionSlowRadius = 6;
        [SerializeField] private float collisionAvoidanceThreshold = 2;

        public void SetAsLeader(Flock flock) {
            _aiMode = AiMode.Seek;
            _flock = flock;
            _isLeader = true;
        }

        public void SetAsFlock(Flock flock) {
            _aiMode = AiMode.Flock;
            _flock = flock;
        }

        public void OnHit() {
            if(_target) _aiMode = AiMode.Flee;
        }

        public void OnHealthRestored() {
            if(!_isLeader) _aiMode = AiMode.Flock;
        }

        private void Awake() {
            _targetSpeed = maxSpeed;
            _steering = GetComponent<Steering2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _movingBody = GetComponent<MovingBody>();
            _transform = transform;
            FindPlayer();
        }

        private void FixedUpdate() {
            if(!_target) OnTargetLost();
            MaybeAttack();
            switch(_aiMode) {
                case AiMode.Wander:
                    _targetSpeed = maxSpeed;
                    Wander();
                    break;
                case AiMode.Seek:
                    _targetSpeed = maxSpeed;
                    Seek();
                    if(_targetDistance < minDistanceFromTarget) _aiMode = AiMode.Flee;
                    break;
                case AiMode.Flee:
                    _targetSpeed = fleeSpeed;
                    Flee();
                    if(_targetDistance > maxDistanceFromTarget) _aiMode = AiMode.Seek;
                    break;
                case AiMode.Flock:
                    _targetSpeed = flockSpeed;
                    Flock();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy() {
            _flock.RemoveMember(_rigidbody);
        }

        private void Fire() {
            if(Time.time - _lastShotTime < minTimeBetweenShots) return;
            _lastShotTime = Time.time;
            GameObject projectile = Instantiate(missilePrefab, _transform.position, _transform.rotation);
            Missile missile = projectile.GetComponent<Missile>();
            if(!missile) return;
            missile.Launch(attackRange, missileSpeed, gameObject, damagePerShot, true);
        }

        private void OnTargetLost() {
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

        private void AvoidCollision() {
            Rigidbody2D rb = _movingBody.FindLikeliestCollision(collisionAvoidanceThreshold, out Vector2 direction);
            if(rb) _steering.UpdateSteering(direction*maxAcceleration, fleeSpeed);
        }

        private void Seek() {
            AvoidCollision();
            Vector2 acceleration;
            if(_target)
                acceleration = _steering.Pursue(_target.position, _target.velocity,
                    maxAcceleration);
            else acceleration = _steering.Seek(_simpleTarget.position, maxAcceleration);
            _steering.UpdateSteering(acceleration, _targetSpeed);
        }

        private void Flee() {
            AvoidCollision();
            Vector2 acceleration;
            if(_target)
                acceleration = _steering.Evade(_target.position, _target.velocity,
                    maxAcceleration);
            else acceleration = _steering.Flee(_simpleTarget.position, maxAcceleration);
            _steering.UpdateSteering(acceleration, _targetSpeed);
        }

        private void Flock() {
            if(_flock == null) {
                _aiMode = AiMode.Seek;
                return;
            }

            AvoidCollision();
            Vector2 targetVelocity = _steering.ComputeFlockVelocity(_flock.AsEnumerable());
            Vector2 acceleration = _steering.MatchVelocity(targetVelocity, maxAcceleration);
            acceleration += _steering.Separation(maxAcceleration, _flock.AsEnumerable(), separationThreshold,
                separationStrength);
            acceleration += _steering.Cohesion(maxAcceleration, _flock.AsEnumerable(), _targetSpeed,
                cohesionTargetRadius, cohesionSlowRadius);
            acceleration = Vector2.ClampMagnitude(acceleration, maxAcceleration);
            _steering.UpdateSteering(acceleration, _targetSpeed);
        }

        private void Wander() {
            AvoidCollision();
            Vector2 acceleration = _steering.Wander(3, 2, 45,
                ref _wanderState, maxAcceleration);
            _steering.UpdateSteering(acceleration, _targetSpeed);
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

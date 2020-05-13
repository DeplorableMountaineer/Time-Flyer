using System;
using Pawn;
using UnityEngine;
using Weapons;
using Random = UnityEngine.Random;

namespace Enemy {
    /**
     * Enemy movement and AI goes here
     */
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
        [Tooltip("A little faster than 'maxSpeed' to allow it to catch up with the leader")]
        [SerializeField] private float flockSpeed = 6;
        [SerializeField] private float rotationRate = 180;
        [SerializeField] private float missileSpeed = 8;
        [SerializeField] private float attackRange = 8;
        [Tooltip("If seeking and gets closer than this, switches to fleeing")]
        [SerializeField] private float minDistanceFromTarget = 3;
        [Tooltip("If fleeing and gets farther than this, switches to seeking")]
        [SerializeField] private float maxDistanceFromTarget = 7;
        [SerializeField] private float minTimeBetweenShots = 1f;
        [SerializeField] private GameObject missilePrefab = null;
        [SerializeField] private float damagePerShot = 30;
        [Tooltip("bigger numbers mean more acceleration to separate from other flock members")]
        [SerializeField] private float separationStrength = 10;
        [Tooltip("Start pushing apart if closer than this")]
        [SerializeField] private float separationThreshold = 4;
        [Tooltip("Try to pull together to this distance from others in flock")]
        [SerializeField] private float cohesionTargetRadius = 3;
        [Tooltip("Slow down moving toward flock when this close")]
        [SerializeField] private float cohesionSlowRadius = 6;
        [Tooltip("If closer than this, check for impending collision")]
        [SerializeField] private float collisionAvoidanceThreshold = 2;

        /**
         * Called on spawning as leader or when leader dies and this enemy is promoted to leader
         */
        public void SetAsLeader(Flock flock) {
            _aiMode = AiMode.Seek;
            _flock = flock;
            _isLeader = true;
        }

        /**
         * Called on enemies not being made leader
         */
        public void SetAsFlock(Flock flock) {
            _aiMode = AiMode.Flock;
            _flock = flock;
        }

        /**
         * Call this when hit to make enemy break from flock and flee to have time to heal
         */
        public void OnHit() {
            if(_target) _aiMode = AiMode.Flee;
        }

        /**
         * Call when health is restored so enemy can rejoin flock
         */
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
            if(Game.Game.Instance.EasyMode) {//easy mode is for testing
                maxSpeed /= 2;
                fleeSpeed /= 2;
                flockSpeed /= 2;
                missileSpeed /= 2;
                attackRange /= 2;
                damagePerShot /= 2;
                minTimeBetweenShots *= 2;
            }

            FindPlayer();//find the player entity and target it
        }

        private void FixedUpdate() {
            if(!_target) OnTargetLost();//in case target is lost for some reason
            MaybeAttack();//check if in range and has a clear shot
            
            
            //main AI
            switch(_aiMode) {
                case AiMode.Wander:
                    _targetSpeed = maxSpeed;
                    if(_targetDistance > maxDistanceFromTarget) _aiMode = AiMode.Seek;
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

                    //prevent wayward enemies from just flying away...this happened sometimes
                    if(_targetDistance > maxDistanceFromTarget*2) _aiMode = AiMode.Seek;
                    Flock();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //when destroyed, notify flock
        private void OnDestroy() {
            _flock.RemoveMember(_rigidbody);
        }
        
        //fire a missile if ready to fire
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
            //try to find the player again
            FindPlayer();
            if(_target) return;
            //if all else fails, just wander around (e.g. player may be dead)
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

        /**
         * Face direction ship is moving
         */
        private void FaceMovement() {
            Vector2 direction = _rigidbody.velocity;
            float orientation = _steering.Face(direction);
            _steering.Align(orientation, rotationRate,
                1, 5);
        }

        /**
         * Aim ship at target to shoot
         */
        private void FaceTarget() {
            float orientation = _steering.Face(_targetDelta);
            _steering.Align(orientation, rotationRate,
                1, 5);
        }

        /**
         * Add acceleration to prevent an impending collision
         */
        private void AvoidCollision() {
            Rigidbody2D rb = _movingBody.FindLikeliestCollision(collisionAvoidanceThreshold, out Vector2 direction);
            if(rb) _steering.UpdateSteering(direction*maxAcceleration, fleeSpeed);
        }

        /**
         * Move toward player, with some linear prediction of player's motion
         * _simpletarget is a fallback if for some reason target is missing its rigidbody
         */
        private void Seek() {
            AvoidCollision();
            Vector2 acceleration;
            if(_target)
                acceleration = _steering.Pursue(_target.position, _target.velocity,
                    maxAcceleration);
            else acceleration = _steering.Seek(_simpleTarget.position, maxAcceleration);
            _steering.UpdateSteering(acceleration, _targetSpeed);
        }

        /**
         * Move away from player, taking into account direction player is moving
         */
        private void Flee() {
            AvoidCollision();
            Vector2 acceleration;
            if(_target)
                acceleration = _steering.Evade(_target.position, _target.velocity,
                    maxAcceleration);
            else acceleration = _steering.Flee(_simpleTarget.position, maxAcceleration);
            _steering.UpdateSteering(acceleration, _targetSpeed);
        }

        /**
         * Flocking consists of: separation from flock members, cohesion with flock, matching velocity with
         * the center of mass of the flock, and collision avoidance.  Leader does not use this, but
         * goes on its own, seeking and fleeing.  This pulls the flock with it.
         */
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

        /**
         *  Just wander around
         */
        private void Wander() {
            AvoidCollision();
            Vector2 acceleration = _steering.Wander(3, 2, 45,
                ref _wanderState, maxAcceleration);
            _steering.UpdateSteering(acceleration, _targetSpeed);
        }


        /**
         * Find the player object, with several fallbacks.
         */
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

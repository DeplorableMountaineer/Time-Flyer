using System;
using UnityEngine;

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

        [SerializeField] private float maxAcceleration = 20;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float rotationRate = 90;
        [SerializeField] private float missileSpeed = 15;
        [SerializeField] private float attackRange = 5;
        [SerializeField] private float minDistanceFromTarget = 2;
        [SerializeField] private float maxDistanceFromTarget = 10;
        [SerializeField] private float maxPrediction = 1;

        private void Awake() {
            _steering = GetComponent<Steering2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            FindPlayer();
        }

        private void FixedUpdate() {
            if(!_target) FindPlayer();
            if(!_simpleTarget) return;
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

        private void MaybeAttack() {
            if(!_target) {
                //cannot attack
                _targetDistance = Mathf.Infinity;
                _targetDelta = default;
                FaceMovement();
                return;
            }

            _targetDelta = _target.position - _rigidbody.position;
            _targetDistance = _targetDelta.magnitude;
            if(_targetDistance > attackRange) {
                //cannot attack
                FaceMovement();
                return;
            }

            //can attack
            float prediction = maxPrediction;
            if(missileSpeed > _targetDistance/maxPrediction) prediction = _targetDistance/missileSpeed;
            _targetDelta += _target.velocity*prediction;
            FaceTarget();

            //TODO attack
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

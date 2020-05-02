using UnityEngine;

namespace Enemy {
    [RequireComponent(typeof(Steering2D)), DisallowMultipleComponent]
    public class EnemyAi : MonoBehaviour {
        private Steering2D _steering;
        private Rigidbody2D _rigidbody;
        private Rigidbody2D _target;
        private Transform _simpleTarget;

        [SerializeField] private float maxAcceleration = 20;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float rotationRate = 90;

        private void Awake() {
            _steering = GetComponent<Steering2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            FindPlayer();
        }

        private void FixedUpdate() {
            if(!_simpleTarget) return;
            Vector2 direction = _rigidbody.velocity;
            float orientation = _steering.Face(direction);
            _steering.Align(orientation, rotationRate,
                1, 5);

            Vector2 acceleration;
            if(!_target) FindPlayer();
            if(_target)
                acceleration = _steering.Pursue(_target.position, _target.velocity,
                    maxAcceleration);
            else acceleration = _steering.Seek(_simpleTarget.position, maxAcceleration);
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
}

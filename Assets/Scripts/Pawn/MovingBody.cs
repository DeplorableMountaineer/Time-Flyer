using UnityEngine;

namespace Pawn {
    [RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
    public class MovingBody : MonoBehaviour {
        private Rigidbody2D _rigidbody;
        private AllMovingBodies _allMovingBodies;

        public Rigidbody2D FindClosest(float collisionAvoidanceThreshold, out Vector2 avoidanceDirection) {
            avoidanceDirection = default;
            return !_allMovingBodies
                ? null
                : _allMovingBodies.FindClosest(_rigidbody, collisionAvoidanceThreshold, out avoidanceDirection);
        }

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _allMovingBodies = FindObjectOfType<AllMovingBodies>();
        }

        private void OnEnable() {
            if(_allMovingBodies) _allMovingBodies.OnSpawn(_rigidbody);
        }

        private void OnDisable() {
            if(_allMovingBodies) _allMovingBodies.OnDeath(_rigidbody);
        }
    }
}

using UnityEngine;

namespace Pawn {
    /**
     * A player, enemy, or missile
     */
    [RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
    public class MovingBody : MonoBehaviour {
        private Rigidbody2D _rigidbody;
        private AllMovingBodies _allMovingBodies;

        /**
         * Search all moving bodies to find one this one will likely collide with
         */
        public Rigidbody2D FindLikeliestCollision(float collisionAvoidanceThreshold, out Vector2 avoidanceDirection) {
            avoidanceDirection = default;
            return !_allMovingBodies
                ? null
                : _allMovingBodies.FindLikeliestCollision(_rigidbody, collisionAvoidanceThreshold,
                    out avoidanceDirection);
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

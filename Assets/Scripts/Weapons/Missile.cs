using UnityEngine;

namespace Weapons {
    [RequireComponent(typeof(Rigidbody2D))] [DisallowMultipleComponent]
    public class Missile : MonoBehaviour {
        private Rigidbody2D _rigidbody;
        private Collider2D _spawner;
        private bool _isLaunched = false;
        private bool _isEnemyMissile = false;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Launch(float range, float speed, GameObject spawner, bool isEnemyMissile=false) {
            _spawner = spawner.GetComponent<Collider2D>();
            Destroy(gameObject, range/speed);
            _rigidbody.velocity = speed*transform.up;
            _isEnemyMissile = isEnemyMissile;
            _isLaunched = true;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if(!_isLaunched) return;
            if(other == _spawner || other.transform == transform) return; //do not shoot self
            if(_isEnemyMissile && !other.CompareTag("Player")) return; //do not shoot teammates
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}

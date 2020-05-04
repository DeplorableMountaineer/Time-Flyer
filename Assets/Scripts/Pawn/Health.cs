using UnityEngine;

namespace Pawn {
    public class Health : MonoBehaviour {
        private float _health;
        [SerializeField] private float startingHealth = 100;
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float healingRate = 10;

        public void TakeDamage(float amount) {
            _health -= amount;
            if(_health < 0) Die();
            enabled = _health < maxHealth;
        }

        public void Die() {
            _health = 0;
            Camera playerCamera = GetComponentInChildren<Camera>();
            if(playerCamera) playerCamera.transform.SetParent(null, true);
            Destroy(gameObject);
        }

        private void Start() {
            _health = startingHealth;
            enabled = _health < maxHealth;
        }

        private void Update() {
            float newHealth = _health + healingRate*Time.smoothDeltaTime;
            if(newHealth >= maxHealth) {
                enabled = false;
                _health = maxHealth;
                return;
            }

            _health = newHealth;
        }
    }
}

using UnityEngine;

namespace Pawn {
    public class Health : MonoBehaviour {
        private float _health;
        [SerializeField] private float startingHealth = 100;
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float healingRate = 10;
        [SerializeField] private ParticleSystem flames = null;

        public void TakeDamage(float amount) {
            _health -= amount;
            if(_health < 0) Die();
            enabled = _health < maxHealth;
            SetFlameProperties();
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
            SetFlameProperties();
        }

        private void Update() {
            float newHealth = _health + healingRate*Time.smoothDeltaTime;
            if(newHealth >= maxHealth) {
                if(enabled) {
                    SetFlameProperties();
                    enabled = false;
                }

                _health = maxHealth;
                return;
            }

            if(Mathf.Abs(_health - newHealth) < .0001f) return;
            SetFlameProperties();
            _health = newHealth;
        }

        private void SetFlameProperties() {
            if(!flames) return;
            if(!enabled) {
                if(flames.isPlaying) flames.Stop();
                return;
            }

            if(!flames.isPlaying) flames.Play();
            ParticleSystem.MainModule main = flames.main;
            main.maxParticles = Mathf.FloorToInt(Mathf.Pow(1000, 1 - _health/maxHealth));
        }
    }
}

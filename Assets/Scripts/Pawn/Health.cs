using System.Globalization;
using Enemy;
using TMPro;
using UnityEngine;

namespace Pawn {
    public class Health : MonoBehaviour {
        private float _health;
        private EnemyAi _enemySelf;
        private Game.Game _gameInstance;
        [SerializeField] private float startingHealth = 100;
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float healingRate = 10;
        [SerializeField] private ParticleSystem flames = null;
        [SerializeField] private TextMeshProUGUI displayText = null;

        public void TakeDamage(float amount) {
            _health -= amount;
            if(_health < 0) Die();
            UpdateDisplay();
            enabled = _health < maxHealth;
            SetFlameProperties();
            if(_enemySelf) _enemySelf.OnHit();
        }

        public void Die() {
            _health = 0;
            UpdateDisplay();
            if(_enemySelf) {
                _gameInstance.AddKill();
            }
            else {
                Camera playerCamera = GetComponentInChildren<Camera>();
                if(playerCamera) playerCamera.transform.SetParent(null, true);
                if(CompareTag("Player")) Game.Game.Instance.GameOver();
            }

            Destroy(gameObject);
        }

        private void Awake() {
            _enemySelf = GetComponent<EnemyAi>();
        }

        private void Start() {
            _health = startingHealth;
            enabled = _health < maxHealth;
            SetFlameProperties();
            UpdateDisplay();
            _gameInstance = Game.Game.Instance; //ensure Game is initialized at beginning
        }

        private void Update() {
            float newHealth = _health + healingRate*Time.smoothDeltaTime;
            if(newHealth >= maxHealth) {
                if(_enemySelf) _enemySelf.OnHealthRestored();
                if(enabled) {
                    enabled = false;
                    SetFlameProperties();
                }

                _health = maxHealth;
                UpdateDisplay();
                return;
            }

            if(Mathf.Abs(_health - newHealth) < .0001f) return;
            SetFlameProperties();
            _health = newHealth;
            UpdateDisplay();
        }

        private void UpdateDisplay() {
            if(!displayText) return;
            displayText.text = Mathf.Floor(_health).ToString(CultureInfo.InvariantCulture);
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

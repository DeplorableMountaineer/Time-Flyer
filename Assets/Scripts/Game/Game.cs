using TMPro;
using UnityEngine;
using Utilities;
//TODO  pause screen, checkpointing, death screen
namespace Game {
    public class Game : Singleton<Game> {
        private int _kills = 0;
        private int _currentLevel = -1;

        [SerializeField] private TextMeshProUGUI killsText = null;
        [SerializeField] private Levels levels = null;

        public int Kills {
            get => _kills;
            set {
                _kills = value;
                UpdateKillsDisplay();
            }
        }

        public void NextScene() {
            _currentLevel++;
            levels.LoadLevel(_currentLevel);
        }

        public void AddKill() {
            Kills++;
        }

        private void Reset() {
            UpdateKillsDisplay();
        }

        private void OnValidate() {
            UpdateKillsDisplay();
        }

        /// <summary>
        ///     Use this for initialization.
        /// </summary>
        protected override void Awake() {
            base.Awake();
            Debug.Assert(levels != null);
        }

        private void Start() {
            UpdateKillsDisplay();
        }

        private void UpdateKillsDisplay() {
            if(!killsText) {
                //may need to find kills text because this is a persistent singleton.
                GameObject go = GameObject.FindGameObjectWithTag("Kills Text");
                if(!go) return;
                killsText = go.GetComponent<TextMeshProUGUI>();
                if(!killsText) return;
            }

            killsText.text = Kills.ToString();
        }
    }
}

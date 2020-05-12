using TMPro;
using UnityEngine;
using Utilities;

namespace Game {
    public class Game : Singleton<Game> {
        private const string HighScore = "High Score";

        private int _score = 0;
        private int _currentLevel = -1;

        [SerializeField] private TextMeshProUGUI killsText = null;
        [SerializeField] private Levels levels = null;

        public int Score {
            get => _score;
            set {
                _score = value;
                UpdateKillsDisplay();
            }
        }

        public void GameOver() {
            float highScore = GetHighScore();
            if(_score > highScore) SetHighScore(_score);
            _currentLevel = -1;
            _score = 0;
            Invoke(nameof(LoadStartScene), 5);
        }


        public void FirstScene() {
            _currentLevel = 0;
            _score = 0;
            levels.LoadLevel(_currentLevel);
        }

        public void NextScene() {
            _currentLevel++;
            levels.LoadLevel(_currentLevel);
        }

        public void AddToScore(int amount) {
            Score += amount;
        }

        public float GetHighScore() {
            if(PlayerPrefs.HasKey(HighScore)) return PlayerPrefs.GetFloat(HighScore);
            return 0;
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

        private void SetHighScore(float score) {
            PlayerPrefs.SetFloat(HighScore, score);
            PlayerPrefs.Save();
        }

        private void LoadStartScene() {
            levels.LoadStartScene();
        }

        private void UpdateKillsDisplay() {
            if(!killsText) {
                //may need to find kills text because this is a persistent singleton.
                GameObject go = GameObject.FindGameObjectWithTag("Kills Text");
                if(!go) return;
                killsText = go.GetComponent<TextMeshProUGUI>();
                if(!killsText) return;
            }

            killsText.text = Score.ToString();
        }
    }
}

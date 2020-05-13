using TMPro;
using UnityEngine;
using Utilities;

namespace Game {
    /**
     * A singleton that keeps track of overall state of the game, persistent between levels
     */
    public class Game : Singleton<Game> {
        private const string HighScore = "High Score";

        private int _score = 0;
        public int CurrentLevel { get; private set; } = -1;

        [SerializeField] private TextMeshProUGUI killsText = null;
        [SerializeField] private Levels levels = null;
        [SerializeField] private GameObject warpEffectPrefab = null;
        [SerializeField, Tooltip("For testing; only works in editor")]
        private bool easyMode = false;

        public int Score {
            get => _score;
            set {
                _score = value;
                UpdateScore();
            }
        }

        public bool EasyMode => easyMode;

        /**
         * When game is over, wait a bit then load the title screen
         */
        public void GameOver() {
            float highScore = GetHighScore();
            if(_score > highScore) SetHighScore(_score);
            CurrentLevel = -1;
            _score = 0;
            Invoke(nameof(LoadStartScene), 5);
        }


        /**
         * Load the first time zone of the game
         */
        public void FirstScene() {
            CurrentLevel = 0;
            _score = 0;
            levels.LoadLevel(CurrentLevel);
        }

        /**
         * Teleport to the next timezone of the game.
         */
        public void NextScene() {
            Player.Player player = FindObjectOfType<Player.Player>();
            if(player) {
                Instantiate(warpEffectPrefab, player.transform.position,
                    Quaternion.Euler(-90, 0, 0));
            }

            CurrentLevel++;
            Invoke(nameof(LoadTheLevel), 2);
        }

        /**
         * Actually load the next level after showing time warp effect
         */
        private void LoadTheLevel() {
            levels.LoadLevel(CurrentLevel);
        }

        /**
         * Update score
         */
        public void AddToScore(int amount) {
            Score += amount;
        }

        /**
         * Used to retrieve the high score from persistent storage
         */
        public float GetHighScore() {
            if(PlayerPrefs.HasKey(HighScore)) return PlayerPrefs.GetFloat(HighScore);
            return 0;
        }

        private void Reset() {
            UpdateScore();
        }

        private void OnValidate() {
            UpdateScore();
        }

        /// <summary>
        ///     Use this for initialization.
        /// </summary>
        protected override void Awake() {
            //only allow easy mode when testing in editor
            if(EasyMode && !Application.isEditor) easyMode = false;
            base.Awake();
            Debug.Assert(levels != null);
        }

        private void Start() {
            UpdateScore();
        }

        /**
         * Store a new high score
         */
        private void SetHighScore(float score) {
            PlayerPrefs.SetFloat(HighScore, score);
            PlayerPrefs.Save();
        }

        /**
         * Load the title screen
         */
        private void LoadStartScene() {
            levels.LoadStartScene();
        }

        /**
         * update the score display
         */
        public void UpdateScore() {
            if(!killsText) {
                //may need to find kills text because this is a persistent singleton and
                //level may have changed.
                GameObject go = GameObject.FindGameObjectWithTag("Kills Text");
                if(!go) return;
                killsText = go.GetComponent<TextMeshProUGUI>();
                if(!killsText) return;
            }

            killsText.text = Score.ToString();
        }
    }
}

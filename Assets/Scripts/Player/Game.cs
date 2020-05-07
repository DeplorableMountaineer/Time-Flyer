using TMPro;
using UnityEngine;
using Utilities;

namespace Player {
    public class Game : Singleton<Game> {
        private int _kills = 0;

        [SerializeField] private TextMeshProUGUI killsText = null;

        public int Kills {
            get => _kills;
            set {
                _kills = value;
                UpdateDisplay();
            }
        }

        public void AddKill() {
            Kills++;
        }

        private void Start() {
            UpdateDisplay();
        }

        private void UpdateDisplay() {
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

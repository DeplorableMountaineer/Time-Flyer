using TMPro;
using UnityEngine;

namespace UI {
    /**
     * Show high score on title screen
     */
    public class HighScore : MonoBehaviour {
        void Start() {
            float score = Game.Game.Instance.GetHighScore();
            TextMeshProUGUI scoreText = GetComponent<TextMeshProUGUI>();
            scoreText.text = Mathf.RoundToInt(score).ToString();
            if(score < 0.5f) transform.parent.gameObject.SetActive(false);
        }
    }
}

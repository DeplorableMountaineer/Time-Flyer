using TMPro;
using UnityEngine;

namespace UI {
    public class HighScore : MonoBehaviour {
        void Start() {
            float score = Game.Game.Instance.GetHighScore();
            TextMeshProUGUI scoreText = GetComponent<TextMeshProUGUI>();
            scoreText.text = Mathf.RoundToInt(score).ToString();
            if(score < 0.5f) transform.parent.gameObject.SetActive(false);
        }
    }
}

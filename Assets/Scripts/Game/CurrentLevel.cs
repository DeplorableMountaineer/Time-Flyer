using TMPro;
using UnityEngine;

namespace Game {
    public class CurrentLevel : MonoBehaviour {
        [SerializeField] private string timeZome = "3000";
        [SerializeField] private float timeScale = 1;

        private void Start() {
            GameObject go = GameObject.FindGameObjectWithTag("Time Zone Text");
            if(go) {
                TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
                if(text) text.text = timeZome;
            }

            if(Game.Instance.CurrentLevel < 11) Time.timeScale = timeScale;
            else Time.timeScale = timeScale*Mathf.Ceil(Game.Instance.CurrentLevel/10f);
        }
    }
}

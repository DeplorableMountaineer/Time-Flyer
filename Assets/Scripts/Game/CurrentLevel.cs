using TMPro;
using UnityEngine;

namespace Game {
    /**
     * Data for the current level
     */
    public class CurrentLevel : MonoBehaviour {
        [SerializeField] private string timeZone = "3000";
        [SerializeField] private float timeScale = 1;

        private void Start() {
            GameObject go = GameObject.FindGameObjectWithTag("Time Zone Text");
            if(go) {
                TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
                if(Game.Instance.CurrentLevel >= 10) {
                    int time = Mathf.RoundToInt(Random.Range(-1000, 1000))*100;
                    if(time == 0) timeZone = "1 AD";
                    else if(time > 0 && time < 1001) timeZone = time.ToString() + " AD";
                    else if(time > 0) timeZone = time.ToString();
                    else timeZone = (-time).ToString() + " BC";
                }

                if(text) text.text = timeZone;
            }

            //Speed things up on later levels
            if(Game.Instance.CurrentLevel < 10) Time.timeScale = timeScale;
            else Time.timeScale = timeScale + Mathf.Ceil(Game.Instance.CurrentLevel/10f);
        }
    }
}

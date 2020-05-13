using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game {
    /**
     * Data for the current level
     */
    public class CurrentLevel : MonoBehaviour {
        [FormerlySerializedAs("TimeZone")] [FormerlySerializedAs("timeZome")] [SerializeField]
        private string timeZone = "3000";
        [SerializeField] private float timeScale = 1;

        private void Start() {
            GameObject go = GameObject.FindGameObjectWithTag("Time Zone Text");
            if(go) {
                TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
                if(text) text.text = timeZone;
            }

            //Speed things up on later levels
            if(Game.Instance.CurrentLevel < 11) Time.timeScale = timeScale;
            else Time.timeScale = timeScale*Mathf.Ceil(Game.Instance.CurrentLevel/10f);
            Debug.Log("Time Scale: " + Time.timeScale);
        }
    }
}

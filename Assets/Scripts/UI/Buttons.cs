using UnityEngine;

namespace UI {
    public class Buttons : MonoBehaviour {

        /**
         * Start game when play button is pressed
         */
        public void PlayButton() {
            Game.Game.Instance.FirstScene();
        }
    }
}

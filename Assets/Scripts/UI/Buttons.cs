using UnityEngine;

namespace UI {
    public class Buttons : MonoBehaviour {
        public void PlayButton() {
            Game.Game.Instance.FirstScene();
        }
    }
}

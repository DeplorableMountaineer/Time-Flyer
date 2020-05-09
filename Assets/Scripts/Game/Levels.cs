using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {
    [CreateAssetMenu(fileName = "Levels", menuName = "Levels", order = 0)]
    public class Levels : ScriptableObject {
        [SerializeField] private string[] levels = null;

        public void LoadLevel(int num) {
            SceneManager.LoadScene(levels[num%levels.Length]);
        }

        public void LoadStartScene() {
            SceneManager.LoadScene("Title");

        }
    }
}

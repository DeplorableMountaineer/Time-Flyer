using UnityEngine;

namespace Background {
    /**
     * Move object with the camera, but do not rotate with camera.  This allows a background to simulate an
     * infinite-distance background to simulate parallax.
     */
    public class FollowCamera : MonoBehaviour {
        [SerializeField] private Camera playerCamera = null;

        private void Reset() {
            if(playerCamera == null) playerCamera = Camera.main; //guess the camera if not already set
        }

        private void Awake() {
            if(playerCamera == null) playerCamera = Camera.main; //guess the camera if not already set
        }

        private void Update() {
            Vector2 pos = playerCamera.transform.position;
            transform.position = pos;
        }
    }
}

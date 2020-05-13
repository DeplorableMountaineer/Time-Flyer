using UnityEngine;

namespace Background {
    /**
     * Move the background image with the camera, but do not rotate it with camera.
     * This allows a background to appear to be an
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

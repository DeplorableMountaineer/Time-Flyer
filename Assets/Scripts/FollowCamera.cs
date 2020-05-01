using UnityEngine;

public class FollowCamera : MonoBehaviour {
    [SerializeField] private Camera playerCamera = null;

    private void Reset() {
        if(playerCamera == null) playerCamera = Camera.main;
    }

    private void Update() {
        Vector2 pos = playerCamera.transform.position;
        transform.position = pos;
    }
}

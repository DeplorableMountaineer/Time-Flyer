using System;
using UnityEngine;

public class FollowCamera : MonoBehaviour {
    [SerializeField] private Camera camera = null;

    private void Reset() {
        if(camera == null) camera = Camera.main;
    }

    private void Update() {
        Vector2 pos = camera.transform.position;
        transform.position = pos;
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
public class Player : MonoBehaviour {
    private Rigidbody2D _rigidbody;

    [SerializeField] private float maxForwardAcceleration = 20;
    [SerializeField] private float maxSidewaysAcceleration = 5;
    [SerializeField] private float maxBackwardAcceleration = 1;
    [SerializeField] private float maxSpeed = 5;
    [SerializeField] private float rotationRate = 180;

    public void Move(Vector2 targetVelocity) {
        Vector3 delta = (targetVelocity - _rigidbody.velocity)/Time.deltaTime;
        float orientation = Mathf.Atan2(targetVelocity.y, targetVelocity.x)*Mathf.Rad2Deg;
        float maxAcceleration = (orientation > 45 && orientation < 135)
            ? maxForwardAcceleration
            : ((orientation < -45 && orientation > -135)
                ? maxBackwardAcceleration
                : maxSidewaysAcceleration);

        delta = Vector3.ClampMagnitude(delta, maxAcceleration);
        _rigidbody.AddForce(delta/_rigidbody.mass, ForceMode2D.Force);
    }

    public void Rotate(float rate) {
        _rigidbody.rotation += rate*Time.deltaTime;
    }

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        Rotate(rotationRate*Input.GetAxis("Rotate"));
    }

    private void FixedUpdate() {
        Vector2 motion = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        if(motion.magnitude < 1) {
            _rigidbody.velocity /= 2;
        }
        else {
            motion = transform.TransformDirection(motion);
            Move(motion*maxSpeed);
        }
    }
}

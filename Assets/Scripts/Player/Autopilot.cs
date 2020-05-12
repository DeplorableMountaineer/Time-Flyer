using System.Linq;
using Enemy;
using Pawn;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(Player)), DisallowMultipleComponent]
    public class Autopilot : MonoBehaviour {
        private Player _player;
        private Health _health;
        private AllMovingBodies _allMovingBodies;
        private Rigidbody2D _rigidbody;
        private Transform _transform;
        private Wave _wave;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];

        private void Awake() {
            _player = GetComponent<Player>();
            _health = GetComponent<Health>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _transform = transform;
            _allMovingBodies = FindObjectOfType<AllMovingBodies>();
            _wave = FindObjectOfType<Wave>();
            
            //Autopilot is broke
           // enabled = Game.Game.Instance.EasyMode;
        }

        private void Update() {
            _allMovingBodies.FindLikeliestCollision(_rigidbody, 3,
                out Vector2 motion);
            if(_health.HealthPercentage > .75f)
                _player.AutopilotMotion = (motion + (Vector2)_transform.up).normalized;
            else
                _player.AutopilotMotion = (motion - (Vector2)_transform.up).normalized;
            _player.AutopilotRotate = 0;
            if(!_wave) return;

            //find a target to aim at
            Vector2 bestDirection = default;
            float bestDotProduct = Mathf.NegativeInfinity;
            foreach(GameObject go in _wave.Flocks) {
                if(!go) continue;
                Rigidbody2D first = go.GetComponent<Flock>().AsEnumerable().First();
                if(!first) continue;
                Vector2 delta = first.transform.position - _transform.position;
                float distance = delta.magnitude;
                if((distance > _player.AttackRange && bestDirection != default) || distance <= 0) continue;
                Vector2 direction = delta/distance;
                float dotProduct = Vector2.Dot(direction, _transform.up);
                if(!(dotProduct > bestDotProduct)) continue;
                bestDirection = direction;
                bestDotProduct = dotProduct;
            }

            if(bestDirection != default) {
                if(bestDotProduct < .999f) {
                    //rotate toward new direction
                    float target = Mathf.Atan2(bestDirection.y, bestDirection.x)*Mathf.Rad2Deg;
                    float diff = (target - _rigidbody.rotation)%360;
                    if(diff > 180) diff -= 360;
                    else if(diff < -180) diff += 360;
                    _player.AutopilotRotate = Mathf.Sign(diff);
                }
            }

            if(!_player.ReadyToFire()) return;
            int size = Physics2D.RaycastNonAlloc(_transform.position, _transform.up,
                _hits, _player.AttackRange);
            for(int i = 0; i < size; i++) {
                if(_hits[i].transform == _transform) continue; //don't count self
                if(_hits[i].rigidbody.velocity.magnitude >= 8*.9f) continue; // don't count missiles

                //has target
                _player.AutopilotFire = true;
                break;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Pawn;
using UnityEngine;

namespace Enemy {
    public class Flock : MonoBehaviour {
        private Transform _transform;
        private Wave _wave;
        private AllMovingBodies _allMovingBodies;
        private readonly List<Rigidbody2D> _flock = new List<Rigidbody2D>();

        [SerializeField] private int numShips = 3;
        [SerializeField] private GameObject leaderPrefab = null;
        [SerializeField] private GameObject supportPrefab = null;

        public IEnumerable<Rigidbody2D> AsEnumerable() {
            return _flock.AsEnumerable();
        }

        public void SetWave(Wave wave) {
            _wave = wave;
        }

        public void RemoveMember(Rigidbody2D rb) {
            if(!this) return;
            _flock.Remove(rb);
            if(_flock.Count == 0) {
                Game.Game.Instance.AddToScore(numShips*5);
                _wave.OnDeath(gameObject);
                if(!this) return;
                Destroy(gameObject);
                return;
            }

            _flock[0].GetComponent<EnemyAi>().SetAsLeader(this);
        }

        private void Awake() {
            _transform = transform;
            _allMovingBodies = FindObjectOfType<AllMovingBodies>();
            if(Game.Game.Instance.CurrentLevel > 10) {
                numShips += 3;
            }
        }

        private void Start() {
            Vector2 position = AvoidSpawnOnOthers(transform.position);
            Quaternion rotation = _transform.rotation;
            SpawnLeader(position, rotation);
            for(int i = 1; i < numShips; i++) {
                position += new Vector2(2*(Random.value - Random.value),
                    2*(Random.value - Random.value)); //2D bernoulli distribution
                position = AvoidSpawnOnOthers(position);
                SpawnFlock(position, rotation);
            }
        }

        private Vector2 AvoidSpawnOnOthers(Vector2 position) {
            Vector2 newPosition = position;
            bool colliding = true;
            while(colliding) {
                colliding = false;
                if(!_allMovingBodies.Bodies.Any(body => !(Vector2.Distance(body.position, newPosition) > 1f))) continue;
                colliding = true;
                newPosition += new Vector2(2*(Random.value - Random.value),
                    2*(Random.value - Random.value)); //2D bernoulli distribution
            }

            return newPosition;
        }

        private void SpawnLeader(Vector2 pos, Quaternion rot) {
            GameObject go = Instantiate(leaderPrefab, pos, rot);
            EnemyAi ship = go.GetComponent<EnemyAi>();
            ship.SetAsLeader(this);
            _flock.Add(ship.GetComponent<Rigidbody2D>());
        }

        private void SpawnFlock(Vector2 pos, Quaternion rot) {
            GameObject go = Instantiate(supportPrefab, pos, rot);
            EnemyAi ship = go.GetComponent<EnemyAi>();
            ship.SetAsFlock(this);
            _flock.Add(ship.GetComponent<Rigidbody2D>());
        }
    }
}

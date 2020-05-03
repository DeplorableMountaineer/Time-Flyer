using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Enemy {
    public class Flock : MonoBehaviour {
        private Transform _transform;
        private readonly List<Rigidbody2D> _flock = new List<Rigidbody2D>();

        [SerializeField] private int numShips = 3;
        [SerializeField] private GameObject leaderPrefab = null;
        [SerializeField] private GameObject supportPrefab = null;

        public IEnumerable<Rigidbody2D> AsEnumerable() {
            return _flock.AsEnumerable();
        }

        public void RemoveMember(Rigidbody2D rb) {
            _flock.Remove(rb);
            if(_flock.Count == 0) {
                if(this) Destroy(gameObject);
                return;
            }

            _flock[0].GetComponent<EnemyAi>().SetAsLeader(this);
        }

        private void Awake() {
            _transform = transform;
        }

        private void Start() {
            Vector2 position = transform.position;
            Quaternion rotation = _transform.rotation;
            SpawnLeader(position, rotation);
            for(int i = 1; i < numShips; i++) {
                position += new Vector2(Random.value - Random.value, Random.value - Random.value)*2;
                SpawnFlock(position, rotation);
            }
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

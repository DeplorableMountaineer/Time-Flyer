using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Enemy {
    /**
     * Each level (time zone) has (at least one) a wave.  A wave spanws a specific number of flocks
     * (formations).
     * When the entire wave is destroyed by the player, the player teleports to a new timezone (next level).
     */
    public class Wave : MonoBehaviour {
        private readonly List<GameObject> _flocks = new List<GameObject>();
        private int _toSpawn = 0;

        [SerializeField] private FlockData[] flockData = null;
        [SerializeField] private TextMeshProUGUI formationsText = null;
        public List<GameObject> Flocks => _flocks;

        /**
         * Called when the last member of a flock is killed
         */
        public void OnDeath(GameObject go) {
            Flocks.Remove(go);
            UpdateDisplay();
            if(Flocks.Count != 0) return;
            Game.Game.Instance.AddToScore(50);
            if(!this) return;
            Destroy(gameObject);
            if(_toSpawn >= flockData.Length) EndOfWave();
        }

        private void Start() {
            SpawnNext(null);
        }

        /**
        * Keep track of number of formations (flocks) attacking
        */
        private void UpdateDisplay() {
            if(!formationsText) return;
            formationsText.text = (Flocks.Count - (Flocks.Count > 0 && Flocks[0] == null ? 1 : 0)).ToString();
        }

        /**
         * Spawn the next formation of the wave
         */
        private void SpawnNext(GameObject go) {
            if(Flocks.Count > 0 && !go) { //a failed spawn
                _toSpawn = flockData.Length + 1;
            }
            else Flocks.Add(go);

            UpdateDisplay();
            if(flockData == null || _toSpawn >= flockData.Length) {
                if(Flocks[0] == null) Flocks.RemoveAt(0);
                return;
            }

            //do it again
            StartCoroutine(flockData[_toSpawn].SpawnFlockInBackground(this, SpawnNext));
            _toSpawn++;
        }

        /**
         * Entire wave wiped out by player, so teleport player to next timezone
         */
        private void EndOfWave() {
            Game.Game.Instance.NextScene();
        }
    }

    [Serializable]
    public class FlockData {
        [SerializeField] private float delayBeforeSpawn = 0;
        [SerializeField] private GameObject flockPrefab = null;
        [SerializeField] private Vector2 startPosition = default;
        [SerializeField] private float startRotation = 0;

        /**
         * Spawn a flock, but return immediately; flock spawns after a delay; onCompletion
         * is invoked at the end
         */
        public IEnumerator SpawnFlockInBackground(Wave wave, Action<GameObject> onCompletion) {
            yield return new WaitForSeconds(
                Game.Game.Instance.CurrentLevel > 10 ? delayBeforeSpawn/3 : delayBeforeSpawn);
            GameObject go = Object.Instantiate(flockPrefab, startPosition,
                Quaternion.Euler(0, 0, startRotation));
            if(!go) {
                Debug.Log("Failed to spawn flock");
                onCompletion?.Invoke(null);
                yield break;
            }

            Flock flock = go.GetComponent<Flock>();
            if(flock) flock.SetWave(wave);
            onCompletion?.Invoke(go);
        }
    }
}

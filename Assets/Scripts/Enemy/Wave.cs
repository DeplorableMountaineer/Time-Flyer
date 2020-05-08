using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Enemy {
    public class Wave : MonoBehaviour {
        private int _toSpawn = 0;
        private readonly List<GameObject> _flocks = new List<GameObject>();

        [SerializeField] private FlockData[] flockData = null;
        [SerializeField] private TextMeshProUGUI formationsText = null;

        public void OnDeath(GameObject go) {
            _flocks.Remove(go);
            UpdateDisplay();
            if(_flocks.Count != 0) return;
            if(!this) return;
            Destroy(gameObject);
            if(_toSpawn >= flockData.Length) EndOfWave();
        }

        private void Start() {
            SpawnNext(null);
        }


        private void UpdateDisplay() {
            if(!formationsText) return;
            formationsText.text = (_flocks.Count - (_flocks.Count > 0 && _flocks[0] == null ? 1 : 0)).ToString();
        }

        private void SpawnNext(GameObject go) {
            _flocks.Add(go);
            UpdateDisplay();
            if(flockData == null || _toSpawn >= flockData.Length) {
                if(_flocks[0] == null) _flocks.RemoveAt(0);
                return;
            }

            StartCoroutine(flockData[_toSpawn].SpawnFlockInBackground(this, SpawnNext));
            _toSpawn++;
        }

        private void EndOfWave() {
            Game.Game.Instance.NextScene();
        }
    }

    [Serializable]
    public class FlockData {
        [SerializeField] private GameObject flockPrefab = null;
        [SerializeField] private Vector2 startPosition = default;
        [SerializeField] private float startRotation = 0;
        [SerializeField] private float delayBeforeSpawn = 0;

        public IEnumerator SpawnFlockInBackground(Wave wave, Action<GameObject> onCompletion) {
            yield return new WaitForSeconds(delayBeforeSpawn);
            GameObject go = Object.Instantiate(flockPrefab, startPosition,
                Quaternion.Euler(0, 0, startRotation));
            Flock flock = go.GetComponent<Flock>();
            if(flock) flock.SetWave(wave);
            onCompletion?.Invoke(go);
        }
    }
}

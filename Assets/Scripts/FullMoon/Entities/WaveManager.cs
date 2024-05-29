using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FullMoon.Entities.Unit;
using FullMoon.UI;
using FullMoon.Util;
using MyBox;
using UnityEngine;

namespace FullMoon.Entities
{
    [Serializable]
    public class EnemyDetail
    {
        public GameObject enemy;
        public int count = 1;
        [DefinedValues("Random", "Left", "Right", "Up", "Down")] public string location = "Random";
    }

    [Serializable]
    public class Wave
    {
        public int level = 1;
        public List<EnemyDetail> enemyDetails;
    }
    
    public class WaveManager : MonoBehaviour
    {
        [ReadOnly] private int currentLevel;
        [SerializeField] private float spawnDistance = 20f;
        [SerializeField] private float spawnInterval = 15f;
        [SerializeField] private List<Wave> waves;
        
        private readonly List<BaseUnitController> enemyWaitList = new();

        private void Start()
        {
            SpawnWaveAsync().Forget();
        }

        private async UniTaskVoid SpawnWaveAsync()
        {
            while (currentLevel < waves.Max(w => w.level))
            {
                bool enemyAlive = enemyWaitList.Any(e => e.Alive);
                
                if (enemyAlive)
                {
                    await UniTask.DelayFrame(1);
                    continue;
                }
                
                enemyWaitList.Clear();
                
                await DisplayCountdown(spawnInterval);
                
                currentLevel++;
                var currentWave = GetRandomWave();
                SpawnWaveTextAsync(2f).Forget();
                await SpawnEnemies(currentWave);
            }
        }
        
        private async UniTaskVoid SpawnWaveTextAsync(float displayTime)
        {
            MainUIController.Instance.PhaseElement.SetVisible(true);
            MainUIController.Instance.PhaseText.text = $"Wave {currentLevel}";
            await UniTask.Delay(TimeSpan.FromSeconds(displayTime));
            MainUIController.Instance.PhaseElement.SetVisible(false);
        }

        private async UniTask DisplayCountdown(float interval)
        {
            float remainingTime = interval;
            MainUIController.Instance.PhaseElement.SetVisible(true);

            while (remainingTime > 0)
            {
                MainUIController.Instance.PhaseText.text = $"다음 전투까지 {remainingTime:F1}초";
                await UniTask.DelayFrame(1);
                remainingTime -= Time.deltaTime;
            }

            MainUIController.Instance.PhaseElement.SetVisible(false);
        }

        private Wave GetRandomWave()
        {
            var currentLevelWaves = waves.Where(w => w.level == currentLevel).ToList();
            return currentLevelWaves[UnityEngine.Random.Range(0, currentLevelWaves.Count)];
        }

        private async UniTask SpawnEnemies(Wave wave)
        {
            foreach (var enemyDetail in wave.enemyDetails)
            {
                for (int i = 0; i < enemyDetail.count; i++)
                {
                    Vector3 spawnPosition = GetSpawnPosition(enemyDetail.location);
                    var unit = ObjectPoolManager.Instance.SpawnObject(enemyDetail.enemy, spawnPosition, Quaternion.identity).GetComponent<BaseUnitController>();
                    enemyWaitList.Add(unit);
                }

                await UniTask.DelayFrame(enemyDetail.count);

                foreach (var unit in enemyWaitList)
                {
                    unit.MoveToPosition(transform.position);
                }
            }
        }

        private Vector3 GetSpawnPosition(string location)
        {
            Vector3 direction = Vector3.zero;

            float randomXY = UnityEngine.Random.Range(-spawnDistance, spawnDistance);
            Vector3 randomSpawnPosition = Vector3.zero;
            
            switch (location)
            {
                case "Left":
                    direction = Vector3.left;
                    randomSpawnPosition = new Vector3(0f, 0f, randomXY);
                    break;
                case "Right":
                    direction = Vector3.right;
                    randomSpawnPosition = new Vector3(0f, 0f, randomXY);
                    break;
                case "Up":
                    direction = Vector3.forward;
                    randomSpawnPosition = new Vector3(randomXY, 0f, 0f);
                    break;
                case "Down":
                    direction = Vector3.back;
                    randomSpawnPosition = new Vector3(randomXY, 0f, 0f);
                    break;
                default:
                    int randomDirection = UnityEngine.Random.Range(0, 4);
                    switch (randomDirection)
                    {
                        case 0:
                            direction = Vector3.left;
                            randomSpawnPosition = new Vector3(0f, 0f, randomXY);
                            break;
                        case 1:
                            direction = Vector3.right;
                            randomSpawnPosition = new Vector3(0f, 0f, randomXY);
                            break;
                        case 2:
                            direction = Vector3.forward;
                            randomSpawnPosition = new Vector3(randomXY, 0f, 0f);
                            break;
                        case 3:
                            direction = Vector3.back;
                            randomSpawnPosition = new Vector3(randomXY, 0f, 0f);
                            break;
                    }
                    break;
            }

            return (transform.position + direction * spawnDistance) + randomSpawnPosition;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Vector3 center = transform.position;
            Vector3 size = new Vector3(spawnDistance * 2, 1, spawnDistance * 2);

            Gizmos.DrawWireCube(center, size);
        }
    }
}

using MyBox;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using FullMoon.UI;
using FullMoon.Util;
using FullMoon.Entities.Unit;

namespace FullMoon.Entities
{
    [Serializable]
    public class EnemyDetail
    {
        public GameObject enemy;
        public int count = 1;
        public float spawnInterval;
        [DefinedValues("Random", "Left", "Right", "Up", "Down")] public string location = "Random";
    }

    [Serializable]
    public class Wave
    {
        public int level = 1;
        public List<EnemyDetail> enemyDetails;
    }
    
    [Serializable]
    public class ButtonUnlock
    {
        public string buttonName;
        public int unlockWave = 1;
        public Button unlockButton;
    }
    
    public class WaveManager : MonoBehaviour
    {
        [ReadOnly] private int currentLevel;
        [SerializeField] private float spawnDistance = 20f;
        [SerializeField] private float spawnInterval = 15f;

        [Separator] 
        
        [SerializeField] private GameObject CraftingButton;
        
        [Separator]
        
        [SerializeField] private List<ButtonUnlock> buildingUnlock;
        [SerializeField] private List<Wave> waves;

        private CancellationTokenSource cancellationTokenSource;
        
        private readonly List<BaseUnitController> enemyWaitList = new();

        private void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            buildingUnlock.ForEach(b => b.unlockButton.interactable = false);
            SpawnWaveAsync(cancellationTokenSource.Token).Forget();
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        private async UniTaskVoid SpawnWaveAsync(CancellationToken cancellationToken)
        {
            await UniTask.NextFrame(cancellationToken);
            while (currentLevel < waves.Max(w => w.level))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                bool enemyAlive = enemyWaitList.Any(e => e.Alive);
                
                if (enemyAlive)
                {
                    await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                    continue;
                }
                
                enemyWaitList.Clear();
                
                MainUIController.Instance.DayCountText.text = $"{currentLevel + 1}";
                
                CraftingButton.SetActive(true);
                
                buildingUnlock.Where(b => currentLevel + 1 >= b.unlockWave)
                    .ForEach(b => b.unlockButton.interactable = true);
                
                await DisplayCountdown(spawnInterval, cancellationToken);
                
                CraftingButton.SetActive(false);
                
                currentLevel++;
                var currentWave = GetRandomWave();
                SpawnWaveTextAsync(5f, cancellationToken).Forget();
                await SpawnEnemies(currentWave, cancellationToken);
            }
        }
        
        private async UniTaskVoid SpawnWaveTextAsync(float displayTime, CancellationToken cancellationToken)
        {
            MainUIController.Instance.BattleIcon.SetVisible(true);
            MainUIController.Instance.RestIcon.SetVisible(false);
            MainUIController.Instance.BattlePhase.SetVisible(true, 1f);
            MainUIController.Instance.BattleDetailText.text = $"WAVE {currentLevel:00}";
            await UniTask.Delay(TimeSpan.FromSeconds(displayTime), cancellationToken: cancellationToken);
            MainUIController.Instance.BattlePhase.SetVisible(false, 1f);
        }

        private async UniTask DisplayCountdown(float interval, CancellationToken cancellationToken)
        {
            MainUIController.Instance.BattleIcon.SetVisible(false);
            MainUIController.Instance.RestIcon.SetVisible(true);
            MainUIController.Instance.RestPhase.SetVisible(true, 1f);
            MainUIController.Instance.RestDetailText.text = $"다음 전투까지 {interval:F1}초";
            await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: cancellationToken);
            MainUIController.Instance.RestPhase.SetVisible(false, 1f);
            
            float remainingTime = interval;

            while (remainingTime > 0)
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (remainingTime > 5f)
                {
                    await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                    remainingTime -= Time.deltaTime;
                    continue;
                }

                MainUIController.Instance.RestPhase.SetVisible(!(remainingTime <= 1f), 1f);

                MainUIController.Instance.RestDetailText.text = $"다음 전투까지 {remainingTime:F1}초";
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                remainingTime -= Time.deltaTime;
            }
        }

        private Wave GetRandomWave()
        {
            var currentLevelWaves = waves.Where(w => w.level == currentLevel).ToList();
            return currentLevelWaves[UnityEngine.Random.Range(0, currentLevelWaves.Count)];
        }

        private async UniTask SpawnEnemies(Wave wave, CancellationToken cancellationToken)
        {
            foreach (var enemyDetail in wave.enemyDetails)
            {
                for (int i = 0; i < enemyDetail.count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    Vector3 spawnPosition = GetSpawnPosition(enemyDetail.location);
                    var unit = ObjectPoolManager.Instance.SpawnObject(enemyDetail.enemy, spawnPosition, Quaternion.identity).GetComponent<BaseUnitController>();
                    enemyWaitList.Add(unit);
                    await UniTask.NextFrame(cancellationToken);
                    unit.MoveToPosition(transform.position);
                    await UniTask.Delay(TimeSpan.FromSeconds(enemyDetail.spawnInterval), cancellationToken: cancellationToken);
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


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Vector3 center = transform.position;
            Vector3 size = new Vector3(spawnDistance * 2, 1, spawnDistance * 2);

            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}
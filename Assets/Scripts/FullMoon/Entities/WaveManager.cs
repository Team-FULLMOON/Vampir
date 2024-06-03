using MyBox;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FullMoon.Camera;
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
        
        [Separator]
        
        public bool changeButtonText;
        [ConditionalField(nameof(changeButtonText))] public string enableText;
        [ConditionalField(nameof(changeButtonText))] public string disableText;

        [Separator]
        
        public Color selectedColor = Color.green;
        public Color unselectedColor = Color.white;
        
        [Separator]
        
        public BuildingType buildingType;
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
        
        private CameraController cameraController;

        private void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cameraController = FindObjectOfType<CameraController>();
            DisableAllUnlockButton();
            buildingUnlock.ForEach(b =>  b.unlockButton.onClick.AddListener(() => OnCraftingButtonClicked(b)));
            SpawnWaveAsync(cancellationTokenSource.Token).Forget();
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
        
        public void DisableAllUnlockButton()
        {
            cameraController.CreateTileSetting(false, BuildingType.None);
            foreach (var button in buildingUnlock)
            {
                button.unlockButton.GetComponent<Image>().color = button.unselectedColor;
            }
        }
        
        private void OnCraftingButtonClicked(ButtonUnlock buttonUnlock)
        {
            DisableAllUnlockButton();
            if (buttonUnlock.unlockWave > currentLevel + 1)
            {
                ToastManager.Instance.ShowToast($"{buttonUnlock.buttonName} 건설은 웨이브 <size=54>{buttonUnlock.unlockWave}</size>에서 해제됩니다", "#FF7C7F");
                return;
            }
            buttonUnlock.unlockButton.GetComponent<Image>().color = buttonUnlock.selectedColor;
            cameraController.CreateTileSetting(true, buttonUnlock.buildingType);
        }
        
        private void UpdateCraftingButton()
        {
            foreach (var button in buildingUnlock)
            {
                if (button.unlockWave <= currentLevel + 1)
                {
                    if (button.changeButtonText)
                    {
                        button.unlockButton.GetComponentInChildren<Text>().text = $"{button.buttonName}\n{button.enableText}";
                    }
                }
                else
                {
                    if (button.changeButtonText)
                    {
                        button.unlockButton.GetComponentInChildren<Text>().text = $"{button.buttonName}\n<color=\"red\">{button.disableText}</color>";
                    }
                }
            }
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
                
                CraftingButton.SetActive(true);
                
                UpdateCraftingButton();
                
                if (currentLevel == 10)
                {
                    MainUIController.Instance.VictoryPhase.SetVisible(true, 1f);
                    await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: cancellationToken);
                    MainUIController.Instance.VictoryPhase.SetVisible(false, 1f);
                    await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);
                }
                
                MainUIController.Instance.DayCountText.text = $"{currentLevel + 1}";
                
                await DisplayCountdown(spawnInterval, cancellationToken);
                
                CraftingButton.SetActive(false);
                DisableAllUnlockButton();
                
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
            int enemyCount = wave.enemyDetails.Sum(e => e.count);
            MainUIController.Instance.ChangeEnemyAmount(enemyCount);
            
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
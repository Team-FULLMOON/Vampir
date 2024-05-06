using Cysharp.Threading.Tasks;
using FullMoon.Util;
using UnityEngine;

namespace FullMoon.Entities.Unit
{
    public class UnitSpawnerDummy : MonoBehaviour
    {
        public GameObject unitPrefab;
        public float spawnRadius = 10f;
        public int unitsToSpawn = 3;
        public float spawnInterval = 5f;
        
        private void Start()
        {
            RepeatSpawnUnitsAsync().Forget();
        }

        private async UniTaskVoid RepeatSpawnUnitsAsync()
        {
            while (true)
            {
                SpawnAllUnitsOnce();
                await UniTask.Delay(System.TimeSpan.FromSeconds(spawnInterval));
            }
        }

        private void SpawnAllUnitsOnce()
        {
            for (int i = 0; i < unitsToSpawn; i++)
            {
                Vector3 randomDirection = Random.insideUnitCircle.normalized;
                Vector3 spawnPosition = transform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * spawnRadius;
                
                var unit = ObjectPoolManager.SpawnObject(unitPrefab, spawnPosition, Quaternion.identity).GetComponent<BaseUnitController>();
                unit.OnStartEvent.AddEvent(() => unit.MoveToPosition(transform.position));
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
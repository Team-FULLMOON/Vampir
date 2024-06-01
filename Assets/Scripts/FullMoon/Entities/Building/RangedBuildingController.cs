using MyBox;
using System;
using Cysharp.Threading.Tasks;
using Unity.Burst;
using UnityEngine;
using FullMoon.Util;
using FullMoon.ScriptableObject;

namespace FullMoon.Entities.Building
{
    [BurstCompile]
    public class RangedBuildingController : BaseBuildingController
    {
        [Foldout("Ranged Building Settings")]
        public GameObject spawnUnitObject;

        public RangedBuildingData OverridenBuildingData { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenBuildingData = buildingData as RangedBuildingData;

            ShowFrame(3f).Forget();
            SpawnUnit(3f).Forget();
        }

        private async UniTaskVoid SpawnUnit(float delay = 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            if (spawnUnitObject != null)
            {
                ObjectPoolManager.Instance.SpawnObject(spawnUnitObject, transform.position, Quaternion.identity);
            }
        }
    }
}

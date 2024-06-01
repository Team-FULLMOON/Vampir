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
    public class MeleeBuildingController : BaseBuildingController
    {
        [Foldout("Melee Building Settings")]
        public GameObject spawnUnitObject;

        public MeleeBuildingData OverridenBuildingData { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenBuildingData = buildingData as MeleeBuildingData;

            ShowFrame(5f).Forget();
            SpawnUnit(5f).Forget();
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
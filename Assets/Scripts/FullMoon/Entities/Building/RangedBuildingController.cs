using FullMoon.ScriptableObject;
using FullMoon.Util;
using MyBox;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

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

            SpawnUnit();
        }

        public void SpawnUnit()
        {
            ObjectPoolManager.Instance.SpawnObject(spawnUnitObject, targetPos, Quaternion.identity);
        }

        public override void Die()
        {
            base.Die();
        }

        public override void Select()
        {
            base.Select();
        }

        public override void Deselect()
        {
            base.Deselect();
        }
    }
}

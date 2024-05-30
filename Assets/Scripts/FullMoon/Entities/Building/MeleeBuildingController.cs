using FullMoon.ScriptableObject;
using FullMoon.Util;
using MyBox;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

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

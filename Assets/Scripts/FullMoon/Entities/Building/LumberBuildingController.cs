using FullMoon.Entities.Unit;
using UnityEngine;
using Unity.Burst;

namespace FullMoon.Entities.Building
{
    [BurstCompile]
    public class LumberBuildingController : BaseBuildingController
    {
        protected override void OnEnable()
        {
            base.OnEnable();
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

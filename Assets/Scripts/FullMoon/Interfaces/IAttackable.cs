using UnityEngine;
using FullMoon.Entities.Unit;
using System.Collections.Generic;

namespace FullMoon.Interfaces
{
    public interface IAttackable
    {
        List<BaseUnitController> UnitInsideViewArea { get; set; }
        void EnterViewRange(Collider unit);
        void ExitViewRange(Collider unit);
        void ExecuteAttack(Transform location);
    }
}
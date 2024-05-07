using UnityEngine;
using FullMoon.Entities.Unit;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FullMoon.Interfaces
{
    public interface IAttackable
    {
        List<BaseUnitController> UnitInsideViewArea { get; set; }
        void EnterViewRange(Collider unit);
        void ExitViewRange(Collider unit);
        UniTaskVoid ExecuteAttack(Transform location);
    }
}
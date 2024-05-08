using UnityEngine;
using FullMoon.Entities.Unit;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FullMoon.Interfaces
{
    public interface IAttackable
    {
        void EnterViewRange(Collider unit);
        void ExitViewRange(Collider unit);
        UniTaskVoid ExecuteAttack(Transform location);
    }
}
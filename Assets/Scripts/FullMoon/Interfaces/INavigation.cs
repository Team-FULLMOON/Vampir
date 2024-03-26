using UnityEngine;
using UnityEngine.AI;

namespace FullMoon.Interfaces
{
    public interface INavigation
    {
        NavMeshAgent Agent { get; set; }
        void MoveToPosition(Transform location);
    }
}


using UnityEngine;
using UnityEngine.AI;

namespace FullMoon.Interfaces
{
    public interface INavigation
    {
        NavMeshAgent Agent { get; set; }
        Vector3 LatestDestination { get; set; }
        void MoveToPosition(Vector3 location);
    }
}


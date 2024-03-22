using System;
using UnityEngine;
using UnityEngine.AI;

namespace FullMoon.Unit
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField] GameObject unitMarker;
        [SerializeField] float speed;
        private Vector3 movePos;

        private NavMeshAgent agent;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void SelectUnit()
        {
            unitMarker.SetActive(true);
        }

        public void DeSelectUnit()
        {
            unitMarker.SetActive(false);
        }

        public void MoveTo(Vector3 pos)
        {
            agent.SetDestination(pos);
        }
    }
}

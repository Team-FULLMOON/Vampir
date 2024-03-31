using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class RangedUnitMove : IState
    {
        private readonly RangedUnitController controller;

        public RangedUnitMove(RangedUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            Debug.Log($"{controller.name} Move Enter");
            controller.Agent.isStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
        }

        public void Execute()
        {
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                Debug.Log($"{controller.name} Destination reached.");
                controller.Agent.isStopped = true; 
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
                return;
            }
            
            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => controller.unitType.Equals(t.unitType))
                .Where(t => t.Agent.isStopped)
                .Where(t => Mathf.Approximately(controller.LatestDestination.x, t.LatestDestination.x)
                            && Mathf.Approximately(controller.LatestDestination.y, t.LatestDestination.y)
                            && Mathf.Approximately(controller.LatestDestination.z, t.LatestDestination.z))
                .FirstOrDefault(t => Vector3.Distance(controller.transform.position, t.transform.position) <= 2f);
            
            if (closestUnit != null)
            {
                Debug.Log($"{controller.name} Destination Near By reached.");
                controller.Agent.isStopped = true; 
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
            }
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            Debug.Log($"{controller.name} Move Exit");
        }
    }
}

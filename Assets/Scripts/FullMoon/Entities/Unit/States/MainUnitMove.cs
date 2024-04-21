using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.Input;

namespace FullMoon.Entities.Unit.States
{
    public class MainUnitMove : IState
    {
        private readonly MainUnitController controller;

        public MainUnitMove(MainUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Agent.isStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
        }

        public void Execute()
        {
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                controller.StateMachine.ChangeState(new MainUnitIdle(controller));
                return;
            }
            
            if (controller.ReviveTarget != null)
            {
                foreach (var unit in controller.RespawnUnitInsideViewArea
                    .Select(u => u.GetComponent<RespawnController>())
                    .Where(c => c != null))
                {
                    controller.CheckAbleToRespawn(controller.ReviveTarget);
                }
            }

            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => controller.UnitType.Equals(t.UnitType))
                .Where(t => t.Agent.isStopped)
                .Where(t => Mathf.Approximately(controller.LatestDestination.x, t.LatestDestination.x)
                            && Mathf.Approximately(controller.LatestDestination.y, t.LatestDestination.y)
                            && Mathf.Approximately(controller.LatestDestination.z, t.LatestDestination.z))
                .FirstOrDefault(t => Vector3.Distance(controller.transform.position, t.transform.position) <= 2f);
            
            if (closestUnit != null)
            {
                controller.StateMachine.ChangeState(new MainUnitIdle(controller));
            }
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            controller.Agent.isStopped = true; 
        }
    }
}

using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class CommonUnitMove : IState
    {
        private readonly CommonUnitController controller;
        private static readonly int MoveHash = Animator.StringToHash("Move");

        public CommonUnitMove(CommonUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Agent.isStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
            controller.SetAnimation(MoveHash);
        }

        [BurstCompile]
        public void Execute()
        {
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
                return;
            }
            
            var unitsInView = controller.UnitInsideViewArea;
            var ownTypeUnits = unitsInView.Where(t => controller.UnitType.Equals(t.UnitType) && t.Agent.isStopped);
            var destination = controller.LatestDestination;
            
            if (Vector3.Distance(controller.MainUnit.transform.position, destination) <= controller.viewRange.radius)
            {
                controller.MoveToPosition(controller.MainUnit.transform.position);
                return;
            }

            BaseUnitController closestUnit = ownTypeUnits.FirstOrDefault(t =>
                Mathf.Approximately(destination.x, t.LatestDestination.x) &&
                Mathf.Approximately(destination.y, t.LatestDestination.y) &&
                Mathf.Approximately(destination.z, t.LatestDestination.z) &&
                Vector3.Distance(controller.transform.position, destination) <= controller.viewRange.radius &&
                Vector3.Distance(controller.transform.position, t.transform.position) <= 2f);

            if (closestUnit != null)
            {
                controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
            }
        }

        public void FixedExecute() { }

        public void Exit()
        {
            controller.Agent.isStopped = true; 
        }
    }
}

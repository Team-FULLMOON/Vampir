using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Burst;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class HammerUnitMove : IState
    {
        private readonly HammerUnitController controller;
        private CancellationTokenSource cts;

        public HammerUnitMove(HammerUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Agent.isStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
            controller.SetAnimation(BaseUnitController.MoveHash);
        }
        
        [BurstCompile]
        public void Execute()
        {
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                controller.StateMachine.ChangeState(new HammerUnitIdle(controller));
                return;
            }
            
            var unitsInView = controller.Flag != null ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea;
            var ownTypeUnits = unitsInView.Where(t => controller.UnitType.Equals(t.UnitType) && t.Agent.isStopped);
            var destination = controller.LatestDestination;

            BaseUnitController closestUnit = ownTypeUnits.FirstOrDefault(t =>
                Mathf.Approximately(destination.x, t.LatestDestination.x) &&
                Mathf.Approximately(destination.y, t.LatestDestination.y) &&
                Mathf.Approximately(destination.z, t.LatestDestination.z) &&
                Vector3.Distance(controller.transform.position, t.transform.position) <= 2f);

            if (closestUnit != null)
            {
                controller.StateMachine.ChangeState(new HammerUnitIdle(controller));
            }
        }

        public void FixedExecute() { }

        public void Exit()
        {
            controller.Agent.isStopped = true; 
        }
    }
}
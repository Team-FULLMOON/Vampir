using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
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
            controller.AnimationController.SetAnimation("Move", 0.1f);
        }

        [BurstCompile]
        public void Execute()
        {
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                controller.StateMachine.ChangeState(new MainUnitIdle(controller));
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
                controller.StateMachine.ChangeState(new MainUnitIdle(controller));
                return;
            }

            if (controller.UnitType is "Player" or "Enemy")
            {
                closestUnit = unitsInView
                    .Where(t => !controller.UnitType.Equals(t.UnitType))
                    .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                    .FirstOrDefault();

                if (closestUnit == null) return;

                float sqrViewRadius = controller.OverridenUnitData.ViewRadius * controller.OverridenUnitData.ViewRadius;
                if ((closestUnit.transform.position - controller.transform.position).sqrMagnitude <= sqrViewRadius)
                {
                    controller.StateMachine.ChangeState(new MainUnitChase(controller));
                }
            }
        }

        public void FixedExecute() { }

        public void Exit()
        {
            controller.Agent.isStopped = true; 
        }
    }
}

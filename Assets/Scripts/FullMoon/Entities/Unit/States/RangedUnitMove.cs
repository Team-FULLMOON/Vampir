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
            controller.Agent.isStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
        }

        public void Execute()   
        {
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                Debug.Log($"{controller.Agent.remainingDistance}");
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
                return;
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
                controller.AttackMove = false;
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
                return;
            }

            if (controller.AttackMove)
            {
                closestUnit = controller.UnitInsideViewArea
                    .Where(t => !controller.UnitType.Equals(t.UnitType))
                    .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                    .FirstOrDefault();

                if (closestUnit == null)
                {
                    return;
                }

                bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                            controller.OverridenUnitData.ViewRadius * controller.OverridenUnitData.ViewRadius;

                if (checkDistance)
                {
                    controller.StateMachine.ChangeState(new RangedUnitChase(controller));
                }
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

using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class RangedUnitAttack : IState
    {
        private readonly RangedUnitController controller;
        private float timer;

        public RangedUnitAttack(RangedUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            controller.Rb.velocity = Vector3.zero;
            timer = controller.OverridenUnitData.AttackDelay;
        }

        public void Execute()
        {
            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new RangedUnitMove(controller));
                return;
            }
            
            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                                 controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            
            if (checkDistance == false)
            {
                controller.StateMachine.ChangeState(new RangedUnitChase(controller));
                return;
            }
            
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                return;
            }
            
            controller.ExecuteAttack(closestUnit.transform);
            timer = controller.OverridenUnitData.AttackSpeed;
        }

        public void FixedExecute()
        {
        }

        public void Exit()
        {
        }
    }
}

using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class RangedUnitAttack : IState
    {
        private readonly RangedUnitController controller;
        private float attackDelay;

        public RangedUnitAttack(RangedUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            attackDelay = controller.OverridenUnitData.AttackDelay;
        }

        public void Execute()
        {
            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
                return;
            }
            
            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                                 controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            
            if (checkDistance == false)
            {
                controller.StateMachine.ChangeState(new RangedUnitChase(controller));
                return;
            }
            
            if (attackDelay > 0)
            {
                attackDelay -= Time.deltaTime;
                return;
            }
            
            if (controller.CurrentAttackCoolTime > 0)
            {
                return;
            }

            controller.CurrentAttackCoolTime = controller.OverridenUnitData.AttackCoolTime;
            controller.ExecuteAttack(closestUnit.transform);
        }

        public void FixedExecute() { }

        public void Exit() { }
    }
}

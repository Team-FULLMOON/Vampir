using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MeleeUnitAttack : IState
    {
        private readonly MeleeUnitController controller;
        private float attackDelay;

        public MeleeUnitAttack(MeleeUnitController controller)
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
                controller.StateMachine.ChangeState(new MeleeUnitIdle(controller));
                return;
            }
            
            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                                 controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            
            if (checkDistance == false)
            {
                controller.StateMachine.ChangeState(new MeleeUnitChase(controller));
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

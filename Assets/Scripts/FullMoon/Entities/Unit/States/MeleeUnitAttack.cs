using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MeleeUnitAttack : IState
    {
        private readonly MeleeUnitController controller;
        private float timer;

        public MeleeUnitAttack(MeleeUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
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

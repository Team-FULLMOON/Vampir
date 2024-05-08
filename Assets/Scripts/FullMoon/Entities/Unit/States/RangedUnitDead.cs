using Cysharp.Threading.Tasks;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.Util;

namespace FullMoon.Entities.Unit.States
{
    public class RangedUnitDead : IState
    {
        private readonly RangedUnitController controller;

        public RangedUnitDead(RangedUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            DisableAfterAnimation(Animator.StringToHash("Dead"));
        }

        public void Execute() { }

        public void FixedExecute() { }

        public void Exit() { }

        private async UniTask DisableAfterAnimation(int animationHash)
        {
            if (controller.SetAnimation(animationHash))
            {
                await UniTask.WaitUntil(() => 
                {
                    var stateInfo = controller.unitAnimator.GetCurrentAnimatorStateInfo(0);
                    return stateInfo.shortNameHash.Equals(animationHash) && stateInfo.normalizedTime >= 1.0f;
                });
                await UniTask.Delay(500);
                ObjectPoolManager.Instance.ReturnObjectToPool(controller.gameObject);
            }
            else
            {
                ObjectPoolManager.Instance.ReturnObjectToPool(controller.gameObject);
            }
        }
    }
}

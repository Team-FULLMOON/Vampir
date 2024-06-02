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
            DisableAfterAnimation("Dead").Forget();
        }

        public void Execute() { }

        public void FixedExecute() { }

        public void Exit() { }

        private async UniTask DisableAfterAnimation(string animationName)
        {
            try
            {
                if (controller.AnimationController.SetAnimation(animationName))
                {
                    await UniTask.WaitUntil(() => 
                    {
                        var stateInfo = controller.unitAnimator.GetCurrentAnimatorStateInfo(0);
                        if (controller.AnimationController.CurrentStateInfo.Item1 != animationName)
                        {
                            controller.AnimationController.SetAnimation(animationName);
                        }
                        return (controller.AnimationController.CurrentStateInfo.Item1 == animationName && stateInfo.normalizedTime >= 1.0f) || stateInfo.loop;
                    });
                    await UniTask.Delay(500);
                }
            }
            finally
            {
                ObjectPoolManager.Instance.ReturnObjectToPool(controller.gameObject);
            }
        }
    }
}
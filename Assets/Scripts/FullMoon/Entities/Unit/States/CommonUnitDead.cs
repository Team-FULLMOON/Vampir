using Cysharp.Threading.Tasks;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.Util;

namespace FullMoon.Entities.Unit.States
{
    public class CommonUnitDead : IState
    {
        private readonly CommonUnitController controller;

        public CommonUnitDead(CommonUnitController controller)
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
                        return (controller.AnimationController.CurrentStateInfo.Item1 == animationName && stateInfo.normalizedTime >= 1.0f);
                    });
                    await UniTask.Delay(1000);
                }
            }
            finally
            {
                ObjectPoolManager.Instance.ReturnObjectToPool(controller.gameObject);
            }
        }
    }
}
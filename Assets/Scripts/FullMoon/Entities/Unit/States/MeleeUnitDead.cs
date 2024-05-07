using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MeleeUnitDead : IState
    {
        private readonly MeleeUnitController controller;

        public MeleeUnitDead(MeleeUnitController controller)
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
                controller.gameObject.SetActive(false);
            }
            else
            {
                controller.gameObject.SetActive(false);
            }
        }
    }
}
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Burst;
using FullMoon.FSM;
using FullMoon.Util;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class CommonUnitMove : IState
    {
        private readonly CommonUnitController controller;
        private CancellationTokenSource cts;

        public CommonUnitMove(CommonUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.IsStopped = true;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;

            cts = new CancellationTokenSource();
            Shock(cts.Token).Forget();
        }
        
        private async UniTask Shock(CancellationToken token)
        {
            if (controller.AnimationController.SetAnimation("Shock", 0f))
            {
                int shockHash = Animator.StringToHash("Shock");
                try
                {
                    await UniTask.WaitUntil(() => 
                    {
                        var stateInfo = controller.unitAnimator.GetCurrentAnimatorStateInfo(0);
                        return stateInfo.shortNameHash == shockHash && stateInfo.normalizedTime >= 1f;
                    }, cancellationToken: token);
                }
                catch
                {
                    controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
                    return;
                }
            }
            controller.moveDustEffect.SetActive(true);
            controller.IsStopped = false;
        }
        
        [BurstCompile]
        public void Execute()
        {
            if (!controller.Agent.enabled || controller.IsStopped)
            {
                return;
            }
            
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                if (controller.IsCraft)
                {
                    ObjectPoolManager.Instance.ReturnObjectToPool(controller.gameObject);
                    ObjectPoolManager.Instance.SpawnObject(controller.hammerPrefab, controller.transform.position, controller.transform.rotation);
                    controller.IsCraft = false;
                    return;
                }

                controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
                return;
            }
            
            var unitsInView = controller.UnitInsideViewArea;
            var ownTypeUnits = unitsInView.Where(t => controller.UnitType.Equals(t.UnitType) && t.IsStopped);
            var destination = controller.LatestDestination;
            
            BaseUnitController closestUnit = ownTypeUnits.FirstOrDefault(t =>
                Mathf.Approximately(destination.x, t.LatestDestination.x) &&
                Mathf.Approximately(destination.y, t.LatestDestination.y) &&
                Mathf.Approximately(destination.z, t.LatestDestination.z) &&
                Vector3.Distance(controller.transform.position, destination) <= controller.viewRange.radius &&
                Vector3.Distance(controller.transform.position, t.transform.position) <= 3f);

            if (closestUnit != null)
            {
                if (controller.IsCraft)
                {
                    ObjectPoolManager.Instance.ReturnObjectToPool(controller.gameObject);
                    ObjectPoolManager.Instance.SpawnObject(controller.hammerPrefab, controller.transform.position, controller.transform.rotation);
                    controller.IsCraft = false;
                }
                else
                {
                    controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
                }
            }
        }

        public void FixedExecute() { }

        public void Exit()
        {
            cts?.Cancel();
            controller.moveDustEffect.SetActive(false);
            controller.IsStopped = true;
        }
    }
}
using MyBox;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using FullMoon.Util;
using FullMoon.Effect;
using FullMoon.Interfaces;
using FullMoon.Entities.Unit.States;
using FullMoon.ScriptableObject;
using Unity.Burst;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent)), BurstCompile]
    public class RangedUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Ranged Unit Settings")]
        public DecalProjector decalProjector;
        
        [Foldout("Ranged Unit Settings")]
        public GameObject attackEffect;

        public RangedUnitData OverridenUnitData { get; private set; }
        
        public float CurrentAttackCoolTime { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenUnitData = unitData as RangedUnitData;
            CurrentAttackCoolTime = unitData.AttackCoolTime;

            var triggerEvent = viewRange.GetComponent<ColliderTriggerEvents>();
            if (triggerEvent is not null)
            {
                float worldRadius = viewRange.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

                var units = Physics.OverlapSphere(transform.position + viewRange.center, worldRadius)
                    .Where(t => triggerEvent.GetFilterTags.Contains(t.tag) && t.gameObject != gameObject)
                    .Where(t => t.GetComponent<BaseUnitController>() is not null)
                    .ToList();

                foreach (var unit in units)
                {
                    UnitInsideViewArea.Add(unit.GetComponent<BaseUnitController>());
                }
            }
            
            if (decalProjector is not null)
            {
                decalProjector.gameObject.SetActive(false);
                decalProjector.size = new Vector3(unitData.AttackRadius * 2f, unitData.AttackRadius * 2f, decalProjector.size.z);
            }

            StateMachine.ChangeState(new RangedUnitIdle(this));
        }

        [BurstCompile]
        protected override void Update()
        {
            ReduceAttackCoolTime();
            UnitInsideViewArea.RemoveWhere(unit => unit is null || !unit.gameObject.activeInHierarchy || !unit.Alive);
            base.Update();
        }
        
        public override void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (StateMachine.CurrentState is RangedUnitIdle)
            {
                MoveToPosition(attacker.transform.position);
                OnUnitStateTransition(attacker);
            }
            base.ReceiveDamage(amount, attacker);
        }

        public override void Die()
        {
            base.Die();
            StateMachine.ChangeState(new RangedUnitDead(this));
        }

        public void EnterViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller is null)
            {
                return;
            }
            UnitInsideViewArea.Add(controller);
        }

        public void ExitViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller is null)
            {
                return;
            }
            UnitInsideViewArea.Remove(controller);
        }

        public async UniTaskVoid ExecuteAttack(Transform target)
        {
            Vector3 targetDirection = target.transform.position - transform.position;

            transform.forward = targetDirection.normalized;
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);
            
            SetAnimation(Animator.StringToHash("Attack"));
            
            await UniTask.DelayFrame(OverridenUnitData.HitAnimationFrame);
            
            GameObject bullet = ObjectPoolManager.Instance.SpawnObject(attackEffect, transform.position, Quaternion.identity);
            bullet.GetComponent<BulletEffectController>().Fire(target, transform, OverridenUnitData.BulletSpeed, OverridenUnitData.AttackDamage);
        }

        public override void Select()
        {
            base.Select();
            decalProjector.gameObject.SetActive(true);
        }

        public override void Deselect()
        {
            base.Deselect();
            decalProjector.gameObject.SetActive(false);
        }
        
        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            StateMachine.ChangeState(new RangedUnitMove(this));
        }

        public override void OnUnitStop()
        {
            base.OnUnitStop();
            StateMachine.ChangeState(new RangedUnitIdle(this));
        }

        [BurstCompile]
        public override void OnUnitStateTransition(BaseUnitController target)
        {
            base.OnUnitStateTransition(target);
            
            List<BaseUnitController> transitionControllers = UnitInsideViewArea
                .Where(t => UnitType.Equals(t.UnitType))
                .Where(t => t.StateMachine.CurrentState is MainUnitIdle or MeleeUnitIdle or RangedUnitIdle)
                .Where(t => (t.transform.position - transform.position).sqrMagnitude <=
                            OverridenUnitData.StateTransitionRadius * OverridenUnitData.StateTransitionRadius).ToList();

            foreach (var unit in transitionControllers)
            {
                unit.UnitInsideViewArea.Add(target);
                
            }
            
            if (StateMachine.CurrentState is not (MainUnitIdle or MeleeUnitIdle or RangedUnitIdle))
            {
                return;
            }
            
            UnitInsideViewArea.Add(target);
        }
        
        private void ReduceAttackCoolTime()
        {
            if (CurrentAttackCoolTime > 0)
            {
                CurrentAttackCoolTime -= Time.deltaTime;
            }
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (decalProjector != null)
            {
                decalProjector.size = new Vector3(unitData.AttackRadius * 2f, unitData.AttackRadius * 2f, decalProjector.size.z);
            }
        }
#endif
    }
}

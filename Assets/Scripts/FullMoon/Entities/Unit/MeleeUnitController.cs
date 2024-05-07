using MyBox;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using FullMoon.Interfaces;
using FullMoon.Entities.Unit.States;
using FullMoon.ScriptableObject;
using FullMoon.Util;
using Unity.Burst;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent)), BurstCompile]
    public class MeleeUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Melee Unit Settings")]
        public DecalProjector decalProjector;
        
        [Foldout("Melee Unit Settings")]
        public GameObject attackEffect;

        public MeleeUnitData OverridenUnitData { get; private set; }
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }
        
        public float CurrentAttackCoolTime { get; set; }

        protected override void Start()
        {
            base.Start();
            OverridenUnitData = unitData as MeleeUnitData;
            UnitInsideViewArea = new List<BaseUnitController>();
            CurrentAttackCoolTime = unitData.AttackCoolTime;

            if (decalProjector is not null)
            {
                decalProjector.gameObject.SetActive(false);
                decalProjector.size = new Vector3(unitData.AttackRadius * 2f, unitData.AttackRadius * 2f, decalProjector.size.z);
            }

            StateMachine.ChangeState(new MeleeUnitIdle(this));
            
            OnStartEvent.TriggerEvent();
        }
        
        [BurstCompile]
        protected override void Update()
        {
            ReduceAttackCoolTime();
            UnitInsideViewArea.RemoveAll(unit => unit is null || !unit.gameObject.activeInHierarchy || !unit.Alive);
            base.Update();
        }

        public override void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (StateMachine.CurrentState is MeleeUnitIdle)
            {
                MoveToPosition(attacker.transform.position);
                OnUnitStateTransition(attacker.transform.position);
            }
            base.ReceiveDamage(amount, attacker);
        }

        public override void Die()
        {
            base.Die();
            StateMachine.ChangeState(new MeleeUnitDead(this));
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

        [BurstCompile]
        public async UniTaskVoid ExecuteAttack(Transform target)
        {
            BaseUnitController targetController = target.GetComponent<BaseUnitController>();

            if (targetController is null || targetController.gameObject.activeInHierarchy == false)
            {
                return;
            }

            Vector3 targetDirection = target.transform.position - transform.position;
            Vector3 hitPosition = target.transform.position;
            if (Physics.Raycast(transform.position + new Vector3(0f, 1f, 0f), targetDirection.normalized, out var hit, OverridenUnitData.AttackRadius, 1 << LayerMask.NameToLayer("Unit")))
            {
                hitPosition = hit.point;
            }

            transform.forward = targetDirection.normalized;
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);
            
            SetAnimation(Animator.StringToHash("Attack"));

            await UniTask.DelayFrame(12);
            
            GameObject hitFX = ObjectPoolManager.SpawnObject(attackEffect, hitPosition, Quaternion.identity);
            hitFX.transform.forward = targetDirection.normalized;
            
            if (targetController.gameObject.activeInHierarchy == false)
            {
                return;
            }
            
            targetController.ReceiveDamage(OverridenUnitData.AttackDamage, this);
        }
        
        public override void Select()
        {
            base.Select();
            // decalProjector.gameObject.SetActive(true);
        }

        public override void Deselect()
        {
            base.Deselect();
            // decalProjector.gameObject.SetActive(false);
        }

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            StateMachine.ChangeState(new MeleeUnitMove(this));
        }

        public override void OnUnitStop()
        {
            base.OnUnitStop();
            StateMachine.ChangeState(new MeleeUnitIdle(this));
        }

        public override void OnUnitHold()
        {
            base.OnUnitHold();
            StateMachine.ChangeState(new MeleeUnitIdle(this));
        }

        public override void OnUnitAttack(Vector3 targetPosition)
        {
            base.OnUnitAttack(targetPosition);
            StateMachine.ChangeState(new MeleeUnitMove(this));
        }

        public override void OnUnitForceAttack(BaseUnitController target)
        {
            base.OnUnitForceAttack(target);
            StateMachine.ChangeState(new MeleeUnitAttack(this));
        }

        [BurstCompile]
        public override void OnUnitStateTransition(Vector3 targetPosition)
        {
            base.OnUnitStateTransition(targetPosition);
            
            List<BaseUnitController> transitionControllers = UnitInsideViewArea
                .Where(t => UnitType.Equals(t.UnitType))
                .Where(t => t.StateMachine.CurrentState is MainUnitIdle or MeleeUnitIdle or RangedUnitIdle)
                .Where(t => (t.transform.position - transform.position).sqrMagnitude <=
                            OverridenUnitData.StateTransitionRadius * OverridenUnitData.StateTransitionRadius).ToList();

            foreach (var unit in transitionControllers)
            {
                unit.MoveToPosition(targetPosition);
            }
            
            if (StateMachine.CurrentState is not MainUnitIdle ||
                StateMachine.CurrentState is not MeleeUnitIdle ||
                StateMachine.CurrentState is not RangedUnitIdle)
            {
                return;
            }
            
            MoveToPosition(targetPosition);
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

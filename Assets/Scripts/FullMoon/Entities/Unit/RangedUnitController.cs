using MyBox;
using System.Linq;
using System.Collections.Generic;
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
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }
        
        public float CurrentAttackCoolTime { get; set; }

        protected override void Start()
        {
            base.Start();
            OverridenUnitData = unitData as RangedUnitData;
            UnitInsideViewArea = new List<BaseUnitController>();
            CurrentAttackCoolTime = unitData.AttackCoolTime;

            if (decalProjector != null)
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
            UnitInsideViewArea.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);
            base.Update();
        }
        
        public override void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (StateMachine.CurrentState is RangedUnitIdle)
            {
                MoveToPosition(attacker.transform.position);
                OnUnitStateTransition(attacker.transform.position);
            }
            base.ReceiveDamage(amount, attacker);
        }

        public void EnterViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Add(controller);
        }

        public void ExitViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Remove(controller);
        }

        public void ExecuteAttack(Transform target)
        {
            GameObject bullet = ObjectPoolManager.SpawnObject(attackEffect, transform.position, Quaternion.identity);
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

        public override void OnUnitAttack(Vector3 targetPosition)
        {
            base.OnUnitAttack(targetPosition);
            StateMachine.ChangeState(new RangedUnitMove(this));
        }

        public override void OnUnitForceAttack(BaseUnitController target)
        {
            base.OnUnitForceAttack(target);
            StateMachine.ChangeState(new RangedUnitAttack(this));
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
            
            if (StateMachine.CurrentState is not MainUnitIdle or MeleeUnitIdle or RangedUnitIdle)
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

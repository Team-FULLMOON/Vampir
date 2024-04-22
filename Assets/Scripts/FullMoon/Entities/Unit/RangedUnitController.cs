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

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
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
        
        protected void LateUpdate()
        {
            ReduceAttackCoolTime();
            UnitInsideViewArea.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);
        }
        
        public override void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (StateMachine.CurrentState.ToString().Equals(typeof(RangedUnitIdle).ToString()))
            {
                MoveToPosition(attacker.transform.position);
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
        
        private void ReduceAttackCoolTime()
        {
            if (CurrentAttackCoolTime > 0)
            {
                CurrentAttackCoolTime -= Time.deltaTime;
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (decalProjector != null)
            {
                decalProjector.size = new Vector3(unitData.AttackRadius * 2f, unitData.AttackRadius * 2f, decalProjector.size.z);
            }
        }
    }
}

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
    public class MainUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Main Unit Settings")]
        public DecalProjector decalProjector;
        
        [Foldout("Main Unit Settings")]
        public GameObject attackEffect;
        
        [Foldout("Main Unit Settings")]
        public GameObject attackPointEffect;

        public MainUnitData OverridenUnitData { get; private set; }
        
        public float CurrentAttackCoolTime { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenUnitData = unitData as MainUnitData;
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

            StateMachine.ChangeState(new MainUnitIdle(this));
        }

        [BurstCompile]
        protected override void Update()
        {
            ReduceAttackCoolTime();
            UnitInsideViewArea.RemoveWhere(unit => unit is null || !unit.gameObject.activeInHierarchy || !unit.Alive);
            base.Update();
        }

        public override void Die()
        {
            base.Die();
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
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

            if (attackEffect != null)
            {
                GameObject attackFX = ObjectPoolManager.Instance.SpawnObject(attackEffect, unitModel.transform.position, Quaternion.identity);
                attackFX.transform.forward = targetDirection.normalized;
            }

            await UniTask.DelayFrame(OverridenUnitData.HitAnimationFrame);
    
            if (attackPointEffect != null)
            {
                GameObject attackPointFX = ObjectPoolManager.Instance.SpawnObject(attackPointEffect, hitPosition, Quaternion.identity);
                attackPointFX.transform.forward = targetDirection.normalized;
            }
    
            if (targetController.gameObject.activeInHierarchy == false)
            {
                return;
            }
    
            targetController.ReceiveDamage(OverridenUnitData.AttackDamage, this);
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
            StateMachine.ChangeState(new MainUnitMove(this));
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

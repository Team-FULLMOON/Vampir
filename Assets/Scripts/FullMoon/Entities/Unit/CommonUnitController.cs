using MyBox;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using FullMoon.UI;
using FullMoon.Util;
using FullMoon.ScriptableObject;
using FullMoon.Entities.Unit.States;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent)), BurstCompile]
    public class CommonUnitController : BaseUnitController
    {
        [Foldout("Common Unit Settings")]
        public DecalProjector decalProjector;
        
        [Foldout("Common Unit Settings")]
        public GameObject moveDustEffect;
        
        public CommonUnitData OverridenUnitData { get; private set; }
        public BaseUnitController MainUnit { get; private set; }

        public bool IsCraft { get; set; }
        
        public GameObject hammerPrefab;

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenUnitData = unitData as CommonUnitData;

            InitializeViewRange();
            
            if (decalProjector != null)
            {
                InitializeDecalProjector();
            }
            
            StateMachine.ChangeState(new CommonUnitIdle(this));
            
            MainUnit = FindObjectsOfType<BaseUnitController>()
                .FirstOrDefault(unit => unit.unitData.UnitType.Equals("Player") && unit.unitData.UnitClass.Equals("Main"));
            
            MainUIController.Instance.ChangeCommonAmount(1);
        }

        public void CraftBuilding(Vector3 pos)
        {
            MainUIController.Instance.ChangeCommonAmount(-1);
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
            
            HammerUnitController hammerUnit = ObjectPoolManager.Instance.SpawnObject(hammerPrefab, transform.position, transform.rotation)
                .GetComponent<HammerUnitController>();
            
            hammerUnit.MoveToPosition(pos);
        }

        public override void Die()
        {
            base.Die();
            MainUIController.Instance.ChangeCommonAmount(-1);
            StateMachine.ChangeState(new CommonUnitDead(this));
        }

        public void EnterViewRange(Collider unit)
        {
            if (unit.TryGetComponent(out BaseUnitController controller))
            {
                UnitInsideViewArea.Add(controller);
            }
        }

        public void ExitViewRange(Collider unit)
        {
            if (unit.TryGetComponent(out BaseUnitController controller))
            {
                UnitInsideViewArea.Remove(controller);
            }
        }
        
        public override void Select()
        {
            base.Select();
            if (Flag != null)
            {
                Flag.Select();
                return;
            }
            decalProjector?.gameObject.SetActive(true);
        }

        public override void Deselect()
        {
            base.Deselect();
            if (Flag != null)
            {
                Flag.Deselect();
                return;
            }
            decalProjector?.gameObject.SetActive(false);
        }

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            StateMachine.ChangeState(new CommonUnitMove(this));
        }

        private void InitializeDecalProjector()
        {
            decalProjector.gameObject.SetActive(false);
        }
        
        private void InitializeViewRange()
        {
            if (viewRange != null && unitData != null)
            {
                float worldRadius = viewRange.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                var triggerEvent = viewRange.GetComponent<ColliderTriggerEvents>();

                if (triggerEvent != null)
                {
                    var units = Physics.OverlapSphere(transform.position + viewRange.center, worldRadius)
                        .Where(t => triggerEvent.GetFilterTags.Contains(t.tag) && t.gameObject != gameObject)
                        .Select(t => t.GetComponent<BaseUnitController>())
                        .Where(unit => unit != null)
                        .ToList();

                    UnitInsideViewArea.UnionWith(units);
                }
            }
        }
    }
}

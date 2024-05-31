using System;
using MyBox;
using System.Linq;
using FullMoon.Entities.Unit.States;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.ScriptableObject;
using FullMoon.Util;
using Unity.Burst;
using UnityEngine.Rendering.Universal;
using FullMoon.UI;

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

        [HideInInspector]
        public bool isCraft = false;
        [HideInInspector]
        public BuildingType buildingType;
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
        }

        public void CraftBuilding(Vector3 pos)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
            HammerUnitController hammerUnit = ObjectPoolManager.Instance.SpawnObject(hammerPrefab, transform.position, transform.rotation)
                                                                        .GetComponent<HammerUnitController>();
                                                                        
            hammerUnit.MoveToPosition(pos);
        }

        public override void Die()
        {
            base.Die();
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

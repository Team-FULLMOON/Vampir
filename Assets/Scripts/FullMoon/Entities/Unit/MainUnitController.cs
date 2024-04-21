using MyBox;
using System.Collections.Generic;
using System.Linq;
using FullMoon.Entities.Unit.States;
using FullMoon.Input;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using FullMoon.ScriptableObject;
using FullMoon.UI;
using FullMoon.Util;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MainUnitController 
        : BaseUnitController
    {
        [Foldout("Main Unit Settings")]
        public DecalProjector decalProjector;

        public MainUnitData OverridenUnitData { get; private set; }
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }
        public List<RespawnController> RespawnUnitInsideViewArea { get; set; }
        public RespawnController ReviveTarget { get; set; }
        
        protected override void Start()
        {
            base.Start();
            OverridenUnitData = unitData as MainUnitData;
            UnitInsideViewArea = new List<BaseUnitController>();
            RespawnUnitInsideViewArea = new List<RespawnController>();

	        if (decalProjector is not null)
            {
                decalProjector.gameObject.SetActive(false);
                decalProjector.size = new Vector3(((MainUnitData)unitData).RespawnRadius * 2f, 
                    ((MainUnitData)unitData).RespawnRadius * 2f, 
                    decalProjector.size.z);
            }

            StateMachine.ChangeState(new MainUnitIdle(this));
        }
        
        protected void LateUpdate()
        {
            UnitInsideViewArea.RemoveAll(unit => unit is null || !unit.gameObject.activeInHierarchy);
            RespawnUnitInsideViewArea.RemoveAll(unit => unit is null || !unit.gameObject.activeInHierarchy);
        }

        public void EnterViewRange(Collider unit)
        {
            switch (unit.tag)
            {
                case "RespawnUnit":
                    RespawnController resController = unit.GetComponent<RespawnController>();
                    if (resController is null)
                    {
                        return;
                    }
                    RespawnUnitInsideViewArea.Add(resController);
                    break;
                default:
                    BaseUnitController controller = unit.GetComponent<BaseUnitController>();
                    if (controller is null)
                    {
                        return;
                    }
                    UnitInsideViewArea.Add(controller);
                    break;
            }
        }

        public void ExitViewRange(Collider unit)
        {
            switch (unit.tag)
            {
                case "RespawnUnit":
                    RespawnController resController = unit.GetComponent<RespawnController>();
                    if (resController is null)
                    {
                        return;
                    }
                    RespawnUnitInsideViewArea.Remove(resController);
                    break;
                default:
                    BaseUnitController controller = unit.GetComponent<BaseUnitController>();
                    if (controller is null)
                    {
                        return;
                    }
                    UnitInsideViewArea.Remove(controller);
                    break;
            }
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
            ReviveTarget = null;
            base.MoveToPosition(location);
            StateMachine.ChangeState(new MainUnitMove(this));
        }

        public override void OnUnitStop()
        {
            base.OnUnitStop();
            StateMachine.ChangeState(new MainUnitIdle(this));
        }

        public override void OnUnitHold()
        {
            base.OnUnitHold();
            StateMachine.ChangeState(new MainUnitIdle(this));
        }

        public override void OnUnitAttack(Vector3 targetPosition)
        {
            MoveToPosition(targetPosition);
        }
        
        public void CheckAbleToRespawn(RespawnController unit)
        {
            if (unit is null || unit.gameObject.activeInHierarchy == false)
            {
                return;
            }
            
            ReviveTarget = unit;
            
            bool checkDistance = (ReviveTarget.transform.position - transform.position).sqrMagnitude <=
                                 OverridenUnitData.RespawnRadius * OverridenUnitData.RespawnRadius;
            
            if (checkDistance == false)
            {
                base.MoveToPosition(ReviveTarget.transform.position);
                StateMachine.ChangeState(new MainUnitMove(this));
                return;
            }
            
            StateMachine.ChangeState(new MainUnitRespawn(this));
        }
        
        public void StartRespawn(RespawnController unit)
        {
            if (MainUIController.Instance.ManaValue < unit.ManaCost ||
                MainUIController.Instance.CurrentUnitValue >= MainUIController.Instance.UnitLimitValue)
            {
                ReviveTarget = null;
                StateMachine.ChangeState(new MainUnitIdle(this));
                return;
            }
            
            ReviveTarget = unit;
            Invoke(nameof(Respawn), ReviveTarget.SummonTime);
        }
        
        public void CancelRespawn()
        {
            ReviveTarget = null;
            CancelInvoke(nameof(Respawn));
        }
        
        private void Respawn()
        {
            MainUIController.Instance.AddMana(-ReviveTarget.ManaCost);
            ObjectPoolManager.SpawnObject(ReviveTarget.UnitTransformObject, ReviveTarget.transform.position, ReviveTarget.transform.rotation);
            ObjectPoolManager.ReturnObjectToPool(ReviveTarget.gameObject);
            ReviveTarget = null;
            StateMachine.ChangeState(new MainUnitIdle(this));
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (decalProjector != null)
            {
                decalProjector.size = new Vector3(((MainUnitData)unitData).RespawnRadius * 2f, ((MainUnitData)unitData).RespawnRadius * 2f, decalProjector.size.z);
            }
        }
    }
}

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
        public RespawnController ReviveTarget;

        private RespawnController respawnController;
        
        protected override void Start()
        {
            base.Start();
            OverridenUnitData = unitData as MainUnitData;
            UnitInsideViewArea = new List<BaseUnitController>();
            RespawnUnitInsideViewArea = new List<RespawnController>();

	        if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);
                decalProjector.size = new Vector3(((MainUnitData)unitData).RespawnRadius * 2f, ((MainUnitData)unitData).RespawnRadius * 2f, decalProjector.size.z);
            }

            StateMachine.ChangeState(new MainUnitIdle(this));
        }
        
        protected void LateUpdate()
        {
            UnitInsideViewArea.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);
            RespawnUnitInsideViewArea.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);
        }

        public void EnterViewRange(Collider unit)
        {
            switch (unit.tag)
            {
                case "RespawnUnit":
                    RespawnController resController = unit.GetComponent<RespawnController>();
                    if (resController == null)
                    {
                        return;
                    }
                    RespawnUnitInsideViewArea.Add(resController);
                    break;
                default:
                    BaseUnitController controller = unit.GetComponent<BaseUnitController>();
                    if (controller == null)
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
                    if (resController == null)
                    {
                        return;
                    }
                    RespawnUnitInsideViewArea.Remove(resController);
                    break;
                default:
                    BaseUnitController controller = unit.GetComponent<BaseUnitController>();
                    if (controller == null)
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
        
        public override void OnUnitAttack(Vector3 targetPosition) { }
        
        public void CheckAbleToRespawn(RespawnController unit)
        {
            PlayerInputManager.Instance.respawn = false;
            ReviveTarget = unit;
                
            if (MainUIController.Instance.CurrentUnitValue >= MainUIController.Instance.UnitLimitValue)
            {
                return;
            }
                
            if (unit == null)
            {
                return;
            }
                
            bool checkDistance = (unit.transform.position - transform.position).sqrMagnitude <=
                                 OverridenUnitData.RespawnRadius * OverridenUnitData.RespawnRadius;
            


            if (checkDistance == false)
            {
                MoveToPosition(unit.transform.position);
                StateMachine.ChangeState(new MainUnitMove(this));
                return;
            }
            else
            {
                StateMachine.ChangeState(new MainUnitRespawn(this));
                return;
            }  
        }
        
        public void StartSpawn(RespawnController unit)
        {
            Debug.Log($"Spawn Start: {name}");
            respawnController = unit;
            Invoke(nameof(Spawn), respawnController.SummonTime);
            ReviveTarget = null;
        }
        
        public void CancelSpawn()
        {
            Debug.Log($"Spawn Cancel: {name}");
            respawnController = null;
            CancelInvoke(nameof(Spawn));
        }
        
        private void Spawn()
        {
            MainUIController.Instance.AddMana(-respawnController.ManaCost);
            ObjectPoolManager.SpawnObject(respawnController.UnitTransformObject, respawnController.transform.position, respawnController.transform.rotation);
            ObjectPoolManager.ReturnObjectToPool(respawnController.gameObject);
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

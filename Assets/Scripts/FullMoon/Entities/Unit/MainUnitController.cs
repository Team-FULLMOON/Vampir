using MyBox;
using System.Collections.Generic;
using FullMoon.Entities.Unit.States;
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

        private RespawnController respawnController;
        
        protected override void Start()
        {
            base.Start();
            OverridenUnitData = (MainUnitData)unitData;
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
                    RespawnController respawnController = unit.GetComponent<RespawnController>();
                    if (respawnController == null)
                    {
                        return;
                    }
                    RespawnUnitInsideViewArea.Add(respawnController);
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
                    RespawnController respawnController = unit.GetComponent<RespawnController>();
                    if (respawnController == null)
                    {
                        return;
                    }
                    RespawnUnitInsideViewArea.Remove(respawnController);
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
        
        public void StartSpawn(RespawnController unit)
        {
            Debug.Log($"Spawn Start: {name}");
            respawnController = unit;
            Invoke(nameof(Spawn), respawnController.SummonTime);
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

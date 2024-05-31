using MyBox;
using FullMoon.Interfaces;
using UnityEngine;
using FullMoon.Entities.Unit;
using FullMoon.ScriptableObject;
using Unity.Burst;
using Cysharp.Threading.Tasks;
using System;

namespace FullMoon.Entities.Building
{
    [BurstCompile]
    public class BaseBuildingController 
        : MonoBehaviour, IDamageable, ISelectable
    {
        [Foldout("Base Building Settings"), DisplayInspector] 
        public BaseBuildingData buildingData;

        [SerializeField, OverrideLabel("Frame")]
        public GameObject buildingFramePrefab;

        [HideInInspector]
        public Vector3 targetPos;
        public int Hp { get; set; }
        public bool Alive { get; private set; }

        protected virtual void OnEnable()
        {
            Alive = true;
            Hp = buildingData.MaxHp;

            ShowFrame().Forget();
        }

        public virtual void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (Alive == false)
            {
                return;
            }

            Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);

            if (Hp <= 0)
            {
                Die();
            }
        }

        public async UniTaskVoid ShowFrame()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            GetComponent<MeshRenderer>().enabled = true;
            return;
        }

        public virtual void Die()
        {
            Alive = false;
        }

        public virtual void Select()
        {

        }

        public virtual void Deselect()
        {

        }
    }
}

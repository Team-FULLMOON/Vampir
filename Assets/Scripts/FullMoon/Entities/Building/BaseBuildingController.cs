using MyBox;
using System;
using Unity.Burst;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FullMoon.Interfaces;
using FullMoon.Entities.Unit;
using FullMoon.ScriptableObject;

namespace FullMoon.Entities.Building
{
    [BurstCompile]
    public class BaseBuildingController 
        : MonoBehaviour, IDamageable, ISelectable
    {
        [Foldout("Base Building Settings"), DisplayInspector] 
        public BaseBuildingData buildingData;

        [SerializeField, OverrideLabel("Frame")]
        public GameObject buildingFrame;

        public int Hp { get; set; }
        public bool Alive { get; private set; }

        protected virtual void OnEnable()
        {
            Alive = true;
            Hp = buildingData.MaxHp;
            
            if (buildingFrame != null)
            {
                buildingFrame.SetActive(false);
            }
        }

        public virtual void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (Alive == false)
            {
                return;
            }

            Hp = Mathf.Clamp(Hp - amount, 0, Int32.MaxValue);

            if (Hp <= 0)
            {
                Die();
            }
        }

        protected async UniTaskVoid ShowFrame(float delay = 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            if (buildingFrame != null)
            {
                buildingFrame.SetActive(true);
            }
        }

        public virtual void Die()
        {
            Alive = false;
        }

        public virtual void Select() { }

        public virtual void Deselect() { }
    }
}

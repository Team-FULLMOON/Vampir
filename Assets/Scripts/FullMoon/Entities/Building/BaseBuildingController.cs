using MyBox;
using System;
using System.Collections.Generic;
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

        [SerializeField, OverrideLabel("Frame Progress")]
        private List<GameObject> frameProgress;

        public int Hp { get; set; }
        public bool Alive { get; private set; }

        protected virtual void OnEnable()
        {
            Alive = true;
            Hp = buildingData.MaxHp;

            foreach (var model in frameProgress)
            {
                model.SetActive(false);
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

        protected async UniTaskVoid ShowFrame(float totalDelay = 0f)
        {
            int modelCount = frameProgress.Count;
            if (modelCount == 0)
            {
                return;
            }

            float delayPerModel = totalDelay / modelCount;

            for (int i = 0; i < modelCount; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delayPerModel));
                if (i > 0)
                {
                    frameProgress[i - 1].SetActive(false);
                }
                frameProgress[i].SetActive(true);
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
using System.Collections;
using System.Collections.Generic;
using FullMoon.Entities.Unit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace FullMoon.UI
{
    public class UnitHPUI : MonoBehaviour
    {
        BaseUnitController unit;
        UnityEngine.Camera mainCamera;
        [SerializeField] Slider hpui;

        void Start()
        {
            mainCamera = UnityEngine.Camera.main;
            GetComponent<Canvas>().worldCamera = mainCamera;
            unit = GetComponentInParent<BaseUnitController>();

            hpui.maxValue = unit.unitData.MaxHp;
            hpui.value = hpui.maxValue;
        }

        void LateUpdate()
        {
            hpui.value = unit.Hp;
        }

        void Update()
        {
            transform.LookAt(mainCamera.transform.position);
        }
    }
}

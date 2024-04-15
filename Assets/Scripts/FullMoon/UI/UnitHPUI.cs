using System.Collections;
using System.Collections.Generic;
using FullMoon.Entities.Unit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UnitHPUI : MonoBehaviour
{
    BaseUnitController unit;
    [SerializeField] Slider hpui;

    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    void Update()
    {
        if (!unit.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            transform.position = unit.transform.position + new Vector3(0, 2.45f, 0);
        }
    }

    public void SetHP(float hp)
    {
        hpui.value = Mathf.Clamp(hpui.value - hp, 0, System.Int32.MaxValue);
    }

    public void SetSlider(BaseUnitController unit)
    {
        this.unit = unit;
        hpui.maxValue = unit.unitData.MaxHp;
        hpui.value = hpui.maxValue;
    }
}

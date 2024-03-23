using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    [Header("최대 체력")]
    [SerializeField] protected float u_maxHp;
    [Header("현재 체력")]
    [SerializeField] protected float u_curHp;
    [Header("공격력")]
    [SerializeField] protected float u_ap;
    [Header("방어력")]
    [SerializeField] protected float u_dp;
    [Header("공격 속도(공격 지연 시간)")]
    [SerializeField] protected float u_as;
    [Header("이동 속도")]
    [SerializeField] protected float u_ms;
    [Header("공격 범위")]
    [SerializeField] protected float u_ar;
    [Header("인식 범위")]
    [SerializeField] protected float u_fr;
    [Header("유닛 타입")]
    [SerializeField] protected BaseUnit.UnitType _unitType;
    [Header("유닛 소유 타입")]
    [SerializeField] protected BaseUnit.UnithandType _unithandType;
    [Header("유닛 상태")]
    [SerializeField] protected BaseUnit.UnitState _unitState;
    
    public BaseUnit.UnithandType GetUnithandType() => _unithandType;
    
    public enum UnitType
    {
        Archer,
    }
    
    public enum UnithandType
    {
        Player,
        Enemy,
    }
    
    public enum UnitState
    {
        Idle,
        Attack,
        Run,
        Patrol,
        Skill,
        Death,
    }
}

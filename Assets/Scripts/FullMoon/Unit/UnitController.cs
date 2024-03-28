using System;
using System.Collections.Generic;
using System.Collections;
using FullMoon.Util;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace FullMoon.Unit
{
    public class UnitController : BaseUnit
    {
        [SerializeField] GameObject unitMarker;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform spawnPoint;

        private NavMeshAgent agent;
        
        [Header("AI")]
        private UnitSpawner _spawner;
        private UnitController _unitTarget;
        private float _findTimer;
        private float _attackTimer;
        private bool _forceMove;
        private bool _forceAttack;
        private bool _isAttack;


        private void Awake()
        {
            _spawner = FindObjectOfType<UnitSpawner>();
            agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            agent.isStopped = false;
            agent.enabled = true;
            StartCoroutine(CheckNavFinish());
        }

        private void Start()
        {
            agent.speed = u_ms;
            u_curHp = u_maxHp;
        }

        private void Update()
        {
            CheckState();
        }
        
        // 유닛 선택
        public void SelectUnit()
        {
            unitMarker.SetActive(true);
        }
        
        // 유닛 선택 비활성화
        public void DeSelectUnit()
        {
            unitMarker.SetActive(false);
        }
        
        // 이동 함수
        public void MoveTo(Vector3 pos)
        {
            if (_unitState == UnitState.Death)
                return;
            
            agent.isStopped = false;
            _isAttack = true;
            agent.SetDestination(pos);
        }
        
        // 이동 함수 (도착 전 까지 공격 못함)
        public void MoveTo(Vector3 pos, bool isForceAttack)
        {
            if (_unitState == UnitState.Death)
                return;
            
            _forceMove = isForceAttack;
            _isAttack = isForceAttack;
            agent.isStopped = false;
            _unitState = UnitState.Run;
            agent.SetDestination(pos);
        }

        #region AI
        
        // 현재 상태 체크
        private void CheckState()
        {
            if (_forceMove)
                return;
            switch (_unitState)
            {
                case UnitState.Idle:
                    if (!_isAttack)
                        break;
                    FindTarget();
                    break;
                case UnitState.Attack:
                    CheckAttack();
                    break;
                case UnitState.Run:
                    if (!_forceAttack)
                        FindTarget();
                    DoMove();
                    break;
            }
        }
        
        // 현재 상태 변환
        private void SetState(UnitState state) => _unitState = state;
        
        // 적과의 거리를 이용하여 타겟 찾기
        public UnitController GetTarget(UnitController unit)
        {
            UnitController target = null;
            List<UnitController> unitControllerList = new List<UnitController>();
            // 아군 유닛이면 적군의 유닛 리스트 사용, 적군이면 아군의 유닛 리스트 사용
            switch (unit._unithandType)
            {
                case UnithandType.Player:
                    unitControllerList = _spawner.GetUnitList(UnithandType.Enemy);
                    break;
                case UnithandType.Enemy:
                    unitControllerList = _spawner.GetUnitList(UnithandType.Player);
                    break;
            }
            float num = 999999f;
            // 유닛의 인식 범위를 사용
            for (int index = 0; index < unitControllerList.Count; ++index)
            {
                float sqrMagnitude = (unitControllerList[index].transform.position - unit.transform.position).sqrMagnitude;
                if (sqrMagnitude <= unit.u_fr && unitControllerList[index]._unitState != UnitState.Death && sqrMagnitude < num)
                {
                    target = unitControllerList[index];
                    num = sqrMagnitude;
                }
            }
            return target;
        }
        
        // 1초 마다 타겟을 탐색한다
        private void FindTarget()
        {
            _findTimer += Time.deltaTime;
            if (_findTimer <= 1.0)
                return;
            _unitTarget = GetTarget(this);
            if (_unitTarget != null)
                SetState(UnitState.Run);
            else if (_unitTarget == null)
                SetState(UnitState.Idle);
            _findTimer = 0.0f;
        }
        
        // 유닛 이동 함수 (이동하다가 공격 가능한 적이 보이면 공격)
        private void DoMove()
        {
            if (!CheckTarget())
                return;
            CheckAttackDis();
            SetDirection();
            MoveTo(_unitTarget.transform.position);
        }
        
        // CheckTarget()와 CheckAttackDis()로 공격 가능한지 체크하고 공격속도에 따라서 공격을 하는 함수
        private void CheckAttack()
        {
            if (!CheckTarget() || !CheckAttackDis())
                return;
            agent.isStopped = true;
            _attackTimer += Time.deltaTime;
            if (_attackTimer <= u_as)
                return;
            DoAttack();
            _attackTimer = 0.0f;
        }
        
        // 공격 범위 안에 들어와있는지 체크
        private bool CheckAttackDis()
        {
            if (new Vector2(_unitTarget.transform.position.x - transform.position.x, 
                    _unitTarget.transform.position.z - transform.position.z).sqrMagnitude <= u_ar)
            {
                SetState(UnitState.Attack);
                return true;
            }
            if (!CheckTarget() || !_forceAttack)
                SetState(UnitState.Idle);
            else
                SetState(UnitState.Run);
            return false;
        }
        
        // 유닛 공격 함수
        private void DoAttack()
        {
            transform.LookAt(_unitTarget.transform.position);
            
            switch (_unitType)
            {
                case UnitType.Archer:
                    GameObject arrow = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity);
                    arrow.GetComponent<ArrowMove>().SetTargetPos(_unitTarget.transform, u_ap, transform);
                    // ObjectPoolManager.SpawnObject(arrowPrefab, spawnPoint.position, Quaternion.identity).
                    //     GetComponent<ArrowMove>().SetTargetPos(_unitTarget.transform, u_ap, transform);
                    break;
            }
        }
        
        // 적이 인식범위를 벗어나거나 죽은 상태인지 체크
        private bool CheckTarget()
        {
            bool check = true;
            if (_unitTarget == null || _unitTarget._unitState == UnitState.Death || !_unitTarget.gameObject.activeSelf)
            {
                check = false;
                _forceAttack = false;
            }
            if (!check)
                SetState(UnitState.Idle);
            return check;
        }
        
        // 유닛의 공격 대상 강제 설정
        public void SetTarget(UnitController unit)
        {
            _unitTarget = unit;
            _forceAttack = true;
            _forceMove = false;
            _unitState = UnitState.Run;
            _isAttack = true;
        }
        
        public void AttackArea(Vector3 pos)
        {
            _unitState = UnitState.Run;
            _forceMove = false;
            MoveTo(pos);
        }
        
        // 유닛의 최대체력 반환
        public float GetMaxHp() => u_maxHp;
        // 유닛의 현재체력 반환
        public float GetCurrentHp() => u_curHp;
        // 유닛이 해당 지점을 바라보도록 설정
        private void SetDirection() => transform.LookAt(_unitTarget.transform);
        public void SetIsAttack(bool isAttack)
        {
            _isAttack = isAttack;
            agent.isStopped = false;
            _unitState = UnitState.Idle;
        }
        
        // 유닛이 데미지를 받았을 시 호출
        public void TakeDamage(float value, UnitController unit)
        {
            u_curHp -= (value - u_dp);
            if (u_curHp <= 0.0)
            {
                _unitState = UnitState.Death;
                agent.enabled = false;
                _spawner.EraseDeathUnit(this);
                gameObject.SetActive(false);
                
                return;
            }
            
            _unitState = UnitState.Run;
            MoveTo(unit.transform.position);
        }
        
        // 유닛이 목표 지점에 도착했는 지 체크
        IEnumerator CheckNavFinish()
        {
            while (true)
            {
                if (agent.velocity.sqrMagnitude >= 0.2f * 0.2f && agent.remainingDistance <= 0.5f)
                {
                    _forceMove = false;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
        
        #endregion AI
    }
}

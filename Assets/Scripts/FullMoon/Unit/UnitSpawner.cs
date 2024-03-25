using System;
using System.Collections.Generic;
using System.Linq;
using FullMoon.Util;
using UnityEngine;

namespace FullMoon.Unit
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] playerUnitPrefab;             // 플레이어 유닛 프리팹
        [SerializeField] private GameObject[] enemyUnitPrefab;              // 적군 유닛 프리팹
        [SerializeField] private GameObject[] playerUnitIcon;               // 플레이어 유닛 아이콘 프리팹
        [SerializeField] private GameObject[] enemyUnitIcon;                // 적군 유닛 아이콘 프리팹
        [SerializeField] private int maxUnitCount;                          // 모든 유닛의 수
        private List<UnitController> _playerUnitList;                       // 플레이어 유닛 리스트
        private List<UnitController> _enemyUnitList;                        // 적군 유닛 리스트
        private List<UnitController> _allUnitList;                          // 모든 유닛 리스트
        
        private void Awake()
        {
            _playerUnitList = new List<UnitController>();
            _enemyUnitList = new List<UnitController>();
        }

        private void Start()
        {
            _allUnitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None).ToList();
            for (int i = 0; i < _allUnitList.Count; ++i)
            {
                if (_allUnitList[i].GetUnitHandType() == BaseUnit.UnithandType.Player)
                    _playerUnitList.Add(_allUnitList[i]);
                else
                    _enemyUnitList.Add(_allUnitList[i]);
            }
        }

        // 죽은 유닛 리스트에서 제거
        public List<UnitController> EraseDeathUnit(UnitController unit)
        {
            switch (unit.GetUnitHandType())
            {
                case BaseUnit.UnithandType.Player:
                  if (_playerUnitList.Contains(unit))
                    _playerUnitList.Remove(unit);
                  return _playerUnitList;
                case BaseUnit.UnithandType.Enemy:
                  if (_enemyUnitList.Contains(unit))
                    _enemyUnitList.Remove(unit);
                  return _enemyUnitList;
                default:
                  return null;
            }
        }
        
        // 모든 유닛 리스트 반환
        public List<UnitController> GetUnitList()
        {
            return _allUnitList;
        }
        
        // 특정 소유주 유닛 리스트 반환
        public List<UnitController> GetUnitList(BaseUnit.UnithandType type)
        {
            return type == BaseUnit.UnithandType.Player ? _playerUnitList : _enemyUnitList;
        }
    }
}

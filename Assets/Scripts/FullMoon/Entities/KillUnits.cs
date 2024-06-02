using System.Linq;
using FullMoon.Entities.Unit;
using MyBox;
using UnityEngine;

namespace FullMoon.Entities
{
    public class KillUnits : MonoBehaviour
    {
        [ButtonMethod]
        public void KillAllUnits()
        {
            var units = GameObject.FindGameObjectsWithTag("Player")
                .Where(unit => unit is not null 
                               && unit.activeInHierarchy 
                               && unit.GetComponent<BaseUnitController>().Alive)
                .ToList();
            
            foreach (var unit in units)
            {
                unit.GetComponent<BaseUnitController>().Die();
            }
        }
        
        [ButtonMethod]
        public void KillAllEnemies()
        {
            var units = GameObject.FindGameObjectsWithTag("Enemy")
                .Where(unit => unit is not null 
                               && unit.activeInHierarchy 
                               && unit.GetComponent<BaseUnitController>().Alive)
                .ToList();
            
            foreach (var unit in units)
            {
                unit.GetComponent<BaseUnitController>().Die();
            }
        }
    }
}
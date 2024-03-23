using UnityEngine;
using FullMoon.Util;

namespace FullMoon.Unit
{
    public class ArrowMove : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private float arrowSpeed;
        private float arrowDamage;
        private Transform targetPos;
        private Transform fromPos;
        private Vector3 pos;
    
        private void Update()
        {
            transform.Translate(Vector3.forward * arrowSpeed * Time.deltaTime);
        }
        
        // 화살 초기 값 설정
        public void SetTargetPos(Transform pos, float damage, Transform fromUnit)
        {
            arrowDamage = damage;
            targetPos = pos;
            transform.LookAt(targetPos);
            fromPos = fromUnit;
        }
    
        private void OnEnable() => transform.LookAt(targetPos);
    
        private void OnTriggerEnter(Collider other)
        {
            // 화살을 쏜 유닛의 소유주에 따라서 콜라이더 반응이 달라지도록 비교
            switch (targetPos.GetComponent<UnitController>().GetUnithandType())
            {
                case BaseUnit.UnithandType.Player:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
                        break;
                    other.GetComponent<UnitController>().TakeDamage(arrowDamage, fromPos.GetComponent<UnitController>());
                    ObjectPoolManager.ReturnObjectToPool(gameObject);
                    break;
                case BaseUnit.UnithandType.Enemy:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
                        break;
                    other.GetComponent<UnitController>().TakeDamage(arrowDamage, fromPos.GetComponent<UnitController>());
                    ObjectPoolManager.ReturnObjectToPool(gameObject);
                    break;
                default:
                    ObjectPoolManager.ReturnObjectToPool(gameObject);
                    break;
            }
        }
    }
}

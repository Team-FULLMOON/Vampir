using System;
using UnityEngine;
using FullMoon.Util;

namespace FullMoon.Unit
{
    public class ArrowMove : MonoBehaviour
    {
        [SerializeField] private GameObject _bulletEmitor;
        [SerializeField] private GameObject _gunHitVariant;

        [SerializeField] private float bulletSpeed;

        private ParticleSystem _ps;
        private float arrowDamage;
        private Transform targetPos;
        private Transform fromPos;
        private Vector3 pos;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            transform.Translate(Vector3.right * bulletSpeed * Time.deltaTime);
        }

        // 불릿 초기 값 설정
        public void SetTargetPos(Transform pos, float damage, Transform fromUnit)
        {
            arrowDamage = damage;
            targetPos = pos;
            transform.LookAt(targetPos);
            transform.eulerAngles += new Vector3(-90f, -90f, 0f);
            fromPos = fromUnit;
            
            ObjectPoolManager.SpawnObject(_bulletEmitor, fromPos.position, Quaternion.identity).transform.LookAt(targetPos);
        }

        private void OnEnable()
        {
            transform.LookAt(targetPos);
        }

        //private void OnParticleCollision(GameObject other)
        //{
        //    gameObject.SetActive(false);
        //    
        //    if (fromPos.CompareTag(other.tag))
        //    {
        //        Destroy(gameObject);
        //        return;
        //    }
        //    
        //    other.GetComponent<UnitController>().TakeDamage(arrowDamage, fromPos.GetComponent<UnitController>());
        //    
        //    Destroy(gameObject);
        //}

        private void OnTriggerEnter(Collider other)
        {
            ObjectPoolManager.SpawnObject(_gunHitVariant, transform.position, Quaternion.Euler(0, 0, 0));
            gameObject.SetActive(false);

            if (fromPos.CompareTag(other.tag) || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Destroy(gameObject);
                return;
            }
            
            other.GetComponent<UnitController>().TakeDamage(arrowDamage, fromPos.GetComponent<UnitController>());
            
            Destroy(gameObject);
        }
    }
}

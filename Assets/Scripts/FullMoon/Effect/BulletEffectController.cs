using UnityEngine;
using FullMoon.Util;
using FullMoon.Entities.Unit;

namespace FullMoon.Effect
{
    public class BulletEffectController : MonoBehaviour
    {
        [SerializeField] private GameObject firingEffect;
        [SerializeField] private GameObject hitEffect;
        
        private Transform target;
        public Transform shooter;
        private string fromType;
        private float speed;
        private int damage;

        private Vector3 lastPosition;
        private int groundLayer;
        private int unitLayer;
        private bool isFired;

        private void OnEnable()
        {
            groundLayer = LayerMask.NameToLayer("Ground");
            unitLayer = LayerMask.NameToLayer("Unit");
            lastPosition = transform.position;
            isFired = false;
            CancelInvoke(nameof(DestroyEffect));
            Invoke(nameof(DestroyEffect), 7f);
        }

        private void Update()
        {
            if (isFired == false)
            {
                return;
            }

            float step = speed * Time.deltaTime;
            Vector3 currentPosition = transform.position;
            Vector3 direction = currentPosition - lastPosition;
            float distance = direction.magnitude;

            if (distance > 0)
            {
                if (Physics.Raycast(lastPosition, direction.normalized, out var hit, distance + step))
                {
                    HandleCollision(hit);
                }
            }

            lastPosition = currentPosition;
            transform.Translate(Vector3.forward * step);
        }
        
        public void Fire(Transform targetTransform, Transform shooterTransform, float speedValue, int damageValue)
        {
            target = targetTransform;
            shooter = shooterTransform;
            fromType = shooterTransform.GetComponent<BaseUnitController>().unitType;
            speed = speedValue;
            damage = damageValue;

            float missRate = targetTransform.GetComponent<BaseUnitController>().unitData.MissRate;
            
            Vector3 toTarget = (target.position - transform.position).normalized;
            
            if (Random.Range(0f, 100f) < missRate)
            {
                target = null;
                
                Vector3 randomDirection = Random.insideUnitSphere * 0.1f;   
                randomDirection -= Vector3.Project(randomDirection, toTarget);
                toTarget += randomDirection;
            }
            else
            {
                Vector3 randomDirection = Random.insideUnitSphere * 0.02f;   
                randomDirection -= Vector3.Project(randomDirection, toTarget);
                toTarget += randomDirection;
            }

            transform.forward = toTarget.normalized;
            ObjectPoolManager.SpawnObject(firingEffect, transform.position, Quaternion.identity);
            isFired = true;
        }

        private void HandleCollision(RaycastHit hit)
        {
            if (target == null)
            {
                return;
            }
            
            int otherLayer = hit.collider.gameObject.layer;

            if (otherLayer == groundLayer)
            {
                ObjectPoolManager.ReturnObjectToPool(gameObject);
                ObjectPoolManager.SpawnObject(hitEffect, hit.point, Quaternion.identity);
            }
            
            if (otherLayer == unitLayer)
            {
                if (target != hit.transform)
                {
                    return;
                }
                
                var unitController = hit.collider.GetComponent<BaseUnitController>();
                if (unitController != null && !unitController.unitType.Equals(fromType))
                {
                    ObjectPoolManager.ReturnObjectToPool(gameObject);
                    ObjectPoolManager.SpawnObject(hitEffect, hit.point, Quaternion.identity);
                }
            }
        }
        
        private void DestroyEffect()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
}
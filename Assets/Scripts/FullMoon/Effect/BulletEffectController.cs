using UnityEngine;
using FullMoon.Util;
using FullMoon.Entities.Unit;

namespace FullMoon.Effect
{
    public class BulletEffectController : MonoBehaviour
    {
        [SerializeField] private GameObject firingEffect;
        [SerializeField] private GameObject hitEffect;
        
        private BaseUnitController target;
        private BaseUnitController shooter;
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
            transform.position += new Vector3(0f, 1f, 0f);
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

            if (target != null && target.gameObject.activeInHierarchy)
            {
                Vector3 targetDirection = ((target.transform.position + new Vector3(0f, 1f, 0f)) - transform.position).normalized;
                transform.forward = targetDirection;
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
            target = targetTransform.GetComponent<BaseUnitController>();
            shooter = shooterTransform.GetComponent<BaseUnitController>();
            speed = speedValue;
            damage = damageValue;

            float missRate = targetTransform.GetComponent<BaseUnitController>().unitData.MissRate;
            
            Vector3 targetDirection = ((target.transform.position + new Vector3(0f, 1f, 0f)) - transform.position).normalized;
            
            if (Random.Range(0f, 100f) < missRate)
            {
                target = null;
                
                Vector3 randomDirection = Random.insideUnitSphere * 0.1f;   
                randomDirection -= Vector3.Project(randomDirection, targetDirection);
                targetDirection += randomDirection;
            }

            transform.forward = targetDirection.normalized;

            GameObject fireFX = ObjectPoolManager.Instance.SpawnObject(firingEffect, transform.position, Quaternion.identity);
            fireFX.transform.forward = targetDirection.normalized;
            fireFX.transform.eulerAngles = new Vector3(0f, fireFX.transform.eulerAngles.y - 90f, 0f);
            
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
                ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
                ObjectPoolManager.Instance.SpawnObject(hitEffect, hit.point, Quaternion.identity);
            }
            
            if (otherLayer == unitLayer)
            {
                if (target.gameObject != hit.transform.gameObject)
                {
                    return;
                }
                
                var unitController = hit.collider.GetComponent<BaseUnitController>();
                if (unitController != null && !unitController.UnitType.Equals(shooter.UnitType))
                {
                    unitController.ReceiveDamage(damage, shooter);
                    ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
                    ObjectPoolManager.Instance.SpawnObject(hitEffect, hit.point, Quaternion.identity);
                }
            }
        }
        
        private void DestroyEffect()
        {
            if (gameObject.activeInHierarchy == false)
            {
                return;
            }
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
        }
    }
}
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
        private Transform from;
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
        
        public void Fire(Transform targetTransform, Transform fireTransform, float speedValue, int damageValue)
        {
            target = targetTransform;
            from = fireTransform;
            fromType = fireTransform.GetComponent<BaseUnitController>().unitType;
            speed = speedValue;
            damage = damageValue;
            
            transform.LookAt(targetTransform);
            ObjectPoolManager.SpawnObject(firingEffect, transform.position, Quaternion.identity).transform.LookAt(targetTransform);
            isFired = true;
        }

        private void HandleCollision(RaycastHit hit)
        {
            int otherLayer = hit.collider.gameObject.layer;

            if (otherLayer == groundLayer || otherLayer == unitLayer)
            {
                ObjectPoolManager.ReturnObjectToPool(gameObject);
                ObjectPoolManager.SpawnObject(hitEffect, hit.point, Quaternion.identity);
            }
            
            if (otherLayer == unitLayer)
            {
                var unitController = hit.collider.GetComponent<BaseUnitController>();
                if (unitController != null && !unitController.unitType.Equals(fromType))
                {
                    Debug.Log($"{from.name}: Hit {hit.collider.name}");
                }
            }
        }
    }
}
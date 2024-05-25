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
        private int unitNonSelectableLayer;
        private bool isFired;

        private void OnEnable()
        {
            isFired = false;
            groundLayer = LayerMask.NameToLayer("Ground");
            unitLayer = LayerMask.NameToLayer("Unit");
            unitNonSelectableLayer = LayerMask.NameToLayer("UnitNonSelectable");
            lastPosition = transform.position;
            CancelInvoke(nameof(DestroyEffect));
            Invoke(nameof(DestroyEffect), 3f);
        }

        private void Update()
        {
            if (isFired == false)
            {
                return;
            }

            if (target != null && target.gameObject.activeInHierarchy)
            {
                Vector3 targetDirection = (GetTargetCenter() - transform.position).normalized;
                transform.forward = targetDirection;
            }

            float step = speed * Time.deltaTime;
            Vector3 currentPosition = transform.position;
            Vector3 direction = currentPosition - lastPosition;
            float distance = direction.magnitude;

            if (distance > 0 && Physics.Raycast(lastPosition, direction.normalized, out var hit, distance + step))
            {
                HandleCollision(hit);
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

            float missRate = target.unitData.MissRate;
            Vector3 targetDirection = (GetTargetCenter() - transform.position).normalized;

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
            int otherLayer = hit.collider.gameObject.layer;

            if (otherLayer == groundLayer)
            {
                ObjectPoolManager.Instance.SpawnObject(hitEffect, hit.point, Quaternion.identity);
                ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
                return;
            }

            if ((otherLayer == unitLayer || otherLayer == unitNonSelectableLayer) && target != null && target.gameObject == hit.transform.gameObject)
            {
                var unitController = hit.collider.GetComponent<BaseUnitController>();
                if (unitController != null)
                {
                    unitController.ReceiveDamage(damage, shooter);
                    ObjectPoolManager.Instance.SpawnObject(hitEffect, hit.point, Quaternion.identity);
                }
                ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
            }
        }
        
        private Vector3 GetTargetCenter()
        {
            CapsuleCollider capsuleCollider = target.GetComponent<CapsuleCollider>();
            return capsuleCollider != null ? target.transform.TransformPoint(capsuleCollider.center) : target.transform.position;
        }
        
        private void DestroyEffect()
        {
            if (gameObject.activeInHierarchy)
            {
                ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
            }
        }
    }
}
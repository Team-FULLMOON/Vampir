using MyBox;
using System.Linq;
using UnityEngine;
using FullMoon.Util;
using FullMoon.Entities.Unit;

namespace FullMoon.Effect
{
    public class BulletEffectController : MonoBehaviour
    {
        [SerializeField] private GameObject firingEffect;
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private float detectionRadius = 0.1f;
        
        [Separator]
        
        [SerializeField] private bool isExplosive;
        [SerializeField, ConditionalField(nameof(isExplosive))]
        private float explosionRadius = 0.1f;

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

            if (target != null && target.gameObject.activeInHierarchy && target.Alive)
            {
                Vector3 targetDirection = (GetTargetCenter() - transform.position).normalized;
                transform.forward = targetDirection;
            }

            float step = speed * Time.deltaTime;
            Vector3 currentPosition = transform.position;
            Vector3 direction = currentPosition - lastPosition;
            float distance = direction.magnitude;

            if (distance >= 0)
            {
                Collider[] hits = Physics.OverlapSphere(lastPosition, detectionRadius);
                var targetHit = hits
                    .FirstOrDefault(hit => (hit.gameObject.layer == unitLayer || hit.gameObject.layer == unitNonSelectableLayer) && 
                                           target != null && hit.gameObject == target.gameObject);
                if (targetHit != null)
                {
                    HandleCollision(targetHit);
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

            GameObject fireFX = ObjectPoolManager.Instance.SpawnObject(firingEffect, shooter.unitModel.transform.position, Quaternion.identity);
            fireFX.transform.eulerAngles = shooter.unitModel.transform.eulerAngles;

            isFired = true;
        }

        private void HandleCollision(Collider hit)
        {
            if (isExplosive)
            {
                Collider[] colliders = Physics.OverlapSphere(hit.transform.position, explosionRadius);
                var unitControllers = colliders
                    .Select(unitCollider => unitCollider.GetComponent<BaseUnitController>())
                    .Where(unitController => unitController != null);

                foreach (var unitController in unitControllers)
                {
                    unitController.ReceiveDamage(damage, shooter);
                }
            }
            else
            {
                var unitController = hit.GetComponent<BaseUnitController>();
                if (unitController != null)
                {
                    unitController.ReceiveDamage(damage, shooter);
                }
            }

            ObjectPoolManager.Instance.SpawnObject(hitEffect, hit.transform.position, Quaternion.identity);
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
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

        private void OnDrawGizmos()
        {
            if (isFired)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, detectionRadius);
            }
            
            if (isExplosive)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, explosionRadius);
            }
        }
    }
}
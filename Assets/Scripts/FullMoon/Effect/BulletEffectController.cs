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

        private int groundLayer;
        private int unitLayer;

        private void Start()
        {
            groundLayer = LayerMask.NameToLayer("Ground");
            unitLayer = LayerMask.NameToLayer("Unit");
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
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
        }
        
        private void OnTriggerEnter(Collider other)
        {
            int otherLayer = other.gameObject.layer;
            
            if (otherLayer == groundLayer) 
            {
                // Destroy(gameObject);
                ObjectPoolManager.ReturnObjectToPool(gameObject);
                ObjectPoolManager.SpawnObject(hitEffect, transform.position, Quaternion.Euler(0, 0, 0));
            } 
            else if (otherLayer == unitLayer)
            {
                if (other.GetComponent<BaseUnitController>().unitType.Equals(fromType))
                {
                    return;
                }
                Debug.Log(other.name);
                // Destroy(gameObject);
                ObjectPoolManager.ReturnObjectToPool(gameObject);
                ObjectPoolManager.SpawnObject(hitEffect, transform.position, Quaternion.Euler(0, 0, 0));
            }
        }
    }
}

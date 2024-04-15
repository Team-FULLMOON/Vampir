using FullMoon.Util;
using UnityEngine;

namespace FullMoon.Entities.Unit
{
    public class RespawnController : MonoBehaviour
    {
        private int _manaCost;
        private float _createPrepareTime;
        private float _summonTime;
        private GameObject _unitTransformObject;

        private void Start()
        {
            CancelInvoke(nameof(Spawn));
        }

        public void Setup(int manaCost, float createPrepareTime, float summonTime, GameObject unitTransformObject)
        {
            _manaCost = manaCost;
            _createPrepareTime = createPrepareTime;
            _summonTime = summonTime;
            _unitTransformObject = unitTransformObject;
        }
        
        public void StartSpawn()
        {
            Invoke(nameof(Spawn), _summonTime);
        }
        
        private void Spawn()
        {
            ObjectPoolManager.SpawnObject(_unitTransformObject, transform.position, transform.rotation);
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
}

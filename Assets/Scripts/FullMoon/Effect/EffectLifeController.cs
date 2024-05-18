using UnityEngine;
using FullMoon.Util;

namespace FullMoon.Effect
{
    public class EffectLifeController : MonoBehaviour
    {
        [SerializeField] private float lifeDuration = 2f;
        
        private void OnEnable()
        {
            CancelInvoke(nameof(DestroyEffect));
            Invoke(nameof(DestroyEffect), lifeDuration);
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
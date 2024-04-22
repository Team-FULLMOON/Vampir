using UnityEngine;
using FullMoon.Util;

namespace FullMoon.Effect
{
    public class SwordEffectController : MonoBehaviour
    {
        private void OnEnable()
        {
            CancelInvoke(nameof(DestroyEffect));
            Invoke(nameof(DestroyEffect), 2f);
        }

        private void DestroyEffect()
        {
            if (gameObject.activeInHierarchy == false)
            {
                return;
            }
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
}
using UnityEngine;
using FullMoon.Util;

namespace FullMoon.Animation
{
    public class AnimController : MonoBehaviour
    {
        public void OnAnimDie()
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
        }
    }
}


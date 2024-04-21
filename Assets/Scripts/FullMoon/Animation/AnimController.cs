using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FullMoon.Util;

namespace FullMoon.Animation
{
    public class AnimController : MonoBehaviour
    {
        public void OnAnimDie()
        {
            ObjectPoolManager.ReturnObjectToPool(this.gameObject);
        }
    }
}


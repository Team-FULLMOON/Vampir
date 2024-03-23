using UnityEngine;

namespace FullMoon.Unit
{
    public interface ITakeDamage
    {
        void TakeDamage(float value, UnitController unit);
    }
}

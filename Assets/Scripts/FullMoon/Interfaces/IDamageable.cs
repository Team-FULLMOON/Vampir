using FullMoon.Entities.Unit;
using Unity.VisualScripting;

namespace FullMoon.Interfaces
{
    public interface IDamageable
    {
        int Hp { get; set; }
        void ReceiveDamage(int amount, BaseUnitController attacker);
    }
}
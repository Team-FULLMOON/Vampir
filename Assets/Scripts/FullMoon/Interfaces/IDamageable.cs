using FullMoon.Entities.Unit;

namespace FullMoon.Interfaces
{
    public interface IDamageable
    {
        int Hp { get; set; }
        void ReceiveDamage(int amount, BaseUnitController attacker);
        void Die();
    }
}
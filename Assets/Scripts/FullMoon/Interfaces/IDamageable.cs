using FullMoon.FSM;

namespace FullMoon.Interfaces
{
    public interface IDamageable
    {
        int Hp { get; set; }
        void ReceiveDamage(int amount, BaseUnitState attacker);
    }
}
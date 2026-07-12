namespace TwoWorlds.Combat
{
    public interface IDamageable
    {
        CombatFaction Faction { get; }
        bool IsAlive { get; }
        void TakeDamage(int amount, CombatActor source);
    }
}

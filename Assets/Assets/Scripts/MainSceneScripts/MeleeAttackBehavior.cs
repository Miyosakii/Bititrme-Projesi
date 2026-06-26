using UnityEngine;

public class MeleeAttackBehavior : BaseAttackBehavior
{
    public override void ExecuteAttack(Unit target, float damage)
    {
        if (target != null && target.IsAlive())
        {
            target.TakeDamage(damage);
        }
    }
}
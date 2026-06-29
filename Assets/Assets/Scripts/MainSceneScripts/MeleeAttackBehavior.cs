using UnityEngine;

public class MeleeAttackBehavior : BaseAttackBehavior
{
    public override void ExecuteAttack(Unit target, float damage)
    {
        Debug.Log("<color=yellow>MELEE SCRIPT TETIKLENDI!</color>");

        if (target != null && target.IsAlive())
        {
            Debug.Log($"<color=green>MELEE VURUÞ BAÞARILI!</color> {target.name} hedefine {damage} hasar veriliyor.");
            target.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("<color=red>MELEE HATA:</color> Hedef ya null ya da zaten ölü.");
        }
    }
}
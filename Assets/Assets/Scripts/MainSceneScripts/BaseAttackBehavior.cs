using UnityEngine;

// Sahnede tek baþýna bir iþe yaramaz, diðer sýnýflar bundan türeyecek
public abstract class BaseAttackBehavior : MonoBehaviour
{
    // Unit, hedefini ve hasar miktarýný bu fonksiyona paslayacak
    public abstract void ExecuteAttack(Unit target, float damage);
}
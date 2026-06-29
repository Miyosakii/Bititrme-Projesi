using UnityEngine;

public class ArrowDamageHandler : MonoBehaviour
{
    [HideInInspector] public int teamId;
    [HideInInspector] public float damage;

    // Send Collision Messages aktif olduðunda, ok bir þeye çarparsa burasý çalýþýr
    void OnParticleCollision(GameObject other)
    {
        // 1. Önce okun çarptýðý objede Unit var mý diye bak
        Unit hitUnit = other.GetComponent<Unit>();

        // 2. YENÝ: Yoksa, üst objelerine (Parent) doðru týrmanarak Unit ara!
        if (hitUnit == null)
        {
            hitUnit = other.GetComponentInParent<Unit>();
        }

        if (hitUnit != null && hitUnit.IsAlive())
        {
            // Kendi takým arkadaþýný vurmasýný engelliyoruz (Friendly Fire)
            if (hitUnit.teamId != this.teamId)
            {
                // Düþmanýn Unit scriptindeki can azaltma fonksiyonunu çaðýr
                hitUnit.TakeDamage(damage);
            }
        }
    }
}
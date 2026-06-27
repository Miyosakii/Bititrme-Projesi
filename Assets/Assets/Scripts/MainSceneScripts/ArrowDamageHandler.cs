using UnityEngine;

public class ArrowDamageHandler : MonoBehaviour
{
    [HideInInspector] public int teamId;
    [HideInInspector] public float damage;

    // Send Collision Messages aktif olduðunda, ok bir þeye çarparsa burasý çalýþýr
    void OnParticleCollision(GameObject other)
    {
        // Okun çarptýðý objede Unit scripti var mý diye bakýyoruz
        Unit hitUnit = other.GetComponent<Unit>();
        
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
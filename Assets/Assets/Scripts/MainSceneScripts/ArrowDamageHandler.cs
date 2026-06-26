using UnityEngine;

public class ArrowDamageHandler : MonoBehaviour
{
    [HideInInspector] public int teamId;
    [HideInInspector] public float damage;

    // Particle System'in Collision modülünden "Send Collision Messages" aktif olmalý!
    void OnParticleCollision(GameObject other)
    {
        // Okun çarptýðý objede Unit bileþeni var mý kontrol et
        Unit hitUnit = other.GetComponent<Unit>();

        if (hitUnit != null && hitUnit.IsAlive())
        {
            // Dost ateþi (Friendly Fire) olmamasý için takým kontrolü yap
            if (hitUnit.teamId != this.teamId)
            {
                hitUnit.TakeDamage(damage);
            }
        }
    }
}
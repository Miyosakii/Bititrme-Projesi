using UnityEngine;

public class RangedAttackBehavior : BaseAttackBehavior
{
    [Header("Okçu Ayarlarý")]
    public ParticleSystem arrowParticleSystem;
    private ArrowDamageHandler arrowDamageHandler;
    private Unit myUnit;

    void Start()
    {
        myUnit = GetComponent<Unit>();

        // Hasar handler'ýný otomatik kur
        if (arrowParticleSystem != null)
        {
            arrowDamageHandler = arrowParticleSystem.gameObject.AddComponent<ArrowDamageHandler>();
            arrowDamageHandler.teamId = myUnit.teamId;
            arrowDamageHandler.damage = myUnit.data.attackDamage;
        }
    }

    public override void ExecuteAttack(Unit target, float damage)
    {
        if (arrowParticleSystem != null && target != null)
        {
            // Oku düþmanýn gövdesine doðru hizala ve fýrlat
            Vector3 targetAimPosition = target.transform.position + (Vector3.up * 1.2f);
            arrowParticleSystem.transform.LookAt(targetAimPosition);
            arrowParticleSystem.Emit(1);
        }
    }
}
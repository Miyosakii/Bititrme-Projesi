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

        if (arrowParticleSystem != null)
        {
            // 1. Hasar sistemini kur
            arrowDamageHandler = arrowParticleSystem.gameObject.AddComponent<ArrowDamageHandler>();
            arrowDamageHandler.teamId = myUnit.teamId;
            arrowDamageHandler.damage = myUnit.data.attackDamage;

            // 2. YENÝ EKLENEN KISIM: Fiziksel çarpýþma maskesini ayarla
            var collisionModule = arrowParticleSystem.collision;

            if (myUnit.teamId == 0)
            {
                // Takým 0 okçusu sadece Takým 1'i ve Zemini vurabilir
                collisionModule.collidesWith = LayerMask.GetMask("Team1", "Zemin");
            }
            else if (myUnit.teamId == 1)
            {
                // Takým 1 okçusu sadece Takým 0'ý ve Zemini vurabilir
                collisionModule.collidesWith = LayerMask.GetMask("Team0", "Zemin");
            }
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
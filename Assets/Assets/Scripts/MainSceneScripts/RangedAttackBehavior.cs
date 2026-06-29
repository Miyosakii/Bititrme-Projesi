using UnityEngine;

public class RangedAttackBehavior : BaseAttackBehavior
{
    [Header("Okçu Ayarlarý")]
    public ParticleSystem arrowParticleSystem;

    [Header("Balistik Ayarlarý")]
    [Tooltip("Okun havaya doðru fýrlatýlma açýsý. Orta çað ok yaðmuru için 45-60 arasý idealdir.")]
    [Range(10f, 80f)]
    public float arcAngle = 50f;

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

            // 2. Fiziksel çarpýþma maskesini ayarla (Dost ateþi korumasý)
            var collisionModule = arrowParticleSystem.collision;

            if (myUnit.teamId == 0)
            {
                collisionModule.collidesWith = LayerMask.GetMask("Team1", "Zemin");
            }
            else if (myUnit.teamId == 1)
            {
                collisionModule.collidesWith = LayerMask.GetMask("Team0", "Zemin");
            }
        }
    }

    public override void ExecuteAttack(Unit target, float damage)
    {
        if (arrowParticleSystem != null && target != null)
        {
            float aimHeight = (target.data != null) ? target.data.aimHeightOffset : 1.2f;
            // 1. Baþlangýç ve bitiþ noktalarýný belirle
            Vector3 startPos = arrowParticleSystem.transform.position;
            // Oku düþmanýn doðrudan merkezine/gövdesine niþan al
            Vector3 targetPos = target.transform.position + (Vector3.up * aimHeight);

            // 2. Yatay mesafe (x) ve dikey yükseklik farkýný (y) hesapla
            Vector3 flatStart = new Vector3(startPos.x, 0f, startPos.z);
            Vector3 flatTarget = new Vector3(targetPos.x, 0f, targetPos.z);
            float x = Vector3.Distance(flatStart, flatTarget);
            float y = targetPos.y - startPos.y;

            // 3. Unity'nin fizik motorundan aktif yerçekimini al
            var mainModule = arrowParticleSystem.main;
            float gravityModifier = mainModule.gravityModifier.constant;
            float gravity = Mathf.Abs(Physics.gravity.y) * gravityModifier;

            // 4. Balistik Matematik (3D Eðik Atýþ Formülü)
            float angleRad = arcAngle * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(angleRad);
            float tanTheta = Mathf.Tan(angleRad);

            // Formülün alt kýsmýný (payda) hesapla
            float denominator = 2 * (cosTheta * cosTheta) * (x * tanTheta - y);

            // Eðer hedef atýþ açýsýndan çok yüksekteyse (Karakter daðýn tepesine dik açýyla atmaya çalýþýyorsa) 
            // payda sýfýrýn altýna düþebilir. Bunu önlemek için bir güvenlik kontrolü yapýyoruz.
            if (denominator > 0.001f)
            {
                // Formülü tamamla ve Particle System'in hýzýný dinamik olarak ata
                float requiredSpeed = Mathf.Sqrt((gravity * x * x) / denominator);
                mainModule.startSpeed = requiredSpeed;
            }
            else
            {
                // Güvenlik: Hedef fiziksel olarak aþýlamayacak bir yerdeyse varsayýlan bir hýzla fýrlat
                mainModule.startSpeed = 25f;
            }

            // 5. Partikül sisteminin yönünü ayarla (Hedefe bak ama yayý havaya kaldýr)
            Vector3 directionToTarget = (flatTarget - flatStart).normalized;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
                // X ekseninde eksi (-) yöne döndürmek obje rotasyonunda yukarý bakmak demektir
                arrowParticleSystem.transform.rotation = lookRotation * Quaternion.Euler(-arcAngle, 0f, 0f);
            }

            // 6. Oku ateþle!
            arrowParticleSystem.Emit(1);
        }
    }
}
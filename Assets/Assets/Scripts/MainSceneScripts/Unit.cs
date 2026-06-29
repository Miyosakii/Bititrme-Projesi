using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class Unit : MonoBehaviour
{
    [Header("Karakter Verisi")]
    public CharacterData data;

    [SerializeField] private Image teamIndicatorImage;
    [SerializeField] private Text victoryText;
    [SerializeField] private TextMeshProUGUI victoryTMP;
    private Canvas victoryCanvas;

    [HideInInspector] public int teamId = 0;
    [HideInInspector] public float health;
    [HideInInspector] public SpawnManager owner;
    [HideInInspector] public float spawnTime;
    [HideInInspector] public bool hasStartedMoving = false;
    [HideInInspector] public int lastPathfindFrame = -999;

    private float lastAttackTime = -999f;
    private Unit currentTarget;
    private NavMeshAgent navAgent;
    private AnimationManager animMgr;

    private float separationRadius;
    private float separationForce;
    private Vector3 separationVector = Vector3.zero;
    private BaseAttackBehavior attackBehavior;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animMgr = GetComponent<AnimationManager>();
        victoryTMP = GetComponent<TextMeshProUGUI>();
        attackBehavior = GetComponent<BaseAttackBehavior>();
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"{gameObject.name} üzerinde CharacterData atanmamış!");
            return;
        }

        health = data.maxHealth;

        separationRadius = data.separationRadius;
        separationForce = data.separationForce;

        if (teamIndicatorImage == null)
        {
            teamIndicatorImage = GetComponentInChildren<Image>();
        }

        if (victoryCanvas == null)
        {
            victoryCanvas = Object.FindAnyObjectByType<Canvas>();
            if (victoryCanvas != null)
            {
                victoryTMP = victoryCanvas.GetComponentInChildren<TextMeshProUGUI>();
                victoryText = victoryCanvas.GetComponentInChildren<Text>();
            }
        }

        if (navAgent != null)
        {
            navAgent.speed = data.moveSpeed;
            // ⭐ YENİ: NavMesh'in otomatik dönmesini kapatıyoruz.
            // Böylece hareket ederken veya geri kaçarken rotasyonu tamamen biz kontrol edebiliriz.
            navAgent.updateRotation = false;
        }

        // Layer ataması artık SpawnManager tarafından yönetiliyor.
        // Eğer Unit tek başına sahneye ekleniyorsa ve layer gereksinimi varsa,
        // SpawnManager.Spawn() kullanılması önerilir.

        CombatSystem.RegisterUnit(this);
    }

    void Update()
    {
        if (navAgent != null && navAgent.enabled && hasStartedMoving)
        {
            if (Time.frameCount % 3 == 0)
            {
                UpdateSeparation();
            }

            ApplySeparationForce();
        }

        // ⭐ YENİ: Karakter hayattaysa ve geçerli bir rakibi/hedefi varsa yüzünü HER ZAMAN ona çevirir
        if (IsAlive() && currentTarget != null && currentTarget.gameObject.activeInHierarchy && currentTarget.IsAlive())
        {
            FaceTarget();
        }
        // ⭐ YENİ: Eğer hedefi yoksa ama hareket ediyorsa (örn: ilk doğma anı veya arama evresi), yürüdüğü yöne baksın
        else if (navAgent != null && navAgent.enabled && navAgent.velocity.sqrMagnitude > 0.01f)
        {
            FaceMovementDirection();
        }
    }

    void OnDisable()
    {
        CombatSystem.UnregisterUnit(this);
        SpawnManager.allUnits.Remove(this);
    }

    private void UpdateSeparation()
    {
        separationVector = Vector3.zero;
        int neighborCount = 0;

        foreach (var other in SpawnManager.allUnits)
        {
            if (other == null || other == this || !other.gameObject.activeInHierarchy)
                continue;

            if (!other.IsAlive())
                continue;

            Vector3 toOther = other.transform.position - transform.position;
            float distance = toOther.magnitude;

            if (distance < separationRadius && distance > 0.01f)
            {
                separationVector -= toOther.normalized / distance;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            separationVector = separationVector.normalized * separationForce;
        }
    }

    private void ApplySeparationForce()
    {
        if (navAgent == null || !navAgent.enabled)
            return;

        Vector3 desiredVelocity = navAgent.desiredVelocity;
        Vector3 combinedVelocity = (desiredVelocity + separationVector).normalized;

        if (desiredVelocity.magnitude > 0)
        {
            navAgent.velocity = combinedVelocity * desiredVelocity.magnitude;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} TakeDamage çağrıldı! health: {health}");

        if (health <= 0)
            Die();
    }

    public void TryAttack()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy) return;
        if (Time.time - lastAttackTime < data.attackCooldown) return;
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance > data.attackRange) return;

        FaceTarget();
        lastAttackTime = Time.time;
    }

    public void Die()
    {
        health = 0;
        Debug.Log($"{gameObject.name} öldü!");

        if (animMgr != null)
            animMgr.SetCharacterState(CharacterStateType.FallingBack);

        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        CheckTeamStatus();

        StartCoroutine(DeactivateAfterDelay(2f));
    }

    private void CheckTeamStatus()
    {
        if (owner == null)
            return;

        if (!owner.HasActiveUnitsInTeam())
        {
            int losingTeamId = owner.teamId;
            int winningTeamId = losingTeamId == 0 ? 1 : 0;

            Debug.Log($"<color=red>❌ Takım {losingTeamId} KAYBETTI!</color>");
            Debug.Log($"<color=green>🏆 Takım {winningTeamId} KAZANDI!</color>");

            StartCoroutine(EndGameWithDelay(winningTeamId));
        }
    }

    private IEnumerator EndGameWithDelay(int winningTeamId)
    {
        yield return new WaitForSeconds(2f);
        Time.timeScale = 0f;
        GameEndManager.ShowVictory(winningTeamId);
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public void SetTeamColor(Color color)
    {
        if (teamIndicatorImage != null)
            teamIndicatorImage.color = color;
    }

    public void SetTarget(Unit target)
    {
        currentTarget = target;
    }

    public Unit GetTarget()
    {
        return currentTarget;
    }

    public Unit GetCurrentTarget()
    {
        return currentTarget;
    }

    public float GetDistanceToTarget()
    {
        if (currentTarget == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, currentTarget.transform.position);
    }

    /// <summary>
    /// Karakterin yüzünü hedefe pürüzsüzce çevirir
    /// </summary>
    private void FaceTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        direction.y = 0f; // Yukarı/aşağı eğilmeleri engelle (Y ekseninde sabit tut)

        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // CharacterData'dan offset uygula (Model ters bakıyorsa düzeltmek için)
        if (data != null && data.aimRotationOffset != 0f)
            lookRotation *= Quaternion.Euler(0f, data.aimRotationOffset, 0f);

        // Slerp ile yumuşak geçiş sağla (Hızı duruma göre 10f yerine değiştirebilirsin)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * 12f
        );
    }

    /// <summary>
    /// ⭐ YENİ: Karakterin aktif hedefi yoksa, hareket ettiği yöne doğru dönmesini sağlar.
    /// </summary>
    private void FaceMovementDirection()
    {
        Vector3 direction = navAgent.velocity.normalized;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * 6f
        );
    }
    public void OnAttackAnimationEvent()
    {
        // Asıl saldırı kodu
        if (currentTarget != null && currentTarget.IsAlive() && attackBehavior != null)
        {
            // Okçuysa ok fırlatır, kılıçlıysa kılıç vurur. Unit bununla ilgilenmez.
            attackBehavior.ExecuteAttack(currentTarget, data.attackDamage);
        }
    }
}
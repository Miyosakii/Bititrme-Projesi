using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI; // ⭐ YENİ: UI bileşenleri için

public class Unit : MonoBehaviour
{
    [Header("Karakter Verisi")]
    public CharacterData data; // Inspector'dan .asset dosyasını sürükle

    // ⭐ YENİ: Canvas Referansı
    [SerializeField] private Image teamIndicatorImage;

    // Çalışma zamanı değerleri
    [HideInInspector] public int teamId = 0;
    [HideInInspector] public float health;
    [HideInInspector] public SpawnManager owner;
    [HideInInspector] public float spawnTime;
    [HideInInspector] public bool hasStartedMoving = false;

    private float lastAttackTime = -999f;
    private Unit currentTarget;
    private NavMeshAgent navAgent;
    private AnimationManager animMgr;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animMgr = GetComponent<AnimationManager>();
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"{gameObject.name} üzerinde CharacterData atanmamış!");
            return;
        }

        health = data.maxHealth;

        // ⭐ YENİ: Eğer Canvas atanmamışsa otomatik bul
        if (teamIndicatorImage == null)
        {
            teamIndicatorImage = GetComponentInChildren<Image>();

        }

        if (navAgent != null)
            navAgent.speed = data.moveSpeed;

        CombatSystem.RegisterUnit(this);
    }

    void OnDisable()
    {
        CombatSystem.UnregisterUnit(this);
        SpawnManager.allUnits.Remove(this);
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
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            return;

        if (Time.time - lastAttackTime < data.attackCooldown)
            return;

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance > data.attackRange)
            return;

        lastAttackTime = Time.time;
        currentTarget.TakeDamage(data.attackDamage);

    }

    public void Die()
    {
        health = 0;
        Debug.Log($"{gameObject.name} öldü!");
        Debug.Log($"{gameObject.name} öldü! health: {health}, IsAlive: {IsAlive()}");

        if (animMgr != null)
            animMgr.SetCharacterState(CharacterStateType.FallingBack);

        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        // ⭐ YENİ: Karakter öldüğü zaman takım durumunu kontrol et
        CheckTeamStatus();

        StartCoroutine(DeactivateAfterDelay(2f));
    }

    // ⭐ YENİ: Takım durumunu kontrol et
    private void CheckTeamStatus()
    {
        if (owner == null)
            return;

        // Bu takımda aktif karakter kaldı mı?
        if (!owner.HasActiveUnitsInTeam())
        {
            // Kaybeden takım
            int losingTeamId = owner.teamId;
            int winningTeamId = losingTeamId == 0 ? 1 : 0;

            Debug.Log($"<color=red>❌ Takım {losingTeamId} KAYBETTI!</color>");
            Debug.Log($"<color=green>🏆 Takım {winningTeamId} KAZANDI!</color>");

            // ⭐ YENİ: Kazanan takımın tüm karakterlerini güncelle
            NotifyGameEnd(winningTeamId);
        }
    }

    // ⭐ YENİ: Oyun bittiğinde tüm karakterlere haber ver
    private void NotifyGameEnd(int winningTeamId)
    {
        foreach (var unit in SpawnManager.allUnits)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy)
                continue;

            // Kazanan takımın karakterleri
            if (unit.owner != null && unit.owner.teamId == winningTeamId && unit.IsAlive())
            {
                unit.OnGameEnd(true); // Kazanan
            }
            // Kaybeden takımın karakterleri
            else if (unit.owner != null && unit.owner.teamId != winningTeamId && unit.IsAlive())
            {
                unit.OnGameEnd(false); // Kaybeden
            }
        }
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    // ⭐ YENİ: Taraf Rengini Set Etme Metodu
    public void SetTeamColor(Color color)
    {
        if (teamIndicatorImage != null)
        {
            teamIndicatorImage.color = color;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} üzerinde Image bulunamadı!");
        }
    }

    // ⭐ YENİ: Oyun bittiğinde çağrılacak metod
    public void OnGameEnd(bool isWinner)
    {
        // NavMesh Agent'i devre dışı bırak (hareket etmesin)
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        // Saldırı ve hareket kontrolünü devre dışı bırak
        currentTarget = null;

        // ⭐ YENİ: AnimationManager'a bildir
        if (animMgr != null)
        {
            animMgr.OnGameEnd();
            
            if (isWinner)
            {
                // Kazanan takım - Idle animasyonu
                animMgr.SetCharacterState(CharacterStateType.Idle);
                Debug.Log($"✓ {gameObject.name} - KAZANDI!");
            }
            else
            {
                // Kaybeden takım - zaten ölü
                Debug.Log($"✗ {gameObject.name} - KAYBETTI!");
            }
        }
    }

    public Unit GetCurrentTarget() => currentTarget;
    public void SetTarget(Unit target) => currentTarget = target;
    public bool IsAlive() => health > 0;
}
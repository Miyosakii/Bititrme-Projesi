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

    private float lastAttackTime = -999f;
    private Unit currentTarget;
    private NavMeshAgent navAgent;
    private AnimationManager animMgr;
    
    // ⭐ DÜZELTME: Data'dan alınan değerler
    private float separationRadius;
    private float separationForce;
    private Vector3 separationVector = Vector3.zero;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animMgr = GetComponent<AnimationManager>();
        victoryTMP = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"{gameObject.name} üzerinde CharacterData atanmamış!");
            return;
        }

        health = data.maxHealth;

        // ⭐ YENİ: Data'dan Separation değerlerini al
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
            navAgent.speed = data.moveSpeed;

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

            // ⭐ GÜNCELLEME: Data'dan alınan separationRadius kullan
            if (distance < separationRadius && distance > 0.01f)
            {
                separationVector -= toOther.normalized / distance;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            // ⭐ GÜNCELLEME: Data'dan alınan separationForce kullan
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
}
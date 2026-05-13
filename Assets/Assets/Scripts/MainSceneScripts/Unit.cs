using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    [Header("Karakter Verisi")]
    public CharacterData data; // Inspector'dan .asset dosyasını sürükle

    // Çalışma zamanı değerleri
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

        if (navAgent != null)
            navAgent.speed = data.moveSpeed;

        // Artık CombatSystem tek liste kullanıyor
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

        StartCoroutine(DeactivateAfterDelay(2f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public Unit GetCurrentTarget() => currentTarget;
    public void SetTarget(Unit target) => currentTarget = target;
    public bool IsAlive() => health > 0;
}
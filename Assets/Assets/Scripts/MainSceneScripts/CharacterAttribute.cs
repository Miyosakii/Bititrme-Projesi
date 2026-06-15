// CharacterAttribute.cs - SADECE can ve hasar sistemi
using UnityEngine;

public class CharacterAttribute : MonoBehaviour
{
    // Prefab ayarları
    public static float prefabMaxHealth = 100f;
    public static float attackPower = 20f;
    public static float collisionDistance = 1f;  // Eski sistem (collider çarpışması)
    public static float attackRange = 2.5f;      // YENİ: Saldırı menzili
    public static float damageInterval = 1f;

    // Örnek ayarları
    [SerializeField] private float currentHealth;
    private float maxHealth;
    private Unit targetUnit;
    private CharacterAttribute targetAttribute;

    // Cache için
    private AnimationManager animationManager;

    void Start()
    {
        maxHealth = prefabMaxHealth;
        currentHealth = maxHealth;
        animationManager = GetComponent<AnimationManager>();
        CombatSystem.RegisterUnit(GetComponent<Unit>());
    }

    void OnDestroy()
    {
        CombatSystem.UnregisterUnit(GetComponent<Unit>());
    }

    void Update()
    {
        // Hedefin mesafesini kontrol et
        if (targetUnit != null && targetUnit.gameObject.activeInHierarchy)
        {
            float distance = Vector3.Distance(transform.position, targetUnit.transform.position);

            // Mesafe çok fazla artarsa hedef bırak (5 birim)
            if (distance > 5f)
            {
                targetUnit = null;
                targetAttribute = null;
            }
        }
    }

    public void DealDamageToTarget()
    {
        Unit unit = GetComponent<Unit>();
        Unit targetUnit = unit.GetTarget();  // ← Unit'den al
        
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(GetAttackPower());
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }

        Debug.Log($"{gameObject.name} hasar aldı: {damage}, Kalan can: {currentHealth}");
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        Debug.Log($"{gameObject.name} iyileştirildi: {amount}, Toplam can: {currentHealth}");
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} öldü!");

        // Ölüm state'ini ayarla
        if (animationManager != null)
            animationManager.PlayFallingBackState();

        targetUnit = null;
        targetAttribute = null;
        gameObject.SetActive(false);
    }

    public float GetAttackPower()
    {
        return attackPower;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return (currentHealth / maxHealth) * 100f;
    }

    public bool IsAlive()
    {
        return currentHealth > 0f;
    }

    public void SetTarget(Unit target)
    {
        targetUnit = target;
        targetAttribute = target?.GetComponent<CharacterAttribute>();
    }

    public Unit GetTarget()
    {
        return targetUnit;
    }

    /// <summary>
    /// Hedefin menzil içinde olup olmadığını kontrol et
    /// </summary>
    public bool IsTargetInAttackRange()
    {
        if (targetUnit == null)
            return false;

        float distance = Vector3.Distance(transform.position, targetUnit.transform.position);
        return distance <= attackRange;
    }

    /// <summary>
    /// Hedefin mesafesini döndür
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (targetUnit == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, targetUnit.transform.position);
    }

    public static float GetAttackRange()
    {
        return attackRange;
    }
}


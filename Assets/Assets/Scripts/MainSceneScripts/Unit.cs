using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public SpawnManager owner;
    public float spawnTime;
    public bool hasStartedMoving = false;

    // Hasar Sistemi
    public float health = 100f;
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private Unit currentTarget;

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} hasar aldı: {damage}, Kalan can: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    public void StartAttack()
    {
        isAttacking = true;
    }

    public void DealDamageToNearby()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var collider in hitColliders)
        {
            Unit targetUnit = collider.GetComponent<Unit>();
            if (targetUnit == null || targetUnit.health <= 0)
                continue;

            if (targetUnit == this || targetUnit.owner == this.owner)
                continue;

            targetUnit.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} → {targetUnit.gameObject.name} e hasar verdi!");
            
            if (currentTarget == null)
                currentTarget = targetUnit;
        }
    }

    public Unit GetCurrentTarget()
    {
        return currentTarget;
    }

    public void SetTarget(Unit target)
    {
        currentTarget = target;
    }

    public void Die()
    {
        Debug.Log($"{gameObject.name} öldü!");
        
        // ⭐ Falling Back animasyonuna geç (ölme animasyonu)
        AnimationManager animMgr = GetComponent<AnimationManager>();
        if (animMgr != null)
            animMgr.SetCharacterState(CharacterStateType.FallingBack);

        // Hareketi durdur
        NavMeshAgent navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        // 2 saniye sonra deaktif et (animasyonun oynaması için zaman ver)
        StartCoroutine(DeactivateAfterDelay(2f));

        // allUnits listesinden kaldır
        if (SpawnManager.allUnits.Contains(this))
            SpawnManager.allUnits.Remove(this);
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
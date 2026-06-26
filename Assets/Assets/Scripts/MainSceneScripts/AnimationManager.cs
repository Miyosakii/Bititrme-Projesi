using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Merkezi animasyon yönetimi sistemi
/// Tüm animasyon tetiklemeleri buradan yapılır
/// GetComponent çağrıları minimize edilir
/// </summary>
public class AnimationManager : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    
    // Cache - Her frame GetComponent çağırmamak için
    private CharacterState characterState;
    private AnimationController animationController;

    private Coroutine attackCoroutine;
    
    // ⭐ YENİ: Oyun bittiğini gösteren flag
    private bool gameEnded = false;

    void Start()
    {
        // Component referanslarını cache'le
        animator = GetComponent<Animator>();
        
        animationController = GetComponent<AnimationController>();
        characterState = GetComponent<CharacterState>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // ⭐ YENİ: Oyun biterse animasyon güncellemesini durdur
        if (gameEnded)
            return;

        // NavMesh Agent hareket hızına göre animasyonu otomatik güncelle
        UpdateAnimationBasedOnNavMeshAgent();
    }

    // ⭐ YENİ: Oyun bittiğinde çağrılacak metod
    public void OnGameEnd()
    {
        gameEnded = true;
        
        // Attack loop'u durdur
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        
        attackCoroutine = null;
    }

    /// <summary>
    /// NavMesh Agent'in hızına göre animasyonu güncelle
    /// </summary>
    private void UpdateAnimationBasedOnNavMeshAgent()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;

        CharacterStateType currentState = characterState?.CurrentState ?? CharacterStateType.Idle;

        // Agent hedefe doğru gidiyor mu?
        if (navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            // Hedefe gitmekte - Running animasyonu çalışmalı
            if (currentState != CharacterStateType.Running && 
                currentState != CharacterStateType.Attack && 
                currentState != CharacterStateType.FallingBack)
            {
                SetCharacterState(CharacterStateType.Running);
            }
        }
        else if (navMeshAgent.velocity.sqrMagnitude <= 0.01f)
        {
            // Agent durmuş - Idle animasyonuna geç
            if (currentState == CharacterStateType.Running)
            {
                SetCharacterState(CharacterStateType.Idle);
            }
        }
    }

    /// <summary>
    /// Karakter durumunu güncelle ve uygun animasyonu çalıştır
    /// SpawnManager ve CombatSystem tarafından çağrılır
    /// </summary>
    public void SetCharacterState(CharacterStateType newState)
    {
        if (characterState == null || animationController == null)
            return;

        CharacterStateType oldState = characterState.CurrentState;
        
        // Durum değişmediyse animasyon tekrar çalıştırma
        if (oldState == newState)
            return;

        characterState.CurrentState = newState;
        PlayAnimationForState(newState);
    }

    /// <summary>
    /// Duruma göre uygun animasyonu çalıştır
    /// </summary>
    private void PlayAnimationForState(CharacterStateType state)
    {
        if (animationController == null)
            return;

        switch (state)
        {
            case CharacterStateType.Idle:
                // Eski attack coroutine'i durdur
                if (attackCoroutine != null)
                    StopCoroutine(attackCoroutine);
                
                animationController.PlayIdleAnimation();
                break;

            case CharacterStateType.Running:
                // Eski attack coroutine'i durdur
                if (attackCoroutine != null)
                    StopCoroutine(attackCoroutine);
                
                animationController.PlayRunningAnimation();
                break;

            case CharacterStateType.Attack:
                // ⭐ Attack coroutine'ini başlat (zaten çalışıyorsa tekrar başlama)
                if (attackCoroutine == null)
                    attackCoroutine = StartCoroutine(ContinuousAttackLoop());
                
                animationController.PlayAttackAnimation();
                break;

            case CharacterStateType.FallingBack:
                if (attackCoroutine != null)
                    StopCoroutine(attackCoroutine);
                
                animationController.PlayFallingBackAnimation();
                break;
        }
    }

    /// <summary>
    /// Sürekli saldırı döngüsü - düşman öldüğünde duracak
    /// </summary>
    private IEnumerator ContinuousAttackLoop()
    {
        Unit unit = GetComponent<Unit>();
        if (unit == null) yield break;

        while (unit != null && unit.gameObject.activeInHierarchy &&
               unit.GetCurrentTarget() != null && unit.GetCurrentTarget().IsAlive())
        {
            // Sadece animasyonu tetikle. 
            // Hasar, animasyon klibindeki Animation Event (OnAttackAnimationEvent) üzerinden otomatik verilecek.
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Bir sonraki saldırı hakkına kadar bekle (Cooldown)
            yield return new WaitForSeconds(unit.data.attackCooldown);

            if (unit.GetCurrentTarget() == null || !unit.GetCurrentTarget().IsAlive())
                break;
        }

        // Saldırı bitti (hedef öldü veya kayboldu)
        attackCoroutine = null;

        if (unit != null)
        {
            unit.SetTarget(null);
        }

        // Sadece Idle durumuna geç.
        // Yeni hedef bulma ve koşturma işini UnitMovementManager kendi Update'inde otomatik yapacak!
        if (characterState != null)
            characterState.CurrentState = CharacterStateType.Idle;

        if (animationController != null)
            animationController.PlayIdleAnimation();
    }

    /// <summary>
    /// Mevcut karakter durumunu döndür
    /// </summary>
    public CharacterStateType GetCurrentState()
    {
        return characterState?.CurrentState ?? CharacterStateType.Idle;
    }

    /// <summary>
    /// Animasyonun bitmesini bekle (coroutine için)
    /// </summary>
    public float GetAnimationDuration(string animationName)
    {
        if (animator == null)
            return 0f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }

    /// <summary>
    /// Karakter öldü/geriye düştü
    /// </summary>
    public void PlayFallingBackState()
    {
        SetCharacterState(CharacterStateType.FallingBack);
    }
}

/// <summary>
/// Karakter durumları enum'u
/// Mevcut animasyonlara göre tanımlandı:
/// - Idle: Boşta durma
/// - Running: Koşma
/// - Attack: Saldırma
/// - FallingBack: Geriye düşme/Ölüm
/// </summary>
public enum CharacterStateType
{
    Idle = 0,
    Running = 1,
    Attack = 2,
    FallingBack = 3,
    Death = 4
}

using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
            Debug.LogError($"Animator component not found on {gameObject.name}");
    }

    /// <summary>
    /// Boþta durma animasyonunu tetikle
    /// </summary>
    public void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Idle");
        }
    }

    /// <summary>
    /// Koþma animasyonunu tetikle
    /// </summary>
    public void PlayRunningAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.SetTrigger("Running");
        }
    }

    /// <summary>
    /// Saldýrý animasyonunu tetikle
    /// </summary>
    public void PlayAttackAnimation(int attackIndex = 0)
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Geriye düþme / Ölüm animasyonunu tetikle
    /// </summary>
    public void PlayFallingBackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("FallingBack");
        }
    }

    /// <summary>
    /// Herhangi bir animation state'ini tetikle (geniþletilebilir)
    /// </summary>
    public void TriggerAnimation(string triggerName)
    {
        if (animator != null)
            animator.SetTrigger(triggerName);
    }

    /// <summary>
    /// Bool parametresi ayarla
    /// </summary>
    public void SetBool(string parameterName, bool value)
    {
        if (animator != null)
            animator.SetBool(parameterName, value);
    }

    /// <summary>
    /// Float parametresi ayarla
    /// </summary>
    public void SetFloat(string parameterName, float value)
    {
        if (animator != null)
            animator.SetFloat(parameterName, value);
    }
}
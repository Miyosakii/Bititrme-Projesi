using UnityEngine;

/// <summary>
/// Karakterin mevcut durumunu takip eder
/// Hangi state'te olduðunu merkezileþtirir
/// </summary>
public class CharacterState : MonoBehaviour
{
    private CharacterStateType currentState = CharacterStateType.Idle;

    public CharacterStateType CurrentState
    {
        get => currentState;
        set => currentState = value;
    }

    /// <summary>
    /// Karakterin belirli bir durumda olup olmadýðýný kontrol et
    /// </summary>
    public bool IsInState(CharacterStateType state)
    {
        return currentState == state;
    }

    /// <summary>
    /// Karakterin hareket edip etmediðini kontrol et
    /// </summary>
    public bool IsMoving()
    {
        return currentState == CharacterStateType.Running;
    }

    /// <summary>
    /// Karakter öldü mü?
    /// </summary>
    public bool IsDead()
    {
        return currentState == CharacterStateType.FallingBack;
    }
}

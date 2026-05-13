using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Battle Sim/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Genel")]
    public string characterName;

    [Header("Can")]
    public float maxHealth = 100f;

    [Header("Saldýrý")]
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Hareket")]
    public float moveSpeed = 3.5f;
}
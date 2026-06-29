using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Battle Sim/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Genel")]
    public string characterName;

    [Header("Can")]
    public float maxHealth = 100f;

    [Header("Saldırı")]
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Hareket")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.5f;

    [Header("Çarpışma Kaçınması")]
    public float separationRadius = 1f;      // ⭐ YENİ: Algılama yarıçapı
    public float separationForce = 1.5f;     // ⭐ YENİ: İtme gücü

    [Header("Animasyon")]
    public float aimRotationOffset = 0f;

    [Header("Birim Tipi")]
    public bool isFlying = false; // Ejderhalar için bunu Unity'den TRUE yapacağız

    [Header("Menzilli Birim Ayarları")]
    public bool isRanged = false;            // Karakter okçu mu?
    public float retreatDistance = 3f;       // Düşman bu mesafeden yakına gelirse geri kaçar
    public float retreatDistanceMove = 4f;   // Bir seferde ne kadar geriye doğru adım atacağı

    [Header("Hedeflenme (Nişan Noktası)")]
    [Tooltip("Düşman okçuları bu karaktere ateş ederken yerden ne kadar yukarıya nişan almalı? (İnsanlar için 1.2, Ejderhalar için örn: 8)")]
    public float aimHeightOffset = 0.7f;

}
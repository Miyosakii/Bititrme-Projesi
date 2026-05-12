// CombatSystem.cs - SADECE hasar mekanizması
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;

    static List<Unit> allUnits = new List<Unit>();
    private Dictionary<Unit, float> lastDamageTime = new Dictionary<Unit, float>();

    void Update()
    {
        UpdateCombat();
    }

    void UpdateCombat()
    {
        foreach (var unit in allUnits)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy)
                continue;

            CharacterAttribute charAttr = unit.GetComponent<CharacterAttribute>();
            if (charAttr == null)
                continue;

            Unit target = charAttr.GetTarget();
            if (target == null)
                continue;

            AnimationManager animMgr = unit.GetComponent<AnimationManager>();
            
            // ⭐ COLLIDER ÇARPIŞMA KONTROL
            if (IsColliding(unit, target))
            {
                Debug.Log($"Collision detected between {unit.name} and {target.name}");
                // Çarpışma var - Saldır!
                if (!lastDamageTime.ContainsKey(unit))
                    lastDamageTime[unit] = 0f;

                if (Time.time - lastDamageTime[unit] >= damageInterval)
                {
                    // Attack animasyonunu tetikle
                    if (animMgr != null)
                        animMgr.SetCharacterState(CharacterStateType.Attack);

                    // Hasar ver
                    charAttr.DealDamageToTarget();
                    lastDamageTime[unit] = Time.time;
                }
            }
            else
            {
                // Çarpışma yok - Running devam et
                if (animMgr != null)
                {
                    CharacterStateType currentState = animMgr.GetCurrentState();
                    if (currentState == CharacterStateType.Attack)
                    {
                        // Attack bitmiş, Running'e dön
                        animMgr.SetCharacterState(CharacterStateType.Running);
                    }
                }
            }
        }
    }

    /// <summary>
    /// İki karakterin collider'larının çakışıp çakışmadığını kontrol et
    /// </summary>
    private bool IsColliding(Unit unit1, Unit unit2)
    {
        Collider col1 = unit1.GetComponent<Collider>();
        Collider col2 = unit2.GetComponent<Collider>();

        if (col1 == null || col2 == null)
            return false;

        return col1.bounds.Intersects(col2.bounds);
    }

    public static void RegisterUnit(Unit unit)
    {
        if (!allUnits.Contains(unit))
            allUnits.Add(unit);
    }

    public static void UnregisterUnit(Unit unit)
    {
        allUnits.Remove(unit);
    }
}
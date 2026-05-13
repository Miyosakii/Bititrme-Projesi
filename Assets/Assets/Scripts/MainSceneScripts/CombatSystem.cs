using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    private static List<Unit> allUnits = new List<Unit>();

    void Update()
    {
        for (int i = allUnits.Count - 1; i >= 0; i--)
        {
            Unit unit = allUnits[i];

            if (unit == null || !unit.gameObject.activeInHierarchy || !unit.IsAlive())
                continue;

            unit.TryAttack();
        }
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
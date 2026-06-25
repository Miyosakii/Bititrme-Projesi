using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    // ⭐ Taraf Rengi
    [Header("Taraf Ayarları")]
    public int teamId = 0;
    public Color teamColor = Color.red;

    public GameObject prefab;
    public int spawnCount = 20;

    public int rowSize = 5;
    public float spacing = 2f;
    public float stoppingDistance = 0.5f;
    public float movementDelay = 5f;

    public float positionJitterRange = 0.3f;
    public bool randomizeRotation = true;
    public bool randomizeScale = false;
    public float scaleVariation = 0.1f;

    public Transform parent;

    public bool autoSpawnOnStart = false;

    List<GameObject> pool = new List<GameObject>();
    public static List<Unit> allUnits = new List<Unit>();

    void Start()
    {
        if (autoSpawnOnStart)
            Spawn();
    }

    public void Update()
    {
        UpdateMovement();
    }

    public void Spawn()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab atanmamış!");
            return;
        }

        Clear();

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = CalculatePosition(i);
            GameObject obj = GetFromPool();
            obj.transform.position = pos;
            
            if (randomizeRotation)
            {
                float randomYRotation = Random.Range(0f, 360f);
                obj.transform.rotation = Quaternion.Euler(0, randomYRotation, 0);
            }

            if (randomizeScale)
            {
                float randomScale = Random.Range(1f - scaleVariation, 1f + scaleVariation);
                obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            }

            obj.SetActive(true);

            EnsureAnimatorExists(obj);

            Unit unit = obj.GetComponent<Unit>();
            if (unit == null)
                unit = obj.AddComponent<Unit>();

            unit.teamId = this.teamId;
            unit.owner = this;
            unit.spawnTime = Time.time;

            unit.SetTeamColor(this.teamColor);

            if (!allUnits.Contains(unit))
                allUnits.Add(unit);

            NavMeshAgent navAgent = obj.GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.stoppingDistance = unit.data != null ? unit.data.stoppingDistance : stoppingDistance;
                navAgent.enabled = true;

                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(obj.transform.position, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    navAgent.Warp(hit.position);
                }
            }

            AnimationManager animMgr = obj.GetComponent<AnimationManager>();
            if (animMgr != null)
                animMgr.SetCharacterState(CharacterStateType.Idle);
        }
    }

    public Vector3 CalculatePosition(int index)
    {
        int row = index / rowSize;
        int col = index % rowSize;

        Vector3 basePos = transform.position + new Vector3(col * spacing, 5f, row * spacing);
        
        RaycastHit hit;
        if (Physics.Raycast(basePos + Vector3.up * 100f, Vector3.down, out hit))
        {
            basePos.y = hit.point.y + 0.1f;
        }
        
        Vector3 jitter = new Vector3(
            Random.Range(-positionJitterRange, positionJitterRange),
            0,
            Random.Range(-positionJitterRange, positionJitterRange)
        );

        return basePos + jitter;
    }

    public GameObject GetFromPool()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }

        GameObject newObj = Instantiate(prefab, parent);
        pool.Add(newObj);
        return newObj;
    }

    public void Clear()
    {
        foreach (var obj in pool)
        {
            obj.SetActive(false);
            
            NavMeshAgent navAgent = obj.GetComponent<NavMeshAgent>();
            if (navAgent != null)
                navAgent.ResetPath();
        }
    }

    void UpdateMovement()
    {
        foreach (var unit in allUnits)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy)
                continue;

            float elapsedTime = Time.time - unit.spawnTime;
            if (elapsedTime < movementDelay)
                continue;

            AnimationManager animMgr = unit.GetComponent<AnimationManager>();
            NavMeshAgent navAgent = unit.GetComponent<NavMeshAgent>();

            if (navAgent == null || !navAgent.enabled)
                continue;

            if (!unit.hasStartedMoving)
            {
                unit.hasStartedMoving = true;
                if (animMgr != null)
                    animMgr.SetCharacterState(CharacterStateType.Running);
            }

            Unit nearest = unit.GetCurrentTarget();

            if (nearest == null || !nearest.gameObject.activeInHierarchy || !nearest.IsAlive())
            {
                nearest = FindNearestEnemy(unit);
                
                if (nearest != null)
                {
                    unit.SetTarget(nearest);
                    if (animMgr != null)
                        animMgr.SetCharacterState(CharacterStateType.Running);
                }
            }
            
            if (nearest == null)
            {
                navAgent.ResetPath();
                unit.SetTarget(null);
                if (animMgr != null)
                    animMgr.SetCharacterState(CharacterStateType.Idle);
                continue;
            }

            if (HasReachedDestination(navAgent))
            {
                navAgent.ResetPath();
                Unit target = unit.GetCurrentTarget();
                if (target != null && target.IsAlive())
                {
                    if (animMgr != null)
                        animMgr.SetCharacterState(CharacterStateType.Attack);
                }
                else
                {
                    unit.SetTarget(null);
                    if (animMgr != null)
                        animMgr.SetCharacterState(CharacterStateType.Idle);
                }
                continue;
            }

            int offset = Mathf.Abs(unit.GetInstanceID()) % 5;
            if ((Time.frameCount + offset - unit.lastPathfindFrame) >= 5)
            {
                unit.lastPathfindFrame = Time.frameCount;
                SetNavMeshDestination(navAgent, nearest.transform.position);
            }
        }
    }

    bool HasReachedDestination(NavMeshAgent navAgent)
    {
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh)
            return false;

        if (!navAgent.hasPath || navAgent.pathPending)
            return false;

        if (navAgent.hasPath && navAgent.remainingDistance > navAgent.stoppingDistance)
            return false;

        if (!navAgent.hasPath || navAgent.desiredVelocity.sqrMagnitude > 0f)
            return false;

        return true;
    }

    void SetNavMeshDestination(NavMeshAgent navAgent, Vector3 targetPosition)
    {
        if (navAgent == null || !navAgent.isOnNavMesh)
            return;

        navAgent.SetDestination(targetPosition);
    }

    public Unit FindNearestEnemy(Unit current)
    {
        Unit nearest = null;
        float minDistance = float.MaxValue;

        foreach (var other in allUnits)
        {
            if (other == null || other == current)
                continue;

            if (!other.gameObject.activeInHierarchy)
                continue;

            if (!other.IsAlive())
                continue;

            if (other.owner == current.owner)
                continue;

            float distance = Vector3.Distance(current.transform.position, other.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = other;
            }
        }

        return nearest;
    }

    private void EnsureAnimatorExists(GameObject character)
    {
        Animator animator = character.GetComponent<Animator>();
        
        if (animator == null)
        {
            animator = character.GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError($"HATA: {character.name} içinde Animator bulunamadı! " +
                           $"Lütfen prefab'a Animator component'i ekleyin.");
        }
        else
        {
            Debug.Log($"✓ {character.name} → Animator bulundu");
        }
    }

    public bool HasActiveUnits()
    {
        return allUnits.Where(unit => unit != null && unit.gameObject.activeInHierarchy && unit.IsAlive()).Count() > 0;
    }

    public bool HasActiveUnitsInTeam()
    {
        return allUnits.Where(unit => 
            unit != null && 
            unit.gameObject.activeInHierarchy && 
            unit.IsAlive() && 
            unit.owner == this).Count() > 0;
    }

    public string GetPoolStatus()
    {
        int activeCount = allUnits.Where(unit => 
            unit != null && 
            unit.gameObject.activeInHierarchy && 
            unit.IsAlive() && 
            unit.owner == this).Count();

        if (activeCount == 0)
        {
            return $"❌ Takım {teamId} - Havuzda karakter kalmadı!";
        }
        else
        {
            return $"✓ Takım {teamId} - Aktif karakterler: {activeCount}";
        }
    }
}
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class UnitMovementManager : MonoBehaviour
{
    [Header("Dinamik Baðlantý")]
    [Tooltip("Bu hareket yöneticisinin kontrol edeceði SpawnManager")]
    public SpawnManager mySpawner;

    void Start()
    {
        // Eðer Inspector'dan sürüklemeyi unutursan, ayný objedeki Spawner'ý otomatik bulsun
        if (mySpawner == null)
            mySpawner = GetComponent<SpawnManager>();

        if (mySpawner == null)
            Debug.LogError($"{gameObject.name} üzerinde SpawnManager bulunamadý!");
    }

    void Update()
    {
        if (mySpawner != null)
        {
            UpdateTeamMovement();
        }
    }

    void UpdateTeamMovement()
    {
        // Ortak listedeki tüm birimleri dönüyoruz...
        foreach (var unit in SpawnManager.allUnits)
        {
            // ...ama SADECE bu spawner'a (bu takýma) ait olanlarý filtreleyip hareket ettiriyoruz!
            if (unit == null || !unit.gameObject.activeInHierarchy || unit.owner != mySpawner)
                continue;

            float elapsedTime = Time.time - unit.spawnTime;
            if (elapsedTime < mySpawner.movementDelay)
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

            // Hedef öldüyse veya yoksa yeni hedef bul
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

            // --- YOL BULMA VE KITING (GERÝ ÇEKÝLME) MANTIÐI ---
            int offset = Mathf.Abs(unit.GetInstanceID()) % 5;
            if ((Time.frameCount + offset - unit.lastPathfindFrame) >= 5)
            {
                unit.lastPathfindFrame = Time.frameCount;

                float distanceToEnemy = Vector3.Distance(unit.transform.position, nearest.transform.position);

                // Eðer birim okçu ise ve düþman 'retreatDistance' mesafesinden daha yakýna girdiyse
                if (unit.data != null && unit.data.isRanged && distanceToEnemy < unit.data.retreatDistance)
                {
                    // Düþmandan zýt yöne doðru bir vektör hesapla
                    Vector3 retreatDir = (unit.transform.position - nearest.transform.position).normalized;
                    Vector3 targetRetreatPos = unit.transform.position + (retreatDir * unit.data.retreatDistanceMove);

                    // Geri çekileceði yerin NavMesh üzerinde (uçurum vs. deðil) olduðundan emin ol
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(targetRetreatPos, out hit, unit.data.retreatDistanceMove, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        SetNavMeshDestination(navAgent, hit.position);
                        if (animMgr != null)
                            animMgr.SetCharacterState(CharacterStateType.Running);
                    }
                    else
                    {
                        // Arkasý duvarsa yapacak bir þey yok, saldýrmaya devam etsin
                        SetNavMeshDestination(navAgent, nearest.transform.position);
                    }
                }
                else
                {
                    // Okçu deðilse veya düþman güvenli mesafedeyse normal takip yap
                    SetNavMeshDestination(navAgent, nearest.transform.position);
                }
            }
        }
    }

    bool HasReachedDestination(NavMeshAgent navAgent)
    {
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh) return false;
        if (!navAgent.hasPath || navAgent.pathPending) return false;
        if (navAgent.hasPath && navAgent.remainingDistance > navAgent.stoppingDistance) return false;
        if (!navAgent.hasPath || navAgent.desiredVelocity.sqrMagnitude > 0f) return false;
        return true;
    }

    void SetNavMeshDestination(NavMeshAgent navAgent, Vector3 targetPosition)
    {
        if (navAgent == null || !navAgent.isOnNavMesh) return;
        navAgent.SetDestination(targetPosition);
    }

    public Unit FindNearestEnemy(Unit current)
    {
        Unit nearest = null;
        float minDistance = float.MaxValue;

        // Düþman bulurken global listeyi kullanýyoruz, bu sayede diðer takýmý görebilirler
        foreach (var other in SpawnManager.allUnits)
        {
            if (other == null || other == current) continue;
            if (!other.gameObject.activeInHierarchy || !other.IsAlive()) continue;

            // Kendi takýmýmýzdan olanlarý es geçiyoruz!
            if (other.owner == current.owner) continue;

            float distance = Vector3.Distance(current.transform.position, other.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = other;
            }
        }

        return nearest;
    }
}
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

    public void Spawn()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab atanmamış!");
            return;
        }

        Clear();

        // Takım layer'ını hesapla (varsa)
        string teamLayerName = $"Team{teamId}";
        int teamLayer = LayerMask.NameToLayer(teamLayerName);
        if (teamLayer == -1)
        {
            Debug.LogWarning($"Layer '{teamLayerName}' bulunamadı! Lütfen Project Settings -> Tags and Layers içinde '{teamLayerName}' layer'ını ekleyin.");
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = CalculatePosition(i);
            GameObject obj = GetFromPool();

            // Pool'dan geliyorsa parent'ı güncelle
            if (parent != null)
            {
                obj.transform.SetParent(parent, true);
            }

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

            // Eğer takım layer'ı geçerliyse objenin tüm altlarını ve üstlerini ayarla
            if (teamLayer != -1)
            {
                SetLayerRecursively(obj, teamLayer);
                SetLayerOnParents(obj, teamLayer);
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
                // Önce her ihtimale karşı ajan kapalı olsun
                navAgent.enabled = false;

                navAgent.stoppingDistance = unit.data != null ? unit.data.stoppingDistance : stoppingDistance;

                // Karakterin altındaki en yakın NavMesh noktasını bul
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(obj.transform.position, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    // Objenin pozisyonunu tam o güvenli noktaya oturt
                    obj.transform.position = hit.position;
                    navAgent.Warp(hit.position);
                }

                // Ayakları sağlam yere bastıktan sonra Agent'ı uyandır!
                navAgent.enabled = true;
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
    }

    // Yeni yardımcı: GameObject ve tüm çocuklarının layer'ını ayarlar
    private void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    // Yeni yardımcı: GameObject'in tüm üst (parent) zincirini ayarlar
    private void SetLayerOnParents(GameObject obj, int layer)
    {
        Transform t = obj.transform.parent;
        while (t != null)
        {
            t.gameObject.layer = layer;
            t = t.parent;
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
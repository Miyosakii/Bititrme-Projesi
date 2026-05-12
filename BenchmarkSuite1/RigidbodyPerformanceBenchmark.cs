using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.Collections.Generic;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class RigidbodyPerformanceBenchmark
{
    private GameObject[] charactersWithoutRigidbody;
    private GameObject[] charactersWithRigidbody;
    private SpawnManager spawnManager;
    private int characterCount = 50;
    [GlobalSetup]
    public void Setup()
    {
        // Test ortamı hazırlama
        var gameObject = new GameObject("BenchmarkSpawner");
        spawnManager = gameObject.AddComponent<SpawnManager>();
        spawnManager.spawnCount = characterCount;
        spawnManager.rowSize = 5;
        spawnManager.spacing = 2f;
        spawnManager.moveSpeed = 5f;
        spawnManager.stoppingDistance = 0.5f;
        spawnManager.movementDelay = 0f;
        // Prefab oluştur (Rigidbody olmadan)
        var prefabWithoutRb = new GameObject("CharacterWithoutRB");
        prefabWithoutRb.AddComponent<BoxCollider>();
        prefabWithoutRb.AddComponent<CharacterAttribute>();
        prefabWithoutRb.AddComponent<AnimationController>();
        spawnManager.prefab = prefabWithoutRb;
        var parentObj = new GameObject("ParentWithoutRB");
        spawnManager.parent = parentObj.transform;
        // Rigidbody olmayan karakterler
        spawnManager.Spawn();
        charactersWithoutRigidbody = new GameObject[characterCount];
        for (int i = 0; i < spawnManager.GetComponentInChildren<Transform>().childCount; i++)
        {
            charactersWithoutRigidbody[i] = spawnManager.parent.GetChild(i).gameObject;
        }

        // Rigidbody olan karakterler için prefab oluştur
        var prefabWithRb = new GameObject("CharacterWithRB");
        var rbComponent = prefabWithRb.AddComponent<Rigidbody>();
        rbComponent.isKinematic = true; // Movement manuel yapılacağı için kinematic
        prefabWithRb.AddComponent<BoxCollider>();
        prefabWithRb.AddComponent<CharacterAttribute>();
        prefabWithRb.AddComponent<AnimationController>();
        spawnManager.prefab = prefabWithRb;
        var parentRbObj = new GameObject("ParentWithRB");
        spawnManager.parent = parentRbObj.transform;
        spawnManager.Spawn();
        charactersWithRigidbody = new GameObject[characterCount];
        for (int i = 0; i < spawnManager.parent.childCount; i++)
        {
            charactersWithRigidbody[i] = spawnManager.parent.GetChild(i).gameObject;
        }
    }

    [Benchmark]
    public void UpdateMovementWithoutRigidbody()
    {
        // Hareket güncellemesini simulate et
        for (int i = 0; i < charactersWithoutRigidbody.Length; i++)
        {
            var character = charactersWithoutRigidbody[i];
            if (character == null || !character.activeInHierarchy)
                continue;
            // Transform direktly güncelle
            character.transform.Translate(Vector3.forward * 0.01f);
        }
    }

    [Benchmark]
    public void UpdateMovementWithRigidbodyKinematic()
    {
        // Rigidbody ile hareket güncellemesi
        for (int i = 0; i < charactersWithRigidbody.Length; i++)
        {
            var character = charactersWithRigidbody[i];
            if (character == null || !character.activeInHierarchy)
                continue;
            var rb = character.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Kinematic rigidbody pozisyon güncelleme
                rb.MovePosition(character.transform.position + Vector3.forward * 0.01f);
            }
        }
    }

    [Benchmark]
    public void PhysicsSimulationWithManyRigidbodies()
    {
        // Physics engine güncellemesi (collision detection vs.)
        Physics.Simulate(Time.deltaTime);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (var character in charactersWithoutRigidbody)
        {
            if (character != null)
                Object.Destroy(character);
        }

        foreach (var character in charactersWithRigidbody)
        {
            if (character != null)
                Object.Destroy(character);
        }

        Object.Destroy(spawnManager.gameObject);
    }
}
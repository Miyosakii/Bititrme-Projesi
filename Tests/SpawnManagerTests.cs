using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SpawnManagerBasicTests
{
    private SpawnManager spawnManager;
    private GameObject testParent;
    private GameObject prefab;

    [SetUp]
    public void Setup()
    {
        // Test setup
        testParent = new GameObject("TestParent");
        prefab = new GameObject("TestPrefab");

        GameObject managerGO = new GameObject("SpawnManager");
        spawnManager = managerGO.AddComponent<SpawnManager>();

        spawnManager.parent = testParent.transform;
        spawnManager.prefab = prefab;
        spawnManager.spawnCount = 10;
        spawnManager.rowSize = 5;
        spawnManager.spacing = 2f;
        spawnManager.positionJitterRange = 0.3f;
    }

    [TearDown]
    public void Cleanup()
    {
        Object.Destroy(testParent);
        Object.Destroy(prefab);
        Object.Destroy(spawnManager.gameObject);
        SpawnManager.allUnits.Clear();
    }

    // ===== POZISYON TESTLERI =====

    [Test]
    [Category("Position")]
    public void Position_Index0_ReturnsBaseWithJitter()
    {
        // Arrange
        spawnManager.transform.position = Vector3.zero;

        // Act
        Vector3 result = spawnManager.CalculatePosition(0);

        // Assert
        Assert.AreEqual(0, result.y, 0.01f);
    }

    [Test]
    [Category("Position")]
    public void Position_Index5_ReturnsSecondRow()
    {
        // Arrange
        spawnManager.transform.position = Vector3.zero;

        // Act
        Vector3 result = spawnManager.CalculatePosition(5);

        // Assert - row = 5 / 5 = 1, z = 1 * 2 = 2
        Assert.IsTrue(result.z > 1.5f && result.z < 2.5f);
    }

    // ===== HAVUZ TESTLERI =====

    [Test]
    [Category("Pool")]
    public void Pool_EmptyPool_CreatesNewObject()
    {
        // Act
        GameObject obj = spawnManager.GetFromPool();

        // Assert
        Assert.IsNotNull(obj);
    }

    [Test]
    [Category("Pool")]
    public void Pool_InactiveExists_ReturnsInactive()
    {
        // Arrange
        GameObject obj1 = spawnManager.GetFromPool();
        GameObject obj2 = spawnManager.GetFromPool();
        obj1.SetActive(false);

        // Act
        GameObject result = spawnManager.GetFromPool();

        // Assert
        Assert.AreEqual(obj1, result);
    }

    // ===== SPAWN TESTLERI =====

    [Test]
    [Category("Spawn")]
    public void Spawn_Valid_CreatesCorrectCount()
    {
        // Arrange
        spawnManager.spawnCount = 3;

        // Act
        spawnManager.Spawn();

        // Assert
        int count = 0;
        foreach (Transform child in testParent.transform)
        {
            if (child.gameObject.activeInHierarchy)
                count++;
        }
        Assert.AreEqual(3, count);
    }

    [Test]
    [Category("Spawn")]
    public void Spawn_PrefabNull_HandlesGracefully()
    {
        // Arrange
        spawnManager.prefab = null;

        // Act & Assert
        Assert.DoesNotThrow(() => spawnManager.Spawn());
    }

    // ===== DÜÞMAN BULMA TESTLERI =====

    [Test]
    [Category("Enemy")]
    public void FindNearestEnemy_NoEnemies_ReturnsNull()
    {
        // Arrange
        GameObject unitGO = new GameObject("Unit");
        Unit unit = unitGO.AddComponent<Unit>();
        unit.owner = spawnManager;

        // Act
        Unit result = spawnManager.FindNearestEnemy(unit);

        // Assert
        Assert.IsNull(result);

        Object.Destroy(unitGO);
    }

    [Test]
    [Category("Enemy")]
    public void FindNearestEnemy_OneEnemy_ReturnsIt()
    {
        // Arrange
        GameObject unit1GO = new GameObject("Unit1");
        unit1GO.transform.position = Vector3.zero;
        Unit unit1 = unit1GO.AddComponent<Unit>();
        unit1.owner = spawnManager;

        GameObject unit2GO = new GameObject("Unit2");
        unit2GO.transform.position = new Vector3(5, 0, 0);
        Unit unit2 = unit2GO.AddComponent<Unit>();
        unit2.owner = new SpawnManager();

        SpawnManager.allUnits.Add(unit1);
        SpawnManager.allUnits.Add(unit2);

        // Act
        Unit result = spawnManager.FindNearestEnemy(unit1);

        // Assert
        Assert.AreEqual(unit2, result);

        Object.Destroy(unit1GO);
        Object.Destroy(unit2GO);
        SpawnManager.allUnits.Clear();
    }

    [Test]
    [Category("Enemy")]
    public void FindNearestEnemy_Multiple_ReturnsNearest()
    {
        // Arrange
        GameObject unit1GO = new GameObject("Unit1");
        unit1GO.transform.position = Vector3.zero;
        Unit unit1 = unit1GO.AddComponent<Unit>();
        unit1.owner = spawnManager;

        SpawnManager owner2 = new SpawnManager();

        GameObject unit2GO = new GameObject("Unit2");
        unit2GO.transform.position = new Vector3(10, 0, 0);
        Unit unit2 = unit2GO.AddComponent<Unit>();
        unit2.owner = owner2;

        GameObject unit3GO = new GameObject("Unit3");
        unit3GO.transform.position = new Vector3(2, 0, 0);
        Unit unit3 = unit3GO.AddComponent<Unit>();
        unit3.owner = owner2;

        SpawnManager.allUnits.Add(unit1);
        SpawnManager.allUnits.Add(unit2);
        SpawnManager.allUnits.Add(unit3);

        // Act
        Unit result = spawnManager.FindNearestEnemy(unit1);

        // Assert
        Assert.AreEqual(unit3, result); // 2 birim uzak olan

        Object.Destroy(unit1GO);
        Object.Destroy(unit2GO);
        Object.Destroy(unit3GO);
        SpawnManager.allUnits.Clear();
    }

    [Test]
    [Category("Enemy")]
    public void FindNearestEnemy_SameOwner_IgnoresThem()
    {
        // Arrange
        GameObject unit1GO = new GameObject("Unit1");
        unit1GO.transform.position = Vector3.zero;
        Unit unit1 = unit1GO.AddComponent<Unit>();
        unit1.owner = spawnManager;

        GameObject unit2GO = new GameObject("Unit2");
        unit2GO.transform.position = new Vector3(1, 0, 0);
        Unit unit2 = unit2GO.AddComponent<Unit>();
        unit2.owner = spawnManager; // Ayný owner

        SpawnManager.allUnits.Add(unit1);
        SpawnManager.allUnits.Add(unit2);

        // Act
        Unit result = spawnManager.FindNearestEnemy(unit1);

        // Assert
        Assert.IsNull(result);

        Object.Destroy(unit1GO);
        Object.Destroy(unit2GO);
        SpawnManager.allUnits.Clear();
    }

    [Test]
    [Category("Enemy")]
    public void FindNearestEnemy_InactiveUnit_IgnoresThem()
    {
        // Arrange
        GameObject unit1GO = new GameObject("Unit1");
        unit1GO.transform.position = Vector3.zero;
        Unit unit1 = unit1GO.AddComponent<Unit>();
        unit1.owner = spawnManager;

        GameObject unit2GO = new GameObject("Unit2");
        unit2GO.transform.position = new Vector3(1, 0, 0);
        Unit unit2 = unit2GO.AddComponent<Unit>();
        unit2.owner = new SpawnManager();
        unit2GO.SetActive(false); // Ýnaktif

        SpawnManager.allUnits.Add(unit1);
        SpawnManager.allUnits.Add(unit2);

        // Act
        Unit result = spawnManager.FindNearestEnemy(unit1);

        // Assert
        Assert.IsNull(result);

        Object.Destroy(unit1GO);
        Object.Destroy(unit2GO);
        SpawnManager.allUnits.Clear();
    }

    // ===== CLEAR TESTLERI =====

    [Test]
    [Category("Clear")]
    public void Clear_Active_DeactivatesAll()
    {
        // Arrange
        spawnManager.spawnCount = 5;
        spawnManager.Spawn();

        // Act
        spawnManager.Clear();

        // Assert
        int count = 0;
        foreach (Transform child in testParent.transform)
        {
            if (child.gameObject.activeInHierarchy)
                count++;
        }
        Assert.AreEqual(0, count);
    }
}
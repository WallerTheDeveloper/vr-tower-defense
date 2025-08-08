using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Factories;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [FormerlySerializedAs("maxEnemies")] [SerializeField] private int maxUnits = 10;
    [SerializeField] private bool startSpawningOnStart = true;
    [SerializeField] private UnitFactory unitFactory;

    [Header("Spawn Area (if no spawn points)")] [SerializeField]
    private float spawnRadius = 5f;

    [SerializeField] private LayerMask groundLayer = 1;

    [Header("Wave Settings")] [SerializeField]
    private bool useWaves = false;

    [FormerlySerializedAs("enemiesPerWave")] [SerializeField] private int unitsPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 10f;

    private List<Unit> _spawnedUnits = new();
    private Coroutine _spawnCoroutine;
    private int _currentWave = 1;
    private int _unitsSpawnedThisWave = 0;

    private void Start()
    {
        if (startSpawningOnStart)
        {
            StartSpawning();
        }
    }

    private void StartSpawning()
    {
        if (_spawnCoroutine == null)
        {
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Clean up destroyed enemies from our list
            CleanupDestroyedUnits();

            // Check if we can spawn more enemies
            if (CanSpawnUnit())
            {
                SpawnUnit();

                if (useWaves)
                {
                    _unitsSpawnedThisWave++;

                    // Check if wave is complete
                    if (_unitsSpawnedThisWave >= unitsPerWave)
                    {
                        yield return StartCoroutine(WaitForWaveComplete());
                        yield return new WaitForSeconds(timeBetweenWaves);

                        _currentWave++;
                        _unitsSpawnedThisWave = 0;
                        Debug.Log($"Starting Wave {_currentWave}");
                    }
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private bool CanSpawnUnit()
    {
        return _spawnedUnits.Count < maxUnits;
    }

    private void SpawnUnit()
    {
        var unit = unitFactory.CreateTower(transform.position, Quaternion.identity);
        _spawnedUnits.Add(unit);
    }

    private void CleanupDestroyedUnits()
    {
        _spawnedUnits.RemoveAll(enemy => enemy == null);
    }

    private IEnumerator WaitForWaveComplete()
    {
        Debug.Log($"Wave {_currentWave} spawning complete. Waiting for enemies to be defeated...");

        while (_spawnedUnits.Count > 0)
        {
            CleanupDestroyedUnits();
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"Wave {_currentWave} complete!");
    }

    public void DestroyAllEnemies()
    {
        foreach (var enemy in _spawnedUnits)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        _spawnedUnits.Clear();
    }
}
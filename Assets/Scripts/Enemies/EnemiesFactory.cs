using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class EnemiesFactory : MonoBehaviour
{
    public UnityAction EnemyKilledAction;
    public UnityAction EnemyReachedFinishLineAction;

    [SerializeField] private float minEnemiesSpawnTimeout;
    [SerializeField] private float maxEnemiesSpawnTimeout;
    [SerializeField] private float minEnemiesSpeed;
    [SerializeField] private float maxEnemiesSpeed;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private GameObject explosion;

    private List<Enemy> activeEnemies = new();
    private ObjectPool<Enemy> enemiesPool;
    private Coroutine enemiesSpawnCoroutine;

    private void Awake()
    {
        enemiesPool = new ObjectPool<Enemy>(CreateEnemy, OnGetEnemyFromPool, OnReturnEnemyToPool);
    }

    private Enemy CreateEnemy()
    {
        var enemy = Instantiate(enemyPrefab);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    private void OnGetEnemyFromPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
        enemy.EnemyKilledAction += EnemyKilledHandler;
        enemy.EnemyReachedFinishLineAction += EnemyReachedFinishLineActionHandler;
        float randomEnemySpeed = Random.Range(minEnemiesSpeed, maxEnemiesSpeed);
        enemy.Init(randomEnemySpeed);
        activeEnemies.Add(enemy);
    }

    private void OnReturnEnemyToPool(Enemy enemy)
    {
        AudioManager.Instance.Play("Explosion");
        explosion.SetActive(false);
        explosion.transform.position = enemy.transform.position;
        explosion.SetActive(true);
        activeEnemies.Remove(enemy);
        enemy.gameObject.SetActive(false);
        enemy.EnemyKilledAction -= EnemyKilledHandler;
        enemy.EnemyReachedFinishLineAction -= EnemyReachedFinishLineActionHandler;
    }

    private void EnemyKilledHandler(Enemy enemy)
    {
        enemiesPool.Release(enemy);
        EnemyKilledAction?.Invoke();
    }

    private void EnemyReachedFinishLineActionHandler(Enemy enemy)
    {
        enemiesPool.Release(enemy);
        EnemyReachedFinishLineAction?.Invoke();
    }

    private IEnumerator EnemiesSpawnCoroutine()
    {
        while (true)
        {
            Enemy e = enemiesPool.Get();
            e.transform.position = GetRandomSpawnPointPosition();
            float randomSpawnTimeout = Random.Range(minEnemiesSpawnTimeout, maxEnemiesSpawnTimeout);
            yield return new WaitForSeconds(randomSpawnTimeout);
        }
    }

    public void StartEnemiesSpawn() => enemiesSpawnCoroutine = StartCoroutine(EnemiesSpawnCoroutine());
    
    public void StopEnemiesSpawn()
    {
        if(enemiesSpawnCoroutine != null)
        {
            StopCoroutine(enemiesSpawnCoroutine);
        }
    }

    public Vector3 GetRandomSpawnPointPosition()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex].position;
    }

    public Enemy GetNearestEnemy(Vector2 playerPosition)
    {
        if(activeEnemies.Count <= 0)
        {
            return null;
        }
        Enemy nearestEnemy = null;
        float shortestDistance = float.MaxValue;
        foreach (Enemy enemy in activeEnemies)
        {
            float distanceToEnemy = Vector2.Distance(playerPosition, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        return nearestEnemy;
    }

}

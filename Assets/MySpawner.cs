using MoreMountains.InfiniteRunnerEngine;
using System.Collections;
using UnityEngine;

public class MySpawner : MonoBehaviour
{
    [Header("Lane Positions")]
    public Transform[] Lanes;

    [Header("Objects to Spawn")]
    public GameObject CoinPrefab;
    public GameObject GemPrefab;
    public GameObject ObstaclePrefab;

    [Header("Coin Settings")]
    public int CoinWaveMin = 5;
    public int CoinWaveMax = 10;
    public float CoinWaveDelay = 1f;
    public Vector2 CoinInterval = new Vector2(1f, 2f); // min/max seconds
    public float CoinSpawnInterval = 0.3f;
    public Vector2 CoinYOffset = new Vector2(-0.5f, 0.5f);
    public float CoinIntervalJitter = 0.1f; // randomize slightly

    [Header("Gem Settings")]
    public float GemChance = 0.1f;
    public Vector2 GemInterval = new Vector2(1f, 2f); // min/max seconds
    public Vector2 GemYOffset = new Vector2(0f, 0.5f);

    [Header("Obstacle Settings")]
    public Vector2 ObstacleInterval = new Vector2(1f, 2f); // min/max seconds
    public Vector2 ObstacleYOffset = new Vector2(-0.2f, 0.2f);


    public void StartGame()
    {
        foreach (Transform lane in Lanes)
        {
            StartCoroutine(SpawnLaneObstacles(lane));
            StartCoroutine(SpawnLaneCoinsAndGems(lane));
        }
    }
    public void StopGame()
    {
        StopAllCoroutines();
    }

    IEnumerator SpawnLaneObstacles(Transform lane)
    {
        while (true)
        {
            if (ObstaclePrefab != null)
            {
                Vector3 pos = lane.position;
                pos.y += Random.Range(ObstacleYOffset.x, ObstacleYOffset.y);
                Spawn(ObstaclePrefab, pos);
            }

            // Random interval between spawns
            float interval = Random.Range(ObstacleInterval.x, ObstacleInterval.y);
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator SpawnLaneCoinsAndGems(Transform lane)
    {
        while (true)
        {
            if (CoinPrefab == null) { yield return null; continue; }

            int coinsThisWave = Random.Range(CoinWaveMin, CoinWaveMax + 1);
            for (int i = 0; i < coinsThisWave; i++)
            {
                Vector3 pos = lane.position;
                pos.y += Random.Range(CoinYOffset.x, CoinYOffset.y);
                Spawn(CoinPrefab, pos);

                // Add slight random jitter so coins are not too uniform
                float interval = CoinSpawnInterval + Random.Range(-CoinIntervalJitter, CoinIntervalJitter);
                interval = Mathf.Max(0.05f, interval); // clamp min interval
                yield return new WaitForSeconds(interval);
            }

            // Spawn a gem occasionally
            if (Random.value < GemChance)
            {
                Vector3 pos = lane.position;
                pos.y += Random.Range(GemYOffset.x, GemYOffset.y);
                Spawn(GemPrefab, pos);
            }

            // Wait a bit before next wave
            yield return new WaitForSeconds(Random.RandomRange(CoinInterval.x, CoinInterval.y));
        }
    }

    void Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}

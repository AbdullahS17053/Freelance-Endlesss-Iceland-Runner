using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObstacleSpawner : MonoBehaviour
{
    [Header("Global")]
    public bool EnableSpawning = true;
    public float SpawnX = 40f;
    public float[] LaneZ = { -3f, 0f, 3f };

    [Header("Ground Reference")]
    public Transform Ground;

    [Header("Spawn Heights")]
    public float ObstacleYOffset = 0f;
    public float CoinYOffset = 1f;
    public float GemYOffset = 1.2f;

    [Header("Difficulty")]
    public float DifficultyIncreaseRate = 0.02f;
    public float MaxDifficulty = 3f;

    [Header("Obstacles")]
    public GameObject[] ObstaclePrefabs;
    public Vector2 ObstacleDelay = new Vector2(1.8f, 3.2f);

    [Header("Coins")]
    public GameObject CoinPrefab;
    public int CoinLineMin = 3;
    public int CoinLineMax = 6;
    public float CoinGapX = 1.5f;

    [Header("Gems")]
    public GameObject GemPrefab;
    [Range(0f, 1f)] public float GemChance = 0.12f;
    public Vector2 GemCooldown = new Vector2(10f, 18f);

    [Header("Patterns")]
    public bool EnablePatterns = true;

    // ==========================
    // INTERNAL
    // ==========================
    private class LaneState
    {
        public bool Busy;
        public int RecentSpawns;
        public float Timer;
        public float NextDelay;
    }

    private LaneState[] _lanes;
    private float _difficulty = 1f;
    private bool _canSpawnGem = true;

    private Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();
    private Vector3 _spawnPosition = Vector3.zero;

    private void Start()
    {
        EnableSpawning = false;

        // Initialize lanes
        _lanes = new LaneState[LaneZ.Length];
        for (int i = 0; i < LaneZ.Length; i++)
        {
            _lanes[i] = new LaneState
            {
                Timer = 0f,
                NextDelay = Random.Range(ObstacleDelay.x, ObstacleDelay.y)
            };
        }
    }

    private void Update()
    {
        if (EnableSpawning)
        {
            // Difficulty increase
            _difficulty = Mathf.Min(_difficulty + DifficultyIncreaseRate * Time.deltaTime, MaxDifficulty);

            // Update each lane
            for (int i = 0; i < _lanes.Length; i++)
            {
                LaneState lane = _lanes[i];
                if (lane.Busy) continue;

                lane.Timer += Time.deltaTime;
                if (lane.Timer >= lane.NextDelay / _difficulty)
                {
                    TrySpawn(i);
                    lane.Timer = 0f;
                    lane.NextDelay = Random.Range(ObstacleDelay.x, ObstacleDelay.y);
                }
            }
        }
    }

    private void TrySpawn(int lane)
    {
        if (WouldBlockAllLanes(lane)) return;

        float fairness = Mathf.Clamp01(1f - _lanes[lane].RecentSpawns * 0.25f);

        // Spawn Gem
        if (_canSpawnGem && Random.value < (GemChance * fairness) / _difficulty)
        {
            SpawnGem(lane);
            return;
        }

        // Spawn Pattern
        float patternChance = Mathf.Lerp(0.08f, 0.18f, _difficulty / MaxDifficulty);
        if (EnablePatterns && Random.value < patternChance * fairness)
        {
            SpawnPattern();
            return;
        }

        // Obstacle vs Coin
        if (Random.value < 0.6f)
            SpawnObstacle(lane);
        else
            SpawnCoinLine(lane);
    }

    private bool WouldBlockAllLanes(int requestingLane)
    {
        int busyCount = 0;
        foreach (var lane in _lanes)
            if (lane.Busy) busyCount++;
        return busyCount >= _lanes.Length - 1;
    }

    // ==========================
    // SPAWN METHODS
    // ==========================
    private void SpawnObstacle(int lane)
    {
        LockLane(lane);
        GameObject prefab = ObstaclePrefabs[Random.Range(0, ObstaclePrefabs.Length)];
        Spawn(prefab, lane, SpawnX, ObstacleYOffset);
        StartCoroutine(UnlockAfterDelay(lane, 1.1f / _difficulty));
    }

    private void SpawnCoinLine(int lane)
    {
        LockLane(lane);
        int count = Random.Range(CoinLineMin, CoinLineMax + 1);
        float x = SpawnX;

        for (int i = 0; i < count; i++)
        {
            Spawn(CoinPrefab, lane, x, CoinYOffset);
            x += CoinGapX;
        }

        StartCoroutine(UnlockAfterDelay(lane, 0.9f));
    }

    private void SpawnGem(int lane)
    {
        _canSpawnGem = false;
        LockLane(lane);
        Spawn(GemPrefab, lane, SpawnX, GemYOffset);
        StartCoroutine(GemCooldownRoutine(lane));
    }

    private void SpawnPattern()
    {
        int safeLane = GetFairestLane();
        for (int i = 0; i < _lanes.Length; i++)
        {
            if (i == safeLane) continue;
            LockLane(i);
            GameObject prefab = ObstaclePrefabs[Random.Range(0, ObstaclePrefabs.Length)];
            Spawn(prefab, i, SpawnX, ObstacleYOffset);
        }
        StartCoroutine(PatternUnlockRoutine());
    }

    // ==========================
    // COROUTINES
    // ==========================
    private IEnumerator UnlockAfterDelay(int lane, float delay)
    {
        yield return new WaitForSeconds(delay);
        UnlockLane(lane);
    }

    private IEnumerator GemCooldownRoutine(int lane)
    {
        float cooldown = Random.Range(GemCooldown.x, GemCooldown.y);
        yield return new WaitForSeconds(cooldown);
        UnlockLane(lane);
        _canSpawnGem = true;
    }

    private IEnumerator PatternUnlockRoutine()
    {
        yield return new WaitForSeconds(1.4f);
        for (int i = 0; i < _lanes.Length; i++)
            UnlockLane(i);
    }

    // ==========================
    // LANE MANAGEMENT
    // ==========================
    private int GetFairestLane()
    {
        int best = 0;
        int lowest = int.MaxValue;
        for (int i = 0; i < _lanes.Length; i++)
        {
            if (_lanes[i].RecentSpawns < lowest)
            {
                lowest = _lanes[i].RecentSpawns;
                best = i;
            }
        }
        return best;
    }

    private void LockLane(int lane)
    {
        _lanes[lane].Busy = true;
        _lanes[lane].RecentSpawns++;
    }

    private void UnlockLane(int lane)
    {
        _lanes[lane].Busy = false;
        _lanes[lane].RecentSpawns = Mathf.Max(0, _lanes[lane].RecentSpawns - 1);
    }

    // ==========================
    // SPAWN HELPER WITH POOLING
    // ==========================
    private void Spawn(GameObject prefab, int lane, float x, float yOffset)
    {
        GameObject obj = GetPooledObject(prefab);
        if (obj == null)
        {
            obj = Instantiate(prefab);
            AddToPool(prefab, obj);
        }

        // Compute spawn position
        _spawnPosition.Set(x, yOffset, LaneZ[lane]);

        // Optional raycast for uneven ground
        if (Ground != null)
        {
            Vector3 rayOrigin = new Vector3(x, 50f, LaneZ[lane]);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f))
            {
                _spawnPosition.y = hit.point.y + yOffset;
            }
        }

        obj.transform.position = _spawnPosition;
        obj.transform.rotation = prefab.transform.rotation;
        obj.SetActive(true);
    }

    private GameObject GetPooledObject(GameObject prefab)
    {
        if (_pool.TryGetValue(prefab, out Queue<GameObject> queue) && queue.Count > 0)
            return queue.Dequeue();
        return null;
    }

    private void AddToPool(GameObject prefab, GameObject obj)
    {
        if (!_pool.ContainsKey(prefab))
            _pool[prefab] = new Queue<GameObject>();
    }

    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        if (!_pool.ContainsKey(prefab)) _pool[prefab] = new Queue<GameObject>();
        _pool[prefab].Enqueue(obj);
    }
}

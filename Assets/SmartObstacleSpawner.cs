using UnityEngine;
using System.Collections;

public class SmartObstacleSpawner : MonoBehaviour
{
    // ======================================================
    // GLOBAL
    // ======================================================
    [Header("Global")]
    public bool EnableSpawning = true;
    public float SpawnX = 40f;
    public float[] LaneZ = { -3f, 0f, 3f };

    [Header("Ground Reference")]
    public Transform Ground; // reference for correct Y position

    // ======================================================
    // HEIGHT OFFSETS
    // ======================================================
    [Header("Spawn Heights")]
    public float ObstacleYOffset = 0f;
    public float CoinYOffset = 1f;
    public float GemYOffset = 1.2f;

    // ======================================================
    // DIFFICULTY
    // ======================================================
    [Header("Difficulty")]
    public float DifficultyIncreaseRate = 0.02f;
    public float MaxDifficulty = 3f;

    // ======================================================
    // OBSTACLES
    // ======================================================
    [Header("Obstacles")]
    public GameObject[] ObstaclePrefabs;
    public Vector2 ObstacleDelay = new Vector2(1.8f, 3.2f);

    // ======================================================
    // COINS
    // ======================================================
    [Header("Coins")]
    public GameObject CoinPrefab;
    public int CoinLineMin = 3;
    public int CoinLineMax = 6;
    public float CoinGapX = 1.5f;

    // ======================================================
    // GEMS
    // ======================================================
    [Header("Gems")]
    public GameObject GemPrefab;
    [Range(0f, 1f)] public float GemChance = 0.12f;
    public Vector2 GemCooldown = new Vector2(10f, 18f);

    // ======================================================
    // PATTERNS
    // ======================================================
    [Header("Patterns")]
    public bool EnablePatterns = true;

    // ======================================================
    // INTERNAL STATE
    // ======================================================
    private class LaneState
    {
        public bool Busy;
        public int RecentSpawns; // fairness tracking
    }

    private LaneState[] _lanes;
    private float _difficulty = 1f;
    private bool _canSpawnGem = true;

    // ======================================================
    // UNITY START
    // ======================================================
    private void Start()
    {
        EnableSpawning = false;

        _lanes = new LaneState[LaneZ.Length];
        for (int i = 0; i < LaneZ.Length; i++)
        {
            _lanes[i] = new LaneState();
            StartCoroutine(LaneLoop(i));
        }
        StartCoroutine(DifficultyLoop());
    }

    // ======================================================
    // DIFFICULTY LOOP
    // ======================================================
    private IEnumerator DifficultyLoop()
    {
        while (true)
        {
            _difficulty = Mathf.Min(_difficulty + DifficultyIncreaseRate, MaxDifficulty);
            yield return new WaitForSeconds(1f);
        }
    }

    // ======================================================
    // LANE LOOP
    // ======================================================
    private IEnumerator LaneLoop(int lane)
    {
        while (true)
        {
            if (EnableSpawning && !_lanes[lane].Busy)
                TrySpawn(lane);

            float wait = Random.Range(ObstacleDelay.x, ObstacleDelay.y) / _difficulty;
            yield return new WaitForSeconds(wait);
        }
    }

    // ======================================================
    // SPAWN DECISION
    // ======================================================
    private void TrySpawn(int lane)
    {
        if (WouldBlockAllLanes(lane))
            return;

        // Lane fairness factor
        float fairness = Mathf.Clamp01(1f - _lanes[lane].RecentSpawns * 0.25f);

        // Gem
        if (_canSpawnGem && Random.value < (GemChance * fairness) / _difficulty)
        {
            StartCoroutine(SpawnGem(lane));
            return;
        }

        // Pattern
        float patternChance = Mathf.Lerp(0.08f, 0.18f, _difficulty / MaxDifficulty);
        if (EnablePatterns && Random.value < patternChance * fairness)
        {
            StartCoroutine(SpawnPattern());
            return;
        }

        // Obstacle vs Coin
        if (Random.value < 0.6f)
            StartCoroutine(SpawnObstacle(lane));
        else
            StartCoroutine(SpawnCoinLine(lane));
    }

    private bool WouldBlockAllLanes(int requestingLane)
    {
        int busyCount = 0;
        foreach (var lane in _lanes)
            if (lane.Busy) busyCount++;
        return busyCount >= _lanes.Length - 1;
    }

    // ======================================================
    // SPAWN OBSTACLE
    // ======================================================
    private IEnumerator SpawnObstacle(int lane)
    {
        LockLane(lane);

        GameObject prefab = ObstaclePrefabs[Random.Range(0, ObstaclePrefabs.Length)];
        Spawn(prefab, lane, SpawnX, ObstacleYOffset);

        yield return new WaitForSeconds(1.1f / _difficulty);
        UnlockLane(lane);
    }

    // ======================================================
    // SPAWN COIN LINE
    // ======================================================
    private IEnumerator SpawnCoinLine(int lane)
    {
        LockLane(lane);

        int count = Random.Range(CoinLineMin, CoinLineMax + 1);
        float x = SpawnX;

        for (int i = 0; i < count; i++)
        {
            Spawn(CoinPrefab, lane, x, CoinYOffset);
            x += CoinGapX;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.9f);
        UnlockLane(lane);
    }

    // ======================================================
    // SPAWN GEM
    // ======================================================
    private IEnumerator SpawnGem(int lane)
    {
        _canSpawnGem = false;
        LockLane(lane);

        Spawn(GemPrefab, lane, SpawnX, GemYOffset);

        yield return new WaitForSeconds(Random.Range(GemCooldown.x, GemCooldown.y));
        UnlockLane(lane);
        _canSpawnGem = true;
    }

    // ======================================================
    // SPAWN PATTERN (SAFE LANE)
    // ======================================================
    private IEnumerator SpawnPattern()
    {
        int safeLane = GetFairestLane();

        for (int i = 0; i < LaneZ.Length; i++)
        {
            if (i == safeLane) continue;
            LockLane(i);

            GameObject prefab = ObstaclePrefabs[Random.Range(0, ObstaclePrefabs.Length)];
            Spawn(prefab, i, SpawnX, ObstacleYOffset);
        }

        yield return new WaitForSeconds(1.4f);

        for (int i = 0; i < LaneZ.Length; i++)
            UnlockLane(i);
    }

    // ======================================================
    // LANE FAIRNESS
    // ======================================================
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

    // ======================================================
    // SPAWN HELPER (ANCHOR TO GROUND)
    // ======================================================
    private void Spawn(GameObject prefab, int lane, float x, float yOffset)
    {
        // Raycast downward from above the track
        Vector3 rayOrigin = new Vector3(x, 50f, LaneZ[lane]); // start high above
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f))
        {
            Vector3 pos = hit.point + new Vector3(0, yOffset, 0);
            Instantiate(prefab, pos, prefab.transform.rotation);
        }
        else
        {
            // fallback if raycast misses
            Vector3 pos = new Vector3(x, yOffset, LaneZ[lane]);
            Instantiate(prefab, pos, prefab.transform.rotation);
            Debug.LogWarning("Spawn raycast missed ground at lane " + lane);
        }
    }

}

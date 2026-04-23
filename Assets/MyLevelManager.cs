using System.Collections;
using UnityEngine;

public class MyLevelManager : MonoBehaviour
{
    public static MyLevelManager Instance;

    [Header("Player Settings")]
    public Transform spawnPoint;
    public GameObject playerPrefab;
    public GameObject _playerInstance;

    [Header("Environment Tiles")]
    public GameObject[] EnvironmentTiles;
    public int currentEnvironment = 2;
    public float lockedZ = 0f;
    public float baseTileSpeed = 5f;
    public float tileSpeedIncreasePerSecond = 0.05f;
    public float tileWidth = 135f;

    [Header("Obstacle Spawner")]
    public SmartObstacleSpawner spawner;

    private float _currentTileSpeed;
    private bool _gameStarted;
    public float CurrentTileSpeed => _obstacleSpeeds;
    private float _obstacleSpeeds;

    // Cached
    private Transform[] _tileTransforms;
    private int _rightMostTileIndex;
    private float _elapsedGameTime;

    public void ChangeLevel(GameObject _1, GameObject _2, GameObject _3, int num)
    {
        currentEnvironment = num;
        EnvironmentTiles[0] = _1;
        EnvironmentTiles[1] = _2;
        EnvironmentTiles[2] = _3;
        CacheTiles();
    }

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // WebGL mobile FPS lock
        Application.targetFrameRate = 60;

        CacheTiles();
    }

    private void Update()
    {
        if (!_gameStarted) return;

        MoveTiles();
        UpdateSpeed();
    }

    #endregion

    #region Tile System (Optimized)

    private void CacheTiles()
    {
        _tileTransforms = new Transform[EnvironmentTiles.Length];

        for (int i = 0; i < EnvironmentTiles.Length; i++)
        {
            _tileTransforms[i] = EnvironmentTiles[i].transform;
            _tileTransforms[i].position = new Vector3(i * tileWidth, _tileTransforms[i].position.y, lockedZ);
        }

        _rightMostTileIndex = EnvironmentTiles.Length - 1;
    }

    private void MoveTiles()
    {
        float move = _currentTileSpeed * Time.deltaTime;

        for (int i = 0; i < _tileTransforms.Length; i++)
        {
            Transform t = _tileTransforms[i];
            Vector3 pos = t.position;
            pos.x -= move;
            t.position = pos;

            if (pos.x < -tileWidth)
            {
                RecycleTile(i);
            }
        }
    }

    private void RecycleTile(int tileIndex)
    {
        Transform tile = _tileTransforms[tileIndex];
        Transform rightMost = _tileTransforms[_rightMostTileIndex];

        tile.position = new Vector3(
            rightMost.position.x + tileWidth,
            tile.position.y,
            lockedZ
        );

        _rightMostTileIndex = tileIndex;
    }

    private void UpdateSpeed()
    {
        _elapsedGameTime += Time.deltaTime;
        _currentTileSpeed = baseTileSpeed + (_elapsedGameTime * tileSpeedIncreasePerSecond);

        if (!_gameStarted)
        {
            _obstacleSpeeds = 0;
        }
        else
        {
            _obstacleSpeeds = _currentTileSpeed;
        }

    }

    #endregion

    #region Player Management (No GC)

    private void SpawnOrResetPlayer()
    {
        if (_playerInstance == null)
        {
            _playerInstance = Instantiate(playerPrefab);
        }
        _playerInstance.transform.position = spawnPoint.position;
        _playerInstance.SetActive(true);
    }

    private void DisablePlayer()
    {
        if (_playerInstance != null)
            _playerInstance.SetActive(false);
    }

    #endregion

    #region Game Flow

    public void StartGame()
    {
        _gameStarted = true;
        _obstacleSpeeds = _currentTileSpeed;
        _elapsedGameTime = 0f;
        _currentTileSpeed = baseTileSpeed;

        SpawnOrResetPlayer();
        spawner.StartStopObjects(false);

        if (spawner != null)
            spawner.EnableSpawning = true;
    }

    public void ResetLevelR()
    {
        StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);
        ResetLevel();
    }

    public void ResetLevel()
    {
        _gameStarted = false;
        _obstacleSpeeds = _currentTileSpeed;

        /*for (int i = 0; i < _tileTransforms.Length; i++)
        {
            _tileTransforms[i].position = new Vector3(
                i * tileWidth,
                _tileTransforms[i].position.y,
                lockedZ
            );
        }*/

        _rightMostTileIndex = _tileTransforms.Length - 1;

        if (spawner != null)
            spawner.EnableSpawning = false;

        StartGame();
    }

    public void CrashLevel()
    {
        _obstacleSpeeds = 0;
        _gameStarted = false;

        if (spawner != null)
            spawner.EnableSpawning = false;

        DisablePlayer();
        spawner.StartStopObjects(true);
    }

    public void EndGame()
    {
        _gameStarted = false;
        _currentTileSpeed = baseTileSpeed;

        if (spawner != null)
            spawner.EnableSpawning = false;

        spawner.ClearObstacles();
    }

    #endregion
}

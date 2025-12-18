using System.Collections;
using UnityEngine;

public class MyLevelManager : MonoBehaviour
{
    public static MyLevelManager Instance;

    [Header("Player Settings")]
    public Transform spawnPoint;
    public GameObject playerPrefab;
    private GameObject _playerInstance;

    [Header("Environment Tiles")]
    public GameObject[] EnvironmentTiles;
    public float _lockedZ = 0f;            // Y-axis is height, Z is fixed
    public float _tileMoveSpeed = 5f;
    public float _tileSpeedIncrease = 0.05f;

    [Header("Obstacle Spawner")]
    public SmartObstacleSpawner spawner;

    private float _originalTileSpeed;
    private bool _gameStarted = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _originalTileSpeed = _tileMoveSpeed;
    }

    private void Start()
    {
        /*
        StartGame();
        */
    }

    private void FixedUpdate()
    {

        // Move tiles along X
        foreach (var tile in EnvironmentTiles)
        {
            if (tile != null)
            {
                tile.transform.Translate(Vector3.left * _tileMoveSpeed * Time.fixedDeltaTime);

                if (tile.transform.position.x < -136.4f) // recycle when passed left
                    RecycleTile(tile);
            }
        }

        if (!_gameStarted) return;
        // Gradually increase tile speed
        _tileMoveSpeed += _tileSpeedIncrease * Time.fixedDeltaTime;
    }

    private void RecycleTile(GameObject tile)
    {
        // Find the rightmost tile
        float maxX = float.MinValue;
        foreach (var t in EnvironmentTiles)
            if (t != tile)
                maxX = Mathf.Max(maxX, t.transform.position.x);

        Vector3 pos = tile.transform.position;
        pos.x = maxX + 136.4f; // adjust based on tile width
        pos.z = _lockedZ;
        tile.transform.position = pos;
    }

    #region Player Management

    private void SpawnPlayer()
    {
        if (_playerInstance != null)
            Destroy(_playerInstance);

        _playerInstance = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
    }

    public void StartGame()
    {
        _gameStarted = true;
        _tileMoveSpeed = _originalTileSpeed;

        SpawnPlayer();

        if (spawner != null)
            spawner.EnableSpawning = true;
    }

    public void ResetLevel()
    {
        _gameStarted = false;

        // Reset environment tiles
        for (int i = 0; i < EnvironmentTiles.Length; i++)
        {
            if (EnvironmentTiles[i] != null)
            {
                EnvironmentTiles[i].transform.position = new Vector3(
                    i * 136.4f,                 // X
                    EnvironmentTiles[i].transform.position.y,
                    _lockedZ
                );
            }
        }

        // Reset spawner
        if (spawner != null)
        {
            spawner.EnableSpawning = false;
        }

        SpawnPlayer();
        _gameStarted = true;

        if (spawner != null)
            spawner.EnableSpawning = true;
    }

    public void CrashLevel()
    {
        _gameStarted = false;

        if (spawner != null)
            spawner.EnableSpawning = false;

        if (_playerInstance != null)
            Destroy(_playerInstance);
    }

    public void EndGame()
    {
        _gameStarted = false;
        _tileMoveSpeed = _originalTileSpeed;

        if (spawner != null)
            spawner.EnableSpawning = false;
    }

    #endregion
}

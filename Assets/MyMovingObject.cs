using UnityEngine;

/// <summary>
/// Optimized moving object for WebGL/mobile.
/// Moves according to speed and direction, can auto-destroy offscreen or after a lifetime.
/// </summary>
public class MyMovingObject : MonoBehaviour
{
    [Header("Movement Settings")]
    public float Speed = 5f;                       // Base speed
    public Vector3 Direction = Vector3.left;       // Movement direction
    public Space MovementSpace = Space.World;      // World or local movement

    [Header("Behaviour")]
    public bool AutoDestroy = true;                // Destroy when off-screen
    public float DestroyXThreshold = -50f;         // X position to destroy object
    public float Lifetime = 60f;                   // Optional self-destroy after time (0 = disabled)

    // Cached values
    private Vector3 _normalizedDirection;
    private float _currentLifetime;
    private bool _lifetimeEnabled;

    private void OnEnable()
    {
        _currentLifetime = 0f;

        // Cache normalized direction to avoid recalculation every frame
        _normalizedDirection = Direction.normalized;

        // Update speed from level manager if available
        if (MyLevelManager.Instance != null)
            Speed = MyLevelManager.Instance.CurrentTileSpeed;

        _lifetimeEnabled = Lifetime > 0f;
    }

    private void Update()
    {
        // Move object
        Vector3 movement = _normalizedDirection * Speed * Time.deltaTime;
        transform.Translate(movement, MovementSpace);

        // Check destruction conditions
        if (_lifetimeEnabled)
        {
            _currentLifetime += Time.deltaTime;
            if (_currentLifetime >= Lifetime)
            {
                Destroy(gameObject);
                return; // early exit
            }
        }

        if (AutoDestroy && transform.position.x <= DestroyXThreshold)
        {
            Destroy(gameObject);
        }
    }
}

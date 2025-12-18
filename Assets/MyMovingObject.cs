using UnityEngine;

/// <summary>
/// Add this class to an object and it will move according to its speed and acceleration.
/// Can move in world or local space, and can optionally destroy itself when off-screen or after a time.
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
    public float Lifetime = 60f;                    // Optional self-destroy after time (0 = disabled)

    private float _currentLifetime;

    private void OnEnable()
    {
        _currentLifetime = 0f;
        Speed = MyLevelManager.Instance._tileMoveSpeed;
    }

    private void Update()
    {
        Move();
        CheckDestroy();
    }

    /// <summary>
    /// Moves the object according to speed and direction.
    /// </summary>
    private void Move()
    {
        Vector3 movement = Direction.normalized * Speed * Time.deltaTime;
        transform.Translate(movement, MovementSpace);
    }

    /// <summary>
    /// Check if object should be destroyed.
    /// </summary>
    private void CheckDestroy()
    {
        // Lifetime destroy
        if (Lifetime > 0f)
        {
            _currentLifetime += Time.deltaTime;
            if (_currentLifetime >= Lifetime)
                Destroy(gameObject);
        }

        // Offscreen destroy (X-axis)
        if (AutoDestroy && transform.position.x <= DestroyXThreshold)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class MyLaneRunner : MonoBehaviour
{
    [Header("Lane")]
    public float LaneWidth = 3f;
    public int NumberOfLanes = 3;
    public float LaneMoveSpeed = 12f;

    [Header("Jump")]
    public float JumpHeight = 3f;
    public float JumpDuration = 0.5f;

    [Header("Slide")]
    public float SlideDuration = 0.5f;
    public float SlideHeight = 0.5f;
    public BoxCollider MainCollider;
    public BoxCollider SlideCollider;
    public ParticleSystem SlideParticles;

    [Header("Death / Invincibility")]
    public ParticleSystem DeathEffect;
    public float RespawnInvincibility = 2f;   // seconds of invincibility after respawn
    public Renderer[] RenderersToBlink;      // assign child renderers for blinking

    private int _laneIndex = 1;
    private float _targetZ;

    private bool _isJumping;
    private bool _isSliding;
    private bool _isInvincible;
    private float _actionTime;

    private Animator _animator;
    private Vector3 _pos;

    private const float LOCKED_X = -18.5f;
    private const float BASE_Y = -3.1f;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _targetZ = LaneToZ(_laneIndex);

        _pos = transform.position;
        _pos.x = LOCKED_X;
        _pos.y = BASE_Y;
        _pos.z = _targetZ;
        transform.position = _pos;

        MainCollider.enabled = true;
        SlideCollider.enabled = false;
    }

    private void OnEnable()
    {
        // Start invisible and invincible at game start
        if (RespawnInvincibility > 0f)
        {
            StartCoroutine(InvincibilityCoroutine(RespawnInvincibility));
        }
    }

    private void Update()
    {
        MoveLane();
        HandleAction();
        // LEFT
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            OnLeft();
        }

        // RIGHT
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            OnRight();
        }

        // UP
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            OnUp();
        }

        // DOWN
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            OnDown();
        }

        // JUMP
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJump();
        }

        transform.position = _pos;
    }
    void OnLeft()
    {
        // TODO: Left logic here
        MoveLeft();
    }

    void OnRight()
    {
        // TODO: Right logic here
        MoveRight();
    }

    void OnUp()
    {
        // TODO: Up logic here
        Jump();
    }

    void OnDown()
    {
        // TODO: Down logic here
        Slide();
    }

    void OnJump()
    {
        // TODO: Jump logic here
        Jump();
    }

    // ======================================================
    // LANE MOVEMENT
    // ======================================================
    private void MoveLane()
    {
        _pos.x = LOCKED_X;
        _pos.z = Mathf.MoveTowards(_pos.z, _targetZ, LaneMoveSpeed * Time.deltaTime);
    }

    public void MoveLeft()
    {
        _laneIndex = Mathf.Clamp(_laneIndex + 1, 0, NumberOfLanes - 1);
        _targetZ = LaneToZ(_laneIndex);
    }

    public void MoveRight()
    {
        _laneIndex = Mathf.Clamp(_laneIndex - 1, 0, NumberOfLanes - 1);
        _targetZ = LaneToZ(_laneIndex);
    }

    // ======================================================
    // ACTIONS
    // ======================================================
    private void HandleAction()
    {
        if (_isJumping)
        {
            _actionTime += Time.deltaTime;
            float t = _actionTime / JumpDuration;
            _pos.y = BASE_Y + (1f - (2f * t - 1f) * (2f * t - 1f)) * JumpHeight;

            if (t >= 1f)
            {
                _isJumping = false;
                _pos.y = BASE_Y;
            }
        }
        else if (_isSliding)
        {
            _actionTime += Time.deltaTime;
            float t = _actionTime / SlideDuration;
            _pos.y = Mathf.Lerp(_pos.y, BASE_Y - SlideHeight, t);

            if (t >= 2f)
                EndSlide();
        }
    }

    public void Jump()
    {
        if (_isJumping) return;

        EndSlide();
        _isJumping = true;
        _actionTime = 0f;

        MainCollider.enabled = !_isInvincible;
        SlideCollider.enabled = false;

        _animator?.SetTrigger("Jump");
    }

    public void Slide()
    {
        if (_isSliding) return;

        _isJumping = false;
        _isSliding = true;
        _actionTime = 0f;

        SlideParticles?.Play();
        SlideCollider.enabled = !_isInvincible;
        MainCollider.enabled = false;

        _animator?.SetTrigger("Slide");
    }

    private void EndSlide()
    {
        if (!_isSliding) return;

        _isSliding = false;
        SlideParticles?.Stop();

        SlideCollider.enabled = false;
        MainCollider.enabled = !_isInvincible;
        _pos.y = BASE_Y;
    }

    private float LaneToZ(int lane) => (lane - 1) * LaneWidth;

    private void OnTriggerEnter(Collider other)
    {
        if (_isInvincible) return;  // fully ignore collisions
        if (!other.CompareTag("Obs")) return;

        DeathEffect?.Play();
        GameplayManager.instance?.Crashed();
        gameObject.SetActive(false);
    }

    // ======================================================
    // RESPAWN / INVINCIBILITY
    // ======================================================
    public void Respawn()
    {
        /*
        transform.position = spawnPosition;
        _pos = spawnPosition;

        gameObject.SetActive(true);
        */

        // Start temporary invincibility
        if (RespawnInvincibility > 0f)
            StartCoroutine(InvincibilityCoroutine(RespawnInvincibility));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {


        _isInvincible = true;

        // Disable colliders fully during invincibility
        MainCollider.enabled = false;
        SlideCollider.enabled = false;

        float blinkInterval = 0.1f;
        float timer = 0f;

        while (timer < duration)
        {
            if (RenderersToBlink != null)
            {
                foreach (var r in RenderersToBlink)
                    r.enabled = !r.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Re-enable colliders and renderers
        MainCollider.enabled = true;
        SlideCollider.enabled = false; // default
        if (RenderersToBlink != null)
        {
            foreach (var r in RenderersToBlink)
                r.enabled = true;
        }

        _isInvincible = false;
    }
}

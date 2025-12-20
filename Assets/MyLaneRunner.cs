using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MyLaneRunner : MonoBehaviour
{
    [Header("Lane Settings")]
    public float LaneWidth = 3f;
    public int NumberOfLanes = 3;
    public float LaneMoveSpeed = 12f; // units/sec (fast & smooth)

    [Header("Jump")]
    public float JumpHeight = 3f;
    public float JumpDuration = 0.5f;

    [Header("Slide")]
    public float SlideDuration = 0.5f;
    public float SlideHeight = 0.5f;
    public BoxCollider MainCollider;
    public BoxCollider SlideCollider;
    public ParticleSystem SlideParticles;

    [Header("Death")]
    public ParticleSystem DeathEffect;

    private int _laneIndex;           // 0,1,2
    private float _targetZ;

    private bool _isJumping;
    private bool _isSliding;
    private float _actionTime;
    private float _actionStartY;

    private Animator _animator;

    private const float LOCKED_X = -20f;
    private const float BASE_Y = -3.1f;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

        _laneIndex = 1; // middle lane
        _targetZ = LaneToZ(_laneIndex);

        Vector3 pos = transform.position;
        pos.x = LOCKED_X;
        pos.y = BASE_Y;
        pos.z = _targetZ;
        transform.position = pos;

        MainCollider.enabled = true;
        SlideCollider.enabled = false;
    }

    // ======================================================
    // INPUT → ONLY CHANGE INDEX
    // ======================================================
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
    // FIXEDUPDATE → MOVEMENT ONLY
    // ======================================================
    private void FixedUpdate()
    {
        // Lock X
        Vector3 pos = transform.position;
        pos.x = LOCKED_X;

        // Smooth Z movement (never stops mid-lane)
        pos.z = Mathf.MoveTowards(
            pos.z,
            _targetZ,
            LaneMoveSpeed * Time.fixedDeltaTime
        );

        transform.position = pos;
    }

    // ======================================================
    // UPDATE → JUMP & SLIDE
    // ======================================================
    private void Update()
    {
        HandleAction();
    }

    private void HandleAction()
    {
        if (_isJumping)
        {
            _actionTime += Time.deltaTime;
            float t = _actionTime / JumpDuration;

            float y = BASE_Y + Mathf.Sin(Mathf.PI * t) * JumpHeight;
            SetY(y);

            if (t >= 1f)
            {
                _isJumping = false;
                SetY(BASE_Y);
            }
        }
        else if (_isSliding)
        {
            _actionTime += Time.deltaTime;
            float t = _actionTime / SlideDuration;

            float y = Mathf.Lerp(transform.position.y, BASE_Y - SlideHeight, t);
            SetY(y);

            if (t >= 1f)
                EndSlide();
        }
    }

    private void SetY(float y)
    {
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    // ======================================================
    // ACTIONS
    // ======================================================
    public void Jump()
    {
        if (_isJumping) return;

        if (_isSliding)
            EndSlide();

        _isJumping = true;
        _actionTime = 0f;

        MainCollider.enabled = true;
        SlideCollider.enabled = false;

        _animator?.SetTrigger("Jump");
    }

    public void Slide()
    {
        if (_isSliding) return;

        if (_isJumping)
            _isJumping = false;

        _isSliding = true;
        _actionTime = 0f;

        SlideParticles?.Play();

        SlideCollider.enabled = true;
        MainCollider.enabled = false;

        _animator?.SetTrigger("Slide");
    }

    private void EndSlide()
    {
        _isSliding = false;
        _actionTime = 0f;

        SlideParticles?.Stop();
        SlideCollider.enabled = false;
        MainCollider.enabled = true;

        SetY(BASE_Y);
    }

    // ======================================================
    // HELPERS
    // ======================================================
    private float LaneToZ(int lane)
    {
        return (lane - 1) * LaneWidth;
    }

    // ======================================================
    // DEATH
    // ======================================================
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obs")) return;

        if (DeathEffect)
            Instantiate(DeathEffect, transform.position, Quaternion.identity);

        GameplayManager.instance?.Crashed();
        Destroy(gameObject);
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class MyLaneRunner : MonoBehaviour
{
    [Header("Lanes")]
    public float LaneWidth = 3f;
    public int NumberOfLanes = 3;
    public float ChangingLaneSpeed = 0.2f;

    private int _currentLane;
    private bool _isMoving;

    [Header("Jump Settings")]
    public float JumpHeight = 3f;
    public float JumpDuration = 0.5f;

    [Header("Slide Settings")]
    public float SlideDuration = 0.5f;
    public float SlideHeight = 0.5f;
    public BoxCollider MainCollider;
    public BoxCollider SlideCollider;
    public ParticleSystem SlideParticles;

    [Header("Death")]
    public ParticleSystem DeathEffect;

    private bool _isJumping;
    private bool _isSliding;

    private float _actionStartY;
    private float _actionTime;
    private float _actionDuration;

    private Animator _animator;

    private const float LOCKED_X = -20f;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

        _currentLane = (NumberOfLanes + 1) / 2;
        _actionStartY = -3.1f;

        MainCollider.enabled = true;
        SlideCollider.enabled = false;
    }

    private void Update()
    {
        // Lock X
        if (transform.localPosition.x != LOCKED_X)
        {
            transform.localPosition = new Vector3(LOCKED_X, transform.localPosition.y, transform.localPosition.z);
        }

        HandleAction();
    }

    #region Lane Movement

    public void MoveLeft()
    {
        if (_currentLane <= 1 || _isMoving) return;
        _currentLane--;
        StartCoroutine(MoveToLane());
    }

    public void MoveRight()
    {
        if (_currentLane >= NumberOfLanes || _isMoving) return;
        _currentLane++;
        StopAllCoroutines();
        StartCoroutine(MoveToLane());
    }

    private IEnumerator MoveToLane()
    {
        _isMoving = true;
        Vector3 startZ = new Vector3(0f, 0f, transform.position.z);
        float middleLane = (NumberOfLanes + 1) / 2f;
        float targetZ = (middleLane - _currentLane) * LaneWidth;
        float elapsed = 0f;

        while (elapsed < ChangingLaneSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ChangingLaneSpeed);
            transform.position = new Vector3(LOCKED_X, transform.position.y, Mathf.Lerp(startZ.z, targetZ, t));
            yield return null;
        }

        transform.position = new Vector3(LOCKED_X, transform.position.y, targetZ);

        Vector3 finalPos = transform.position;
        finalPos.z = targetZ;
        transform.position = finalPos;

        _isMoving = false;
    }

    #endregion

    #region Jump & Slide

    private void HandleAction()
    {
        if (_isJumping)
        {
            _actionTime += Time.deltaTime;
            float t = Mathf.Clamp01(_actionTime / _actionDuration);
            float newY = _actionStartY + Mathf.Sin(Mathf.PI * t) * JumpHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            if (_actionTime >= _actionDuration) EndJump();
        }
        else if (_isSliding)
        {
            _actionTime += Time.deltaTime;
            float t = Mathf.Clamp01(_actionTime / _actionDuration);
            float newY = Mathf.Lerp(transform.position.y, _actionStartY - SlideHeight, t);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            if (_actionTime >= _actionDuration) EndSlide();
        }
    }

    public void Jump()
    {
        if (_isJumping) return;
        if (_isSliding) EndSlide();

        _actionStartY = transform.position.y;
        _actionTime = 0f;
        _actionDuration = JumpDuration;
        _isJumping = true;

        MainCollider.enabled = true;
        SlideCollider.enabled = false;

        _animator?.SetTrigger("Jump");
    }

    public void Slide()
    {
        if (_isSliding) return;
        if (_isJumping) EndJump();

        SlideParticles?.Play();

        _actionTime = 0f;
        _actionDuration = SlideDuration;
        _isSliding = true;

        SlideCollider.enabled = true;
        MainCollider.enabled = false;

        _animator?.SetTrigger("Slide");
    }

    private void EndJump()
    {
        _isJumping = false;
        _actionTime = 0f;
    }

    private void EndSlide()
    {
        SlideParticles?.Stop();
        _isSliding = false;
        _actionTime = 0f;
        SlideCollider.enabled = false;
        MainCollider.enabled = true;
    }

    #endregion

    #region Death Trigger

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obs"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (DeathEffect != null)
        {
            ParticleSystem effect = Instantiate(DeathEffect, transform.position, Quaternion.identity);
            effect.Play();
        }

        GameplayManager.instance?.Crashed();

        Destroy(gameObject);
    }

    #endregion
}

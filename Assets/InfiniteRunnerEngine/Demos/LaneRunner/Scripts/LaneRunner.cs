using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InfiniteRunnerEngine
{
    public class LaneRunner : PlayableCharacter
    {
        [Header("Lanes")]
        public float LaneWidth = 3f;
        public int NumberOfLanes = 3;
        public float ChangingLaneSpeed = 1f;
        public GameObject Explosion;

        protected int _currentLane;
        protected bool _isMoving = false;

        [Header("Jump Settings")]
        public float JumpHeight = 3f;
        public float JumpDuration = 0.5f;

        [Header("Slide Settings")]
        public float SlideDuration = 0.5f;
        public float SlideHeight = 0.5f;
        public BoxCollider SlideCollider;
        public BoxCollider MainCollider;
        public ParticleSystem slideParticles;

        private bool _isJumping = false;
        private bool _isSliding = false;

        private float _actionStartY;    // Base Y for current action
        private float _actionTime;      // Time elapsed in current action
        private float _actionDuration;  // Duration of current action


        private Animator _animator;

        protected override void Awake()
        {
            Initialize();

            _currentLane = NumberOfLanes / 2;
            if (NumberOfLanes % 2 == 1)
            {
                _currentLane++;
            }
        }

        private void Start()
        {
            transform.localPosition = new Vector3(-14f, transform.localPosition.y, transform.localPosition.z);
            _animator = GetComponentInChildren<Animator>();

            _actionStartY = -3.27f;
        }

        protected override void Update()
        {
            // Lock X position
            if (transform.localPosition.x != -14f)
            {
                transform.localPosition = new Vector3(-14f, transform.localPosition.y, transform.localPosition.z);
            }

            UpdateAnimator();
            CheckDeathConditions();

            HandleAction(); // Smooth jump & slide
        }

        #region Lane Movement

        public override void LeftStart()
        {
            if (_currentLane <= 1 || _isMoving) return;
            _currentLane--;
            StartCoroutine(MoveLaneToCurrentIndex());
        }

        public override void RightStart()
        {
            if (_currentLane >= NumberOfLanes || _isMoving) return;
            _currentLane++;
            StartCoroutine(MoveLaneToCurrentIndex());
        }

        private IEnumerator MoveLaneToCurrentIndex()
        {
            _isMoving = true;

            float elapsedTime = 0f;
            Vector3 startPos = transform.position;
            float middleLane = (NumberOfLanes + 1) / 2f;
            float targetZ = (middleLane - _currentLane) * LaneWidth;
            Vector3 destination = new Vector3(transform.position.x, transform.position.y, targetZ);

            while (elapsedTime < ChangingLaneSpeed)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / ChangingLaneSpeed);

                Vector3 pos = Vector3.Lerp(startPos, destination, t);
                pos.y = transform.position.y; // preserve current Y
                transform.position = pos;

                yield return null;
            }

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

                // Jump follows a sine curve
                float targetY = _actionStartY + Mathf.Sin(Mathf.PI * t) * JumpHeight;
                Vector3 pos = transform.position;
                pos.y = targetY;
                transform.position = pos;

                if (_actionTime >= _actionDuration)
                {
                    EndJump();
                }
            }
            else if (_isSliding)
            {
                _actionTime += Time.deltaTime;
                float t = Mathf.Clamp01(_actionTime / _actionDuration);

                float targetY = _actionStartY - SlideHeight;
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, targetY, t); // smooth linear slide
                transform.position = pos;

                if (_actionTime >= _actionDuration)
                {
                    EndSlide();
                }
            }
        }

        public override void UpStart()
        {
            if (_isJumping) return;

            if (_isSliding) EndSlide(); // Cancel slide



            _actionStartY = transform.position.y;
            _actionTime = 0f;
            _actionDuration = JumpDuration;
            _isJumping = true;

            MainCollider.enabled = true;
            SlideCollider.enabled = false;

            _animator.SetTrigger("Jump");
        }

        public override void DownStart()
        {
            if (_isSliding) return;

            if (_isJumping) EndJump(); // Cancel jump


            slideParticles.Play();
            //_actionStartY = transform.position.y;
            _actionTime = 0f;
            _actionDuration = SlideDuration;
            _isSliding = true;

            SlideCollider.enabled = true;
            MainCollider.enabled = false;

            _animator.SetTrigger("Slide");
        }

        private void EndJump()
        {
            
            _isJumping = false;
            _actionTime = 0f;
            // Keep current Y for smooth transition to slide
        }

        private void EndSlide()
        {


            slideParticles.Stop();
            _isSliding = false;
            _actionTime = 0f;

            SlideCollider.enabled = false;
            MainCollider.enabled = true;
            // Keep current Y for smooth transition to jump
        }

        public override void UpEnd() { }
        public override void UpOngoing() { }

        #endregion

        #region Death

        public override void Die()
        {
            if (Explosion != null)
            {
                GameObject explosion = Instantiate(Explosion);
                explosion.transform.position = GetComponent<BoxCollider>().bounds.center;
            }

            GameplayManager.instance.Crashed();
            Destroy(gameObject);
        }

        #endregion
    }
}

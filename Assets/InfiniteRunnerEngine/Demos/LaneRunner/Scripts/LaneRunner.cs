using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InfiniteRunnerEngine
{
    public class LaneRunner : PlayableCharacter
    {
        [Space(10)]
        [Header("Lanes")]
        public float LaneWidth = 3f;
        public int NumberOfLanes = 3;
        public float ChangingLaneSpeed = 1f;
        public GameObject Explosion;

        protected int _currentLane;
        protected bool _isMoving = false;

        [Header("Jump Settings")]
        public float JumpHeight = 3f;     // Max jump height
        public float JumpDuration = 0.5f; // Time to reach apex

        private bool _isJumping = false;
        private float _jumpStartY;
        private float _jumpTime;

        protected override void Awake()
        {
            Initialize();
            _currentLane = NumberOfLanes / 2;
            if (NumberOfLanes % 2 == 1) _currentLane++;
            
        }
        private void Start()
        {
            transform.localPosition = new Vector3(-14f, transform.localPosition.y, transform.localPosition.z);
        }

        protected override void Update()
        {
            if(transform.localPosition.x != -14f)
            {
                transform.localPosition = new Vector3(-14f, transform.localPosition.y, transform.localPosition.z);
            }

            UpdateAnimator();
            CheckDeathConditions();
            HandleJump();
        }

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

            // Calculate target Z from lane index
            float middleLane = (NumberOfLanes + 1) / 2f; // e.g., 3 lanes -> middle = 2
            float targetZ = (middleLane - _currentLane) * LaneWidth;

            Vector3 destination = new Vector3(transform.position.x, transform.position.y, targetZ);

            while (elapsedTime < ChangingLaneSpeed)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / ChangingLaneSpeed);

                Vector3 pos = Vector3.Lerp(startPos, destination, t);
                pos.y = transform.position.y; // preserve jump height
                transform.position = pos;
                yield return null;
            }

            // Snap exactly to lane
            Vector3 finalPos = transform.position;
            finalPos.z = targetZ;
            transform.position = finalPos;

            _isMoving = false;
        }


        public override void UpStart()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _jumpStartY = transform.position.y;
                _jumpTime = 0f;
            }
        }

        public override void UpEnd() { }
        public override void UpOngoing() { }

        private void HandleJump()
        {
            if (!_isJumping) return;

            _jumpTime += Time.deltaTime;
            float t = Mathf.Clamp01(_jumpTime / JumpDuration);

            // Smooth parabola using sine
            float newY = _jumpStartY + Mathf.Sin(Mathf.PI * t) * JumpHeight;

            Vector3 pos = transform.position;
            pos.y = newY;
            transform.position = pos;

            if (_jumpTime >= JumpDuration)
            {
                _isJumping = false;
                _jumpTime = 0f;
                pos.y = _jumpStartY; // land smoothly
                transform.position = pos;
            }
        }

        public override void Die()
        {
            if (Explosion != null)
            {
                GameObject explosion = Instantiate(Explosion);
                explosion.transform.position = transform.GetComponent<BoxCollider>().bounds.center;
            }
            GameplayManager.instance.Crashed();
            Destroy(gameObject);
        }
    }
}

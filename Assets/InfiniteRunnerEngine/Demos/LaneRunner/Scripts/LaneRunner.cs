using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.InfiniteRunnerEngine
{
    public class LaneRunner : PlayableCharacter
    {
        [Header("Lanes")]
        public float LaneWidth = 3f;
        public int NumberOfLanes = 3;
        public float ChangingLaneSpeed = 10f;

        [Header("Position Lock")]
        public float LockedX = -14f;

        [Header("Jump")]
        public float JumpHeight = 3f;
        public float JumpDuration = 0.5f;

        [Header("Slide")]
        public float SlideDuration = 0.5f;
        public float SlideHeight = 0.5f;
        public BoxCollider SlideCollider;
        public BoxCollider MainCollider;
        public ParticleSystem slideParticles;

        [Header("VFX")]
        public GameObject Explosion;

        private Transform _tr;
        private Animator _anim;

        private int _currentLane;
        private float _targetZ;

        private bool _jumping;
        private bool _sliding;

        private float _actionTime;
        private float _groundY;

        #region Init

        protected override void Awake()
        {
            Initialize();
            _tr = transform;
            _anim = GetComponentInChildren<Animator>();

            _currentLane = NumberOfLanes / 2;
        }

        private void Start()
        {
            Vector3 pos = _tr.localPosition;
            pos.x = LockedX;
            _tr.localPosition = pos;

            _groundY = pos.y;
            UpdateTargetZ();

            SlideCollider.enabled = false;
            MainCollider.enabled = true;
        }

        #endregion

        protected override void Update()
        {
            UpdateLaneMovement();
            UpdateVerticalAction();
            UpdateAnimator();
            CheckDeathConditions();
        }

        #region Lane Movement (NO Coroutine)

        private void UpdateLaneMovement()
        {
            Vector3 pos = _tr.localPosition;

            pos.z = Mathf.MoveTowards(
                pos.z,
                _targetZ,
                ChangingLaneSpeed * Time.deltaTime
            );

            pos.x = LockedX;
            _tr.localPosition = pos;
        }

        private void UpdateTargetZ()
        {
            float middle = (NumberOfLanes - 1) * 0.5f;
            _targetZ = (middle - _currentLane) * LaneWidth;
        }

        public override void LeftStart()
        {
            if (_currentLane <= 0) return;
            _currentLane--;
            UpdateTargetZ();
        }

        public override void RightStart()
        {
            if (_currentLane >= NumberOfLanes - 1) return;
            _currentLane++;
            UpdateTargetZ();
        }

        #endregion

        #region Jump & Slide (No state spam)

        private void UpdateVerticalAction()
        {
            if (!_jumping && !_sliding) return;

            _actionTime += Time.deltaTime;
            float t = Mathf.Clamp01(_actionTime / (_jumping ? JumpDuration : SlideDuration));

            float y;

            if (_jumping)
                y = _groundY + Mathf.Sin(t * Mathf.PI) * JumpHeight;
            else
                y = Mathf.Lerp(_groundY, _groundY - SlideHeight, t);

            Vector3 pos = _tr.localPosition;
            pos.y = y;
            _tr.localPosition = pos;

            if (t >= 1f)
                EndAction();
        }

        public override void UpStart()
        {
            if (_jumping) return;

            StartAction(true);
            _anim.SetTrigger("Jump");
        }

        public override void DownStart()
        {
            if (_sliding) return;

            StartAction(false);
            slideParticles?.Play();
            _anim.SetTrigger("Slide");
        }

        private void StartAction(bool jump)
        {
            _jumping = jump;
            _sliding = !jump;
            _actionTime = 0f;

            SlideCollider.enabled = !jump;
            MainCollider.enabled = jump;
        }

        private void EndAction()
        {
            _jumping = false;
            _sliding = false;
            _actionTime = 0f;

            slideParticles?.Stop();

            SlideCollider.enabled = false;
            MainCollider.enabled = true;

            Vector3 pos = _tr.localPosition;
            pos.y = _groundY;
            _tr.localPosition = pos;
        }

        #endregion

        #region Death (NO Destroy)

        public override void Die()
        {
            if (Explosion != null)
            {
                Explosion.transform.position = MainCollider.bounds.center;
                Explosion.SetActive(true);
            }

            GameplayManager.instance.Crashed();
            gameObject.SetActive(false);
        }

        #endregion
    }
}

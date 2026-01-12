using UnityEngine;

/// <summary>
/// Swipe input detection for WebGL/mobile.
/// Supports left/right/up/down gestures with proper mobile responsiveness.
/// </summary>
public class SwipeInputCallbacks : MonoBehaviour
{
    [Header("Swipe Settings")]
    [Range(0f, 0.5f)]
    public float MinSwipePercent = 0.1f; // Minimum swipe distance as % of screen height

    [Header("References")]
    public MyLaneRunner Runner;

    private Vector2 _startPos;
    private bool _dragging;
    private float _minSwipeDistance;

    private void Start()
    {
        // Scale swipe distance to screen size
        _minSwipeDistance = Screen.height * MinSwipePercent;
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        HandleMouseSwipe();
#else
        HandleTouchSwipe();
#endif
    }

    // -------------------------------
    // Editor / Standalone (mouse)
    // -------------------------------
    private void HandleMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _startPos = Input.mousePosition;
            _dragging = true;
        }
        else if (Input.GetMouseButtonUp(0) && _dragging)
        {
            _dragging = false;
            DetectSwipe((Vector2)Input.mousePosition);
        }
    }

    // -------------------------------
    // Mobile touch input
    // -------------------------------
    private void HandleTouchSwipe()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _startPos = touch.position;
                _dragging = true;
                break;

            case TouchPhase.Moved:
                // Optional: detect swipe during drag
                if (_dragging)
                    DetectSwipeDuringDrag(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (_dragging)
                {
                    _dragging = false;
                    DetectSwipe(touch.position);
                }
                break;
        }
    }

    // -------------------------------
    // Swipe detection
    // -------------------------------
    private void DetectSwipe(Vector2 endPos)
    {
        Vector2 swipe = endPos - _startPos;

        if (swipe.sqrMagnitude < _minSwipeDistance * _minSwipeDistance)
            return;

        ProcessSwipeDirection(swipe);
    }

    private void DetectSwipeDuringDrag(Vector2 currentPos)
    {
        Vector2 swipe = currentPos - _startPos;

        if (swipe.sqrMagnitude < _minSwipeDistance * _minSwipeDistance)
            return;

        _dragging = false; // Consume swipe
        ProcessSwipeDirection(swipe);
    }

    private void ProcessSwipeDirection(Vector2 swipe)
    {
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            if (swipe.x > 0) Runner.MoveRight();
            else Runner.MoveLeft();
        }
        else
        {
            if (swipe.y > 0) Runner.Jump();
            else Runner.Slide();
        }
    }
}

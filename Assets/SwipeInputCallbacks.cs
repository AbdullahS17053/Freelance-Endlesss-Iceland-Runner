using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeInputCallbacks : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float MinSwipeDistance = 80f;

    [Header("Target")]
    public MyLaneRunner Runner;

    private Vector2 _startPos;
    private Vector2 _endPos;
    private bool _isDragging;

    private void Update()
    {
        // --------------------------
        // Mouse input
        // --------------------------
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _startPos = Mouse.current.position.ReadValue();
                _isDragging = true;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _endPos = Mouse.current.position.ReadValue();
                _isDragging = false;
                DetectSwipe();
            }
        }

        // --------------------------
        // Touch input
        // --------------------------
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (!_isDragging)
            {
                _startPos = Touchscreen.current.primaryTouch.position.ReadValue();
                _isDragging = true;
            }
            _endPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (_isDragging && Touchscreen.current != null)
        {
            _isDragging = false;
            DetectSwipe();
        }
    }

    private void DetectSwipe()
    {
        Vector2 swipe = _endPos - _startPos;
        if (swipe.sqrMagnitude < MinSwipeDistance * MinSwipeDistance) return;

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

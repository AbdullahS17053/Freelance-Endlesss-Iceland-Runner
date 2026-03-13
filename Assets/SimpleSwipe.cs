using UnityEngine;
using UnityEngine.Events;

public class SimpleSwipe : MonoBehaviour
{
    public float minSwipeDistance = 50f;

    [Header("Swipe Events")]
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;

    private Vector2 startPos;
    private bool swiping = false;

    void Update()
    {
        HandleMouse();
        HandleTouch();
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            swiping = true;
        }

        if (Input.GetMouseButtonUp(0) && swiping)
        {
            swiping = false;
            DetectSwipe((Vector2)Input.mousePosition);
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startPos = touch.position;
            swiping = true;
        }

        if (touch.phase == TouchPhase.Ended && swiping)
        {
            swiping = false;
            DetectSwipe(touch.position);
        }
    }

    void DetectSwipe(Vector2 endPos)
    {
        Vector2 swipe = endPos - startPos;

        if (Mathf.Abs(swipe.x) < minSwipeDistance)
            return;

        if (swipe.x > 0)
            OnSwipeRight?.Invoke();
        else
            OnSwipeLeft?.Invoke();
    }
}
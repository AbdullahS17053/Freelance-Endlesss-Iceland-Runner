using UnityEngine;

public class FaceCameraY : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Direction from object → camera
        Vector3 direction = cam.position - transform.position;

        // Remove vertical influence (ignore XZ tilt)
        direction.y = 0f;

        // Don't look if zero vector
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}

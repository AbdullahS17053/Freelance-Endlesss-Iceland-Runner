using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float DestroyX = -40f; // X position at which the object destroys itself

    void Update()
    {
        if (transform.position.x <= DestroyX)
        {
            Destroy(gameObject);
        }
    }
}

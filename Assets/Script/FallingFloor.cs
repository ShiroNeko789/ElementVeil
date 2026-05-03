using UnityEngine;

public class FallingFloor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float targetYPosition; // Only set the Y coordinate you want
    public float fallSpeed = 5f;

    private Vector3 originalPosition;
    private bool shouldFall = false;

    void Start()
    {
        // Remember exactly where we started
        originalPosition = transform.position;
    }

    void Update()
    {
        // Determine if we are heading to the target Y or back to the start Y
        float destinationY = shouldFall ? targetYPosition : originalPosition.y;

        // Create a target vector that keeps the CURRENT X and Z
        Vector3 targetVec = new Vector3(transform.position.x, destinationY, transform.position.z);

        // Move only towards that vertical target
        transform.position = Vector3.MoveTowards(transform.position, targetVec, fallSpeed * Time.deltaTime);
    }

    public void Fall() => shouldFall = true;
    public void ResetFloor() => shouldFall = false;
}
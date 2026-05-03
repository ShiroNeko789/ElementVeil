using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    public Vector3 cameraFixPosition;
    private CameraFollow camScript;

    void Start()
    {
        // Find the camera script
        if (Camera.main != null)
        {
            camScript = Camera.main.GetComponent<CameraFollow>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && camScript != null)
        {
            // This now matches the name in CameraFollow
            camScript.LockCamera(cameraFixPosition);
        }
    }
}
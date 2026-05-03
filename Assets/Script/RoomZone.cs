using UnityEngine;

public class RoomZone : MonoBehaviour
{
    [Header("Camera Limits for THIS Room")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Find the camera and update its limits to this room's size
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.SetRoomLimits(minX, maxX, minY, maxY);
            }
        }
    }
}
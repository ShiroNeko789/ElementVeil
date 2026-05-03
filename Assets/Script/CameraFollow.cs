using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    [Header("Boundaries")]
    public bool useLimits = true;
    public float minX, maxX, minY, maxY;

    [Header("Boss State")]
    public bool isLocked = false;
    public Vector3 lockedPos;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition;

        if (isLocked)
        {
            targetPosition = new Vector3(lockedPos.x, lockedPos.y, -10f);
        }
        else
        {
            targetPosition = player.position;
            targetPosition.z = -10f;

            if (useLimits)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // These two functions fix your BossRoom and RoomZone errors
    public void LockCamera(Vector3 bossPos) { lockedPos = bossPos; isLocked = true; }
    public void UnlockCamera() => isLocked = false;

    public void SetRoomLimits(float xMin, float xMax, float yMin, float yMax)
    {
        minX = xMin; maxX = xMax; minY = yMin; maxY = yMax;
        useLimits = true; isLocked = false;
    }
}
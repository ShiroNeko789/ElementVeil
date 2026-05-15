using UnityEngine;

public class CameraBossLock : MonoBehaviour
{
    public static CameraBossLock Instance;

    private bool isLocked = false;
    private Vector3 lockPosition;
    private float targetSize;
    private float smoothSpeed = 5f;

    private Camera cam;
    private float defaultSize;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        defaultSize = cam.orthographicSize;
    }

    void LateUpdate() // LateUpdate ensures we override other camera scripts
    {
        if (isLocked)
        {
            // Lock position (keeping Z at -10)
            Vector3 targetPos = new Vector3(lockPosition.x, lockPosition.y, -10f);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

            // Lock/Resize Zoom
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * smoothSpeed);
        }
    }

    public void LockToRoom(Vector3 position, float size)
    {
        lockPosition = position;
        targetSize = size;
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
        cam.orthographicSize = defaultSize;
    }
}
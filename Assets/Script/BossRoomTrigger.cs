using UnityEngine;
using System.Collections;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("References")]
    public MushroomBoss boss;
    public GameObject wall;              // the wall that drops down to block exit
    public GameObject bossHUD;           // the BossHUD parent object on the Canvas
    public Camera mainCamera;            // drag your Main Camera here

    [Header("Camera Settings")]
    public float normalSize = 5f;        // your current camera orthographic size
    public float bossRoomSize = 8f;      // zoomed out size for boss room
    public float zoomSpeed = 2f;

    [Header("Camera Lock")]
    public Transform bossRoomCenter;     // empty GameObject placed at center of boss room

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(BossIntroSequence(other.gameObject));
    }

    IEnumerator BossIntroSequence(GameObject player)
    {
        // 1. Drop the wall
        if (wall != null) wall.SetActive(true);

        // 2. Lock camera to boss room center and zoom out
        if (mainCamera != null)
            StartCoroutine(ZoomCamera(bossRoomSize));

        // Optional: disable player input here if you have a PlayerController
        // player.GetComponent<PlayerController>()?.SetInputEnabled(false);

        // 3. Wait for camera to settle
        yield return new WaitForSeconds(1f);

        // 4. Show boss HUD
        if (bossHUD != null) bossHUD.SetActive(true);

        // 5. Tell boss to do intro scream
        if (boss != null)
            yield return StartCoroutine(boss.PlayIntroScream());

        // 6. Re-enable player input
        // player.GetComponent<PlayerController>()?.SetInputEnabled(true);
    }

    IEnumerator ZoomCamera(float targetSize)
    {
        // Lock camera position to boss room center
        if (bossRoomCenter != null && mainCamera != null)
            mainCamera.transform.position = new Vector3(
                bossRoomCenter.position.x,
                bossRoomCenter.position.y,
                mainCamera.transform.position.z
            );

        // Zoom out smoothly
        float elapsed = 0f;
        float startSize = mainCamera.orthographicSize;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed);
            yield return null;
        }
        mainCamera.orthographicSize = targetSize;
    }
}
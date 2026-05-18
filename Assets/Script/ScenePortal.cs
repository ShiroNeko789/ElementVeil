using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Scene To Load")]
    public string targetScene;          // exact scene name to load

    [Header("Optional — spawn position in next scene")]
    public string spawnPointID = "";    // match this to a SpawnPoint in the next scene

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        // Store spawn point so next scene knows where to place player
        if (!string.IsNullOrEmpty(spawnPointID))
            PlayerPrefs.SetString("SpawnPointID", spawnPointID);

        SceneTransitionManager.Instance.LoadScene(targetScene);
    }
}
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string spawnPointID;         // match this to ScenePortal.spawnPointID

    void Start()
    {
        string targetID = PlayerPrefs.GetString("SpawnPointID", "");
        if (string.IsNullOrEmpty(targetID)) return;
        if (targetID != spawnPointID) return;

        // Move player to this spawn point
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        player.transform.position = transform.position;
        PlayerPrefs.DeleteKey("SpawnPointID"); // clear after use
        Debug.Log("Player spawned at: " + spawnPointID);
    }
}
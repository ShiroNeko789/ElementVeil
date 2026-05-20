using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;

    [Header("Item Database")]
    public Item[] allItemsInGame;

    [Header("Scene Name")]
    public string gameSceneName = "Game";

    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private MushroomBoss boss;
    private List<string> collectedPickups = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        if (scene.name == "MainMenu") return;
        StartCoroutine(InitSceneReferences());
    }

    IEnumerator InitSceneReferences()
    {
        yield return null;
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogError("Player not found in scene"); yield break; }
        playerTransform = player.transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        boss = FindObjectOfType<MushroomBoss>();

        int shouldLoad = PlayerPrefs.GetInt("ShouldLoadSave", 0);
        int isNewGame = PlayerPrefs.GetInt("IsNewGame", 0);

        if (shouldLoad == 1 && SaveSystem.HasSave())
        {
            PlayerPrefs.SetInt("ShouldLoadSave", 0);
            PlayerPrefs.Save();
            LoadGame();
        }
        else if (isNewGame == 1)
        {
            PlayerPrefs.SetInt("IsNewGame", 0);
            PlayerPrefs.Save();
            NewGame();
        }
        // If neither flag is set: health is carried by PlayerHealth.Start()
        // reading the CarriedHealth PlayerPrefs key — no action needed here.
    }

    // ── Scene Transition ──────────────────────────────────────────────────
    /// <summary>
    /// Call this instead of SceneManager.LoadScene() whenever moving the
    /// player to a new scene mid-game (e.g. after boss defeated, portal, etc.).
    /// It saves the current health so PlayerHealth.Start() can restore it.
    /// </summary>
    public void LoadSceneKeepHealth(string sceneName)
    {
        // Write health to PlayerPrefs before scene unloads
        if (playerHealth != null)
            playerHealth.SaveHealthForTransition();

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Convenience overload using the configured gameSceneName.</summary>
    public void LoadNextScene() => LoadSceneKeepHealth(gameSceneName);

    // ── New Game ──────────────────────────────────────────────────────────
    void NewGame()
    {
        // Clear carry key so PlayerHealth starts at max
        PlayerPrefs.DeleteKey("CarriedHealth");
        PlayerPrefs.Save();

        collectedPickups.Clear();
        if (Inventory.Instance != null)
        {
            Inventory.Instance.items.Clear();
            Debug.Log("New game — inventory cleared");
        }
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
            playerHealth.UpdateUIPublic();
        }
    }

    // ── Save / Load ───────────────────────────────────────────────────────
    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogError("SaveGame: Player not found"); return; }
        playerTransform = player.transform;
        playerHealth = player.GetComponent<PlayerHealth>();

        if (Inventory.Instance == null) { Debug.LogError("SaveGame: Inventory.Instance is null"); return; }

        SaveData data = new SaveData();
        data.playerX = playerTransform.position.x;
        data.playerY = playerTransform.position.y;
        data.playerHealth = playerHealth.currentHealth;

        List<int> indexes = new List<int>();
        foreach (Item item in Inventory.Instance.items)
        {
            int idx = System.Array.IndexOf(allItemsInGame, item);
            if (idx >= 0) indexes.Add(idx);
            else Debug.LogWarning("Item not found in allItemsInGame: " + item.itemName);
        }
        data.inventoryItemIndexes = indexes.ToArray();
        data.collectedPickups = collectedPickups.ToArray();
        data.bossDefeated = boss != null && boss.IsDead();

        SaveSystem.Save(data);
        Debug.Log("Save complete — " + indexes.Count + " items saved");
    }

    public void LoadGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) { Debug.LogError("LoadGame: no save data found"); return; }

        // Position
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }
        playerTransform.position = new Vector3(data.playerX, data.playerY, 0f);
        if (rb != null) rb.simulated = true;

        // Health — set directly (no carry key needed for a full load)
        playerHealth.currentHealth = data.playerHealth;
        playerHealth.UpdateUIPublic();

        // Inventory
        Inventory.Instance.items.Clear();
        foreach (int idx in data.inventoryItemIndexes)
        {
            if (idx >= 0 && idx < allItemsInGame.Length)
                Inventory.Instance.items.Add(allItemsInGame[idx]);
            else
                Debug.LogWarning("Invalid item index: " + idx);
        }
        Inventory.Instance.TriggerCallback();

        // Pickups
        collectedPickups.Clear();
        if (data.collectedPickups != null)
            foreach (string n in data.collectedPickups)
                collectedPickups.Add(n);

        ItemPickup[] allPickups = FindObjectsOfType<ItemPickup>();
        foreach (ItemPickup pickup in allPickups)
            if (collectedPickups.Contains(pickup.gameObject.name))
                pickup.gameObject.SetActive(false);

        // Boss
        if (data.bossDefeated && boss != null)
            boss.gameObject.SetActive(false);

        Debug.Log("Load complete. Items: " + Inventory.Instance.items.Count);
    }

    public void RegisterCollectedPickup(string pickupName)
    {
        if (!collectedPickups.Contains(pickupName))
            collectedPickups.Add(pickupName);
    }

    public static GameSaveManager Get()
    {
        if (Instance != null) return Instance;
        Instance = FindObjectOfType<GameSaveManager>();
        if (Instance == null) Debug.LogError("GameSaveManager not found in scene");
        return Instance;
    }
}
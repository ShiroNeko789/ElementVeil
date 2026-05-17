[System.Serializable]
public class SaveData
{
    public float playerX;
    public float playerY;
    public int playerHealth;
    public int[] inventoryItemIndexes; // index into allItemsInGame array
    public bool bossDefeated;
    public string[] collectedPickups;
}
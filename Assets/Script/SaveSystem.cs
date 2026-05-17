using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string savePath => Application.persistentDataPath + "/save.dat";

    public static void Save(SaveData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(savePath, FileMode.Create))
        {
            formatter.Serialize(stream, data);
        }
        Debug.Log("Game saved to: " + savePath);
    }

    public static SaveData Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file found");
            return null;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(savePath, FileMode.Open))
        {
            return formatter.Deserialize(stream) as SaveData;
        }
    }

    public static bool HasSave()
    {
        return File.Exists(savePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
    }
}
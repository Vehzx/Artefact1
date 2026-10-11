using System;
using System.IO;
using UnityEngine;

public class InventoryHolder : MonoBehaviour
{
    // inventory slots
    [SerializeField, Min(1)] private int capacity = 20;

    [SerializeField] private ItemRegistry registry;

    // save to persistent data folder
    [SerializeField] private string saveFileName = "inventory.json";

    private Inventory inventory;

    // do not create in awake, this avoids the UI finding null inventory sometimes
    public Inventory Inventory
    {
        get
        {
            if (inventory == null)
                inventory = new Inventory(capacity);
            return inventory;
        }
    }

    // full save path, path combine to add each folder seperator
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public bool Save()
    {
        // try and catch to stop crashes when no write permissions
        try
        {
            // convert to plain save data and then to json
            string json = JsonUtility.ToJson(Inventory.ToSaveData(), true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Inventory saved to {SavePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Inventory save failed {e.Message}");
            return false;
        }
    }

    public bool Load()
    {
        if (registry == null)
        {
            Debug.LogError("Cannot load inventory, no item registery", this);
            return false;
        }

        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"Inventory no save file at {SavePath}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

            // log if data is corrupt
            if (data == null)
            {
                Debug.LogError("Inventory save file is empty or unreadable");
                return false;
            }

            // load from will skip invalid entries, log them
            int skipped = Inventory.LoadFrom(data, registry);
            if (skipped > 0)
                Debug.LogWarning($"Inventory loaded with {skipped} invalid entr{(skipped == 1 ? "y" : "ies")} skipped");
            else
                Debug.Log("Inventory loaded");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Inventory load failed {e.Message}");
            return false;
        }
    }
}
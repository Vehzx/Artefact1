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

    [Header("Dropping")]

    // how far in front of the player dropped items spawn
    [SerializeField, Min(0.5f)] private float dropDistance = 1.2f;

    // spawn height (roughly chest height)
    [SerializeField, Min(0f)] private float dropHeight = 1f;

    // closest an item can spawn when a wall is in the way, any closer and drop is refused
    [SerializeField, Min(0f)] private float minDropDistance = 0.7f;

    // gap kept between the item and a wall in front
    [SerializeField, Min(0f)] private float wallPadding = 0.25f;

    // what counts as a wall for the drop check
    [SerializeField] private LayerMask dropBlockingLayers = ~0;

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

    // drop the whole stack in a slot into the world in front of the player
    // returns false if nothing was dropped
    public bool DropSlot(int index)
    {
        ItemStack stack = Inventory.GetSlot(index);
        if (stack == null)
            return false;

        ItemDefinition item = stack.Item;

        if (item.PickupPrefab == null)
        {
            Debug.LogWarning($"'{item.name}' has no pickup prefab, cannot drop it", item);
            return false;
        }

        if (!TryGetDropPosition(out Vector3 position))
        {
            Debug.Log("No room to drop item here");
            return false;
        }

        GameObject spawned = Instantiate(item.PickupPrefab, position, Quaternion.identity);

        if (!spawned.TryGetComponent(out ItemPickup pickup))
        {
            Debug.LogError($"Pickup prefab for '{item.name}' has no ItemPickup component", item.PickupPrefab);
            Destroy(spawned);
            return false;
        }

        // remove from inventory only once the pickup definitely exists, so items are never lost
        int removed = Inventory.RemoveFromSlot(index, stack.Quantity);
        pickup.Initialise(item, removed);
        return true;
    }

    /*
    DOCS: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.Raycast.html
    */

    // spawn point in front of the player, pulled back if a wall is in the way
    private bool TryGetDropPosition(out Vector3 position)
    {
        Vector3 origin = transform.position + Vector3.up * dropHeight;
        Vector3 forward = transform.forward;
        float distance = dropDistance;

        // start ray inside the character controller so it will not hit the player
        // ignore triggers so other pickups do not block the drop
        if (Physics.Raycast(origin, forward, out RaycastHit hit, dropDistance, dropBlockingLayers, QueryTriggerInteraction.Ignore))
            distance = hit.distance - wallPadding;

        position = origin + forward * distance;
        return distance >= minDropDistance;
    }

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

#if UNITY_EDITOR
    // shows where items will drop when the player is selected
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * dropHeight;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + transform.forward * dropDistance);
        Gizmos.DrawWireSphere(origin + transform.forward * dropDistance, 0.15f);
    }
#endif
}

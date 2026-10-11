using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Registry", menuName = "Inventory/Item Registry")]
public class ItemRegistry : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();

    private Dictionary<string, ItemDefinition> lookup;

    // finds item with given ID, used for loading save file
    public bool TryGet(string id, out ItemDefinition item)
    {
        if (lookup == null)
            BuildLookup();

        // empty ID can never match an item
        if (string.IsNullOrEmpty(id))
        {
            item = null;
            return false;
        }

        return lookup.TryGetValue(id, out item);
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, ItemDefinition>();

        foreach (ItemDefinition item in items)
        {
            // skip if there are empty slots left in the list
            if (item == null)
                continue;

            // log if item has no ID
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                Debug.LogWarning($"[ItemRegistry] '{item.name}' has no ID and cannot be saved or loaded.", item);
                continue;
            }

            // log duplicates, only keep first
            if (lookup.ContainsKey(item.Id))
            {
                Debug.LogError($"[ItemRegistry] Duplicate item ID '{item.Id}' on '{item.name}'. IDs must be unique.", item);
                continue;
            }

            lookup.Add(item.Id, item);
        }
    }

    // get rid of the cached lookup whenever the asset loads so it is rebuilt fresh
    private void OnEnable() => lookup = null;

#if UNITY_EDITOR
    // runs in editor when list is changed in inspector, so new or renamed items are picked up
    private void OnValidate() => lookup = null;
#endif
}
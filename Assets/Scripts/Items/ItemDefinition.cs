using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private string id;

    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea(3, 8)] private string description;

    [SerializeField, Min(1)] private int maxStack = 1;

    [SerializeField] private GameObject pickupPrefab;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;
    public int MaxStack => maxStack;
    public GameObject PickupPrefab => pickupPrefab;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // fill ID automatically when creating item
        if (string.IsNullOrWhiteSpace(id))
            id = name.Trim().ToLowerInvariant().Replace(' ', '_');

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;
    }
#endif
}

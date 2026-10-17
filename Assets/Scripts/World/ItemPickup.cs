using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int quantity = 1;

    // make the collider a trigger automatically
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // used when an item is dropped from the inventory, overrides whatever the prefab had set
    public void Initialise(ItemDefinition newItem, int newQuantity)
    {
        item = newItem;
        quantity = Mathf.Max(1, newQuantity);
    }

    private void OnTriggerEnter(Collider other)
    {
        // GetComponentInParent allows this to still work if the player collider is on a child object
        /*
        DOCS: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GameObject.GetComponentInParent.html
        */
        InventoryHolder holder = other.GetComponentInParent<InventoryHolder>();
        if (holder == null)
            return;

        if (item == null)
        {
            Debug.LogWarning($"'{name}' has no item assigned", this);
            return;
        }

        int leftover = holder.Inventory.Add(item, quantity);

        // everything was picked up
        if (leftover == 0)
            Destroy(gameObject);

        // inventory full, keep whatever did not fit
        else
            quantity = leftover;
    }
}

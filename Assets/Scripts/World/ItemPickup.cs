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

        if (leftover == 0)
            Destroy(gameObject);   // everything was picked up
        else
            quantity = leftover;    // inventory full, no pickup
    }
}

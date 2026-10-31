using System;
using UnityEngine;
using UnityEngine.Events;

// a world container with its own inventory
// the items live on this component, not the UI, so they stay in the chest after the UI closes
// needs a collider (solid or trigger) so PlayerInteractor can find it
public class Chest : MonoBehaviour, IInteractable
{
    // chest slots
    [SerializeField, Min(1)] private int capacity = 12;

    // optional items placed in the chest when it is first used, handy for testing
    [SerializeField] private ItemAmount[] startingItems;

    // hook a lid animation or sound up here in the inspector
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;

    private Inventory inventory;

    // the toggle currently showing this chest, null when closed
    private InventoryToggle openedBy;

    public bool IsOpen => openedBy != null;

    // created on first use, same reason as InventoryHolder
    public Inventory Inventory
    {
        get
        {
            if (inventory == null)
            {
                inventory = new Inventory(capacity);
                AddStartingItems();
            }
            return inventory;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (interactor != null && interactor.Toggle != null)
            interactor.Toggle.OpenContainer(this);
    }

    // called by InventoryToggle when the chest UI opens / closes
    public void NotifyOpened(InventoryToggle toggle)
    {
        openedBy = toggle;
        onOpened?.Invoke();
    }

    public void NotifyClosed()
    {
        if (openedBy == null)
            return;

        openedBy = null;
        onClosed?.Invoke();
    }

    // chest destroyed or disabled while open, close the UI so it is not showing a missing chest
    // skipped while the scene unloads, the UI may already be destroyed by then
    private void OnDisable()
    {
        if (openedBy != null && gameObject.scene.isLoaded)
            openedBy.CloseContainer(this);
    }

    private void AddStartingItems()
    {
        if (startingItems == null)
            return;

        foreach (ItemAmount entry in startingItems)
        {
            if (entry.item == null || entry.quantity <= 0)
                continue;

            int leftover = inventory.Add(entry.item, entry.quantity);
            if (leftover > 0)
                Debug.LogWarning($"'{name}' is too small for its starting items, {leftover} {entry.item.DisplayName} did not fit", this);
        }
    }
}

// item + amount pair for filling chests in the inspector
[Serializable]
public struct ItemAmount
{
    public ItemDefinition item;
    public int quantity;
}

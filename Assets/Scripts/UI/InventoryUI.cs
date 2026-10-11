using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryHolder holder;

    [SerializeField] private SlotUI slotPrefab;

    /*
     Grid layouts to automatically arrange
    DOCS: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GridLayout.html
    */
    [SerializeField] private Transform slotContainer;

    private readonly List<SlotUI> slotViews = new List<SlotUI>();

    private Inventory subscribedInventory;

    private void OnEnable()
    {
        // safety check to not throw null references
        if (holder == null || slotPrefab == null || slotContainer == null)
        {
            Debug.LogError("Holders slot prefab or slot container is not assigned", this);
            enabled = false;
            return;
        }

        // only build the slots the first time
        if (slotViews.Count == 0)
            BuildSlots();

        subscribedInventory = holder.Inventory;
        subscribedInventory.Changed += Refresh;
        Refresh();
    }

    // MUST ALWAYS UNSUBSCRIBE! Otherwise inventory keeps
    // reference to UI after destroyed and keeps calling it
    private void OnDisable()
    {
        if (subscribedInventory != null)
        {
            subscribedInventory.Changed -= Refresh;
            subscribedInventory = null;
        }
    }

    // creates one slot for every inventory slot
    private void BuildSlots()
    {
        for (int i = 0; i < holder.Inventory.Capacity; i++)
        {
            SlotUI view = Instantiate(slotPrefab, slotContainer);
            view.name = $"Slot {i}";
            slotViews.Add(view);
        }
    }

    // when inventory changes, redraw every slot from its current contents
    private void Refresh()
    {
        for (int i = 0; i < slotViews.Count; i++)
            slotViews[i].Show(holder.Inventory.GetSlot(i));
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// draws one inventory as a grid of slots
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryHolder holder;

    [SerializeField] private SlotUI slotPrefab;

    /*
     Grid layouts to automatically arrange
    DOCS: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GridLayout.html
    */
    [SerializeField] private Transform slotContainer;

    // shared by every inventory panel so items can be dragged between them
    [SerializeField] private InventoryDragController dragController;

    private readonly List<SlotUI> slotViews = new List<SlotUI>();

    // the inventory this panel shows
    private Inventory inventory;

    // the inventory this panel is currently listening to
    private Inventory subscribedInventory;

    public Inventory Inventory => inventory;

    private void OnEnable()
    {
        // safety check to not throw null references
        if (slotPrefab == null || slotContainer == null || dragController == null)
        {
            Debug.LogError("Slot prefab, slot container or drag controller is not assigned", this);
            enabled = false;
            return;
        }

        if (inventory == null && holder != null)
            inventory = holder.Inventory;

        Subscribe();
    }

    // MUST ALWAYS UNSUBSCRIBE! otherwise inventory keeps
    // reference to UI after destroyed and keeps calling it
    private void OnDisable()
    {
        // panel closed mid drag, put everything back
        if (dragController != null)
            dragController.CancelDrag();

        Unsubscribe();
    }

    // point this panel at a different inventory
    public void Bind(Inventory newInventory)
    {
        if (newInventory == inventory)
            return;

        Unsubscribe();
        inventory = newInventory;

        if (isActiveAndEnabled)
            Subscribe();
    }

    private void Subscribe()
    {
        if (inventory == null)
        {
            BuildSlots(0);
            return;
        }

        BuildSlots(inventory.Capacity);

        subscribedInventory = inventory;
        subscribedInventory.Changed += Refresh;
        Refresh();
    }

    private void Unsubscribe()
    {
        if (subscribedInventory != null)
        {
            subscribedInventory.Changed -= Refresh;
            subscribedInventory = null;
        }
    }

    // makes sure there is exactly one slot view per inventory slot
    // chests can be different sizes, so slots are rebuilt when the size changes
    private void BuildSlots(int count)
    {
        if (slotViews.Count == count)
            return;

        foreach (SlotUI old in slotViews)
        {
            // hide first, Destroy only happens at the end of the frame
            old.gameObject.SetActive(false);
            Destroy(old.gameObject);
        }
        slotViews.Clear();

        for (int i = 0; i < count; i++)
        {
            SlotUI view = Instantiate(slotPrefab, slotContainer);
            view.name = $"Slot {i}";
            view.Init(this, i);
            slotViews.Add(view);
        }
    }

    // when inventory changes, redraw every slot from its current contents
    private void Refresh()
    {
        if (inventory == null)
            return;

        for (int i = 0; i < slotViews.Count; i++)
            slotViews[i].Show(inventory.GetSlot(i));
    }

    // fade for one slot icon, called by drag controller
    public void SetSlotDragSource(int index, bool isSource)
    {
        if (index >= 0 && index < slotViews.Count)
            slotViews[index].SetDragSource(isSource);
    }

    // forward for drag events
    public void BeginDrag(int index, PointerEventData eventData) => dragController.BeginDrag(this, index, eventData);

    public void UpdateDrag(PointerEventData eventData) => dragController.UpdateDrag(eventData);

    public void DropOnSlot(int targetIndex) => dragController.DropOn(this, targetIndex);

    public void EndDrag(PointerEventData eventData) => dragController.EndDrag(eventData);
}

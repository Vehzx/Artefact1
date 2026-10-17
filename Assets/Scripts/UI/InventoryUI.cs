using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryHolder holder;

    [SerializeField] private SlotUI slotPrefab;

    /*
     Grid layouts to automatically arrange
    DOCS: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GridLayout.html
    */
    [SerializeField] private Transform slotContainer;

    // icon that follows the cursor while dragging
    [SerializeField] private Image dragGhost;

    private readonly List<SlotUI> slotViews = new List<SlotUI>();

    private Inventory subscribedInventory;
    private RectTransform canvasRect;

    // -1 when nothing is being dragged
    private int dragFromIndex = -1;
    private bool dropHandled;

    public bool IsDragging => dragFromIndex >= 0;

    private void OnEnable()
    {
        // safety check to not throw null references
        if (holder == null || slotPrefab == null || slotContainer == null || dragGhost == null)
        {
            Debug.LogError("Holder, slot prefab, slot container or drag ghost is not assigned", this);
            enabled = false;
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        canvasRect = (RectTransform)canvas.rootCanvas.transform;

        // ghost must never block raycasts, otherwise OnDrop hits the ghost instead of the slot
        dragGhost.raycastTarget = false;
        dragGhost.gameObject.SetActive(false);

        // only build the slots the first time
        if (slotViews.Count == 0)
            BuildSlots();

        subscribedInventory = holder.Inventory;
        subscribedInventory.Changed += Refresh;
        Refresh();
    }

    // MUST ALWAYS UNSUBSCRIBE! otherwise inventory keeps
    // reference to UI after destroyed and keeps calling it
    private void OnDisable()
    {
        // inventory closed mid drag, put everything back
        CancelDrag();

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
            view.Init(this, i);
            slotViews.Add(view);
        }
    }

    // when inventory changes, redraw every slot from its current contents
    private void Refresh()
    {
        for (int i = 0; i < slotViews.Count; i++)
            slotViews[i].Show(holder.Inventory.GetSlot(i));
    }

    public void BeginDrag(int index, PointerEventData eventData)
    {
        // nothing in slot
        ItemStack stack = holder.Inventory.GetSlot(index);
        if (stack == null)
            return;

        // remember the slot the drag starts from
        dragFromIndex = index;
        dropHandled = false;

        dragGhost.sprite = stack.Item.Icon;
        dragGhost.gameObject.SetActive(true);

        // draw on top of everything else in its parent
        dragGhost.transform.SetAsLastSibling();

        // fade icon to see what slot drag starts from
        slotViews[index].SetDragSource(true);

        // snap to cursor instantly (fixes ghost suddenly teleporting to cursor)
        MoveGhost(eventData);
    }

    public void UpdateDrag(PointerEventData eventData)
    {
        if (IsDragging)
            MoveGhost(eventData);
    }

    // a slot received the drop
    public void DropOnSlot(int targetIndex)
    {
        if (!IsDragging)
            return;

        dropHandled = true;
        holder.Inventory.MoveOrSwap(dragFromIndex, targetIndex);
    }

    // drag finished, runs after DropOnSlot if there was one
    public void EndDrag(PointerEventData eventData)
    {
        if (!IsDragging)
            return;

        int from = dragFromIndex;
        bool handled = dropHandled;

        // releasing over the panel background cancels the drag
        bool releasedOverUI = eventData.pointerCurrentRaycast.gameObject != null;

        CancelDrag();

        if (!handled && !releasedOverUI)
            holder.DropSlot(from);
    }

    private void CancelDrag()
    {
        if (!IsDragging)
            return;

        slotViews[dragFromIndex].SetDragSource(false);
        dragGhost.gameObject.SetActive(false);
        dragFromIndex = -1;
        dropHandled = false;
    }

    // works for both render modes, screen space overlay and camera
    /*
    DOCS: https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.RectTransformUtility.html
    */
    private void MoveGhost(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint))
        {
            dragGhost.rectTransform.position = worldPoint;
        }
    }
}

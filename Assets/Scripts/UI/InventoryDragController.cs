using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// owns the drag state for EVERY inventory panel (player and chest)
// one shared controller is what lets an item be dragged out of one panel and dropped into another
// put this on the Canvas
public class InventoryDragController : MonoBehaviour
{
    // icon that follows the cursor while dragging
    // must be OUTSIDE any slot container (grid layout would move it)
    // raycast target is turned off in Awake so the ghost never blocks OnDrop on the slot underneath
    [SerializeField] private Image dragGhost;

    // items released over the game world spawn in front of this player
    [SerializeField] private InventoryHolder playerHolder;

    private RectTransform canvasRect;

    // where the drag started, -1 when nothing is being dragged
    private InventoryUI sourceView;
    private Inventory sourceInventory;
    private int sourceIndex = -1;
    private bool dropHandled;

    public bool IsDragging => sourceIndex >= 0;

    private void Awake()
    {
        if (dragGhost == null)
        {
            Debug.LogError("Drag ghost is not assigned", this);
            enabled = false;
            return;
        }

        // true includes inactive objects, the ghost may start switched off
        Canvas canvas = dragGhost.GetComponentInParent<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogError("Drag ghost is not under a Canvas", this);
            enabled = false;
            return;
        }

        canvasRect = (RectTransform)canvas.rootCanvas.transform;

        dragGhost.raycastTarget = false;
        dragGhost.gameObject.SetActive(false);
    }

    public void BeginDrag(InventoryUI view, int index, PointerEventData eventData)
    {
        if (!enabled || view == null || view.Inventory == null)
            return;

        // nothing in slot
        ItemStack stack = view.Inventory.GetSlot(index);
        if (stack == null)
            return;

        // remember where the drag starts from
        // inventory is stored too, in case the view gets bound to something else mid drag
        sourceView = view;
        sourceInventory = view.Inventory;
        sourceIndex = index;
        dropHandled = false;

        dragGhost.sprite = stack.Item.Icon;
        dragGhost.gameObject.SetActive(true);

        // draw on top of everything else in its parent
        dragGhost.transform.SetAsLastSibling();

        // fade icon to see what slot drag starts from
        view.SetSlotDragSource(index, true);

        // snap to cursor instantly (fixes ghost suddenly teleporting to cursor)
        MoveGhost(eventData);
    }

    public void UpdateDrag(PointerEventData eventData)
    {
        if (IsDragging)
            MoveGhost(eventData);
    }

    // a slot in any panel received the drop, can be the same panel or a different one
    public void DropOn(InventoryUI targetView, int targetIndex)
    {
        if (!IsDragging || targetView == null || targetView.Inventory == null)
            return;

        dropHandled = true;
        Inventory.Move(sourceInventory, sourceIndex, targetView.Inventory, targetIndex);
    }

    // drag finished, runs after DropOn if there was one
    public void EndDrag(PointerEventData eventData)
    {
        if (!IsDragging)
            return;

        Inventory from = sourceInventory;
        int fromIndex = sourceIndex;
        bool handled = dropHandled;

        // releasing over the panel background cancels the drag
        bool releasedOverUI = eventData.pointerCurrentRaycast.gameObject != null;

        CancelDrag();

        if (handled || releasedOverUI)
            return;

        if (playerHolder != null)
            playerHolder.DropFromInventory(from, fromIndex);
        else
            Debug.LogWarning("No player holder assigned, cannot drop items into the world", this);
    }

    // put everything back, also used when a panel closes mid drag
    public void CancelDrag()
    {
        if (!IsDragging)
            return;

        if (sourceView != null)
            sourceView.SetSlotDragSource(sourceIndex, false);

        if (dragGhost != null)
            dragGhost.gameObject.SetActive(false);

        sourceView = null;
        sourceInventory = null;
        sourceIndex = -1;
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

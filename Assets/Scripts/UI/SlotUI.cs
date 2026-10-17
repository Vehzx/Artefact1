using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// drag events come from the EventSystem, the slot just forwards them to InventoryUI
/*
DOCS: https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/SupportedEvents.html
*/
public class SlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;

    // icon fade amount for the slot being dragged from
    [SerializeField, Range(0f, 1f)] private float dragSourceAlpha = 0.35f;

    private InventoryUI owner;
    private int index = -1;
    private bool hasItem;

    public int Index => index;

    // called on slot creation
    public void Init(InventoryUI owner, int index)
    {
        this.owner = owner;
        this.index = index;
    }

    public void Show(ItemStack stack)
    {
        hasItem = stack != null;

        if (stack == null)
        {
            icon.enabled = false;
            icon.sprite = null;
            quantityText.text = string.Empty;
            return;
        }

        icon.enabled = true;
        icon.sprite = stack.Item.Icon;

        // only show a number for stacks above one
        quantityText.text = stack.Quantity > 1 ? stack.Quantity.ToString() : string.Empty;
    }

    // fade icon while being dragged
    public void SetDragSource(bool isSource)
    {
        Color colour = icon.color;
        colour.a = isSource ? dragSourceAlpha : 1f;
        icon.color = colour;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // nothing to drag
        // clearing pointerDrag stops below functions from firing
        if (owner == null || !hasItem || eventData.button != PointerEventData.InputButton.Left)
        {
            eventData.pointerDrag = null;
            return;
        }

        owner.BeginDrag(index, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (owner != null)
            owner.UpdateDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (owner != null)
            owner.EndDrag(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (owner != null)
            owner.DropOnSlot(index);
    }
}

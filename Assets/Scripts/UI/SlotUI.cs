using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;

    public void Show(ItemStack stack)
    {
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
}

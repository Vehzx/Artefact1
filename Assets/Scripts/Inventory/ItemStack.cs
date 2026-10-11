public class ItemStack
{
    public ItemDefinition Item { get; }

    // internal setter because only inventory changes quantity
    public int Quantity { get; internal set; }

    public bool IsFull => Quantity >= Item.MaxStack;
    public int SpaceRemaining => Item.MaxStack - Quantity;

    public ItemStack(ItemDefinition item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }
}

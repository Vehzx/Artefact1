using System;

public class Inventory
{
    private readonly ItemStack[] slots;

    // instead of checking inventory every frame, UI listens to this
    public event Action Changed;

    public int Capacity => slots.Length;

    public Inventory(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Inventory capacity minimum must be 1");

        slots = new ItemStack[capacity];
    }

    // return stack in a slot, null if empty
    public ItemStack GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return slots[index];
    }

    // add items, top up existing stacks then use empty slots
    // return how many cannot be added
    public int Add(ItemDefinition item, int amount)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (amount <= 0)
            return 0;

        int remaining = amount;

        // first pass, fill stacks that are not full
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            ItemStack stack = slots[i];
            if (stack != null && stack.Item == item && !stack.IsFull)
            {
                int toAdd = Math.Min(remaining, stack.SpaceRemaining);
                stack.Quantity += toAdd;
                remaining -= toAdd;
            }
        }

        // second pass, create new empty stacks
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i] == null)
            {
                int toAdd = Math.Min(remaining, item.MaxStack);
                slots[i] = new ItemStack(item, toAdd);
                remaining -= toAdd;
            }
        }

        if (remaining != amount)
            Changed?.Invoke();

        return remaining;
    }

    // remove up to an amount of items from a slot, return amount removed
    public int RemoveFromSlot(int index, int amount)
    {
        ItemStack stack = GetSlot(index);
        if (stack == null || amount <= 0)
            return 0;

        int removed = Math.Min(amount, stack.Quantity);
        stack.Quantity -= removed;

        if (stack.Quantity == 0)
            slots[index] = null;

        Changed?.Invoke();
        return removed;
    }

    // total number of a given item across all slots
    public int CountOf(ItemDefinition item)
    {
        int total = 0;
        foreach (ItemStack stack in slots)
        {
            if (stack != null && stack.Item == item)
                total += stack.Quantity;
        }
        return total;
    }

    public void Clear()
    {
        Array.Clear(slots, 0, slots.Length);
        Changed?.Invoke();
    }

    // conver to plain text so json utility can write it
    public InventorySaveData ToSaveData()
    {
        var data = new InventorySaveData();

        for (int i = 0; i < slots.Length; i++)
        {
            ItemStack stack = slots[i];
            if (stack == null)
                continue;

            data.slots.Add(new SlotSaveData
            {
                index = i,
                itemId = stack.Item.Id,
                quantity = stack.Quantity
            });
        }

        return data;
    }

    // replaces current contents with saved data
    // skip any invalid entries and return them
    public int LoadFrom(InventorySaveData data, ItemRegistry registry)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        if (registry == null)
            throw new ArgumentNullException(nameof(registry));

        Array.Clear(slots, 0, slots.Length);
        int skipped = 0;

        foreach (SlotSaveData saved in data.slots)
        {
            bool validIndex = saved.index >= 0 && saved.index < slots.Length;

            if (!validIndex || saved.quantity <= 0 || !registry.TryGet(saved.itemId, out ItemDefinition item))
            {
                skipped++;
                continue;
            }

            // Never trust a save file to respect the stack limit.
            int quantity = Math.Min(saved.quantity, item.MaxStack);
            slots[saved.index] = new ItemStack(item, quantity);
        }

        Changed?.Invoke();
        return skipped;
    }
}

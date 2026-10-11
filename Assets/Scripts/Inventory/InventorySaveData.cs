using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<SlotSaveData> slots = new List<SlotSaveData>();
}

[Serializable]
public class SlotSaveData
{
    public int index;
    public string itemId;
    public int quantity;
}

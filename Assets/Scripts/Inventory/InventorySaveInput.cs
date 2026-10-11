using UnityEngine;

public class InventorySaveInput : MonoBehaviour
{
    [SerializeField] private InventoryHolder holder;
    [SerializeField] private KeyCode saveKey = KeyCode.F5;
    [SerializeField] private KeyCode loadKey = KeyCode.F9;

    private void Update()
    {
        if (holder == null)
            return;

        if (Input.GetKeyDown(saveKey))
            holder.Save();

        if (Input.GetKeyDown(loadKey))
            holder.Load();
    }
}

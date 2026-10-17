using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

// opens and closes the inventory
// while open: cursor is free, camera and player are frozen, world keeps running
// put this on the player (PlayerArmature), NOT on the inventory panel, or it disables itself
public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs starterInputs;

    // name of the action added to StarterAssets.inputactions (Player map)
    [SerializeField] private string toggleActionName = "Inventory";

    // starter asset actions switched off while the inventory is open
    [SerializeField] private string[] actionsToBlock = { "Move", "Look", "Jump", "Sprint" };

    private InputAction toggleAction;

    public bool IsOpen { get; private set; }

    private void Reset()
    {
        playerInput = GetComponent<PlayerInput>();
        starterInputs = GetComponent<StarterAssetsInputs>();
    }

    // Start instead of OnEnable so PlayerInput has finished setting up its actions first
    private void Start()
    {
        if (inventoryPanel == null || playerInput == null || starterInputs == null)
        {
            Debug.LogError("Inventory panel, PlayerInput or StarterAssetsInputs is not assigned", this);
            enabled = false;
            return;
        }

        // look the action up on PlayerInput's own copy of the asset
        // an InputActionReference can point at a different copy and never fire
        toggleAction = playerInput.actions.FindAction(toggleActionName);
        if (toggleAction == null)
        {
            Debug.LogError($"No '{toggleActionName}' action found, add it to the Player map in StarterAssets.inputactions", this);
            enabled = false;
            return;
        }

        toggleAction.performed += OnTogglePerformed;
        SetOpen(false);
    }

    private void OnDestroy()
    {
        if (toggleAction != null)
            toggleAction.performed -= OnTogglePerformed;
    }

    private void OnTogglePerformed(InputAction.CallbackContext context) => SetOpen(!IsOpen);

    public void SetOpen(bool open)
    {
        IsOpen = open;
        inventoryPanel.SetActive(open);

        // stop the actions firing, then clear whatever they last wrote
        // without clearing, the player keeps running if W was held when the inventory opened
        foreach (string actionName in actionsToBlock)
        {
            InputAction action = playerInput.actions.FindAction(actionName);
            if (action == null)
                continue;

            if (open)
                action.Disable();
            else
                action.Enable();
        }

        if (open)
        {
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
            starterInputs.jump = false;
            starterInputs.sprint = false;
        }

        // StarterAssetsInputs re applies cursorLocked in OnApplicationFocus,
        // so it has to be changed too or alt tabbing re locks the cursor
        starterInputs.cursorLocked = !open;
        starterInputs.cursorInputForLook = !open;

        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
    }
}

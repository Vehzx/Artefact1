using UnityEngine;
using UnityEngine.InputSystem;

// press interact to use closest IInteractable
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InventoryToggle inventoryToggle;

    [SerializeField] private string interactActionName = "Interact";

    [SerializeField, Min(0.1f)] private float interactRadius = 1.2f;

    // centre of check sphere
    [SerializeField] private Vector3 interactOffset = new Vector3(0f, 1f, 0.8f);

    // which layers can contain interactables
    [SerializeField] private LayerMask interactLayers = ~0;

    private InputAction interactAction;

    public InventoryToggle Toggle => inventoryToggle;

    private void Reset()
    {
        playerInput = GetComponent<PlayerInput>();
        inventoryToggle = GetComponent<InventoryToggle>();
    }

    // Start instead of OnEnable, let PlayerInput finish setup first
    private void Start()
    {
        if (playerInput == null || inventoryToggle == null)
        {
            Debug.LogError("PlayerInput or InventoryToggle is not assigned", this);
            enabled = false;
            return;
        }

        // look the action up on PlayerInput copy
        interactAction = playerInput.actions.FindAction(interactActionName);
        if (interactAction == null)
        {
            Debug.LogError($"No '{interactActionName}' action found, add it to the Player map in StarterAssets.inputactions", this);
            enabled = false;
            return;
        }

        interactAction.performed += OnInteractPerformed;
    }

    private void OnDestroy()
    {
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // a menu is already open, now close it
        if (inventoryToggle.IsOpen)
        {
            inventoryToggle.SetOpen(false);
            return;
        }

        IInteractable target = FindClosest();
        if (target != null)
            target.Interact(this);
    }

    /*
    DOCS: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.OverlapSphere.html
    */
    private IInteractable FindClosest()
    {
        Vector3 centre = transform.TransformPoint(interactOffset);

        // collide so chests work with both solid collider or trigger
        Collider[] hits = Physics.OverlapSphere(centre, interactRadius, interactLayers, QueryTriggerInteraction.Collide);

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            // collider on child of the chest
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null)
                continue;

            // interfaces have no transform, cast back to component to get one
            float distance = (((Component)interactable).transform.position - centre).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

#if UNITY_EDITOR
    // shows interact range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(interactOffset), interactRadius);
    }
#endif
}

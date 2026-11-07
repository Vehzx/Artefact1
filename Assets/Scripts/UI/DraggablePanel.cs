using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    // the panel that moves, leave empty to move it
    [SerializeField] private RectTransform target;

    // stop the panel being dragged off screen
    [SerializeField] private bool keepOnScreen = true;

    // bring the panel in front of other panels when clicked
    [SerializeField] private bool bringToFrontOnClick = true;

    private Canvas canvas;
    private RectTransform canvasRect;
    private bool dragging;

    // reused every frame instead of creating new arrays
    private readonly Vector3[] targetCorners = new Vector3[4];
    private readonly Vector3[] canvasCorners = new Vector3[4];

    private void Awake()
    {
        if (target == null)
            target = (RectTransform)transform;

        canvas = GetComponentInParent<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogError("DraggablePanel isnt under a canvas", this);
            enabled = false;
            return;
        }

        canvas = canvas.rootCanvas;
        canvasRect = (RectTransform)canvas.transform;
    }

    // stop a drag half way if the panel is closed
    private void OnDisable() => dragging = false;

    // clicking anywhere on the panel brings it to the front
    public void OnPointerDown(PointerEventData eventData)
    {
        if (bringToFrontOnClick && eventData.button == PointerEventData.InputButton.Left)
            target.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = enabled && eventData.button == PointerEventData.InputButton.Left;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;

        // delta = how far mouse moved in screen pixels
        // have to divide by scaleFactor so panel stays under mouse
        // if Canvas Scaler resizes for different resolution
        target.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (keepOnScreen)
            ClampToCanvas();
    }

    // if any edge of the panel is outside the canvas, push it back in
    private void ClampToCanvas()
    {
        target.GetWorldCorners(targetCorners);
        canvasRect.GetWorldCorners(canvasCorners);

        // corners = bottom left [0], top left [1], top right [2], bottom right [3]
        Vector3 min = targetCorners[0];
        Vector3 max = targetCorners[2];
        Vector3 canvasMin = canvasCorners[0];
        Vector3 canvasMax = canvasCorners[2];

        Vector3 offset = Vector3.zero;

        if (min.x < canvasMin.x)
            offset.x = canvasMin.x - min.x;
        else if (max.x > canvasMax.x)
            offset.x = canvasMax.x - max.x;

        if (min.y < canvasMin.y)
            offset.y = canvasMin.y - min.y;
        else if (max.y > canvasMax.y)
            offset.y = canvasMax.y - max.y;

        target.position += offset;
    }
}

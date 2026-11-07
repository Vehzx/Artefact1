using System.Collections;
using TMPro;
using UnityEngine;

// drop blocked message that shows then fades out
[RequireComponent(typeof(CanvasGroup))]
public class ScreenMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    // optional, show this player's "drop blocked" messages automatically
    [SerializeField] private InventoryHolder holder;

    // how long the message stays fully visible
    [SerializeField, Min(0f)] private float displayTime = 1.5f;

    // how long it takes to fade out afterwards
    [SerializeField, Min(0f)] private float fadeTime = 0.5f;

    private CanvasGroup canvasGroup;
    private Coroutine showRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // IMPORTANT, the message must never block mouse raycasts
        // otherwise releasing a drag over it counts as over UI and cancels world drops
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // hidden until something is shown
        // alpha 0, not SetActive(false), lets the coroutine still run
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (holder != null)
            holder.DropBlocked += Show;
    }

    private void OnDisable()
    {
        if (holder != null)
            holder.DropBlocked -= Show;

        // when the object disables the coroutine stops, make sure text does not stay on the screen
        showRoutine = null;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    // public so anything else can show a message too
    public void Show(string message)
    {
        if (messageText == null)
        {
            Debug.LogWarning("ScreenMessage has no text assigned", this);
            return;
        }

        messageText.text = message;

        // restart the timer if a message is already showing (for spamming drops)
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowThenFade());
    }

    private IEnumerator ShowThenFade()
    {
        canvasGroup.alpha = 1f;

        // unscaled time incase the game is paused
        yield return new WaitForSecondsRealtime(displayTime);

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        showRoutine = null;
    }
}

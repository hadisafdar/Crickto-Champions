using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// This script allows a ScrollView to snap smoothly to the nearest child when scrolling stops.
/// </summary>
public class ScrollViewSnap : MonoBehaviour
{
    public ScrollRect scrollRect; // The Scroll View component
    public RectTransform content; // The Content panel inside the Scroll View
    public float snapDuration = 0.3f; // Time taken to snap to the nearest mode
    public float snapThreshold = 0.1f; // How much movement is considered "scrolling"

    private RectTransform[] modeOptions; // Stores all child elements
    private float[] positions; // Normalized positions of each mode
    private int targetIndex = 0; // Current target selection

    private bool isSnapping = false;

    private void Start()
    {
        InitializeModes();
    }

    /// <summary>
    /// Initializes mode positions inside the scroll view.
    /// </summary>
    private void InitializeModes()
    {
        int childCount = content.childCount;
        modeOptions = new RectTransform[childCount];
        positions = new float[childCount];

        for (int i = 0; i < childCount; i++)
        {
            modeOptions[i] = content.GetChild(i).GetComponent<RectTransform>();
            positions[i] = i / (float)(childCount - 1); // Normalized positions (0 to 1)
        }
        SnapToTarget();
    }

    /// <summary>
    /// Checks when scrolling stops and snaps to the closest mode.
    /// </summary>
    public void OnScrollChanged()
    {
        if (isSnapping) return;

        float closestPosition = float.MaxValue;
        int closestIndex = targetIndex;

        for (int i = 0; i < positions.Length; i++)
        {
            float distance = Mathf.Abs(scrollRect.horizontalNormalizedPosition - positions[i]);
            if (distance < closestPosition)
            {
                closestPosition = distance;
                closestIndex = i;
            }
        }

        targetIndex = closestIndex;
        SnapToTarget();
    }

    /// <summary>
    /// Snaps the ScrollView to the selected mode.
    /// </summary>
    private void SnapToTarget()
    {
        isSnapping = true;
        scrollRect.DOKill(); // Stop any ongoing tweens

        scrollRect.DONormalizedPos(new Vector2(positions[targetIndex], 0), snapDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => isSnapping = false);
    }

    public void ScrollLeft()
    {
        if (targetIndex > 0)
        {
            targetIndex--;
            SnapToTarget();
            AudioManager.instance.Play("Click");
        }
    }

    public void ScrollRight()
    {
        if (targetIndex < positions.Length - 1)
        {
            targetIndex++;
            SnapToTarget();
            AudioManager.instance.Play("Click");
        }
    }
}

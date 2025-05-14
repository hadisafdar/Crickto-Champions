using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
#endif
public class AnchorToCorners : MonoBehaviour
{
    public void SetAnchorsToCorners()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform parentRectTransform = transform.parent as RectTransform;

        if (parentRectTransform == null)
        {
            Debug.LogWarning("The UI element needs to have a RectTransform parent.");
            return;
        }

        Vector2 newAnchorMin = new Vector2(
            rectTransform.anchorMin.x + rectTransform.offsetMin.x / parentRectTransform.rect.width,
            rectTransform.anchorMin.y + rectTransform.offsetMin.y / parentRectTransform.rect.height);

        Vector2 newAnchorMax = new Vector2(
            rectTransform.anchorMax.x + rectTransform.offsetMax.x / parentRectTransform.rect.width,
            rectTransform.anchorMax.y + rectTransform.offsetMax.y / parentRectTransform.rect.height);

        rectTransform.anchorMin = newAnchorMin;
        rectTransform.anchorMax = newAnchorMax;

        rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;
    }
}
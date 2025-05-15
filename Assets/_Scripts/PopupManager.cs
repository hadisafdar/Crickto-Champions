using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Canvas")]
    public Canvas popupCanvas;

    [Header("Confirmation Popup")]
    public GameObject confirmationPopup;
    public TextMeshProUGUI confirmationText;
    public Button yesButton;
    public Button noButton;

    [Header("Notification Popup")]
    public GameObject notificationPopup;
    public TextMeshProUGUI notificationText;
    public Button okButton;

    [Header("Custom Popup")]
    public GameObject customPopup;
    public TextMeshProUGUI customText;
    public Button customFirstButton;
    public Button customSecondButton;
    public Button closeButton;

    [Header("Animated Text Popup")]
    public GameObject animatedTextPopup;
    public TextMeshProUGUI animatedTextMessage;
    public TextMeshProUGUI animatedDotsText;
    private bool isAnimatingDots = false;

    [Header("Loading Bar Popup")]
    public GameObject loadingBarPopup;       // The popup panel that appears at the bottom
    public RectTransform loadingIcon;        // The loading icon that rotates
    public TextMeshProUGUI loadingBarText;     // The text inside the loading popup

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public Vector3 popupScale = Vector3.one;
    public Ease animationEase = Ease.OutBack;

    // Internal variable to store the final anchored position for the loading popup
    private Vector2 loadingBarFinalPos;
    private RectTransform loadingBarRect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        popupCanvas.gameObject.SetActive(false); // Ensure the canvas starts disabled

        // Cache the RectTransform for the loading popup and record its final position.
        if (loadingBarPopup != null)
        {
            loadingBarRect = loadingBarPopup.GetComponent<RectTransform>();
            loadingBarFinalPos = loadingBarRect.anchoredPosition;
        }
    }

    private void EnableCanvas()
    {
        popupCanvas.gameObject.SetActive(true);
    }

    private void DisableCanvas()
    {
        // Disable the canvas only if none of the popups are active.
        if (!confirmationPopup.activeSelf &&
            !notificationPopup.activeSelf &&
            !customPopup.activeSelf &&
            !animatedTextPopup.activeSelf &&
            !loadingBarPopup.activeSelf)
        {
            popupCanvas.gameObject.SetActive(false);
        }
    }

    // Core Popup Animations (for popups that use scaling)

    private void ShowPopup(GameObject popup)
    {
        EnableCanvas();
        popup.SetActive(true);
        popup.transform.localScale = Vector3.zero;
        popup.transform.DOScale(popupScale, animationDuration).SetEase(animationEase);
    }

    private void HidePopup(GameObject popup, Action onCloseCallback = null)
    {
        popup.transform.DOScale(0, animationDuration).SetEase(animationEase).OnComplete(() =>
        {
            popup.SetActive(false);
            DisableCanvas();
            onCloseCallback?.Invoke();
        });
    }

    // Animated Text Popup Methods

    public void ShowTextPopup(string message)
    {
        animatedTextMessage.text = message;
        animatedDotsText.text = "";
        isAnimatingDots = true;

        ShowPopup(animatedTextPopup);
        StartCoroutine(AnimateDots());
    }

    private IEnumerator AnimateDots()
    {
        string[] dots = { "", ".", "..", "..." };
        int index = 0;

        while (isAnimatingDots)
        {
            animatedDotsText.text = dots[index];
            index = (index + 1) % dots.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void CloseTextPopup(string closingMessage = null, Action onCloseCallback = null)
    {
        if (closingMessage != null)
        {
            StartCoroutine(ShowClosingMessageThenClose(closingMessage, onCloseCallback));
        }
        else
        {
            isAnimatingDots = false;
            HidePopup(animatedTextPopup, onCloseCallback);
        }
    }

    private IEnumerator ShowClosingMessageThenClose(string closingMessage, Action onCloseCallback)
    {
        isAnimatingDots = false;
        animatedTextMessage.text = closingMessage;
        animatedDotsText.text = "";
        yield return new WaitForSeconds(1.5f);
        HidePopup(animatedTextPopup, onCloseCallback);
    }

    // Confirmation Popup

    public void ShowConfirmationPopup(string message, Action onYesAction)
    {
        confirmationText.text = message;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            onYesAction?.Invoke();
            HidePopup(confirmationPopup);
        });

        noButton.onClick.AddListener(() =>
        {
            HidePopup(confirmationPopup);
        });

        ShowPopup(confirmationPopup);
    }

    // Notification Popup

    public void ShowNotificationPopup(string message)
    {
        notificationText.text = message;

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() =>
        {
            HidePopup(notificationPopup);
        });

        ShowPopup(notificationPopup);
    }

    // Custom Popup (Two-Button Version)

    public void ShowCustomPopup(string message, string firstButtonName, Action firstButtonAction, string secondButtonName, Action secondButtonAction)
    {
        customText.text = message;

        customFirstButton.onClick.RemoveAllListeners();
        customSecondButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        customFirstButton.gameObject.SetActive(true);
        customSecondButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);

        customFirstButton.onClick.AddListener(() =>
        {
            firstButtonAction?.Invoke();
            HidePopup(customPopup);
        });

        customSecondButton.onClick.AddListener(() =>
        {
            secondButtonAction?.Invoke();
            HidePopup(customPopup);
        });

        closeButton.onClick.AddListener(() =>
        {
            HidePopup(customPopup);
        });

        customFirstButton.GetComponentInChildren<TextMeshProUGUI>().text = firstButtonName;
        customSecondButton.GetComponentInChildren<TextMeshProUGUI>().text = secondButtonName;

        ShowPopup(customPopup);
    }

    // Custom Popup (Single-Button Version)

    public void ShowCustomPopup(string message, string buttonName, Action buttonFunction)
    {
        customText.text = message;

        customFirstButton.onClick.RemoveAllListeners();
        customSecondButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        // For a single-button popup, hide one of the buttons and disable the close button.
        customSecondButton.onClick.AddListener(() =>
        {
            buttonFunction?.Invoke();
            HidePopup(customPopup);
        });

        closeButton.onClick.AddListener(() =>
        {
            HidePopup(customPopup);
        });
        closeButton.gameObject.SetActive(false);
        customFirstButton.gameObject.SetActive(false);
        customSecondButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonName;

        ShowPopup(customPopup);
    }

    // New Loading Bar Popup Methods (Slide-Up from Bottom with Rotating Icon and Text)

    /// <summary>
    /// Shows the loading popup. It will slide up from the bottom of the screen.
    /// </summary>
    public void ShowLoadingBar()
    {
        if (loadingBarPopup == null || loadingBarRect == null)
            return;

        EnableCanvas();
        loadingBarPopup.SetActive(true);

        // Move the popup off-screen (below) and then slide it up to its final position.
        loadingBarRect.anchoredPosition = new Vector2(loadingBarFinalPos.x, -Screen.height);
        loadingBarRect.DOAnchorPos(loadingBarFinalPos, animationDuration).SetEase(Ease.InOutSine);

        // Optionally set a default loading text.
        if (loadingBarText != null)
        {
            loadingBarText.text = "Loading...";
        }

        // Start the rotation animation on the loading icon.
        if (loadingIcon != null)
        {
            loadingIcon.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
                       .SetEase(Ease.Linear)
                       .SetLoops(-1, LoopType.Restart);
        }
    }

    /// <summary>
    /// Updates the loading popup text.
    /// </summary>
    /// <param name="text">The new text to display.</param>
    public void UpdateLoadingBarText(string text)
    {
        if (loadingBarText != null)
        {
            loadingBarText.text = text;
        }
    }

    /// <summary>
    /// Hides the loading popup by sliding it back down off-screen.
    /// </summary>
    public void HideLoadingBar(Action onCloseCallback = null)
    {
        if (loadingBarPopup == null || loadingBarRect == null)
            return;

        // Stop the rotation tween on the loading icon.
        if (loadingIcon != null)
        {
            loadingIcon.DOKill();
        }

        // Slide the popup down off-screen.
        Vector2 offScreenPos = new Vector2(loadingBarFinalPos.x, -Screen.height);
        loadingBarRect.DOAnchorPos(offScreenPos, animationDuration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            loadingBarPopup.SetActive(false);
            DisableCanvas();
            onCloseCallback?.Invoke();
        });
    }
}

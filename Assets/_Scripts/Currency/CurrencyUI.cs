using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [Header("Currency Settings")]
    [SerializeField] private CurrencyManager.CurrencyType currencyType;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI currencyText;

    private void Awake()
    {
        // Auto-grab the Text component if you forgot to hook it up
        if (currencyText == null)
            currencyText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Subscribe to the new OnCurrencyChanged event
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            // Initialize immediately
            UpdateCurrencyDisplay(CurrencyManager.Instance.Get(currencyType));
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    private void HandleCurrencyChanged(CurrencyManager.CurrencyType type, int newAmount)
    {
        if (type == currencyType)
            UpdateCurrencyDisplay(newAmount);
    }

    private void UpdateCurrencyDisplay(int amount)
    {
        currencyText.text = amount.ToString();
    }
}

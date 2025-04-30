using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [Header("Currency Settings")]
    [SerializeField] private CurrencyManager.CurrencyType currencyType; // The type of currency this UI displays

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI currencyText; // Text component to display the currency amount

    private void OnEnable()
    {
        // Subscribe to the currency update event
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyUpdated += UpdateCurrencyUI;
        }

        // Initialize UI with the current currency amount
        if (CurrencyManager.Instance != null)
        {
            UpdateCurrencyUI(currencyType, CurrencyManager.Instance.GetCurrencyAmount(currencyType));
        }
    }

    private void Start()
    {
        if(currencyText == null)
        {
            currencyText = GetComponent<TextMeshProUGUI>();
        }
    }
    private void OnDisable()
    {
        // Unsubscribe from the currency update event
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyUpdated -= UpdateCurrencyUI;
        }
    }

    private void UpdateCurrencyUI(CurrencyManager.CurrencyType type, int amount)
    {
        // Update the UI only if this is the relevant currency type
        if (type == currencyType)
        {
            currencyText.text = $"{amount}";
        }
    }
}

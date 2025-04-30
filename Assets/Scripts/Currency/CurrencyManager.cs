using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    // Enum for currency types
    public enum CurrencyType
    {
        Coins,
        Diamonds
    }

    public event Action<CurrencyType, int> OnCurrencyUpdated; // Event for UI updates

    [Serializable]
    public class Currency
    {
        public CurrencyType Type;
        public int Amount;

        public Currency(CurrencyType type, int initialAmount)
        {
            Type = type;
            Amount = initialAmount;
        }
    }

    private Dictionary<CurrencyType, Currency> currencies = new Dictionary<CurrencyType, Currency>();

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
    }

    private void Start()
    {
        Initialize();
    }

    // Initialize with default currencies or load saved values
    public void Initialize()
    {
        LoadCurrencies(); // Load currencies from storage
        //AddCurrencyType(CurrencyType.Primary, GetCurrencyAmount(CurrencyType.Primary) > 0 ? GetCurrencyAmount(CurrencyType.Primary) : 10000);
            
    }

    // Add a new currency type
    public void AddCurrencyType(CurrencyType type, int initialAmount)
    {
        if (!currencies.ContainsKey(type))
        {
            currencies[type] = new Currency(type, initialAmount);
            OnCurrencyUpdated?.Invoke(type, initialAmount);
        }
        else
        {
            Debug.LogWarning($"Currency {type} already exists!");
        }
    }

    // Add currency
    public bool AddCurrency(CurrencyType type, int amount)
    {
        if (currencies.ContainsKey(type))
        {
            currencies[type].Amount += amount;
            SaveCurrency(type); // Save updated currency
            OnCurrencyUpdated?.Invoke(type, currencies[type].Amount);
            return true;
        }
        else
        {
            Debug.LogError($"Currency {type} not found!");
            return false;
        }
    }

    // Add currency with a callback
    public bool AddCurrency(CurrencyType type, int amount, Action callback)
    {
        if (AddCurrency(type, amount))
        {
            callback?.Invoke();
            return true;
        }
        return false;
    }

    // Subtract currency
    public bool SubtractCurrency(CurrencyType type, int amount)
    {
        if (currencies.ContainsKey(type) && currencies[type].Amount >= amount)
        {
            currencies[type].Amount -= amount;
            SaveCurrency(type); // Save updated currency
            OnCurrencyUpdated?.Invoke(type, currencies[type].Amount);
            return true;
        }
        else
        {
            Debug.LogError($"Not enough {type} currency or currency not found!");
            return false;
        }
    }

    // Subtract currency with a callback
    public bool SubtractCurrency(CurrencyType type, int amount, Action callback)
    {
        if (SubtractCurrency(type, amount))
        {
            callback?.Invoke();
            return true;
        }
        return false;
    }

    // Get the current amount of a currency
    public int GetCurrencyAmount(CurrencyType type)
    {
        if (currencies.ContainsKey(type))
        {
            return currencies[type].Amount;
        }
        else
        {
            Debug.LogError($"Currency {type} not found!");
            return 0;
        }
    }

    // Save a specific currency to PlayerPrefs
    private void SaveCurrency(CurrencyType type)
    {
        if (currencies.ContainsKey(type))
        {
            PlayerPrefs.SetInt(type.ToString(), currencies[type].Amount);
            PlayerPrefs.Save();
        }
    }

    // Load currencies from PlayerPrefs
    private void LoadCurrencies()
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            if (PlayerPrefs.HasKey(type.ToString()))
            {
                int amount = PlayerPrefs.GetInt(type.ToString());
                currencies[type] = new Currency(type, amount);
            }
        }
    }

    // Clear saved data for testing or reset purposes
    public void ClearCurrencyData()
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            PlayerPrefs.DeleteKey(type.ToString());
        }
        PlayerPrefs.Save();
    }
}

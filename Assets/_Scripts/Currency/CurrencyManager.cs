using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    // 1) Your enum of currencies
    public enum CurrencyType
    {
        Coins,
        Diamonds,
        // … add more here …
    }

    // 2) How you configure defaults in the Inspector
    [Serializable]
    public class CurrencyEntry
    {
        public CurrencyType type;
        [Min(0)]
        public int defaultAmount = 0;
    }

    [Header("Configure your currencies here")]
    [Tooltip("List each CurrencyType once, with its default starting value.")]
    public List<CurrencyEntry> currencyEntries;

    // 3) Storage interface & default implementation
    interface ICurrencyStorage
    {
        bool HasKey(string key);
        int Load(string key);
        void Save(string key, int amount);
        void Delete(string key);
    }

    class PlayerPrefsStorage : ICurrencyStorage
    {
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public int Load(string key) => PlayerPrefs.GetInt(key);
        public void Save(string key, int amt)
        {
            PlayerPrefs.SetInt(key, amt);
            PlayerPrefs.Save();
        }
        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }

    // 4) Internal data
    ICurrencyStorage storage;
    Dictionary<CurrencyType, int> amounts = new Dictionary<CurrencyType, int>();
    HashSet<CurrencyType> defined = new HashSet<CurrencyType>();

    /// <summary>Fires whenever any currency changes: (type, newAmount)</summary>
    public event Action<CurrencyType, int> OnCurrencyChanged;

    void Awake()
    {
        // singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // choose storage
        storage = new PlayerPrefsStorage();

        // validate & register definitions
        foreach (var entry in currencyEntries)
        {
            if (!defined.Add(entry.type))
            {
                Debug.LogError($"[CurrencyManager] Duplicate entry for {entry.type}");
                continue;
            }

            // load saved or use default
            int val = storage.HasKey(entry.type.ToString())
                      ? storage.Load(entry.type.ToString())
                      : entry.defaultAmount;

            amounts[entry.type] = val;
        }

        // warn if you forgot an enum
        foreach (CurrencyType t in Enum.GetValues(typeof(CurrencyType)))
        {
            if (!defined.Contains(t))
                Debug.LogWarning($"[CurrencyManager] You did not configure default for {t}");
        }
    }

    /// <summary>Add amount (must be >0). Fires OnCurrencyChanged.</summary>
    public void Add(CurrencyType type, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Add called with non-positive amount {amount} for {type}");
            return;
        }
        if (!amounts.ContainsKey(type))
        {
            Debug.LogError($"Add: {type} not defined");
            return;
        }

        amounts[type] += amount;
        storage.Save(type.ToString(), amounts[type]);
        OnCurrencyChanged?.Invoke(type, amounts[type]);
    }

    /// <summary>Try to subtract; returns false if not enough or undefined.</summary>
    public bool TrySubtract(CurrencyType type, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Subtract called with non-positive amount {amount} for {type}");
            return true;
        }
        if (!amounts.ContainsKey(type))
        {
            Debug.LogError($"Subtract: {type} not defined");
            return false;
        }
        if (amounts[type] < amount)
        {
            Debug.LogWarning($"Not enough {type} (have {amounts[type]}, need {amount})");
            return false;
        }

        amounts[type] -= amount;
        storage.Save(type.ToString(), amounts[type]);
        OnCurrencyChanged?.Invoke(type, amounts[type]);
        return true;
    }

    /// <summary>Get current amount (0 if undefined).</summary>
    public int Get(CurrencyType type)
    {
        if (!amounts.ContainsKey(type))
        {
            Debug.LogError($"Get: {type} not defined");
            return 0;
        }
        return amounts[type];
    }

    /// <summary>Resets a currency back to its configured default.</summary>
    public void Reset(CurrencyType type)
    {
        var entry = currencyEntries.Find(e => e.type == type);
        if (entry == null)
        {
            Debug.LogError($"Reset: no entry for {type}");
            return;
        }

        amounts[type] = entry.defaultAmount;
        storage.Save(type.ToString(), amounts[type]);
        OnCurrencyChanged?.Invoke(type, amounts[type]);
    }

    /// <summary>Deletes all saved data & restores all to defaults.</summary>
    public void ClearAll()
    {
        foreach (var entry in currencyEntries)
        {
            storage.Delete(entry.type.ToString());
            amounts[entry.type] = entry.defaultAmount;
            OnCurrencyChanged?.Invoke(entry.type, amounts[entry.type]);
        }
    }

    /// <summary>Returns true if this currency has been configured.</summary>
    public bool IsDefined(CurrencyType type) => defined.Contains(type);
}

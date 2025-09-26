using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class SoldierManager : MonoBehaviour
{
    public static SoldierManager Instance { get; private set; }

    [Header("Asker Türleri")]
    public List<SoldierData> allSoldierTypes;

    public event Action<string, int> OnSoldierCountChanged;

    private Dictionary<string, int> soldierCounts = new Dictionary<string, int>();

    public long TotalHealth { get; private set; }
    public long TotalAttack { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSoldiers();
        }
    }

    // Metot artık string yerine SoldierData alıyor.
    public void AddSoldier(SoldierData soldierData, int amount)
    {
        string name = soldierData.soldierName;

        if (!soldierCounts.ContainsKey(name))
        {
            soldierCounts[name] = 0;
        }
        soldierCounts[name] += amount;
        SetSoldierCount(name, soldierCounts[name]);
    }

    private void SetSoldierCount(string name, int count)
    {
        soldierCounts[name] = count;
        PlayerPrefs.SetInt("SoldierCount_" + name, count);
        PlayerPrefs.Save();
        RecalculateArmyStats();

        OnSoldierCountChanged?.Invoke(name, count);
    }

    public int GetSoldierCount(string name)
    {
        return soldierCounts.ContainsKey(name) ? soldierCounts[name] : 0;
    }

    public int GetTotalSoldierCount()
    {
        return soldierCounts.Values.Sum();
    }

    private void LoadSoldiers()
    {
        soldierCounts.Clear();
        foreach (var soldierData in allSoldierTypes)
        {
            int count = PlayerPrefs.GetInt("SoldierCount_" + soldierData.soldierName, 0);
            soldierCounts[soldierData.soldierName] = count;
        }
        RecalculateArmyStats();
    }

    private void RecalculateArmyStats()
    {
        TotalHealth = 0;
        TotalAttack = 0;

        foreach (var soldierData in allSoldierTypes)
        {
            int count = GetSoldierCount(soldierData.soldierName);
            if (count > 0)
            {
                TotalHealth += (long)soldierData.health * count;
                TotalAttack += (long)soldierData.attack * count;
            }
        }
    }
}
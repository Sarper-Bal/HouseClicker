using UnityEngine;
using System;
using System.Collections.Generic;

public class SoldierManager : MonoBehaviour
{
    public static SoldierManager Instance { get; private set; }

    [Header("Asker Veritabanı")]
    [Tooltip("Oyunda var olan TÜM asker türlerinin ScriptableObject'lerini buraya sürükleyin.")]
    public List<SoldierData> allSoldierTypes;

    public long TotalHealth { get; private set; }
    public long TotalAttack { get; private set; }

    private Dictionary<string, int> soldiersOwned = new Dictionary<string, int>();

    public event Action OnSoldierDataChanged;

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
            LoadSoldiers(); // Oyun açıldığında verileri yükle
        }
    }

    public void AddSoldier(SoldierData soldierData)
    {
        if (soldierData == null) return;

        string soldierName = soldierData.soldierName;

        if (!soldiersOwned.ContainsKey(soldierName))
        {
            soldiersOwned[soldierName] = 0;
        }

        soldiersOwned[soldierName]++;

        TotalHealth += soldierData.health;
        TotalAttack += soldierData.attack;

        Debug.Log($"{soldierName} orduya katıldı. Bu birimden mevcut: {soldiersOwned[soldierName]}");

        SaveSoldiers();
        OnSoldierDataChanged?.Invoke();
    }

    public int GetSoldierCount(string soldierName)
    {
        return soldiersOwned.ContainsKey(soldierName) ? soldiersOwned[soldierName] : 0;
    }

    public int GetTotalSoldierCount()
    {
        int total = 0;
        foreach (int count in soldiersOwned.Values)
        {
            total += count;
        }
        return total;
    }

    private void SaveSoldiers()
    {
        // Sözlükteki her bir askeri "Soldier_Piyade" gibi bir anahtarla kaydet
        foreach (var pair in soldiersOwned)
        {
            PlayerPrefs.SetInt("Soldier_" + pair.Key, pair.Value);
        }

        PlayerPrefs.SetString("TotalHealth", TotalHealth.ToString());
        PlayerPrefs.SetString("TotalAttack", TotalAttack.ToString());
        PlayerPrefs.Save(); // PlayerPrefs'in hemen kaydedildiğinden emin olmak için
    }

    private void LoadSoldiers()
    {
        // --- DÜZELTME BURADA ---
        // Yüklemeden önce mevcut verileri sıfırla
        soldiersOwned.Clear();
        TotalHealth = 0;
        TotalAttack = 0;

        // Tanımladığımız tüm asker türlerini döngüye al
        foreach (SoldierData soldierType in allSoldierTypes)
        {
            string key = "Soldier_" + soldierType.soldierName;

            // Bu asker türü için kayıtlı bir veri var mı diye kontrol et
            if (PlayerPrefs.HasKey(key))
            {
                int count = PlayerPrefs.GetInt(key);
                if (count > 0)
                {
                    // Kayıtlı askerleri ve istatistiklerini yükle
                    soldiersOwned[soldierType.soldierName] = count;
                    TotalHealth += (long)count * soldierType.health;
                    TotalAttack += (long)count * soldierType.attack;
                }
            }
        }

        // --- DÜZELTME SONU ---
        Debug.Log($"Asker verileri yüklendi. Toplam Asker: {GetTotalSoldierCount()}, Sağlık: {TotalHealth}, Saldırı: {TotalAttack}");
    }
}
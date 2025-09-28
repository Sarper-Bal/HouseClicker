using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance { get; private set; }

    [Header("Referanslar")]
    [SerializeField] private MapUIController uiController;
    [Tooltip("Oyuncunun haritadaki başlangıç kalesini buraya sürükleyin.")]
    [SerializeField] private Castle playerBase;
    [SerializeField] private List<SoldierData> allSoldierTypes;

    [Header("Level Verileri")]
    [Tooltip("Tüm LevelData ScriptableObject'lerini buraya sürükleyin.")]
    public List<LevelData> levelConfigs;

    private Castle selectedCastle;
    private List<Castle> allCastlesOnMap;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    private void Start()
    {
        allCastlesOnMap = FindObjectsOfType<Castle>().ToList();

        // Kayıtlı bir harita durumu varsa yükle, yoksa haritayı sıfırdan başlat.
        LoadMapState();

        if (playerBase == null) { playerBase = allCastlesOnMap.FirstOrDefault(c => c.isPlayerBase); }

        if (playerBase != null)
        {
            int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
            LevelData currentLevelData = levelConfigs.FirstOrDefault(ld => ld.levelIndex == currentLevel);
            if (currentLevelData != null)
            {
                playerBase.SyncWithPlayerData(currentLevelData);
            }
        }
    }

    public void InitiateBattle()
    {
        if (selectedCastle == null || selectedCastle.isPlayerBase) return;

        List<SoldierData> playerArmy = GetPlayerArmyFromSave();

        if (playerArmy.Count == 0)
        {
            Debug.Log("Oyuncu ordusu boş! Savaşa girilemez.");
            return;
        }

        List<EnemyArmyUnit> enemyArmy = selectedCastle.castleData.armyComposition;

        // BattleManager'a saldırılan kalenin referansını da gönder.
        BattleManager.Instance.StartBattle(playerArmy, enemyArmy, allSoldierTypes, selectedCastle);
    }

    /// <summary>
    /// Belirtilen kalenin fethedilmesini sağlar. BattleManager tarafından çağrılır.
    /// </summary>
    public void ConquerCastle(Castle castleToConquer)
    {
        if (castleToConquer != null)
        {
            castleToConquer.Conquer();
        }
    }

    private void SaveMapState()
    {
        if (allCastlesOnMap == null) return;
        foreach (var castle in allCastlesOnMap)
        {
            PlayerPrefs.SetInt("CastleState_" + castle.gameObject.name, (int)castle.CurrentState);
        }
        PlayerPrefs.Save();
        Debug.Log("Harita durumu kaydedildi.");
    }

    private void LoadMapState()
    {
        if (allCastlesOnMap.Count == 0) return;

        // Kayıtlı veri olup olmadığını kontrol etmek için ilk kaleyi kullan.
        bool isFirstLoad = !PlayerPrefs.HasKey("CastleState_" + allCastlesOnMap[0].gameObject.name);

        if (isFirstLoad)
        {
            InitializeMapForFirstTime();
        }
        else
        {
            foreach (var castle in allCastlesOnMap)
            {
                CastleState savedState = (CastleState)PlayerPrefs.GetInt("CastleState_" + castle.gameObject.name);
                castle.SetState(savedState);
            }
            Debug.Log("Harita durumu yüklendi.");
        }
    }

    private void InitializeMapForFirstTime()
    {
        Debug.Log("İlk açılış, harita sıfırlanıyor.");
        foreach (var castle in allCastlesOnMap)
        {
            if (castle.isPlayerBase)
            {
                castle.SetState(CastleState.PlayerOwned);
            }
            else
            {
                castle.SetState(CastleState.Locked);
            }
        }

        // Başlangıç kalesine bağlı yolları aç ve durumu kaydet.
        if (playerBase != null)
        {
            UnlockAdjacentCastles(playerBase);
        }
    }

    public void UnlockAdjacentCastles(Castle sourceCastle)
    {
        if (sourceCastle.attackableCastles == null) return;

        foreach (var adjacentCastle in sourceCastle.attackableCastles)
        {
            if (adjacentCastle.CurrentState == CastleState.Locked)
            {
                adjacentCastle.SetState(CastleState.Attackable);
            }
        }
        // Harita durumu her değiştiğinde (yeni yol açıldığında veya kale fethedildiğinde) kaydet.
        SaveMapState();
    }

    private List<SoldierData> GetPlayerArmyFromSave()
    {
        List<SoldierData> army = new List<SoldierData>();
        if (allSoldierTypes == null || allSoldierTypes.Count == 0) return army;

        foreach (var soldierType in allSoldierTypes)
        {
            int count = PlayerPrefs.GetInt("SoldierCount_" + soldierType.soldierName, 0);
            for (int i = 0; i < count; i++)
            {
                army.Add(soldierType);
            }
        }
        return army;
    }

    public void SelectCastle(Castle castle)
    {
        selectedCastle = castle;
        if (castle.CurrentState == CastleState.PlayerOwned)
        {
            uiController.ShowPlayerCastlePanel();
        }
        else if (castle.CurrentState == CastleState.Attackable)
        {
            if (castle.castleData != null)
            {
                uiController.ShowEnemyCastlePanel(castle.castleData, true);
            }
        }
    }
}
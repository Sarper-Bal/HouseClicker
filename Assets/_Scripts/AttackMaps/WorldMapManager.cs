using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance { get; private set; }

    [Header("Referanslar")]
    [SerializeField] private MapUIController uiController;
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
        if (playerBase == null) { playerBase = allCastlesOnMap.FirstOrDefault(c => c.isPlayerBase); }
        InitializeMap();
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

    // --- GÜNCELLENEN FONKSİYON ---
    public void InitiateBattle()
    {
        if (selectedCastle == null || selectedCastle.isPlayerBase) return;

        List<SoldierData> playerArmy = GetPlayerArmyFromSave();

        if (playerArmy.Count == 0)
        {
            Debug.LogError("Oyuncu ordusu boş! Savaşa girilemez. 'WorldMapManager' Inspector'ündeki 'All Soldier Types' listesini kontrol et.");
            return;
        }

        List<EnemyArmyUnit> enemyArmy = selectedCastle.castleData.armyComposition;

        // BattleManager'a tüm asker türlerinin listesini de gönderiyoruz.
        BattleManager.Instance.StartBattle(playerArmy, enemyArmy, allSoldierTypes);
    }
    // ----------------------------

    private List<SoldierData> GetPlayerArmyFromSave()
    {
        List<SoldierData> army = new List<SoldierData>();
        if (allSoldierTypes == null || allSoldierTypes.Count == 0)
        {
            Debug.LogError("'All Soldier Types' listesi WorldMapManager'da atanmamış!");
            return army;
        }

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

    private void InitializeMap()
    {
        foreach (var castle in allCastlesOnMap)
        {
            if (castle.isPlayerBase) { castle.SetState(CastleState.PlayerOwned); }
            else { castle.SetState(CastleState.Locked); }
        }
        foreach (var ownedCastle in allCastlesOnMap.Where(c => c.CurrentState == CastleState.PlayerOwned))
        {
            UnlockAdjacentCastles(ownedCastle);
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
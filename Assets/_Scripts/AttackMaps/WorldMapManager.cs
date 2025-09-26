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

    [Header("Level Verileri")]
    [Tooltip("Tüm LevelData ScriptableObject'lerini buraya sürükleyin.")]
    public List<LevelData> levelConfigs;

    private Castle selectedCastle;
    private List<Castle> allCastlesOnMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        allCastlesOnMap = FindObjectsOfType<Castle>().ToList();

        if (playerBase == null)
        {
            playerBase = allCastlesOnMap.FirstOrDefault(c => c.isPlayerBase);
        }

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

    private void InitializeMap()
    {
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

    // --- GÜNCELLENEN METOT ---
    public void SelectCastle(Castle castle)
    {
        selectedCastle = castle;

        if (castle.CurrentState == CastleState.PlayerOwned)
        {
            uiController.ShowPlayerCastlePanel();
        }
        else if (castle.CurrentState == CastleState.Attackable)
        {
            // Debug.Log yerine artık düşman paneli açılıyor.
            if (castle.castleData != null)
            {
                uiController.ShowEnemyCastlePanel(castle.castleData);
            }
        }
    }
}
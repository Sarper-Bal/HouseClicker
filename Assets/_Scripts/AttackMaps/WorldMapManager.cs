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
    private List<Castle> allCastlesOnMap; // Haritadaki tüm kalelerin listesi

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
        // Haritadaki tüm kaleleri bul ve listeye ata
        allCastlesOnMap = FindObjectsOfType<Castle>().ToList();

        if (playerBase == null)
        {
            playerBase = allCastlesOnMap.FirstOrDefault(c => c.isPlayerBase);
        }

        InitializeMap(); // Haritayı başlangıç durumuna getir

        if (playerBase != null)
        {
            // Oyuncu üssünün sprite'ını ayarla
            int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
            LevelData currentLevelData = levelConfigs.FirstOrDefault(ld => ld.levelIndex == currentLevel);
            if (currentLevelData != null)
            {
                playerBase.SyncWithPlayerData(currentLevelData);
            }
        }
    }

    // --- YENİ EKLENEN METOTLAR ---
    /// <summary>
    /// Haritayı oyun başlangıcı için hazırlar. Tüm kalelerin durumunu ayarlar.
    /// </summary>
    private void InitializeMap()
    {
        foreach (var castle in allCastlesOnMap)
        {
            if (castle.isPlayerBase)
            {
                // Başlangıç kalesini "PlayerOwned" yap
                castle.SetState(CastleState.PlayerOwned);
            }
            else
            {
                // Diğer tüm kaleleri "Locked" yap
                castle.SetState(CastleState.Locked);
            }
        }

        // Başlangıç kalesine komşu olan ve fethedilmiş tüm kalelerin yollarını aç
        foreach (var ownedCastle in allCastlesOnMap.Where(c => c.CurrentState == CastleState.PlayerOwned))
        {
            UnlockAdjacentCastles(ownedCastle);
        }
    }

    /// <summary>
    /// Verilen bir kalenin komşularını "Attackable" durumuna getirir.
    /// </summary>
    public void UnlockAdjacentCastles(Castle sourceCastle)
    {
        if (sourceCastle.attackableCastles == null) return;

        foreach (var adjacentCastle in sourceCastle.attackableCastles)
        {
            // Sadece kilitli olan kalelerin durumunu değiştir
            if (adjacentCastle.CurrentState == CastleState.Locked)
            {
                adjacentCastle.SetState(CastleState.Attackable);
            }
        }
    }
    // -------------------------

    public void SelectCastle(Castle castle)
    {
        selectedCastle = castle;

        if (castle.CurrentState == CastleState.PlayerOwned)
        {
            uiController.ShowPlayerCastlePanel();
        }
        else if (castle.CurrentState == CastleState.Attackable)
        {
            // Düşman kalesine tıklandı. Şimdilik sadece log yazdırıyoruz.
            // Gelecekte burada düşman bilgi paneli açılabilir.
            Debug.Log($"{castle.castleData.castleName} adlı düşman kalesine tıklandı.");
        }
    }
}
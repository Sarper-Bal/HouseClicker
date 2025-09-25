using UnityEngine;
using System.Linq;
using System.Collections.Generic; // List kullanabilmek için eklendi

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance { get; private set; }

    [Header("Referanslar")]
    [SerializeField] private MapUIController uiController;
    [Tooltip("Oyuncunun haritadaki ana kalesini buraya sürükleyin.")]
    [SerializeField] private Castle playerBase;

    // --- YENİ EKLENEN KISIM ---
    [Header("Level Verileri")]
    [Tooltip("Tüm LevelData ScriptableObject'lerini buraya sürükleyin.")]
    public List<LevelData> levelConfigs;
    // -------------------------

    private Castle selectedCastle;

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
        if (playerBase == null)
        {
            playerBase = FindObjectsOfType<Castle>().FirstOrDefault(c => c.isPlayerBase);
        }

        if (playerBase != null)
        {
            // Kayıtlı seviyeyi PlayerPrefs'ten oku
            int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);

            // Mevcut seviyeye karşılık gelen LevelData'yı listeden bul
            LevelData currentLevelData = levelConfigs.FirstOrDefault(ld => ld.levelIndex == currentLevel);

            if (currentLevelData != null)
            {
                // Bulunan LevelData ile oyuncu kalesinin görselini güncelle
                playerBase.SyncWithPlayerData(currentLevelData);
            }
            else
            {
                Debug.LogError($"Seviye {currentLevel} için LevelData bulunamadı! WorldMapManager üzerindeki 'levelConfigs' listesini kontrol edin.");
            }
        }
    }

    /// <summary>
    /// Bir kaleye tıklandığında Castle script'i tarafından çağrılır.
    /// </summary>
    public void SelectCastle(Castle castle)
    {
        selectedCastle = castle;

        if (castle.isPlayerBase)
        {
            // Oyuncu kalesine tıklandı, bilgi panelini göster
            uiController.ShowPlayerCastlePanel();
        }
        else
        {
            // Düşman kalesine tıklandı. Şimdilik hiçbir şey yapma.
            Debug.Log($"{castle.castleData.castleName} adlı düşman kalesine tıklandı.");
        }
    }
}
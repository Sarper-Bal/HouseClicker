using UnityEngine;
using System.Linq;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance { get; private set; }

    [Header("Referanslar")]
    [SerializeField] private MapUIController uiController;
    [Tooltip("Oyuncunun haritadaki ana kalesini buraya sürükleyin.")]
    [SerializeField] private Castle playerBase;

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
            playerBase.SyncWithPlayerData();
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
            // uiController.ShowEnemyCastlePanel(...) çağrısı buradan kaldırıldı.
        }
    }

    // --- KALDIRILAN FONKSİYON ---
    // AttackSelectedCastle() fonksiyonu şimdilik gereksiz olduğu için kaldırıldı.
    // Bu fonksiyonu daha sonra savaş sistemini eklerken geri getireceğiz.
    // --- KALDIRMA SONU ---
}
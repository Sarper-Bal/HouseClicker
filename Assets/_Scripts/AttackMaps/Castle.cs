using UnityEngine;
using UnityEngine.UI;

public class Castle : MonoBehaviour
{
    [Header("Kale Ayarları")]
    [Tooltip("Bu kale oyuncunun ana üssü mü?")]
    public bool isPlayerBase = false;
    [Tooltip("Eğer bu bir düşman kalesiyse, özelliklerini belirleyen veri dosyası.")]
    public EnemyCastleData castleData;

    // Kaledeki mevcut ordu bilgilerini tutacak değişkenler
    public int ArmySize { get; private set; }
    public long ArmyHealth { get; private set; }
    public long ArmyAttack { get; private set; }

    private Button button;
    private bool isConquered = false;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(OnCastleClicked);

        // Kalenin türüne göre verileri yükle
        // Oyuncu üssü ise yükleme artık WorldMapManager tarafından tetiklenecek
        if (!isPlayerBase)
        {
            LoadFromEnemyData();
        }
    }

    /// <summary>
    /// Oyuncu üssü ise, kalenin görselini ve içindeki ordu verilerini günceller.
    /// </summary>
    public void SyncWithPlayerData(LevelData currentLevelData) // Metot artık LevelData parametresi alıyor
    {
        if (!isPlayerBase) return;

        // Sprite'ı parametre olarak gelen LevelData'dan al ve güncelle
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (currentLevelData != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = currentLevelData.houseSprite;
        }

        // Ordu verilerini SoldierManager'dan al ve güncelle
        if (SoldierManager.Instance != null)
        {
            this.ArmySize = SoldierManager.Instance.GetTotalSoldierCount();
            this.ArmyHealth = SoldierManager.Instance.TotalHealth;
            this.ArmyAttack = SoldierManager.Instance.TotalAttack;
        }

        Debug.Log($"Oyuncu kalesi senkronize edildi. Seviye sprite'ı ayarlandı.");
    }

    /// <summary>
    /// Düşman kalesi ise, verilerini ScriptableObject'ten yükler.
    /// </summary>
    private void LoadFromEnemyData()
    {
        if (castleData != null)
        {
            this.ArmySize = castleData.soldiers;
            this.ArmyHealth = castleData.totalHealth;
            this.ArmyAttack = castleData.totalAttack;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = castleData.castleSprite;
            }
        }
    }

    public void OnCastleClicked()
    {
        if (WorldMapManager.Instance != null)
        {
            WorldMapManager.Instance.SelectCastle(this);
        }
    }

    public void Conquer()
    {
        isConquered = true;
        isPlayerBase = true;
        castleData = null; // Artık düşman verisine ihtiyacı yok
    }
}
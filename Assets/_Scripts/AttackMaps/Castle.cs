using UnityEngine;
using UnityEngine.UI;

public class Castle : MonoBehaviour
{
    [Header("Kale Ayarları")]
    [Tooltip("Bu kale oyuncunun ana üssü mü?")]
    public bool isPlayerBase = false;
    [Tooltip("Eğer bu bir düşman kalesiyse, özelliklerini belirleyen veri dosyası.")]
    public EnemyCastleData castleData;

    // --- KALDIRILAN KISIM ---
    // Görsel ayarlar buradan kaldırıldı.
    // [Header("Görsel Ayarlar")]
    // [SerializeField] private SpriteRenderer castleSpriteRenderer;
    // [SerializeField] private Color playerOwnedColor = Color.blue;
    // [SerializeField] private Color enemyOwnedColor = Color.red;
    // --- KALDIRMA SONU ---

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
        if (isPlayerBase)
        {
            SyncWithPlayerData();
        }
        else
        {
            LoadFromEnemyData();
        }
    }

    /// <summary>
    /// Oyuncu üssü ise, kalenin görselini ve içindeki ordu verilerini
    /// kalıcı yöneticilerden (UpgradeManager, SoldierManager) senkronize eder.
    /// </summary>
    public void SyncWithPlayerData()
    {
        if (!isPlayerBase) return;

        // Sprite'ı UpgradeManager'dan al ve güncelle
        // Not: Sprite'ı değiştirebilmek için bir SpriteRenderer referansı hala gerekli olabilir.
        // Eğer kalelerin görselleri seviyeye göre değişmeyecekse bu kısım da silinebilir.
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (UpgradeManager.Instance != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = UpgradeManager.Instance.GetCurrentLevelData().houseSprite;
        }

        // Ordu verilerini SoldierManager'dan al ve güncelle
        if (SoldierManager.Instance != null)
        {
            this.ArmySize = SoldierManager.Instance.GetTotalSoldierCount();
            this.ArmyHealth = SoldierManager.Instance.TotalHealth;
            this.ArmyAttack = SoldierManager.Instance.TotalAttack;
        }

        Debug.Log($"Oyuncu kalesi senkronize edildi. Asker: {ArmySize}");
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
        // Renk değiştirme kodu buradan kaldırıldı.
    }

    // --- KALDIRILAN FONKSİYON ---
    // private void UpdateCastleVisuals() { ... }
    // --- KALDIRMA SONU ---
}
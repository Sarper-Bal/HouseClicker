using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

        if (isPlayerBase)
        {
            // Oyuncu üssünün verileri WorldMapManager tarafından senkronize edilecek.
        }
        else
        {
            LoadFromEnemyData();
        }
    }

    public void SyncWithPlayerData(LevelData currentLevelData)
    {
        if (!isPlayerBase) return;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (currentLevelData != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = currentLevelData.houseSprite;
        }

        if (SoldierManager.Instance != null)
        {
            this.ArmySize = SoldierManager.Instance.GetTotalSoldierCount();
            this.ArmyHealth = SoldierManager.Instance.TotalHealth;
            this.ArmyAttack = SoldierManager.Instance.TotalAttack;
        }

        Debug.Log($"Oyuncu kalesi senkronize edildi. Seviye sprite'ı ayarlandı.");
    }

    /// <summary>
    /// Düşman kalesi ise, verilerini ScriptableObject'ten yükler ve gücünü dinamik olarak hesaplar.
    /// </summary>
    private void LoadFromEnemyData()
    {
        if (isPlayerBase || castleData == null) return;

        ArmySize = 0;
        ArmyHealth = 0;
        ArmyAttack = 0;

        foreach (var armyUnit in castleData.armyComposition)
        {
            if (armyUnit.soldierData != null && armyUnit.count > 0)
            {
                ArmySize += armyUnit.count;
                // --- HATA DÜZELTİLDİ ---
                // 'baseHealth' yerine 'health' kullanıldı.
                ArmyHealth += (long)armyUnit.soldierData.health * armyUnit.count;
                // 'baseAttack' yerine 'attack' kullanıldı.
                ArmyAttack += (long)armyUnit.soldierData.attack * armyUnit.count;
                // -------------------------
            }
        }

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = castleData.castleSprite;
        }

        Debug.Log($"{castleData.castleName} kalesi yüklendi. Toplam Asker: {ArmySize}, Sağlık: {ArmyHealth}, Saldırı: {ArmyAttack}");
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
        castleData = null;
    }
}
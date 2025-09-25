using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // List kullanabilmek için eklendi
using System.Linq;

// --- YENİ EKLENEN KISIM ---
// Kalenin sahip olabileceği durumları tanımlar.
public enum CastleState
{
    Locked,       // Henüz saldırılamaz
    Attackable,   // Saldırıya açık
    PlayerOwned   // Oyuncuya ait
}
// -------------------------

public class Castle : MonoBehaviour
{
    [Header("Kale Ayarları")]
    [Tooltip("Bu kale oyuncunun başlangıç üssü mü?")]
    public bool isPlayerBase = false;
    [Tooltip("Eğer bu bir düşman kalesiyse, özelliklerini belirleyen veri dosyası.")]
    public EnemyCastleData castleData;

    // --- YENİ EKLENEN KISIM ---
    [Header("Fetih Sistemi")]
    [Tooltip("Bu kale fethedildiğinde saldırılabilir hale gelecek kaleleri buraya sürükleyin.")]
    public List<Castle> attackableCastles; // Saldırı yolları listesi

    [Header("Görsel Ayarlar")]
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Gri ve yarı saydam
    [SerializeField] private Color attackableColor = Color.white; // Normal renk
    [SerializeField] private Color playerOwnedColor = new Color(0.3f, 0.5f, 1f); // Mavi tonu

    public CastleState CurrentState { get; private set; } = CastleState.Locked;
    private SpriteRenderer spriteRenderer;
    // -------------------------

    // Ordu bilgileri
    public int ArmySize { get; private set; }
    public long ArmyHealth { get; private set; }
    public long ArmyAttack { get; private set; }

    private Button button;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(OnCastleClicked);
    }

    private void Start()
    {
        if (!isPlayerBase)
        {
            LoadFromEnemyData();
        }
    }

    // --- GÜNCELLENEN METOT ---
    public void OnCastleClicked()
    {
        // Sadece oyuncuya ait veya saldırılabilir kalelere tıklanabilir.
        if (CurrentState == CastleState.PlayerOwned || CurrentState == CastleState.Attackable)
        {
            if (WorldMapManager.Instance != null)
            {
                WorldMapManager.Instance.SelectCastle(this);
            }
        }
        else
        {
            Debug.Log($"{castleData.castleName} kalesi kilitli, henüz saldırılamaz.");
        }
    }

    // --- YENİ EKLENEN METOTLAR ---
    /// <summary>
    /// Kalenin durumunu ayarlar ve görselini günceller.
    /// </summary>
    public void SetState(CastleState newState)
    {
        CurrentState = newState;
        UpdateCastleVisuals();
    }

    /// <summary>
    /// Kalenin rengini mevcut durumuna göre günceller.
    /// </summary>
    private void UpdateCastleVisuals()
    {
        if (spriteRenderer == null) return;

        switch (CurrentState)
        {
            case CastleState.Locked:
                spriteRenderer.color = lockedColor;
                break;
            case CastleState.Attackable:
                spriteRenderer.color = attackableColor;
                break;
            case CastleState.PlayerOwned:
                spriteRenderer.color = playerOwnedColor;
                break;
        }
    }

    // Bu metot şimdilik çağrılmıyor, savaş sistemi eklenince kullanılacak.
    public void Conquer()
    {
        isPlayerBase = true; // Artık bu da oyuncunun bir üssü
        castleData = null;
        SetState(CastleState.PlayerOwned);

        // WorldMapManager'a haber vererek yeni yolları açmasını sağla
        if (WorldMapManager.Instance != null)
        {
            WorldMapManager.Instance.UnlockAdjacentCastles(this);
        }
    }
    // -------------------------

    public void SyncWithPlayerData(LevelData currentLevelData)
    {
        if (!isPlayerBase) return;

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
    }

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
                ArmyHealth += (long)armyUnit.soldierData.health * armyUnit.count;
                ArmyAttack += (long)armyUnit.soldierData.attack * armyUnit.count;
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = castleData.castleSprite;
        }
    }
}
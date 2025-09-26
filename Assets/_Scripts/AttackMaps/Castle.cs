using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // DOTween kütüphanesini kullanmak için eklendi

// Kalenin sahip olabileceği durumları tanımlar.
public enum CastleState
{
    Locked,       // Henüz saldırılamaz
    Attackable,   // Saldırıya açık
    PlayerOwned   // Oyuncuya ait
}

public class Castle : MonoBehaviour
{
    [Header("Kale Ayarları")]
    [Tooltip("Bu kale oyuncunun başlangıç üssü mü?")]
    public bool isPlayerBase = false;
    [Tooltip("Eğer bu bir düşman kalesiyse, özelliklerini belirleyen veri dosyası.")]
    public EnemyCastleData castleData;

    [Header("Fetih Sistemi")]
    [Tooltip("Bu kale fethedildiğinde saldırılabilir hale gelecek kaleleri buraya sürükleyin.")]
    public List<Castle> attackableCastles; // Saldırı yolları listesi

    [Header("Görsel Ayarlar")]
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Gri ve yarı saydam
    [SerializeField] private Color attackableColor = Color.white; // Normal renk
    [SerializeField] private Color playerOwnedColor = new Color(0.3f, 0.5f, 1f); // Mavi tonu

    public CastleState CurrentState { get; private set; } = CastleState.Locked;
    private SpriteRenderer spriteRenderer;
    private Button button;
    private Tween pulseAnimation; // DOTween animasyonu için referans

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
    }

    /// <summary>
    /// Kalenin durumunu ayarlar ve görselini günceller.
    /// </summary>
    public void SetState(CastleState newState)
    {
        CurrentState = newState;
        UpdateCastleVisuals();
    }

    /// <summary>
    /// Kalenin rengini ve animasyonunu mevcut durumuna göre günceller.
    /// </summary>
    private void UpdateCastleVisuals()
    {
        if (spriteRenderer == null) return;

        // Yeni animasyonu başlatmadan önce çalışan bir animasyon varsa durdur ve ölçeği sıfırla.
        if (pulseAnimation != null)
        {
            pulseAnimation.Kill();
            transform.localScale = Vector3.one;
        }

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
                // Oyuncuya ait kaleler için tekrarlayan bir büyüme-küçülme animasyonu başlat.
                pulseAnimation = transform.DOScale(1.1f, 1.0f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    /// <summary>
    /// Bu kalenin fethedilme işlemini gerçekleştirir.
    /// </summary>
    public void Conquer()
    {
        isPlayerBase = true; // Artık bu kale de oyuncunun.
        castleData = null;   // Düşman verisi artık gereksiz.
        SetState(CastleState.PlayerOwned);

        // WorldMapManager'a haber vererek yeni yolları açmasını sağla.
        if (WorldMapManager.Instance != null)
        {
            WorldMapManager.Instance.UnlockAdjacentCastles(this);
        }
    }

    // Bu metot, oyuncunun ana üssünün görselini ve asker gücünü senkronize etmek için kullanılır.
    public void SyncWithPlayerData(LevelData currentLevelData)
    {
        if (!isPlayerBase) return;

        if (currentLevelData != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = currentLevelData.houseSprite;
        }

        // Asker gücü artık MapUIController tarafından PopulateSoldierList içinde yönetiliyor.
        // Bu metot şimdilik sadece görsel güncelleme için kalabilir.
    }
}
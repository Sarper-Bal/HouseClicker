using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // DOTween kütüphanesini kullanmak için

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
    private Tween currentAnimation; // Animasyon referansı

    // Her kalenin kendine özgü orijinal boyutunu saklamak için.
    private Vector3 originalScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        button = GetComponent<Button>();

        // Oyun başladığında kalenin mevcut boyutunu kaydet.
        originalScale = transform.localScale;

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(OnCastleClicked);
    }


    // --- YENİ EKLENEN METOT: ANİMASYONU GÜVENLE DURDURMA ---
    private void OnDestroy()
    {
        // Bu kale objesi yok edilirken, ona bağlı olan ve çalışan bir
        // animasyon varsa, onu anında ve güvenli bir şekilde durdur.
        // Bu, sahne geçişlerindeki "Missing Target" hatasını çözecektir.
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
        }
    }
    // --------------------------------------------------------

    public void OnCastleClicked()
    {
        if (CurrentState == CastleState.PlayerOwned || CurrentState == CastleState.Attackable)
        {
            if (WorldMapManager.Instance != null)
            {
                WorldMapManager.Instance.SelectCastle(this);
            }
        }
    }

    public void SetState(CastleState newState)
    {
        CurrentState = newState;
        UpdateCastleVisuals();
    }

    private void UpdateCastleVisuals()
    {
        if (spriteRenderer == null) return;

        if (currentAnimation != null)
        {
            currentAnimation.Kill();
            // Ölçeği (1,1,1) yerine kaydettiğimiz orijinal boyutuna geri döndür.
            transform.localScale = originalScale;
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

                // --- YAVAŞLATILMIŞ VE ORİJİNAL BOYUTA UYGUN JÖLE ANİMASYONU ---
                currentAnimation = DOTween.Sequence()
                    // Yandan sıkışma (orijinal X boyutunun %85'i)
                    .Append(transform.DOScaleX(originalScale.x * 0.85f, 0.4f).SetEase(Ease.OutQuad))
                    // Yukarı uzama (orijinal Y boyutunun %115'i)
                    .Join(transform.DOScaleY(originalScale.y * 1.15f, 0.4f).SetEase(Ease.OutQuad))
                    // Hafifçe genişleme
                    .Append(transform.DOScaleX(originalScale.x * 1.05f, 0.3f).SetEase(Ease.OutQuad))
                    // Hafifçe çökme
                    .Join(transform.DOScaleY(originalScale.y * 0.95f, 0.3f).SetEase(Ease.OutQuad))
                    // Esnek bir şekilde orijinal boyutuna geri dönme
                    .Append(transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBounce))
                    // Döngü başlamadan önce bekleme süresi
                    .AppendInterval(1.0f)
                    .SetLoops(-1);
                // -----------------------------------------------------------------
                break;
        }
    }

    public void Conquer()
    {
        isPlayerBase = true;
        castleData = null;
        SetState(CastleState.PlayerOwned);

        if (WorldMapManager.Instance != null)
        {
            WorldMapManager.Instance.UnlockAdjacentCastles(this);
        }
    }

    public void SyncWithPlayerData(LevelData currentLevelData)
    {
        if (!isPlayerBase) return;

        if (currentAnimation != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = currentLevelData.houseSprite;
        }
    }
}
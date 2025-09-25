using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class HouseController : MonoBehaviour, IPointerDownHandler
{
    public event Action<long> OnClickedForGold;

    [Header("Animation Settings")]
    [SerializeField] private GameObject shockwavePrefab;

    private Sequence clickSequence;
    private SpriteRenderer houseSpriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;

    private void Awake()
    {
        houseSpriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        originalColor = houseSpriteRenderer.color;
    }

    // --- YENİ EKLENEN FONKSİYON ---
    // Bu obje (ve sahne) yok olmadan hemen önce çalışır.
    private void OnDestroy()
    {
        // Çalışan bir animasyon varsa, sahne değişmeden önce onu güvenli bir şekilde durdur.
        // Bu, "missing target" hatasını çözecektir.
        if (clickSequence != null)
        {
            clickSequence.Kill();
        }
    }
    // --- YENİ KISIM SONU ---

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSequence != null && clickSequence.IsActive())
        {
            clickSequence.Complete(true);
        }

        HandleGameplayLogic();
        PlayExaggeratedClickAnimation();
    }

    private void HandleGameplayLogic()
    {
        if (UpgradeManager.Instance == null || CurrencyManager.Instance == null)
        {
            Debug.LogError("Manager'lar sahnede bulunamadı!");
            return;
        }

        LevelData currentLevelData = UpgradeManager.Instance.GetCurrentLevelData();

        if (currentLevelData != null)
        {
            long goldToAdd = currentLevelData.goldPerClick;
            CurrencyManager.Instance.AddGold(goldToAdd);
            OnClickedForGold?.Invoke(goldToAdd);
        }
    }

    private void PlayExaggeratedClickAnimation()
    {
        clickSequence = DOTween.Sequence();

        clickSequence.Append(transform.DOScale(originalScale * 0.8f, 0.07f));
        clickSequence.Join(houseSpriteRenderer.DOColor(Color.white, 0.07f));

        clickSequence.AppendCallback(CreateShockwave);
        clickSequence.Join(transform.DOPunchPosition(Vector3.up * 0.2f, 0.4f, 1, 0.5f));

        clickSequence.Append(transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutElastic));
        clickSequence.Join(transform.DOPunchRotation(new Vector3(0, 0, 8f), 0.5f, 15, 1));

        clickSequence.Join(houseSpriteRenderer.DOColor(originalColor, 0.4f));
    }

    private void CreateShockwave()
    {
        if (shockwavePrefab == null) return;

        GameObject shockwaveInstance = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        Destroy(shockwaveInstance, 0.5f);

        SpriteRenderer swRenderer = shockwaveInstance.GetComponent<SpriteRenderer>();
        if (swRenderer == null) return;

        shockwaveInstance.transform.localScale = originalScale * 0.1f;

        Sequence shockwaveSequence = DOTween.Sequence();
        shockwaveSequence.Append(shockwaveInstance.transform.DOScale(originalScale * 2f, 0.5f));
        shockwaveSequence.Join(swRenderer.DOFade(0, 0.5f).SetEase(Ease.InQuad));
    }
}
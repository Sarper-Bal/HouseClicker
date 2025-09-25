using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class VisualManager : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private SpriteRenderer houseSpriteRenderer;

    [Header("Animation Settings")]
    [SerializeField] private float spriteTransitionDuration = 0.25f;
    [SerializeField] private float cosmeticAppearDuration = 0.5f;

    private Dictionary<string, CosmeticObject> cosmeticObjectMap;
    private Dictionary<string, Vector3> cosmeticObjectInitialScales;
    private Sequence transitionSequence; // Animasyonu takip etmek için bir değişken

    private void Awake()
    {
        cosmeticObjectMap = new Dictionary<string, CosmeticObject>();
        cosmeticObjectInitialScales = new Dictionary<string, Vector3>();

        CosmeticObject[] allCosmeticObjects = FindObjectsOfType<CosmeticObject>(true);

        foreach (var cosmeticObject in allCosmeticObjects)
        {
            if (!string.IsNullOrEmpty(cosmeticObject.objectID) && !cosmeticObjectMap.ContainsKey(cosmeticObject.objectID))
            {
                cosmeticObjectMap.Add(cosmeticObject.objectID, cosmeticObject);
                cosmeticObjectInitialScales.Add(cosmeticObject.objectID, cosmeticObject.transform.localScale);
                cosmeticObject.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnLevelUp += HandleLevelUp;
            HandleLevelUp(UpgradeManager.Instance.GetCurrentLevelData());
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnLevelUp -= HandleLevelUp;
        }

        // --- DEĞİŞİKLİK BURADA ---
        // Bu obje yok olmadan önce, çalışan sprite geçiş animasyonunu durdur.
        if (transitionSequence != null)
        {
            transitionSequence.Kill();
        }
        // --- DEĞİŞİKLİK SONU ---
    }

    private void HandleLevelUp(LevelData newLevelData)
    {
        if (newLevelData == null)
        {
            Debug.LogError("HandleLevelUp fonksiyonuna gelen LevelData null!");
            return;
        }
        UpdateHouseSprite(newLevelData);
        UpdateCosmeticObjects(newLevelData);
    }

    private void UpdateHouseSprite(LevelData newLevelData)
    {
        if (houseSpriteRenderer == null) return;

        if (newLevelData.houseSprite != null && houseSpriteRenderer.sprite != newLevelData.houseSprite)
        {
            // Önceki animasyon çalışıyorsa onu durdur
            if (transitionSequence != null) transitionSequence.Kill();

            // Yeni animasyonu başlat ve değişkene ata
            transitionSequence = DOTween.Sequence();
            transitionSequence.Append(houseSpriteRenderer.DOFade(0, spriteTransitionDuration));
            transitionSequence.AppendCallback(() => houseSpriteRenderer.sprite = newLevelData.houseSprite);
            transitionSequence.Append(houseSpriteRenderer.DOFade(1, spriteTransitionDuration));
        }
    }

    private void UpdateCosmeticObjects(LevelData newLevelData)
    {
        if (newLevelData.cosmeticObjectsToShow == null) return;

        foreach (var cosmetic in cosmeticObjectMap.Values)
        {
            cosmetic.gameObject.SetActive(false);
        }

        foreach (string idToShow in newLevelData.cosmeticObjectsToShow)
        {
            if (cosmeticObjectMap.ContainsKey(idToShow))
            {
                GameObject objToShow = cosmeticObjectMap[idToShow].gameObject;
                Vector3 initialScale = cosmeticObjectInitialScales[idToShow];

                if (objToShow.activeSelf) continue;

                objToShow.SetActive(true);
                objToShow.transform.localScale = Vector3.zero;
                objToShow.transform.DOScale(initialScale, cosmeticAppearDuration).SetEase(Ease.OutBack);
            }
        }
    }
}
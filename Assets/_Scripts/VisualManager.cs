using UnityEngine;
using DG.Tweening; // Animasyonlar için DOTween kütüphanesi
using System.Collections.Generic; // Dictionary kullanmak için
using System.Linq; // FindObjectsOfType'ı kolay kullanmak için

public class VisualManager : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("Görseli değiştirilecek olan ana evin SpriteRenderer'ını buraya sürükleyin.")]
    [SerializeField] private SpriteRenderer houseSpriteRenderer;

    [Header("Animation Settings")]
    [Tooltip("Sprite değişimi sırasında ne kadar sürecek bir kararma/belirme efekti olsun?")]
    [SerializeField] private float spriteTransitionDuration = 0.25f;
    [Tooltip("Kozmetik objeler belirirken ne kadar sürede büyüsün?")]
    [SerializeField] private float cosmeticAppearDuration = 0.5f;

    private Dictionary<string, CosmeticObject> cosmeticObjectMap;
    private Dictionary<string, Vector3> cosmeticObjectInitialScales;

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
            // Gelecekteki seviye atlamaları için event'e abone ol.
            UpgradeManager.Instance.OnLevelUp += HandleLevelUp;

            // --- ÇÖZÜM BURADA ---
            // Oyuna başlarken, anonsu kaçırma ihtimaline karşı mevcut yüklü seviye ne ise
            // görselleri ona göre GÜNCELLE. Bu, başlangıç senkronizasyonunu sağlar.
            HandleLevelUp(UpgradeManager.Instance.GetCurrentLevelData());
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnLevelUp -= HandleLevelUp;
        }
    }

    // Bu fonksiyon hem oyun başlangıcında hem de seviye atlandığında çalışacak.
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

        // Sprite null ise veya zaten aynı sprite ise işlem yapma.
        // Başlangıçta aynı sprite olsa bile kozmetik objelerin güncellenmesi için devam etmesi gerekiyor,
        // bu yüzden bu kontrolü daha spesifik hale getirelim.
        if (newLevelData.houseSprite != null && houseSpriteRenderer.sprite != newLevelData.houseSprite)
        {
            Sequence transitionSequence = DOTween.Sequence();
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

                // Eğer obje zaten aktifse animasyon yapma, sadece orada kalsın.
                // Bu, oyun başlangıcında her şeyin sıfırdan büyümesini engeller.
                if (objToShow.activeSelf) continue;

                objToShow.SetActive(true);
                objToShow.transform.localScale = Vector3.zero;
                objToShow.transform.DOScale(initialScale, cosmeticAppearDuration).SetEase(Ease.OutBack);
            }
        }
    }
}
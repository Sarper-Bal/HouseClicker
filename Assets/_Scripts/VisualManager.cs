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
    private Sequence transitionSequence;

    // --- YENİ EKLENEN KONTROL DEĞİŞKENİ ---
    // Bu, görsel güncellemenin sadece bir kez yapılmasını sağlar.
    private bool isInitialized = false;

    private void Awake()
    {
        // Tüm kozmetik objeleri bul ve başlangıç durumuna getir.
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

    // --- YENİ VE DAHA GARANTİLİ YÜKLEME METODU ---
    // Start() yerine bu yöntemi kullanacağız.
    // Update, her karede çalışır. Bu kod bloğu, UpgradeManager hazır olur olmaz sadece bir kez çalışacak şekilde ayarlandı.
    private void Update()
    {
        // Eğer görseller henüz yüklenmediyse VE UpgradeManager artık hazırsa...
        if (!isInitialized && UpgradeManager.Instance != null)
        {
            // Görsel yükleme işlemini başlat.
            InitializeVisuals();

            // Bu bloğun tekrar çalışmaması için bayrağı true yap.
            isInitialized = true;
        }
    }

    // Görsel yüklemeyi ve event aboneliğini yapan ana fonksiyon.
    private void InitializeVisuals()
    {
        Debug.Log("UpgradeManager bulundu, görseller yükleniyor...");

        // Gelecekteki seviye atlama olaylarını dinlemek için event'e abone ol.
        UpgradeManager.Instance.OnLevelUp += HandleLevelUp;

        // Sahne yüklendiğinde mevcut görselleri anında güncelle.
        LevelData currentLevelData = UpgradeManager.Instance.GetCurrentLevelData();
        HandleLevelUp(currentLevelData);
    }

    private void OnDestroy()
    {
        // Hata ve bellek sızıntılarını önlemek için olay aboneliğinden çık.
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnLevelUp -= HandleLevelUp;
        }

        // Animasyonları güvenli bir şekilde durdur.
        if (transitionSequence != null)
        {
            transitionSequence.Kill();
        }
    }

    private void HandleLevelUp(LevelData newLevelData)
    {
        if (newLevelData == null)
        {
            Debug.LogError("HandleLevelUp fonksiyonuna gelen LevelData null! UpgradeManager'daki levelConfigs listesi doğru ayarlanmış mı?");
            return;
        }

        Debug.Log(newLevelData.name + " seviyesi için görseller güncelleniyor.");
        UpdateHouseSprite(newLevelData);
        UpdateCosmeticObjects(newLevelData);
    }

    private void UpdateHouseSprite(LevelData newLevelData)
    {
        if (houseSpriteRenderer == null) return;

        if (newLevelData.houseSprite != null && houseSpriteRenderer.sprite != newLevelData.houseSprite)
        {
            if (transitionSequence != null) transitionSequence.Kill();

            transitionSequence = DOTween.Sequence();
            transitionSequence.Append(houseSpriteRenderer.DOFade(0, spriteTransitionDuration));
            transitionSequence.AppendCallback(() => houseSpriteRenderer.sprite = newLevelData.houseSprite);
            transitionSequence.Append(houseSpriteRenderer.DOFade(1, spriteTransitionDuration));
        }
        else if (houseSpriteRenderer.color.a < 1f)
        {
            houseSpriteRenderer.DOFade(1, spriteTransitionDuration);
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

                objToShow.SetActive(true);

                if (objToShow.transform.localScale != initialScale)
                {
                    objToShow.transform.localScale = Vector3.zero;
                    objToShow.transform.DOScale(initialScale, cosmeticAppearDuration).SetEase(Ease.OutBack);
                }
            }
            else
            {
                Debug.LogWarning($"LevelData'da belirtilen '{idToShow}' ID'li obje sahnede bulunamadı!");
            }
        }
    }
}
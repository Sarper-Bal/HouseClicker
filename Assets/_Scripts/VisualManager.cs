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

    // Sahnedeki tüm kozmetik objeleri ID'leriyle birlikte hızlıca bulmak için bir sözlük (dictionary).
    private Dictionary<string, CosmeticObject> cosmeticObjectMap;
    // --- YENİ EKLENEN KISIM ---
    // Her bir kozmetik objenin başlangıçtaki orijinal ölçeğini saklamak için yeni bir sözlük.
    private Dictionary<string, Vector3> cosmeticObjectInitialScales;
    // -------------------------

    private void Awake()
    {
        // Sözlükleri (dictionary) başlat
        cosmeticObjectMap = new Dictionary<string, CosmeticObject>();
        cosmeticObjectInitialScales = new Dictionary<string, Vector3>(); // Yeni sözlüğü başlat

        CosmeticObject[] allCosmeticObjects = FindObjectsOfType<CosmeticObject>(true);

        Debug.Log($"Toplam {allCosmeticObjects.Length} adet CosmeticObject bulundu. Listeleniyor...");

        foreach (var cosmeticObject in allCosmeticObjects)
        {
            if (!string.IsNullOrEmpty(cosmeticObject.objectID) && !cosmeticObjectMap.ContainsKey(cosmeticObject.objectID))
            {
                Debug.Log($"Kozmetik obje bulundu ve eklendi: {cosmeticObject.name}, ID: {cosmeticObject.objectID}");

                // Objeyi ve orijinal ölçeğini sözlüklere ekle
                cosmeticObjectMap.Add(cosmeticObject.objectID, cosmeticObject);
                // --- YENİ EKLENEN KISIM ---
                // Objeyi kapatmadan ÖNCE mevcut ölçeğini hafızaya alıyoruz.
                cosmeticObjectInitialScales.Add(cosmeticObject.objectID, cosmeticObject.transform.localScale);
                // -------------------------

                // Başlangıçta hepsi kapalı olsun.
                cosmeticObject.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Bulunan kozmetik objenin ID'si boş veya bu ID zaten kullanılmış: {cosmeticObject.name}");
            }
        }
    }

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnLevelUp -= HandleLevelUp;
        }
    }

    private void HandleLevelUp(LevelData newLevelData)
    {
        UpdateHouseSprite(newLevelData);
        UpdateCosmeticObjects(newLevelData);
    }

    private void UpdateHouseSprite(LevelData newLevelData)
    {
        if (houseSpriteRenderer == null)
        {
            Debug.LogError("House Sprite Renderer referansı VisualManager'da atanmamış!");
            return;
        }
        if (newLevelData.houseSprite == null || houseSpriteRenderer.sprite == newLevelData.houseSprite)
        {
            return;
        }

        Sequence transitionSequence = DOTween.Sequence();
        transitionSequence.Append(houseSpriteRenderer.DOFade(0, spriteTransitionDuration));
        transitionSequence.AppendCallback(() => houseSpriteRenderer.sprite = newLevelData.houseSprite);
        transitionSequence.Append(houseSpriteRenderer.DOFade(1, spriteTransitionDuration));
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
                objToShow.SetActive(true);

                // --- GÜNCELLENEN KISIM ---
                // Hafızaya aldığımız orijinal ölçeği al.
                Vector3 initialScale = cosmeticObjectInitialScales[idToShow];

                // Animasyonu başlat.
                objToShow.transform.localScale = Vector3.zero;
                // Animasyonun hedefi artık Vector3.one değil, hafızadaki 'initialScale' değeri.
                objToShow.transform.DOScale(initialScale, cosmeticAppearDuration).SetEase(Ease.OutBack);
                // -------------------------
            }
            else
            {
                Debug.LogWarning($"Görünmesi istenen '{idToShow}' ID'li obje sahnede bulunamadı!");
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // .Contains() metodunu string dizilerinde kullanmak için bu satır GEREKLİ!

public class VisualManager : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Sahnedeki tüm CosmeticObject'lerin parent'ı olan obje.")]
    [SerializeField] private Transform cosmeticsParent;

    private List<CosmeticObject> allCosmeticObjectsInScene = new List<CosmeticObject>();

    private void Awake()
    {
        // Sahne her yüklendiğinde, liste temizlenir ve sahnedeki MEVCUT
        // kozmetik objelerle yeniden doldurulur.
        if (cosmeticsParent != null)
        {
            allCosmeticObjectsInScene = cosmeticsParent.GetComponentsInChildren<CosmeticObject>(true).ToList();
        }
        else
        {
            Debug.LogError("Cosmetics Parent referansı VisualManager'da atanmamış!", this.gameObject);
        }

        // UpgradeManager'a abone ol.
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

    // UIManager'ın sahne açıldığında çağırması için public metot.
    public void InitializeVisuals(LevelData currentLevelData)
    {
        if (currentLevelData == null) return;
        UpdateCosmeticObjects(currentLevelData);
    }

    private void HandleLevelUp(LevelData newLevelData)
    {
        UpdateCosmeticObjects(newLevelData);
    }

    // --- SON VE BU SEFER KESİNLİKLE DOĞRU KOD ---
    private void UpdateCosmeticObjects(LevelData levelData)
    {
        // LevelData referansının var olduğundan emin ol.
        if (levelData == null) return;

        // LevelData içindeki 'cosmeticObjectsToShow' string dizisini referans al.
        if (levelData.cosmeticObjectsToShow == null)
        {
            foreach (var cosmetic in allCosmeticObjectsInScene)
            {
                if (cosmetic != null) cosmetic.gameObject.SetActive(false);
            }
            return;
        }

        // Sahnedeki her bir kozmetik objeyi döngüye al.
        foreach (var cosmeticInScene in allCosmeticObjectsInScene)
        {
            // Objenin kendisinin yok edilmediğinden emin ol.
            if (cosmeticInScene == null) continue;

            // Sahnedeki bu objenin objectID'sinin (string),
            // mevcut seviyenin "gösterilecekler" string dizisinde olup olmadığını kontrol et.
            bool shouldBeActive = levelData.cosmeticObjectsToShow.Contains(cosmeticInScene.objectID);
            cosmeticInScene.gameObject.SetActive(shouldBeActive);
        }
    }
}
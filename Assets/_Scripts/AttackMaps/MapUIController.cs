using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class MapUIController : MonoBehaviour
{
    [Header("Panel Referansları")]
    [SerializeField] private GameObject playerCastlePanel;
    // Düşman ve sonuç panelleri buradan kaldırıldı.

    [Header("Oyuncu Kalesi Paneli UI")]
    [Tooltip("Asker listesi elemanlarının (prefab) oluşturulacağı parent obje.")]
    [SerializeField] private Transform soldierListContent;
    [Tooltip("Tek bir asker türünü gösteren prefab.")]
    [SerializeField] private GameObject soldierListItemPrefab;
    [SerializeField] private Button closePlayerPanelButton;

    [Header("Genel Butonlar")]
    [SerializeField] private Button backToMainSceneButton;

    private void Start()
    {
        // Butonların tıklama olaylarını ayarla
        closePlayerPanelButton.onClick.AddListener(HideAllPanels);
        backToMainSceneButton.onClick.AddListener(() => SceneLoader.Instance.LoadMainScene());

        HideAllPanels(); // Başlangıçta tüm panelleri gizle
    }

    /// <summary>
    /// Oyuncu kalesi bilgi panelini, sahip olunan askerlerin listesiyle birlikte gösterir.
    /// </summary>
    public void ShowPlayerCastlePanel()
    {
        HideAllPanels();
        playerCastlePanel.SetActive(true);

        PopulateSoldierList();
    }

    /// <summary>
    /// SoldierManager'daki verilere göre sahip olunan askerlerin listesini UI'da oluşturur.
    /// </summary>
    private void PopulateSoldierList()
    {
        // Önce mevcut listeyi temizle
        foreach (Transform child in soldierListContent)
        {
            Destroy(child.gameObject);
        }

        if (SoldierManager.Instance == null || SoldierManager.Instance.allSoldierTypes == null)
        {
            return;
        }

        // SoldierManager'da tanımlı olan tüm asker türlerini döngüye al
        foreach (SoldierData soldierType in SoldierManager.Instance.allSoldierTypes)
        {
            int count = SoldierManager.Instance.GetSoldierCount(soldierType.soldierName);

            if (count > 0)
            {
                GameObject itemGO = Instantiate(soldierListItemPrefab, soldierListContent);
                SoldierListItemUI listItem = itemGO.GetComponent<SoldierListItemUI>();
                if (listItem != null)
                {
                    listItem.Setup(soldierType.shopIcon, count);
                }
            }
        }
    }

    // --- KALDIRILAN FONKSİYONLAR ---
    // ShowEnemyCastlePanel() ve ShowBattleResultPanel() buradan kaldırıldı.
    // --- KALDIRMA SONU ---

    public void HideAllPanels()
    {
        playerCastlePanel.SetActive(false);
        // Diğer paneller kaldırıldığı için gizleme komutlarına gerek kalmadı.
    }
}
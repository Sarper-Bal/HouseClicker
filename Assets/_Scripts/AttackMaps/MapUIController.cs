using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapUIController : MonoBehaviour
{
    [Header("Panel Referansları")]
    [SerializeField] private GameObject playerCastlePanel;
    [SerializeField] private GameObject enemyCastlePanel;

    [Header("Oyuncu Kalesi Paneli UI")]
    [Tooltip("Asker listesi elemanlarının (prefab) oluşturulacağı parent obje.")]
    [SerializeField] private Transform soldierListContent;
    [Tooltip("Tek bir asker türünü gösteren prefab.")]
    [SerializeField] private GameObject soldierListItemPrefab;
    [SerializeField] private Button closePlayerPanelButton;

    [Header("Düşman Kalesi Paneli UI")]
    [SerializeField] private TextMeshProUGUI enemyCastleNameText;
    [SerializeField] private Transform enemySoldierListContent;
    [SerializeField] private Button closeEnemyPanelButton;
    [SerializeField] private Button attackButton; // --- YENİ EKLENDİ ---

    [Header("Genel Butonlar")]
    [SerializeField] private Button backToMainSceneButton;

    private void Start()
    {
        // Butonların tıklama olaylarını ayarla
        closePlayerPanelButton.onClick.AddListener(HideAllPanels);
        closeEnemyPanelButton.onClick.AddListener(HideAllPanels);
        backToMainSceneButton.onClick.AddListener(() => SceneLoader.Instance.LoadMainScene());

        // AttackButton'a şimdilik sadece bir log mesajı ekleyelim.
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(() => Debug.Log("Saldır butonuna tıklandı! Savaş sistemi gelecekte eklenecek."));
        }

        HideAllPanels(); // Başlangıçta tüm panelleri gizle
    }

    public void ShowPlayerCastlePanel()
    {
        HideAllPanels();
        playerCastlePanel.SetActive(true);
        PopulateSoldierList();
    }

    // --- GÜNCELLENEN FONKSİYON ---
    /// <summary>
    /// Düşman kalesi bilgi panelini gösterir ve kalenin durumuna göre 'Saldır' butonunu ayarlar.
    /// </summary>
    public void ShowEnemyCastlePanel(EnemyCastleData castleData, bool isAttackable)
    {
        HideAllPanels();
        enemyCastlePanel.SetActive(true);

        // Butonun tıklanabilirliğini ayarla
        attackButton.interactable = isAttackable;

        if (castleData != null)
        {
            enemyCastleNameText.text = castleData.castleName;
            PopulateEnemySoldierList(castleData);
        }
    }
    // -------------------------

    private void PopulateSoldierList()
    {
        foreach (Transform child in soldierListContent)
        {
            Destroy(child.gameObject);
        }

        if (SoldierManager.Instance == null || SoldierManager.Instance.allSoldierTypes == null)
        {
            return;
        }

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

    private void PopulateEnemySoldierList(EnemyCastleData castleData)
    {
        foreach (Transform child in enemySoldierListContent)
        {
            Destroy(child.gameObject);
        }

        if (castleData.armyComposition == null) return;

        foreach (EnemyArmyUnit unit in castleData.armyComposition)
        {
            if (unit.count > 0 && unit.soldierData != null)
            {
                GameObject itemGO = Instantiate(soldierListItemPrefab, enemySoldierListContent);
                SoldierListItemUI listItem = itemGO.GetComponent<SoldierListItemUI>();
                if (listItem != null)
                {
                    listItem.Setup(unit.soldierData.shopIcon, unit.count);
                }
            }
        }
    }

    public void HideAllPanels()
    {
        playerCastlePanel.SetActive(false);
        enemyCastlePanel.SetActive(false);
    }
}
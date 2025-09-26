using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // List kullanabilmek için eklendi

public class MapUIController : MonoBehaviour
{
    [Header("Panel Referansları")]
    [SerializeField] private GameObject playerCastlePanel;
    [SerializeField] private GameObject enemyCastlePanel; // --- YENİ EKLENDİ ---

    [Header("Oyuncu Kalesi Paneli UI")]
    [Tooltip("Asker listesi elemanlarının (prefab) oluşturulacağı parent obje.")]
    [SerializeField] private Transform soldierListContent;
    [Tooltip("Tek bir asker türünü gösteren prefab.")]
    [SerializeField] private GameObject soldierListItemPrefab;
    [SerializeField] private Button closePlayerPanelButton;

    // --- YENİ EKLENEN KISIM ---
    [Header("Düşman Kalesi Paneli UI")]
    [SerializeField] private TextMeshProUGUI enemyCastleNameText;
    [SerializeField] private Transform enemySoldierListContent;
    [SerializeField] private Button closeEnemyPanelButton;
    // -------------------------

    [Header("Genel Butonlar")]
    [SerializeField] private Button backToMainSceneButton;

    private void Start()
    {
        // Butonların tıklama olaylarını ayarla
        closePlayerPanelButton.onClick.AddListener(HideAllPanels);
        closeEnemyPanelButton.onClick.AddListener(HideAllPanels); // --- YENİ EKLENDİ ---
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

    // --- YENİ EKLENEN FONKSİYON ---
    /// <summary>
    /// Düşman kalesi bilgi panelini, verilen kale verilerine göre doldurarak gösterir.
    /// </summary>
    public void ShowEnemyCastlePanel(EnemyCastleData castleData)
    {
        HideAllPanels();
        enemyCastlePanel.SetActive(true);

        if (castleData != null)
        {
            enemyCastleNameText.text = castleData.castleName;
            PopulateEnemySoldierList(castleData);
        }
    }
    // -------------------------

    /// <summary>
    /// SoldierManager'daki verilere göre sahip olunan askerlerin listesini UI'da oluşturur.
    /// </summary>
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

    // --- YENİ EKLENEN FONKSİYON ---
    /// <summary>
    /// Verilen düşman kalesi verilerine göre asker listesini UI'da oluşturur.
    /// </summary>
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
    // -------------------------

    public void HideAllPanels()
    {
        playerCastlePanel.SetActive(false);
        enemyCastlePanel.SetActive(false); // --- YENİ EKLENDİ ---
    }
}